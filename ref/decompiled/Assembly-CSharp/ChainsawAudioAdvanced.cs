using System;
using UnityEngine;

public class ChainsawAudioAdvanced : FacepunchBehaviour, IClientComponent
{
	[Serializable]
	public class SoundLayer
	{
		public SoundDefinition loopDef;

		public AnimationCurve gainCurve;

		public AnimationCurve pitchCurve;

		public Sound loop;

		public SoundModulation.Modulator gainMod;

		public SoundModulation.Modulator pitchMod;
	}

	public SoundLayer engineIdleLayer;

	public SoundLayer engineLayer;

	public SoundLayer mechanicsLayer;

	public SoundDefinition engineStartDef;

	public SoundDefinition engineStopDef;

	public bool engineOn;

	public bool active;

	public bool hit;

	public float engineChangeRateUp = 2f;

	public float engineChangeRateDown = 1f;

	public float mechanicsChangeRateUp = 2f;

	public float mechanicsChangeRateDown = 0.25f;

	public float runSpeedMaxActive = 1f;

	public float runSpeedMaxHit = 0.8f;

	public float runSpeed;

	public float engine;

	public float mechanics;

	public float hitNoiseFrequency = 1f;

	public float hitNoiseDepth = 0.3f;

	public float activeNoiseFrequency = 1f;

	public float activeNoiseDepth = 0.1f;
}
