#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Linq;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class RepairBench : StorageContainer
{
	public float maxConditionLostOnRepair = 0.2f;

	public GameObjectRef skinchangeEffect;

	public const float REPAIR_COST_FRACTION = 0.2f;

	private float nextSkinChangeAudioTime;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("RepairBench.OnRpcMessage"))
		{
			if (rpc == 1942825351 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - ChangeSkin");
				}
				using (TimeWarning.New("ChangeSkin"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1942825351u, "ChangeSkin", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg2 = rPCMessage;
							ChangeSkin(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in ChangeSkin");
					}
				}
				return true;
			}
			if (rpc == 1178348163 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RepairItem");
				}
				using (TimeWarning.New("RepairItem"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1178348163u, "RepairItem", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg3 = rPCMessage;
							RepairItem(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in RepairItem");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public static float GetRepairFraction(Item itemToRepair)
	{
		return GetRepairFraction(itemToRepair.condition, itemToRepair.maxCondition);
	}

	public static float GetRepairFraction(float condition, float maxCondition)
	{
		return 1f - condition / maxCondition;
	}

	public static float RepairCostFraction(Item itemToRepair)
	{
		return GetRepairFraction(itemToRepair) * 0.2f;
	}

	public static float RepairCostFraction(float condition, float maxCondition)
	{
		return GetRepairFraction(condition, maxCondition) * 0.2f;
	}

	public static void GetRepairCostList(ItemBlueprint bp, List<ItemAmount> allIngredients)
	{
		ItemModRepair itemModRepair = bp.targetItem?.GetComponent<ItemModRepair>();
		if (itemModRepair != null && itemModRepair.canUseRepairBench)
		{
			return;
		}
		foreach (ItemAmount ingredient in bp.GetIngredients())
		{
			allIngredients.Add(new ItemAmount(ingredient.itemDef, ingredient.amount));
		}
		StripComponentRepairCost(allIngredients);
	}

	public static void StripComponentRepairCost(List<ItemAmount> allIngredients, float repairCostMultiplier = 1f)
	{
		if (allIngredients == null)
		{
			return;
		}
		for (int i = 0; i < allIngredients.Count; i++)
		{
			ItemAmount itemAmount = allIngredients[i];
			if (itemAmount.itemDef.category != ItemCategory.Component && !itemAmount.itemDef.treatAsComponentForRepairs)
			{
				continue;
			}
			if (itemAmount.itemDef.Blueprint != null)
			{
				bool flag = false;
				ItemAmount itemAmount2 = itemAmount.itemDef.Blueprint.GetIngredients()[0];
				foreach (ItemAmount allIngredient in allIngredients)
				{
					if (allIngredient.itemDef == itemAmount2.itemDef)
					{
						allIngredient.amount += Mathf.Max(itemAmount2.amount * itemAmount.amount * repairCostMultiplier, 1f);
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					allIngredients.Add(new ItemAmount(itemAmount2.itemDef, Mathf.Max(itemAmount2.amount * itemAmount.amount * repairCostMultiplier, 1f)));
				}
			}
			allIngredients.RemoveAt(i);
			i--;
		}
	}

	public void debugprint(string toPrint)
	{
		if (ConVar.Global.developer > 0)
		{
			Debug.LogWarning(toPrint);
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void ChangeSkin(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		int inventoryId = msg.read.Int32();
		ItemId itemId = ((msg.read.Unread > 0) ? new ItemId(msg.read.UInt64()) : default(ItemId));
		bool isValid = itemId.IsValid;
		bool flag = !isValid || UnityEngine.Time.realtimeSinceStartup > nextSkinChangeAudioTime;
		Item slot = base.inventory.GetSlot(0);
		if (slot == null || Interface.CallHook("OnItemSkinChange", inventoryId, slot, this, player) != null || (isValid && slot.uid != itemId))
		{
			return;
		}
		if (inventoryId != 0 && !player.blueprints.CheckSkinOwnership(inventoryId, player))
		{
			debugprint("RepairBench.ChangeSkin player does not have item :" + inventoryId + ":");
			return;
		}
		ulong Skin = ItemDefinition.FindSkin(slot.info.itemid, inventoryId);
		if (Skin == slot.skin && slot.info.isRedirectOf == null)
		{
			debugprint("RepairBench.ChangeSkin cannot apply same skin twice : " + Skin + ": " + slot.skin);
			return;
		}
		ItemSkinDirectory.Skin skin = slot.info.skins.FirstOrDefault((ItemSkinDirectory.Skin x) => (ulong)x.id == Skin);
		ItemDefinition itemDefinition = slot.info;
		int num = 0;
		if (slot.info.isRedirectOf != null)
		{
			Skin = ItemDefinition.FindSkin(slot.info.isRedirectOf.itemid, inventoryId);
			skin = slot.info.isRedirectOf.skins.FirstOrDefault((ItemSkinDirectory.Skin x) => (ulong)x.id == Skin);
			if (skin.invItem == null)
			{
				if (slot.info.isRedirectOf.skins2.FirstOrDefault((IPlayerItemDefinition x) => x.DefinitionId == inventoryId) != null)
				{
					itemDefinition = slot.info.isRedirectOf;
					num = inventoryId;
				}
				else
				{
					itemDefinition = slot.info.isRedirectOf;
					num = 0;
				}
			}
			else
			{
				num = skin.invItem.id;
				if (skin.invItem is ItemSkin itemSkin)
				{
					if (itemSkin.Redirect != null)
					{
						itemDefinition = itemSkin.Redirect;
						num = 0;
					}
					else if (itemSkin.Redirect == null && slot.info.isRedirectOf != null)
					{
						itemDefinition = slot.info.isRedirectOf;
					}
				}
			}
		}
		else if (slot.info.skins.FirstOrDefault((ItemSkinDirectory.Skin x) => (ulong)x.id == Skin).invItem is ItemSkin itemSkin2 && itemSkin2.Redirect != null)
		{
			itemDefinition = itemSkin2.Redirect;
		}
		if ((itemDefinition == slot.info && itemDefinition.isRedirectOf != null && num == 0) || (itemDefinition.isRedirectOf == null && itemDefinition.hidden))
		{
			return;
		}
		if (flag)
		{
			nextSkinChangeAudioTime = UnityEngine.Time.realtimeSinceStartup + 0.75f;
		}
		if (itemDefinition != slot.info)
		{
			bool flag2 = false;
			flag2 = num != 0;
			float condition = slot.condition;
			float maxCondition = slot.maxCondition;
			int amount = slot.amount;
			ulong attachment = slot.attachment;
			int num2 = 0;
			int num3 = 0;
			ItemModContainerArmorSlot component = slot.info.GetComponent<ItemModContainerArmorSlot>();
			if (component != null && slot.contents != null)
			{
				num3 = slot.contents.capacity;
			}
			ItemDefinition ammoType = null;
			if (slot.GetHeldEntity() != null && slot.GetHeldEntity() is BaseProjectile { primaryMagazine: not null } baseProjectile)
			{
				num2 = baseProjectile.primaryMagazine.contents;
				ammoType = baseProjectile.primaryMagazine.ammoType;
			}
			if (slot.GetHeldEntity() != null && slot.GetHeldEntity() is Chainsaw chainsaw)
			{
				num2 = chainsaw.ammo;
			}
			List<Item> obj = Facepunch.Pool.Get<List<Item>>();
			if (slot.contents != null && slot.contents.itemList != null && slot.contents.itemList.Count > 0)
			{
				if (slot.contents.itemList.Count > obj.Capacity)
				{
					obj.Capacity = slot.contents.itemList.Count;
				}
				foreach (Item item2 in slot.contents.itemList)
				{
					obj.Add(item2);
				}
				foreach (Item item3 in obj)
				{
					item3.RemoveFromContainer();
				}
			}
			Item item = ItemManager.Create(itemDefinition, 1, 0uL, isServerSide: true, 0uL);
			item.ownershipShares = slot.ownershipShares;
			slot.ownershipShares = null;
			slot.Remove();
			ItemManager.DoRemoves();
			item.MoveToContainer(base.inventory, 0, allowStack: false);
			item.maxCondition = maxCondition;
			item.condition = condition;
			item.amount = amount;
			if (item.GetHeldEntity() != null && item.GetHeldEntity() is BaseProjectile baseProjectile2)
			{
				if (baseProjectile2.primaryMagazine != null)
				{
					baseProjectile2.SetAmmoCount(num2);
					baseProjectile2.primaryMagazine.ammoType = ammoType;
				}
				baseProjectile2.ForceModsChanged();
			}
			if (item.GetHeldEntity() != null && item.GetHeldEntity() is Chainsaw chainsaw2)
			{
				chainsaw2.ammo = num2;
			}
			if (num3 > 0)
			{
				component = item.info.GetComponent<ItemModContainerArmorSlot>();
				component.CreateAtCapacity(num3, item);
			}
			if (obj.Count > 0 && item.contents != null)
			{
				if (component != null)
				{
					for (int i = 0; i < obj.Count; i++)
					{
						obj[i]?.MoveToContainer(item.contents, i, allowStack: false);
					}
				}
				else
				{
					foreach (Item item4 in obj)
					{
						item4.MoveToContainer(item.contents);
					}
				}
			}
			Facepunch.Pool.Free(ref obj, freeElements: false);
			if (attachment != 0L && item.info.supportsAccessories)
			{
				item.attachment = attachment;
				item.MarkDirty();
				BaseEntity heldEntity = item.GetHeldEntity();
				if (heldEntity != null)
				{
					heldEntity.attachmentID = item.attachment;
					heldEntity.SendNetworkUpdate();
				}
			}
			if (flag2)
			{
				ApplySkinToItem(item, Skin);
			}
			Facepunch.Rust.Analytics.Azure.OnSkinChanged(player, this, item, Skin);
		}
		else
		{
			ApplySkinToItem(slot, Skin);
			Facepunch.Rust.Analytics.Azure.OnSkinChanged(player, this, slot, Skin);
		}
		if (flag && skinchangeEffect.isValid)
		{
			Effect.server.Run(skinchangeEffect.resourcePath, this, 0u, new Vector3(0f, 1.5f, 0f), Vector3.zero);
		}
	}

	private void ApplySkinToItem(Item item, ulong Skin)
	{
		item.skin = Skin;
		item.MarkDirty();
		BaseEntity heldEntity = item.GetHeldEntity();
		if (heldEntity != null)
		{
			heldEntity.skinID = Skin;
			heldEntity.SendNetworkUpdate();
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RepairItem(RPCMessage msg)
	{
		Item slot = base.inventory.GetSlot(0);
		if (slot != null)
		{
			BasePlayer player = msg.player;
			float conditionLost = maxConditionLostOnRepair;
			ItemModRepair component = slot.info.GetComponent<ItemModRepair>();
			if (component != null)
			{
				conditionLost = component.conditionLost;
			}
			RepairAnItem(slot, player, this, conditionLost, mustKnowBlueprint: true);
		}
	}

	public override int GetIdealSlot(BasePlayer player, ItemContainer container, Item item)
	{
		return 0;
	}

	public static void RepairAnItem(Item itemToRepair, BasePlayer player, BaseEntity repairBenchEntity, float maxConditionLostOnRepair, bool mustKnowBlueprint)
	{
		if (itemToRepair == null)
		{
			return;
		}
		ItemDefinition info = itemToRepair.info;
		ItemBlueprint blueprint = info.Blueprint;
		if (!blueprint)
		{
			return;
		}
		ItemModRepair component = itemToRepair.info.GetComponent<ItemModRepair>();
		if (!info.condition.repairable || itemToRepair.condition == itemToRepair.maxCondition)
		{
			return;
		}
		if (mustKnowBlueprint)
		{
			ItemDefinition itemDefinition = ((info.isRedirectOf != null) ? info.isRedirectOf : info);
			bool flag = player.blueprints.HasUnlocked(itemDefinition) || (itemDefinition.Blueprint != null && !itemDefinition.Blueprint.isResearchable);
			if (!flag && BaseGameMode.svActiveGameMode != null && BaseGameMode.svActiveGameMode.canRepairIfCraftingBanned && !itemDefinition.IsAllowed(EraRestriction.Craft))
			{
				flag = true;
			}
			if (!flag)
			{
				return;
			}
		}
		if (Interface.CallHook("OnItemRepair", player, itemToRepair) != null)
		{
			return;
		}
		float num = RepairCostFraction(itemToRepair);
		bool flag2 = false;
		List<ItemAmount> obj = Facepunch.Pool.Get<List<ItemAmount>>();
		GetRepairCostList(blueprint, obj);
		foreach (ItemAmount item in obj)
		{
			if (item.itemDef.category != ItemCategory.Component)
			{
				int amount = player.inventory.GetAmount(item.itemDef.itemid);
				if (Mathf.CeilToInt(item.amount * num) > amount)
				{
					flag2 = true;
					break;
				}
			}
		}
		if (flag2)
		{
			Facepunch.Pool.FreeUnmanaged(ref obj);
			return;
		}
		foreach (ItemAmount item2 in obj)
		{
			if (item2.itemDef.category != ItemCategory.Component)
			{
				int amount2 = Mathf.CeilToInt(item2.amount * num);
				player.inventory.Take(null, item2.itemid, amount2);
				Facepunch.Rust.Analytics.Azure.LogResource(Facepunch.Rust.Analytics.Azure.ResourceMode.Consumed, "repair", item2.itemDef.shortname, amount2, repairBenchEntity, null, safezone: false, null, 0uL, null, itemToRepair, null, 0uL);
			}
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
		float conditionNormalized = itemToRepair.conditionNormalized;
		float maxConditionNormalized = itemToRepair.maxConditionNormalized;
		itemToRepair.DoRepair(maxConditionLostOnRepair);
		Facepunch.Rust.Analytics.Azure.OnItemRepaired(player, repairBenchEntity, itemToRepair, conditionNormalized, maxConditionNormalized);
		if (ConVar.Global.developer > 0)
		{
			Debug.Log("Item repaired! condition : " + itemToRepair.condition + "/" + itemToRepair.maxCondition);
		}
		string strName = "assets/bundled/prefabs/fx/repairbench/itemrepair.prefab";
		if (component != null && component.successEffect?.Get() != null)
		{
			strName = component.successEffect.resourcePath;
		}
		Effect.server.Run(strName, repairBenchEntity, 0u, Vector3.zero, Vector3.zero);
	}
}
