using System;

public class TutorialContainer : StorageContainer
{
	private bool acceptingItems;

	public void LoadStorage(ItemAmount[] toLoad)
	{
		base.inventory.Clear();
		acceptingItems = true;
		foreach (ItemAmount itemAmount in toLoad)
		{
			base.inventory.GiveItem(ItemManager.Create(itemAmount.itemDef, (int)itemAmount.amount, 0uL, isServerSide: true, 0uL));
		}
		acceptingItems = false;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		ItemContainer itemContainer = base.inventory;
		itemContainer.canAcceptItem = (Func<BasePlayer, Item, int, bool>)Delegate.Combine(itemContainer.canAcceptItem, new Func<BasePlayer, Item, int, bool>(CanAcceptItem));
	}

	private bool CanAcceptItem(BasePlayer player, Item item, int targetSlot)
	{
		return acceptingItems;
	}
}
