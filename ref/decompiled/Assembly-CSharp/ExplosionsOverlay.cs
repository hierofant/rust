using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

public class ExplosionsOverlay : MonoBehaviour, IClientComponent
{
	public static ExplosionsOverlay Instance;

	public PostProcessVolume postProcessVolume;

	public Volume rrpVolume;

	public AnimationCurve radialBlurStartCurve;

	public AnimationCurve radialBlurAmountCurve;

	public AnimationCurve lensDirtinessCurve;
}
