using UnityEngine;

public class MouseSteerableSeat : BaseVehicleSeat
{
	[SerializeField]
	private bool supportsMouseSteer;

	[SerializeField]
	private ReactMountableAnimationSubsystem mountedAnimationSystem;

	public const BUTTON MouseSteerButton = BUTTON.FIRE_THIRD;
}
