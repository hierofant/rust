using System.Collections.Generic;
using UnityEngine;

public class ItemModWorkbenchEfficiency : ItemModWorkbenchUpgrade
{
	[Tooltip("Probability (0-1) of producing a free extra item per craft tick.")]
	[Range(0f, 1f)]
	[Header("Efficiency")]
	public float bonusChance = 0.1f;

	[Tooltip("Items that are excluded from the bonus.")]
	public List<ItemDefinition> ExcludedItems;

	public override void GetBonusItems(Workbench workbench, BasePlayer crafter, ItemCraftTask task, Item craftedItem, Item upgradeItem, List<Item> bonusItems)
	{
		if (task.amount + task.numCrafted > 1 && (ExcludedItems == null || !ExcludedItems.Contains(craftedItem.info)) && !(Random.value > bonusChance))
		{
			Item item = ItemManager.CreateByItemID(craftedItem.info.itemid, craftedItem.amount, craftedItem.skin, 0uL);
			if (item != null)
			{
				bonusItems.Add(item);
			}
		}
	}
}
