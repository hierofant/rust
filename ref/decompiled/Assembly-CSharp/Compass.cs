using UnityEngine;

public class Compass : HeldEntity
{
	private const string WORLDMODEL_NEEDLE_PATH = "w_compass_rig/weapon_parent/weapon_root/needle";

	private const string ADMIRE_ANIMEVENT_TAP1 = "AdmireTap1";

	private const string ADMIRE_ANIMEVENT_TAP2 = "AdmireTap2";

	[SerializeField]
	[Tooltip("Change this if it does not feel like the needle direction is accurate.")]
	[Header("Compass")]
	private float needleYawAngleOffset;

	[SerializeField]
	[Min(0f)]
	private float needleStiffness = 50f;

	[Range(0f, 1f)]
	[SerializeField]
	private float needleDamping = 0.5f;

	[Min(0f)]
	[SerializeField]
	private float maxAcceleration = 1500f;

	[SerializeField]
	[Min(0f)]
	private float maxSpeed = 360f;

	[SerializeField]
	[MinMax(0f, 180f)]
	[Tooltip("Random variance in how far off the needle will be from target when first deployed.")]
	private MinMax startingAngleDelta;

	[SerializeField]
	private float wiggleAmplitude = 0.5f;

	[SerializeField]
	private float wiggleFrequency = 1.5f;

	[Tooltip("Velocity change when the admire animation taps the needle")]
	[Min(0f)]
	[SerializeField]
	private float admireTapNeedleVelocity = 300f;
}
