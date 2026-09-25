using System;
using System.Collections.Generic;
using Facepunch;
using Facepunch.Rust;
using Oxide.Core;
using Rust;
using UnityEngine;

public class ResourceDispenser : EntityComponent<BaseEntity>, IServerComponent
{
	public enum GatherType
	{
		Tree,
		Ore,
		Flesh,
		UNSET,
		LAST
	}

	[Serializable]
	public class GatherPropertyEntry
	{
		public float gatherDamage;

		public float destroyFraction;

		public float conditionLost;
	}

	[Serializable]
	public class GatherProperties
	{
		public GatherPropertyEntry Tree;

		public GatherPropertyEntry Ore;

		public GatherPropertyEntry Flesh;

		public bool ProduceHeadItem;

		public float GetProficiency()
		{
			float num = 0f;
			for (int i = 0; i < 3; i++)
			{
				GatherPropertyEntry fromIndex = GetFromIndex((GatherType)i);
				float num2 = fromIndex.gatherDamage * fromIndex.destroyFraction;
				if (num2 > 0f)
				{
					num += fromIndex.gatherDamage / num2;
				}
			}
			return num;
		}

		public bool Any()
		{
			for (int i = 0; i < 3; i++)
			{
				GatherPropertyEntry fromIndex = GetFromIndex((GatherType)i);
				if (fromIndex.gatherDamage > 0f || fromIndex.conditionLost > 0f)
				{
					return true;
				}
			}
			return false;
		}

		public GatherPropertyEntry GetFromIndex(GatherType index)
		{
			return index switch
			{
				GatherType.Tree => Tree, 
				GatherType.Ore => Ore, 
				GatherType.Flesh => Flesh, 
				_ => null, 
			};
		}
	}

	public Translate.Phrase OwnershipPhrase;

	public GatherType gatherType = GatherType.UNSET;

	public List<ItemAmount> containedItems;

	public float maxDestroyFractionForFinishBonus = 0.2f;

	public List<ItemAmount> finishBonus;

	public bool forceFullFinishBonus;

	public float fractionRemaining = 1f;

	private float categoriesRemaining;

	private float startingItemCounts;

	private static Dictionary<GatherType, HashSet<int>> cachedResourceItemTypes;

	public void Start()
	{
		Initialize();
	}

	public void Initialize()
	{
		CacheResourceTypeItems();
		UpdateFraction();
		UpdateRemainingCategories();
		CountAllItems();
	}

	private void CacheResourceTypeItems()
	{
		if (cachedResourceItemTypes == null)
		{
			cachedResourceItemTypes = new Dictionary<GatherType, HashSet<int>>();
			HashSet<int> hashSet = new HashSet<int>();
			hashSet.Add(ItemManager.FindItemDefinition("wood").itemid);
			cachedResourceItemTypes.Add(GatherType.Tree, hashSet);
			HashSet<int> hashSet2 = new HashSet<int>();
			hashSet2.Add(ItemManager.FindItemDefinition("stones").itemid);
			hashSet2.Add(ItemManager.FindItemDefinition("sulfur.ore").itemid);
			hashSet2.Add(ItemManager.FindItemDefinition("metal.ore").itemid);
			hashSet2.Add(ItemManager.FindItemDefinition("hq.metal.ore").itemid);
			cachedResourceItemTypes.Add(GatherType.Ore, hashSet2);
		}
	}

