using Rust;
using UnityEngine;

public class ItemModContainerArmorSlot : ItemModContainer
{
	public int MinSlots;

	public int MaxSlots = 3;

	public bool CraftedItemsOnly = true;

	private static float[] RollChances = new float[3] { 0.25f, 0.1f, 0.05f };

	public override void OnVirginItem(Item item, BasePlayer creatingPlayer)
	{
		if (item.isServer)
		{
			base.OnVirginItem(item, creatingPlayer);
			CreateForPlayer(item, creatingPlayer);
		}
	}

	private static int GetRandomSlotCount(float improveChance, int min, int max)
	{
		float num = Random.Range(0f, 1f - improveChance);
		int num2 = 0;
		if (num <= 0.5f || improveChance > 0f)
		{
			num2 = min;
			for (int i = 0; i < max - min && num <= RollChances[Mathf.Clamp(i, 0, RollChances.Length)]; i++)
			{
				num2++;
			}
		}
		return num2;
	}

	public void CreateForPlayer(Item item, BasePlayer player)
	{
		if (!(player == null) || !CraftedItemsOnly)
		{
			int cap = 0;
			if (player != null)
			{
				cap = GetRandomSlotCount(Mathf.Clamp01(player.modifiers.GetValue(Modifier.ModifierType.Crafting_Quality)), MinSlots, MaxSlots);
			}
			CreateAtCapacity(cap, item);
		}
	}

	public void SetSlotAmount(Item item, int amount)
	{
		if (item.contents != null)
		{
			item.contents.capacity = amount;
			item.MarkDirty();
		}
		else
		{
			CreateAtCapacity(amount, item);
		}
	}

	public void CreateAtCapacity(int cap, Item item)
	{
		capacity = cap;
		if (capacity != 0)
		{
			CreateContents(item);
			if (item != null && item.contents != null)
			{
				item.contents.canAcceptItem = CanAcceptArmorItem;
			}
		}
	}

	public override void OnItemCreated(Item item)
	{
		if (capacity > 0 && item != null && item.contents != null)
		{
			item.contents.canAcceptItem = CanAcceptArmorItem;
		}
	}

	protected bool CanAcceptArmorItem(BasePlayer player, Item item, int count)
	{
		if (item == null || item.info == null)
		{
			return false;
		}
		if (item.info.GetComponent<ItemModArmorInsert>() == null)
		{
			return false;
		}
		return true;
	}

	public float TotalSpeedReduction(Item item)
	{
		if (item == null)
		{
			return 0f;
		}
		if (item.contents == null)
		{
			return 0f;
		}
		if (item.contents.itemList == null)
		{
			return 0f;
		}
		float num = 0f;
		foreach (Item item2 in item.contents.itemList)
		{
			if (item2.info.TryGetComponent<ItemModArmorInsert>(out var component))
			{
				num += component.SpeedReduction;
			}
		}
		return num;
	}

	public float GetProtection(Item item, DamageType damageType)
	{
		if (item == null)
		{
			return 0f;
		}
		if (item.contents == null)
		{
			return 0f;
		}
		if (item.contents.itemList == null)
		{
			return 0f;
		}
		float num = 0f;
		foreach (Item item2 in item.contents.itemList)
		{
			if (item2.info.TryGetComponent<ItemModArmorInsert>(out var component))
			{
				num += component.protectionProperties.amounts[(int)damageType];
			}
		}
		return num;
	}

	public void CollectProtection(Item item, ProtectionProperties protection, float multiplier = 1f)
	{
		if (item == null || item.contents == null || protection == null)
		{
			return;
		}
		foreach (Item item2 in item.contents.itemList)
		{
			if (item2.info.TryGetComponent<ItemModArmorInsert>(out var component) && !(component.protectionProperties == null))
			{
				protection.Add(component.protectionProperties, multiplier);
			}
		}
	}
}
