using UnityEngine;

public class BoxStorage : StorageContainer
{
	public Hopper.MountType HopperMountType;

	public override bool PreserveChildrenWhenReskinning => true;

	public override Vector3 GetDropPosition()
	{
		return ClosestPoint(base.GetDropPosition() + base.LastAttackedDir * 10f);
	}

	public override bool SupportsChildDeployables()
	{
		return true;
	}

	protected override bool CanCompletePickup(BasePlayer player)
	{
		if (children.Count != 0)
		{
			pickupErrorToFormat = (format: PickupErrors.ItemHasAttachment, arg0: pickup.itemTarget.displayName);
			return false;
		}
		return base.CanCompletePickup(player);
	}
}
