using System.Collections.Generic;
using UnityEngine;

public class ItemModWorkbenchSurplus : ItemModWorkbenchUpgrade
{
	public override void GetBonusItems(Workbench workbench, BasePlayer crafter, ItemCraftTask task, Item craftedItem, Item upgradeItem, List<Item> bonusItems)
	{
		List<ItemBlueprint.SurplusEntry> list = task.blueprint?.surplusItems;
		if (list == null || list.Count == 0)
		{
			return;
		}
		foreach (ItemBlueprint.SurplusEntry item2 in list)
		{
			if (!(item2.itemDef == null) && item2.amount > 0 && !(Random.value > item2.chance))
			{
				Item item = ItemManager.Create(item2.itemDef, item2.amount, 0uL, isServerSide: true, 0uL);
				if (item != null)
				{
					bonusItems.Add(item);
				}
			}
		}
	}
}
