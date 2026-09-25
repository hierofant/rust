using UnityEngine;

public class ExplosionsScaleCurves : MonoBehaviour, IClientComponent
{
	public AnimationCurve ScaleCurveX = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public AnimationCurve ScaleCurveY = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public AnimationCurve ScaleCurveZ = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public Vector3 GraphTimeMultiplier = Vector3.one;

	public Vector3 GraphScaleMultiplier = Vector3.one;
}
