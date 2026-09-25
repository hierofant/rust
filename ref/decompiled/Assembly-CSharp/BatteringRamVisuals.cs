using UnityEngine;

public class BatteringRamVisuals : BaseSiegeWeaponVisuals, IClientComponent
{
	private BatteringRam ram;

	public Transform frontAxle;

	public Transform middleAxle;

	public Transform rearAxle;

	[Space]
	public ParticleSystem rockDoorParticle;

	public ParticleSystem dirtDoorParticle;
}
