using System.Collections.Generic;
using UnityEngine;

public class ItemModWorkbenchUpgrade : ItemMod
{
	[Header("Workbench Upgrade")]
	public int minimumWorkbenchLevel;

	[Header("Visuals")]
	public GameObjectRef upgradeVisualPrefab = new GameObjectRef();

	[Tooltip("Optional override visual for Tier 2 workbenches. Falls back to upgradeVisualPrefab if unset.")]
	public GameObjectRef upgradeVisualPrefabLevel2 = new GameObjectRef();

	[Tooltip("Optional override visual for Tier 3 workbenches. Falls back to upgradeVisualPrefab if unset.")]
	public GameObjectRef upgradeVisualPrefabLevel3 = new GameObjectRef();

	[Header("Effects")]
	public GameObjectRef installEffectPrefab = new GameObjectRef();

	public GameObjectRef GetVisualPrefab(int workbenchLevel)
	{
		switch (workbenchLevel)
		{
		case 3:
			if (upgradeVisualPrefabLevel3.isValid)
			{
				return upgradeVisualPrefabLevel3;
			}
			break;
		case 2:
			if (upgradeVisualPrefabLevel2.isValid)
			{
				return upgradeVisualPrefabLevel2;
			}
			break;
		}
		return upgradeVisualPrefab;
	}

	public virtual bool CanBypassTechTreePath()
	{
		return false;
	}

	public virtual float GetTechTreeFailChance()
	{
		return 0f;
	}

	public virtual float GetBypassCostMultiplier()
	{
		return 1f;
	}

	public virtual bool CanInstallInWorkbench(Workbench workbench, Item item, int targetSlot)
	{
		if (workbench == null)
		{
			return false;
		}
		if (minimumWorkbenchLevel > 0 && workbench.Workbenchlevel < minimumWorkbenchLevel)
		{
			return false;
		}
		ItemContainer inventory = workbench.inventory;
		if (inventory != null)
		{
			for (int i = 2; i < workbench.RequiredInventorySlots; i++)
			{
				if (i != targetSlot)
				{
					Item slot = inventory.GetSlot(i);
					if (slot != null && slot.info == item.info)
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	public virtual void OnUpgradeInstalled(Workbench workbench, Item upgradeItem)
	{
	}

	public virtual void OnUpgradeRemoved(Workbench workbench, Item upgradeItem)
	{
	}

	public virtual void ApplyToCraftedItem(Workbench workbench, BasePlayer crafter, ItemCraftTask task, Item craftedItem, Item upgradeItem)
	{
	}

	public virtual float GetCraftSpeedMultiplier(ItemCraftTask task)
	{
		return 1f;
	}

	public virtual float GetRangeMultiplier()
	{
		return 1f;
	}

	public virtual float GetMinComfortLevel()
	{
		return 0f;
	}

	public virtual float GetExplosiveDamageReduction()
	{
		return 0f;
	}

	public virtual float GetTechTreeCostMultiplier()
	{
		return 1f;
	}

	public virtual void GetBonusItems(Workbench workbench, BasePlayer crafter, ItemCraftTask task, Item craftedItem, Item upgradeItem, List<Item> bonusItems)
	{
	}

	public virtual bool TryGiveBonusItem(Workbench workbench, BasePlayer crafter, Item upgradeItem, Item bonusItem)
	{
		return false;
	}
}