	public void DoGather(HitInfo info, BaseCorpse corpse = null)
	{
		if (!base.baseEntity.isServer || !info.CanGather || info.DidGather)
		{
			return;
		}
		if (gatherType == GatherType.UNSET)
		{
			Debug.LogWarning("Object :" + base.gameObject.name + ": has unset gathertype!");
			return;
		}
		float num = 0f;
		float num2 = 0f;
		float num3 = ((info.InitiatorPlayer != null && info.InitiatorPlayer.HasPlayerFlag(BasePlayer.PlayerFlags.IsInTutorial)) ? 3f : 1f);
		BaseMelee baseMelee = ((info.Weapon == null) ? null : (info.Weapon as BaseMelee));
		if (baseMelee != null)
		{
			GatherPropertyEntry gatherInfoFromIndex = baseMelee.GetGatherInfoFromIndex(gatherType);
			num = gatherInfoFromIndex.gatherDamage * info.gatherScale * num3;
			num2 = gatherInfoFromIndex.destroyFraction;
			if (num == 0f)
			{
				return;
			}
			baseMelee.SendPunch(new Vector3(UnityEngine.Random.Range(0.5f, 1f), UnityEngine.Random.Range(-0.25f, -0.5f), 0f) * -30f * (gatherInfoFromIndex.conditionLost / 6f), 0.05f);
			baseMelee.LoseCondition(gatherInfoFromIndex.conditionLost);
			if (!baseMelee.IsValid() || baseMelee.IsBroken())
			{
				return;
			}
			info.DidGather = true;
		}
		else
		{
			num = info.damageTypes.Total();
			num2 = 0.5f;
		}
		float num4 = fractionRemaining;
		GiveResources(info.InitiatorPlayer, num, num2, info.Weapon);
		UpdateFraction();
		float num5 = 0f;
		if (fractionRemaining <= 0f)
		{
			num5 = base.baseEntity.MaxHealth();
			if (info.DidGather && num2 < maxDestroyFractionForFinishBonus)
			{
				AssignFinishBonus(info.InitiatorPlayer, 1f - num2, info.Weapon);
			}
			if (base.gameObject.TryGetComponent<HeadDispenser>(out var component))
			{
				component.DispenseHead(info, corpse);
			}
		}
		else
		{
			num5 = (num4 - fractionRemaining) * base.baseEntity.MaxHealth();
		}
		HitInfo obj = Pool.Get<HitInfo>();
		obj.Init(info.Initiator, base.baseEntity, DamageType.Generic, num5, base.transform.position);
		obj.gatherScale = 0f;
		obj.PointStart = info.PointStart;
		obj.PointEnd = info.PointEnd;
		obj.WeaponPrefab = info.WeaponPrefab;
		obj.Weapon = info.Weapon;
		base.baseEntity.OnAttacked(obj);
		Pool.Free(ref obj);
	}

	public void AssignFinishBonus(BasePlayer player, float fraction, AttackEntity weapon)
	{
		if (forceFullFinishBonus)
		{
			fraction = 1f;
		}
		SendMessage("FinishBonusAssigned", SendMessageOptions.DontRequireReceiver);
		if (fraction <= 0f || finishBonus == null)
		{
			return;
		}
		foreach (ItemAmount finishBonu in finishBonus)
		{
			int num = Mathf.CeilToInt((float)(int)finishBonu.amount * Mathf.Clamp01(fraction));
			int num2 = CalculateGatherBonus(player, finishBonu, num);
			Item item = ItemManager.Create(finishBonu.itemDef, ScaleGatherAmount(num + num2), 0uL, isServerSide: true, 0uL);
			if (item != null)
			{
				object obj = Interface.CallHook("OnDispenserBonus", this, player, item);
				if (obj is Item)
				{
					item = (Item)obj;
				}
				ApplyItemOwnership(player, item);
				Facepunch.Rust.Analytics.Azure.OnGatherItem(item.info.shortname, item.amount, base.baseEntity, player, weapon);
				Interface.CallHook("OnDispenserBonusReceived", this, player, item);
				while (item.amount > item.MaxStackable())
				{
					Item item2 = item.SplitItem(item.MaxStackable());
					player.GiveItem(item2, BaseEntity.GiveItemReason.ResourceHarvested, GiveItemOptions.BackpackOverflow);
				}
				player.GiveItem(item, BaseEntity.GiveItemReason.ResourceHarvested, GiveItemOptions.BackpackOverflow);
			}
		}
	}

	public void OnResourceDispenserAttacked(HitInfo info)
	{
		DoGather(info);
	}

