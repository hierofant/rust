using UnityEngine;

public class SiegeWeaponVehicleAudio : GroundVehicleAudio
{
	[Header("Reload")]
	public SoundDefinition reloadProgressDef;

	public SoundDefinition reloadStartDef;

	public SoundDefinition reloadStopDef;

	public SoundDefinition reloadCompleteDef;

	[SerializeField]
	[Header("Rattles")]
	private SoundDefinition movementRattleLoop;

	[SerializeField]
	private float movementRattleMaxSpeed = 10f;

	[SerializeField]
	private float movementRattleIdleGain = 0.3f;

	[SerializeField]
	private float movementRattleGainChangeSpeed = 1f;

	[SerializeField]
	[Header("Wheels")]
	private SoundDefinition tyreRollingSoundDef;

	[SerializeField]
	private SoundDefinition tyreRollingWaterSoundDef;

	[SerializeField]
	private SoundDefinition tyreRollingGrassSoundDef;

	[SerializeField]
	private SoundDefinition tyreRollingSnowSoundDef;

	[SerializeField]
	private AnimationCurve tyreRollGainCurve;
}
