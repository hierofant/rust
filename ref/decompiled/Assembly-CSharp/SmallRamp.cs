using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class SmallRamp : Door, global::IBoatBuildingPiece
{
	[Header("SmallRamp")]
	public TriggerParent ParentTrigger;

	protected override bool IgnoreBlockageDotCheck => true;

	public void OnAddedToBoat(PlayerBoat boat)
	{
		if ((bool)ParentTrigger)
		{
			ParentTrigger.associatedMountable = boat;
		}
	}

	private bool IsBoundsClearOfWorldObstacles()
	{
		return !GamePhysics.CheckOBBAndEntity(WorldSpaceBounds(), 65536, QueryTriggerInteraction.UseGlobal, this);
	}

	protected override void ReverseDoorAnimation(bool wasOpening, bool reverse)
	{
		AnimatorStateInfo currentAnimatorStateInfo = model.animator.GetCurrentAnimatorStateInfo(0);
		model.animator.Play("small_ramp_raise", 0, 1f - currentAnimatorStateInfo.normalizedTime);
	}

	protected override bool CheckOnClose()
	{
		return false;
	}

	protected override bool OnlyCheckForVehicles()
	{
		return false;
	}

	public override void StabilityCheck()
	{
		if (TryGetComponent<GroundWatch>(out var component))
		{
			component.DirectCallOnPhysicsNeighbourChanged();
		}
	}

	protected override void OnPlayerClosedDoor(BasePlayer player)
	{
		base.OnPlayerClosedDoor(player);
		List<BasePlayer> obj = Pool.Get<List<BasePlayer>>();
		foreach (BaseEntity child in children)
		{
			if (child is BasePlayer basePlayer && basePlayer.IsSleeping())
			{
				obj.Add(basePlayer);
			}
		}
		foreach (BasePlayer ply in obj)
		{
			ply.SetParent(null, worldPositionStays: true, sendImmediate: true);
			Invoke(delegate
			{
				ply.SetServerFall(wantsOn: true);
			}, 1.5f);
		}
		Pool.FreeUnmanaged(ref obj);
	}

	protected override bool CanDoorBeOpened()
	{
		if (base.CanDoorBeOpened())
		{
			return IsBoundsClearOfWorldObstacles();
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

	protected override bool ShouldDisplayPickupOption(BasePlayer player)
	{
		if (base.ShouldDisplayPickupOption(player))
		{
			return !PlayerBoat.IsChildOfInteractablePlayerBoat(this);
		}
		return false;
	}
}
