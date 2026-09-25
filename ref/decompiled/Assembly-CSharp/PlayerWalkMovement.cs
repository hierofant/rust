using UnityEngine;

public class PlayerWalkMovement : BaseMovement
{
	public const float WaterLevelHead = 0.75f;

	public const float WaterLevelNeck = 0.65f;

	public const float DefaultWalkSpeed = 2.8f;

	public PhysicsMaterial zeroFrictionMaterial;

	public PhysicsMaterial highFrictionMaterial;
}
