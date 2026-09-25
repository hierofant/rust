using UnityEngine;

public class ChristmasTree : StorageContainer
{
	public GameObject[] decorations;

	public override bool ItemFilter(BasePlayer player, Item item, int targetSlot)
	{
		if (item.info.GetComponent<ItemModXMasTreeDecoration>() == null)
		{
			return false;
		}
		foreach (Item item2 in base.inventory.itemList)
		{
			if (item2.info == item.info)
			{
				return false;
			}
		}
		return base.ItemFilter(player, item, targetSlot);
	}

	public override void OnItemAddedOrRemoved(Item item, bool added)
	{
		ItemModXMasTreeDecoration component = item.info.GetComponent<ItemModXMasTreeDecoration>();
		if (component != null)
		{
			using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set((Flags)component.flagsToChange, added);
		}
		base.OnItemAddedOrRemoved(item, added);
	}
}
