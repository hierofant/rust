using UnityEngine;

public class ExplosionsShaderFloatCurves : MonoBehaviour, IClientComponent
{
	public string ShaderProperty = "_BumpAmt";

	public int MaterialID;

	public AnimationCurve FloatPropertyCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public float GraphTimeMultiplier = 1f;

	public float GraphScaleMultiplier = 1f;
}