	private void GiveResources(BasePlayer entity, float gatherDamage, float destroyFraction, AttackEntity attackWeapon)
	{
		if (!entity.IsValid() || gatherDamage <= 0f)
		{
			return;
		}
		ItemAmount itemAmount = null;
		int num = containedItems.Count;
		int num2 = UnityEngine.Random.Range(0, containedItems.Count);
		while (num > 0)
		{
			if (num2 >= containedItems.Count)
			{
				num2 = 0;
			}
			if (containedItems[num2].amount > 0f)
			{
				itemAmount = containedItems[num2];
				break;
			}
			num2++;
			num--;
		}
		if (itemAmount != null)
		{
			GiveResourceFromItem(entity, itemAmount, gatherDamage, destroyFraction, attackWeapon);
			UpdateVars();
		}
	}

	public float GiveResourcesForDamage(BasePlayer player, float damage, float destroyFraction, AttackEntity attackWeapon)
	{
		if (!player.IsValid())
		{
			return 0f;
		}
		if (damage <= 0f)
		{
			return 0f;
		}
		if (startingItemCounts <= 0f)
		{
			return 0f;
		}
		float num = base.baseEntity.MaxHealth();
		if (num <= 0f)
		{
			return 0f;
		}
		float num2 = Mathf.Min(damage, base.baseEntity.Health());
		float num3 = num / startingItemCounts;
		float num4 = Mathf.Floor(num2 / num3);
		if (num4 < 1f)
		{
			return 0f;
		}
		float num5 = num4 * num3;
		GiveResources(player, num5, destroyFraction, attackWeapon);
		return num5;
	}

	public void DestroyFraction(float fraction)
	{
		foreach (ItemAmount containedItem in containedItems)
		{
			if (containedItem.amount > 0f)
			{
				containedItem.amount -= fraction / categoriesRemaining;
			}
		}
		UpdateVars();
	}

	private void GiveResourceFromItem(BasePlayer entity, ItemAmount itemAmt, float gatherDamage, float destroyFraction, AttackEntity attackWeapon)
	{
		if (itemAmt.amount == 0f)
		{
			return;
		}
		float num = Mathf.Min(gatherDamage, base.baseEntity.Health()) / base.baseEntity.MaxHealth();
		float num2 = itemAmt.startAmount / startingItemCounts;
		float f = Mathf.Clamp(itemAmt.startAmount * num / num2, 0f, itemAmt.amount);
		f = Mathf.Round(f);
		float num3 = f * destroyFraction * 2f;
		if (itemAmt.amount <= f + num3)
		{
			float num4 = (f + num3) / itemAmt.amount;
			f /= num4;
			num3 /= num4;
		}
		itemAmt.amount -= Mathf.Floor(f);
		itemAmt.amount -= Mathf.Floor(num3);
		if (f < 1f)
		{
			f = ((UnityEngine.Random.Range(0f, 1f) <= f) ? 1f : 0f);
			itemAmt.amount = 0f;
		}
		if (itemAmt.amount < 0f)
		{
			itemAmt.amount = 0f;
		}
		if (!(f >= 1f))
		{
			return;
		}
		int num5 = CalculateGatherBonus(entity, itemAmt, f);
		int iAmount = ScaleGatherAmount(Mathf.FloorToInt(f) + num5);
		Item item = ItemManager.CreateByItemID(itemAmt.itemid, iAmount, 0uL, 0uL);
		if (Interface.CallHook("OnDispenserGather", this, entity, item) == null && item != null)
		{
			ApplyItemOwnership(entity, item);
			OverrideOwnership(item, attackWeapon);
			Facepunch.Rust.Analytics.Azure.OnGatherItem(item.info.shortname, item.amount, base.baseEntity, entity, attackWeapon);
			Interface.CallHook("OnDispenserGathered", this, entity, item);
			while (item.amount > item.MaxStackable())
			{
				Item item2 = item.SplitItem(item.MaxStackable());
				entity.GiveItem(item2, BaseEntity.GiveItemReason.ResourceHarvested, GiveItemOptions.BackpackOverflow);
			}
			entity.GiveItem(item, BaseEntity.GiveItemReason.ResourceHarvested, GiveItemOptions.BackpackOverflow);
		}
	}

