public class SatelliteFuelStorage : StorageContainer
{
	public override void ServerInit()
	{
		base.ServerInit();
		SyncSlotsToPowerCost();
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		SyncSlotsToPowerCost();
	}

	private void SyncSlotsToPowerCost()
	{
		if (GetParentEntity() is SatelliteControlComputer { powerCost: not null } satelliteControlComputer && satelliteControlComputer.powerCost.Count != 0)
		{
			inventorySlots = satelliteControlComputer.powerCost.Count;
			if (base.inventory != null)
			{
				base.inventory.capacity = inventorySlots;
			}
		}
	}

	public override bool ItemFilter(BasePlayer player, Item item, int targetSlot)
	{
		if (item?.info == null)
		{
			return false;
		}
		SatelliteControlComputer satelliteControlComputer = GetParentEntity() as SatelliteControlComputer;
		if (satelliteControlComputer == null || satelliteControlComputer.powerCost == null)
		{
			return false;
		}
		foreach (ItemAmountRanged item2 in satelliteControlComputer.powerCost)
		{
			if (item2?.itemDef == item.info)
			{
				return true;
			}
		}
		return false;
	}
}
