using System;
using System.Linq;
using ConVar;
using Rust;
using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Loot Spawn")]
public class LootSpawn : ScriptableObject
{
	[Serializable]
	public struct Entry
	{
		[Tooltip("If this category is chosen, we will spawn 1+ this amount")]
		public int extraSpawns;

		[Tooltip("If a subcategory exists we'll choose from there instead of any items specified")]
		public LootSpawn category;

		[Tooltip("The higher this number, the more likely this will be chosen")]
		public int weight;

		[Tooltip("Adding eras to this list will restrict the loot table to only work in these eras")]
		public Era[] restrictedEras;

		public int RuntimeWeightBonus()
		{
			return (BaseGameMode.GetActiveGameMode(serverside: true)?.GetLootWeightModifier(GetWeightBonusItem())).GetValueOrDefault();
		}

		public ItemDefinition GetWeightBonusItem()
		{
			if (category.allowedItems.Length == 1)
			{
				return category.allowedItems[0].itemDef;
			}
			return null;
		}
	}

	public ItemAmountRanged[] items;

	public Entry[] subSpawn;

	[NonSerialized]
	private Entry[] allowedSubSpawn;

	[NonSerialized]
	private ItemAmountRanged[] allowedItems;

	private Era era;

	private uint lastGameModeFilterApplied;

	public bool HasAnySpawns()
	{
		EnsureFilterUpdated();
		if (allowedSubSpawn.Length == 0)
		{
			return allowedItems.Length != 0;
		}
		return true;
	}

	public void ClearCache()
	{
		allowedItems = null;
		allowedSubSpawn = null;
		if (subSpawn != null)
		{
			Entry[] array = subSpawn;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].category?.ClearCache();
			}
		}
	}

	private void EnsureFilterUpdated()
	{
		bool num = era != ConVar.Server.Era;
		bool flag = lastGameModeFilterApplied != BaseGameMode.GetActiveGameModeId(serverside: true);
		if (!num && !flag && allowedSubSpawn != null)
		{
			return;
		}
		era = ConVar.Server.Era;
		lastGameModeFilterApplied = BaseGameMode.GetActiveGameModeId(serverside: true);
		Entry[] array = subSpawn;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].category.EnsureFilterUpdated();
		}
		if (subSpawn == null || subSpawn.Length == 0)
		{
			allowedSubSpawn = Array.Empty<Entry>();
		}
		else
		{
			allowedSubSpawn = subSpawn.Where((Entry x) => x.category.HasAnySpawns() && (x.restrictedEras == null || x.restrictedEras.Length == 0 || Array.IndexOf(x.restrictedEras, ConVar.Server.Era) != -1)).ToArray();
		}
		if (items == null || items.Length == 0)
		{
			allowedItems = Array.Empty<ItemAmountRanged>();
			return;
		}
		allowedItems = items.Where((ItemAmountRanged x) => x.itemDef.IsAllowed(EraRestriction.Loot)).ToArray();
	}

	public ItemDefinition GetBlueprintBaseDef()
	{
		return ItemManager.FindItemDefinition("blueprintbase");
	}

	public void SpawnIntoContainer(ItemContainer container, ItemOwnershipShare ownership = default(ItemOwnershipShare), ItemContainer fallbackContainer = null)
	{
		EnsureFilterUpdated();
		if (allowedSubSpawn != null && allowedSubSpawn.Length != 0)
		{
			SubCategoryIntoContainer(container, ownership, fallbackContainer);
		}
		else
		{
			if (allowedItems == null)
			{
				return;
			}
			ItemAmountRanged[] array = allowedItems;
			foreach (ItemAmountRanged itemAmountRanged in array)
			{
				if (itemAmountRanged == null)
				{
					continue;
				}
				int num = (int)itemAmountRanged.GetAmount();
				ItemDefinition itemDef = itemAmountRanged.itemDef;
				if (itemDef == null)
				{
					continue;
				}
				int num2 = 0;
				while (num > 0 && num2 < 20)
				{
					num2++;
					Item item = null;
					if (itemAmountRanged.itemDef.spawnAsBlueprint)
					{
						ItemDefinition blueprintBaseDef = GetBlueprintBaseDef();
						if (blueprintBaseDef == null)
						{
							break;
						}
						Item item2 = ItemManager.Create(blueprintBaseDef, 1, 0uL, isServerSide: true, 0uL);
						item2.blueprintTarget = itemAmountRanged.itemDef.itemid;
						item = item2;
						num = 0;
					}
					else
					{
						int num3 = itemDef.stackable;
						if (container.maxStackSize > 0)
						{
							num3 = Mathf.Min(num3, container.maxStackSize);
						}
						item = ItemManager.Create(itemDef, Mathf.Max(1, Mathf.Min(num, num3)), 0uL, isServerSide: true, 0uL);
					}
					if (item == null)
					{
						continue;
					}
					num -= item.amount;
					item.OnVirginSpawn();
					if (ownership.IsValid())
					{
						item.SetItemOwnership(ownership.username, ownership.reason);
					}
					if (!item.MoveToContainer(container) && (fallbackContainer == null || !item.MoveToContainer(fallbackContainer)))
					{
						if ((bool)container.playerOwner)
						{
							item.Drop(container.playerOwner.GetDropPosition(), container.playerOwner.GetDropVelocity());
						}
						else
						{
							item.Remove();
						}
					}
				}
			}
		}
	}

	private void SubCategoryIntoContainer(ItemContainer container, ItemOwnershipShare ownership = default(ItemOwnershipShare), ItemContainer fallbackContainer = null)
	{
		int num = allowedSubSpawn.Sum((Entry x) => x.weight + x.RuntimeWeightBonus());
		int num2 = UnityEngine.Random.Range(0, num);
		for (int i = 0; i < allowedSubSpawn.Length; i++)
		{
			if (allowedSubSpawn[i].category == null)
			{
				continue;
			}
			num -= allowedSubSpawn[i].weight + allowedSubSpawn[i].RuntimeWeightBonus();
			if (num2 >= num)
			{
				for (int j = 0; j < 1 + allowedSubSpawn[i].extraSpawns; j++)
				{
					allowedSubSpawn[i].category.SpawnIntoContainer(container, ownership, fallbackContainer);
				}
				return;
			}
		}
		string text = ((container.entityOwner != null) ? container.entityOwner.name : "Unknown");
		Debug.LogWarning($"SubCategoryIntoContainer for loot '{base.name}' for entity '{text}' ended with randomWeight ({num2}) < totalWeight ({num}). This should never happen! ", this);
	}
}
