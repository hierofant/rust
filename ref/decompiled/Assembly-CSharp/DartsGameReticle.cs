using System.Collections.Generic;
using UnityEngine;

public class DartsGameReticle : FacepunchBehaviour
{
	private static readonly int EMISSION_COLOR = Shader.PropertyToID("_EmissionColor");

	private static readonly int COLOR = Shader.PropertyToID("_Color");

	public Transform AccuracyRing;

	public float FlashColoursWhenTimeRemaining = 2f;

	public float FlashColoursSpeed = 5f;

	public List<Renderer> TimerMaterialRenderers;

	public Color StartTimerColour;

	public float StartReticleColourIntensity = 1f;

	public Color EndTimerColour;

	public float EndReticleColourIntensity = 2f;
}
