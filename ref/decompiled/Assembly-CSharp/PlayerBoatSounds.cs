using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBoatSounds : FacepunchBehaviour, IClientComponent
{
	[Serializable]
	public class AmbientSting
	{
		public SoundDefinition soundDefinition;

		public SoundDefinition lurchSoundDefinition;

		public SoundDefinition steeringCreakDefinition;

		public int minPieces;

		public int maxPieces;
	}

	private class SideWaterSound
	{
		public Line line;

		public Sound sound;

		public SoundModulation.Modulator gainMod;
	}

	public PlayerBoat boat;

	[Header("Ambience")]
	public List<AmbientSting> ambientStings;

	public float ambientStingCooldown = 2f;

	public float ambientStingAngleDeltaThreshold = 0.01f;

	public List<SoundDefinition> availableAmbientStings = new List<SoundDefinition>();

	public List<SoundDefinition> availablePhysicsLurches = new List<SoundDefinition>();

	public List<SoundDefinition> availableSteeringCreaks = new List<SoundDefinition>();

	public List<BoatBuildingBlock> hullBlocks = new List<BoatBuildingBlock>();

	[Header("Damage")]
	public SoundDefinition damagedStingSoundDef;

	public float damagedStingIntervalMin = 2f;

	public float damagedStingIntervalMax = 10f;

	public float damagedStingIntervalVariance = 0.5f;

	public float damagedStingThreshold = 0.2f;

	public SoundDefinition sinkSoundDef;

	[Header("Steering")]
	public float minSteeringCreakDelay;

	public float maxSteeringCreakDelay = 1f;

	public float steeringCreakCooldown = 2f;

	public int steeringCreakCount = 3;

	[Header("Water")]
	public SoundDefinition sideWaterIdleSoundDef;

	public SoundDefinition sideWaterMovingSoundDef;

	public SoundDefinition sideWaterMovingFastSoundDef;

	public AnimationCurve waterMovementGainCurve;

	public AnimationCurve waterEdgeDistanceGainCurve;

	public float waterMovementGainChangeRate = 1f;

	[Header("Physics")]
	public float physicsLurchHardnessThreshold = 0.25f;

	public float nextDamagedSting { get; set; }

	public float lastDamagedSting { get; set; }

	public float prevHealth { get; set; }
}
