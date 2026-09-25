using System;
using UnityEngine;

public class PowergridEngineFx : FacepunchBehaviour, IClientComponent
{
	public int requiredPowergridStage = 1;

	[Header("Particle FX")]
	public ParticleSystem turbineParticleSystem;

	public LODComponentParticleSystem turbineParticleLodComponent;

	public ParticleSystem[] subParticleSystems = Array.Empty<ParticleSystem>();

	public LODComponentParticleSystem[] subParticleLodComponents = Array.Empty<LODComponentParticleSystem>();

	public AnimationCurve turbineSpeedUpCurve;

	public AnimationCurve turbineWindDownCurve;

	public float maxParticleSimulationSpeed = 1f;

	public float speedUpTime = 10f;

	public float windDownTime = 5f;

	public float subParticleSystemsDelayVisibility = 5f;

	[Header("Audio")]
	public SoundDefinition startupSoundDefinition;

	public SoundDefinition loopSoundDefinition;

	public SoundDefinition stopSoundDefinition;

	public float loopFadeInTime = 5f;

	public float loopFadeOutTime = 1f;
}
