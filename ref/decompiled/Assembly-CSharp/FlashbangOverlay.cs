using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

public class FlashbangOverlay : MonoBehaviour, IClientComponent
{
	public static FlashbangOverlay Instance;

	public PostProcessVolume postProcessVolume;

	public Volume rrpVolume;

	public AnimationCurve burnIntensityCurve;

	public AnimationCurve whiteoutIntensityCurve;

	public SoundDefinition deafLoopDef;
}
