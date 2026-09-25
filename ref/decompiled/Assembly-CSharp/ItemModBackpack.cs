using System;
using UnityEngine;

public class ItemModBackpack : ItemMod
{
	public SoundDefinition ZipSound;

	[Header("Should the 'selected' panel be hidden when the backpack is selected when equipped")]
	public bool hideSelectedPanel;

	[Header("Backpack's item volume when items are in it")]
	public int containerVolumeWhenFilled = 1;

	public bool DropWhenDowned;

	public override void OnItemCreated(Item item)
	{
		base.OnItemCreated(item);
		if (item.contents != null)
		{
			ItemContainer contents = item.contents;
			contents.canAcceptItem = (Func<BasePlayer, Item, int, bool>)Delegate.Combine(contents.canAcceptItem, (Func<BasePlayer, Item, int, bool>)((BasePlayer player, Item subItem, int slot) => CanAcceptItem(player, item, subItem, slot)));
		}
	}

	private bool CanAcceptItem(BasePlayer player, Item backpack, Item item, int slot)
	{
		if (backpack.parent == null)
		{
			return true;
		}
		if (backpack.parent.HasFlag(ItemContainer.Flag.Clothing))
		{
			return true;
		}
		return false;
	}
}
