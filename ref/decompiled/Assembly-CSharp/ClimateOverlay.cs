using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

public class ClimateOverlay : MonoBehaviour
{
	[Range(0f, 1f)]
	public float blendingSpeed = 1f;

	public PostProcessVolume[] biomeVolumes;

	public Volume[] rrpBiomeVolumes;

	public const int biomeCount = 6;

	public const int volumeCount = 7;
}
