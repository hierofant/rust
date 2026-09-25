using UnityEngine;

public class Plank : DecayEntity, global::IBoatBuildingPiece
{
	[Header("Plank")]
	public TriggerParent ParentTrigger;

	void global::IBoatBuildingPiece.OnAddedToBoat(PlayerBoat boat)
	{
		if ((bool)ParentTrigger)
		{
			ParentTrigger.associatedMountable = boat;
		}
	}

	protected override bool ShouldDisplayPickupOption(BasePlayer player)
	{
		if (base.ShouldDisplayPickupOption(player))
		{
			return !PlayerBoat.IsChildOfFinishedPlayerBoat(this);
		}
		return false;
	}

	public override void Hurt(HitInfo info)
	{
		PlayerBoat parentPlayerBoat = PlayerBoat.GetParentPlayerBoat(this);
		if (parentPlayerBoat != null && !parentPlayerBoat.IsDestructibleWreck)
		{
			parentPlayerBoat.OnBoatDeployableHurt(this, info);
		}
		else
		{
			base.Hurt(info);
		}
	}
}
