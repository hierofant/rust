using UnityEngine;

public class ExplosionsLightCurves : MonoBehaviour, IClientComponent
{
	public AnimationCurve LightCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public float GraphTimeMultiplier = 1f;

	public float GraphIntensityMultiplier = 1f;
}
