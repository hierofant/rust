public class LiquidContainerCrude : LiquidContainer
{
	public static int MaxStackSizeCrude = 2000;

	private static ItemDefinition _crudeItem;

	public static ItemDefinition CrudeItem => _crudeItem ?? (_crudeItem = ItemManager.FindItemDefinition("crude.oil"));

	public override void ServerInit()
	{
		base.ServerInit();
		base.inventory.canAcceptItem = CanAcceptItem;
		base.inventory.allowItemsToIncreaseToMaxStackSize = true;
	}

	private bool CanAcceptItem(BasePlayer player, Item arg1, int arg2)
	{
		bool flag = false;
		ItemDefinition[] validItems = ValidItems;
		for (int i = 0; i < validItems.Length; i++)
		{
			if (validItems[i] == arg1.info)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return false;
		}
		if (arg1.info == CrudeItem)
		{
			base.inventory.maxStackSize = MaxStackSizeCrude;
			BaseEntity baseEntity = GetParentEntity();
			if (baseEntity != null && baseEntity is VehicleModuleStorage vehicleModuleStorage && vehicleModuleStorage.Vehicle.inEditableLocation)
			{
				return false;
			}
			if (arg1.amount > MaxStackSizeCrude)
			{
				return false;
			}
		}
		else
		{
			base.inventory.maxStackSize = maxStackSize;
		}
		return true;
	}

	public override void OnItemAddedOrRemoved(Item item, bool added)
	{
		base.OnItemAddedOrRemoved(item, added);
		if (added && item.info == CrudeItem)
		{
			base.inventory.maxStackSize = MaxStackSizeCrude;
			item.LockUnlock(bNewState: true);
		}
		else
		{
			base.inventory.maxStackSize = maxStackSize;
		}
	}
}
