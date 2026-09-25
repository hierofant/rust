using UnityEngine;

public class ItemModWorkbenchDefensive : ItemModWorkbenchUpgrade
{
	[Header("Defensive")]
	[Tooltip("Number of bonus armor insert slots added to the normal roll.")]
	public int bonusSlots = 1;

	public override void ApplyToCraftedItem(Workbench workbench, BasePlayer crafter, ItemCraftTask task, Item craftedItem, Item upgradeItem)
	{
		ItemModContainerArmorSlot component = craftedItem.info.GetComponent<ItemModContainerArmorSlot>();
		if (!(component == null))
		{
			int num = craftedItem.contents?.capacity ?? 0;
			int num2 = Mathf.Min(num + bonusSlots, component.MaxSlots);
			if (num2 > num)
			{
				component.SetSlotAmount(craftedItem, num2);
			}
		}
	}
}
