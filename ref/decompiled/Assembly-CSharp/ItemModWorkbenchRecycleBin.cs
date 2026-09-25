using System.Collections.Generic;
using UnityEngine;

public class ItemModWorkbenchRecycleBin : ItemModWorkbenchUpgrade
{
	[Range(0f, 0.5f)]
	[Tooltip("Maximum fraction of each ingredient that can be refunded (0-1). A random value between 0 and this is rolled each craft.")]
	[Header("Recycle Bin")]
	public float maxRefundFraction = 0.1f;

	public override void GetBonusItems(Workbench workbench, BasePlayer crafter, ItemCraftTask task, Item craftedItem, Item upgradeItem, List<Item> bonusItems)
	{
		if (task.blueprint == null)
		{
			return;
		}
		foreach (ItemAmount ingredient in task.blueprint.GetIngredients())
		{
			if (ingredient.itemDef == null)
			{
				continue;
			}
			float num = Random.Range(0f, maxRefundFraction);
			float num2 = ingredient.amount * num;
			int num3 = Mathf.FloorToInt(num2);
			float num4 = num2 - (float)num3;
			if (num4 > float.Epsilon && Random.Range(0f, 1f) <= num4)
			{
				num3++;
			}
			if (num3 > 0)
			{
				Item item = ItemManager.CreateByItemID(ingredient.itemDef.itemid, num3, 0uL, 0uL);
				if (item != null)
				{
					bonusItems.Add(item);
				}
			}
		}
	}

	public override bool TryGiveBonusItem(Workbench workbench, BasePlayer crafter, Item upgradeItem, Item bonusItem)
	{
		if (upgradeItem?.contents == null)
		{
			return false;
		}
		return bonusItem.MoveToContainer(upgradeItem.contents);
	}
}