	private static int ScaleGatherAmount(int amount)
	{
		if (amount <= 0)
		{
			return amount;
		}
		float num = 1f;
		if (BaseGameMode.GetActiveGameMode(serverside: true) is GameModeSoftcore)
		{
			num = Mathf.Max(0f, GameModeSoftcore.gather_rate);
		}
		return Mathf.Max(1, Mathf.RoundToInt((float)amount * num));
	}

	private void ApplyItemOwnership(BasePlayer player, Item item)
	{
		if (OwnershipPhrase != null && !string.IsNullOrEmpty(OwnershipPhrase.token))
		{
			item.SetItemOwnership(player, OwnershipPhrase.token);
		}
		else
		{
			item.SetItemOwnership(player, ItemOwnershipPhrases.GatheredPhrase);
		}
	}

	private int CalculateGatherBonus(BaseEntity entity, ItemAmount item, float amountToGive)
	{
		if (entity == null)
		{
			return 0;
		}
		BasePlayer basePlayer = entity.ToPlayer();
		if (basePlayer == null)
		{
			return 0;
		}
		if (basePlayer.modifiers == null)
		{
			return 0;
		}
		if (base.baseEntity is MonumentBlocker)
		{
			return 0;
		}
		amountToGive = Mathf.FloorToInt(amountToGive);
		float num = 1f;
		Modifier.ModifierType type;
		switch (gatherType)
		{
		case GatherType.Tree:
			type = Modifier.ModifierType.Wood_Yield;
			break;
		case GatherType.Ore:
			type = Modifier.ModifierType.Ore_Yield;
			break;
		case GatherType.Flesh:
			type = Modifier.ModifierType.Harvesting;
			break;
		default:
			return 0;
		}
		if (!IsProducedItemOfGatherType(item))
		{
			return 0;
		}
		num += basePlayer.modifiers.GetValue(type);
		float variableValue = basePlayer.modifiers.GetVariableValue(type, 0f);
		float num2 = ((num > 1f) ? Mathf.Max(amountToGive * num - amountToGive, 0f) : 0f);
		variableValue += num2;
		int num3 = 0;
		if (variableValue >= 1f)
		{
			num3 = (int)variableValue;
			variableValue -= (float)num3;
		}
		basePlayer.modifiers.SetVariableValue(type, variableValue);
		return num3;
	}

	private bool IsProducedItemOfGatherType(ItemAmount item)
	{
		if (gatherType == GatherType.Tree)
		{
			return cachedResourceItemTypes[GatherType.Tree].Contains(item.itemid);
		}
		if (gatherType == GatherType.Ore)
		{
			return cachedResourceItemTypes[GatherType.Ore].Contains(item.itemid);
		}
		if (gatherType == GatherType.Flesh)
		{
			return item.startAmount > 1f;
		}
		return false;
	}

	public virtual bool OverrideOwnership(Item item, AttackEntity weapon)
	{
		return false;
	}

	private void UpdateVars()
	{
		UpdateFraction();
		UpdateRemainingCategories();
	}

	public void UpdateRemainingCategories()
	{
		int num = 0;
		foreach (ItemAmount containedItem in containedItems)
		{
			if (containedItem.amount > 0f)
			{
				num++;
			}
		}
		categoriesRemaining = num;
	}

	public void CountAllItems()
	{
		float num = 0f;
		foreach (ItemAmount containedItem in containedItems)
		{
			num += containedItem.startAmount;
		}
		startingItemCounts = num;
	}

	private void UpdateFraction()
	{
		float num = 0f;
		float num2 = 0f;
		foreach (ItemAmount containedItem in containedItems)
		{
			num += containedItem.startAmount;
			num2 += containedItem.amount;
		}
		if (num == 0f)
		{
			fractionRemaining = 0f;
		}
		else
		{
			fractionRemaining = num2 / num;
		}
	}

	public bool HasItemToDispense(ItemDefinition def)
	{
		if (def == null)
		{
			return false;
		}
		foreach (ItemAmount containedItem in containedItems)
		{
			if (containedItem.itemDef == def && containedItem.amount > 0f)
			{
				return true;
			}
		}
		return false;
	}
}
