using System;
using UnityEngine;

public class v_chainsaw : MonoBehaviour
{
	[NonSerialized]
	public bool bAttacking;

	[NonSerialized]
	public bool bHitMetal;

	[NonSerialized]
	public bool bHitWood;

	[NonSerialized]
	public bool bHitFlesh;

	[NonSerialized]
	public bool bEngineOn;

	[NonSerialized]
	public bool bCanHitParticlesPlay;

	public ParticleSystem[] hitMetalFX;

	public ParticleSystem[] hitWoodFX;

	public ParticleSystem[] hitFleshFX;

	public SoundDefinition hitMetalSoundDef;

	public SoundDefinition hitWoodSoundDef;

	public SoundDefinition hitFleshSoundDef;

	public float hitSoundFadeTime = 0.1f;

	public ParticleSystem[] smokeEffects = Array.Empty<ParticleSystem>();

	public Animator chainsawAnimator;

	public Renderer chainRenderer;
}
