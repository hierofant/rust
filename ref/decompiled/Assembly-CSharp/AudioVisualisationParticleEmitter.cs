using UnityEngine;

public class AudioVisualisationParticleEmitter : MonoBehaviour, IClientComponent
{
	public ParticleSystem[] TargetParticles;

	public BoomBox TargetBoomBox;

	public bool DebugMode;

	public float MinimumTimeBetweenEmissions = 0.05f;

	public float VolumeCutoffLerpRate = 0.4f;
}
