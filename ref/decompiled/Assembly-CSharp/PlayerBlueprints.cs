using System.Collections.Generic;
using Facepunch;
using Oxide.Core;
using ProtoBuf;

public class PlayerBlueprints : EntityComponent<BasePlayer>
{
	public SteamInventory steamInventory;

	public void Reset()
	{
		PersistantPlayer persistantPlayerInfo = base.baseEntity.PersistantPlayerInfo;
		if (persistantPlayerInfo.unlockedItems != null)
		{
			persistantPlayerInfo.unlockedItems.Clear();
		}
		else
		{
			persistantPlayerInfo.unlockedItems = Pool.Get<List<int>>();
		}
		base.baseEntity.PersistantPlayerInfo = persistantPlayerInfo;
		base.baseEntity.SendNetworkUpdate();
	}

	public void UnlockAll()
	{
		PersistantPlayer persistantPlayerInfo = base.baseEntity.PersistantPlayerInfo;
		foreach (ItemBlueprint bp in ItemManager.bpList)
		{
			if (bp.userCraftable && !bp.defaultBlueprint && !persistantPlayerInfo.unlockedItems.Contains(bp.targetItem.itemid))
			{
				persistantPlayerInfo.unlockedItems.Add(bp.targetItem.itemid);
			}
		}
		base.baseEntity.PersistantPlayerInfo = persistantPlayerInfo;
		base.baseEntity.SendNetworkUpdateImmediate();
		base.baseEntity.ClientRPC(RpcTarget.Player("UnlockedBlueprint", base.baseEntity), 0);
	}

	public bool IsUnlocked(ItemDefinition itemDef)
	{
		PersistantPlayer persistantPlayerInfo = base.baseEntity.PersistantPlayerInfo;
		if (persistantPlayerInfo.unlockedItems != null)
		{
			return persistantPlayerInfo.unlockedItems.Contains(itemDef.itemid);
		}
		return false;
	}

	public void Unlock(ItemDefinition itemDef)
	{
		PersistantPlayer persistantPlayerInfo = base.baseEntity.PersistantPlayerInfo;
		if (!persistantPlayerInfo.unlockedItems.Contains(itemDef.itemid))
		{
			persistantPlayerInfo.unlockedItems.Add(itemDef.itemid);
			base.baseEntity.PersistantPlayerInfo = persistantPlayerInfo;
			base.baseEntity.SendNetworkUpdateImmediate();
			base.baseEntity.ClientRPC(RpcTarget.Player("UnlockedBlueprint", base.baseEntity), itemDef.itemid);
			base.baseEntity.stats.Add("blueprint_studied", 1, (Stats)5);
		}
	}

	public void UnlockList(List<ItemDefinition> itemDefList)
	{
		PersistantPlayer persistantPlayerInfo = base.baseEntity.PersistantPlayerInfo;
		foreach (ItemDefinition itemDef in itemDefList)
		{
			if (!persistantPlayerInfo.unlockedItems.Contains(itemDef.itemid))
			{
				persistantPlayerInfo.unlockedItems.Add(itemDef.itemid);
			}
		}
		base.baseEntity.PersistantPlayerInfo = persistantPlayerInfo;
		base.baseEntity.SendNetworkUpdateImmediate();
		base.baseEntity.ClientRPC(RpcTarget.Player("UnlockedBlueprint", base.baseEntity));
		base.baseEntity.stats.Add("blueprint_studied", itemDefList.Count, (Stats)5);
	}

	public bool HasUnlocked(ItemDefinition targetItem)
	{
		if (base.baseEntity.IsCraftingTutorialBlocked(targetItem, out var forceUnlock))
		{
			return false;
		}
		if (forceUnlock)
		{
			return true;
		}
		if ((bool)targetItem.Blueprint)
		{
			if (targetItem.Blueprint.RequireUnlockedItem != null && !HasUnlocked(targetItem.Blueprint.RequireUnlockedItem))
			{
				return false;
			}
			if (targetItem.Blueprint.NeedsSteamItem)
			{
				if (targetItem.steamItem != null)
				{
					if (targetItem.steamItem.UnlockedViaSteamItem != null)
					{
						if (!steamInventory.HasItem(targetItem.steamItem.UnlockedViaSteamItem.id))
						{
							return false;
						}
					}
					else if (!steamInventory.HasItem(targetItem.steamItem.id))
					{
						return false;
					}
				}
				if (targetItem.steamItem == null)
				{
					bool flag = false;
					ItemSkinDirectory.Skin[] skins = targetItem.skins;
					for (int i = 0; i < skins.Length; i++)
					{
						ItemSkinDirectory.Skin skin = skins[i];
						if (steamInventory.HasItem(skin.id))
						{
							flag = true;
							break;
						}
					}
					if (!flag && steamInventory.HasWorkshopSkin(targetItem))
					{
						flag = true;
					}
					if (!flag)
					{
						return false;
					}
				}
				return true;
			}
			if (targetItem.Blueprint.NeedsSteamDLC)
			{
				return ItemPassesDlcChecks(targetItem);
			}
		}
		int[] defaultBlueprints = ItemManager.defaultBlueprints;
		for (int i = 0; i < defaultBlueprints.Length; i++)
		{
			if (defaultBlueprints[i] == targetItem.itemid)
			{
				return true;
			}
		}
		if (base.baseEntity.isServer)
		{
			return IsUnlocked(targetItem);
		}
		return false;
	}

	public bool ItemPassesDlcChecks(ItemDefinition targetItem)
	{
		if (!targetItem.Blueprint.NeedsSteamDLC)
		{
			return true;
		}
		if (targetItem.steamDlc == null)
		{
			return false;
		}
		if (!base.baseEntity.DefaultSkinAccess)
		{
			if (!base.baseEntity.AllSkinsUnlocked)
			{
				return targetItem.steamDlc.bypassLicenseCheck;
			}
			return true;
		}
		if (targetItem.steamDlc.HasLicense(base.baseEntity))
		{
			return true;
		}
		return false;
	}

	public bool CanCraft(int itemid, int skinItemId, BasePlayer player)
	{
		ItemDefinition itemDefinition = ItemManager.FindItemDefinition(itemid);
		if (itemDefinition == null)
		{
			return false;
		}
		object obj = Interface.CallHook("CanCraft", this, itemDefinition, skinItemId);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (skinItemId != 0 && !CheckSkinOwnership(skinItemId, player))
		{
			return false;
		}
		if (base.baseEntity.currentCraftLevel < (float)itemDefinition.Blueprint.GetWorkbenchLevel())
		{
			return false;
		}
		if (HasUnlocked(itemDefinition))
		{
			return true;
		}
		return false;
	}

	public bool CheckSkinOwnership(int skinItemId, BasePlayer player)
	{
		ItemSkinDirectory.Skin skin = ItemSkinDirectory.FindByInventoryDefinitionId(skinItemId);
		if (skin.invItem != null && skin.invItem.HasUnlocked(player))
		{
			return true;
		}
		return steamInventory.HasItem(skinItemId);
	}
}
