using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

public class ColdOverlay : MonoBehaviour
{
	public PostProcessVolume postProcessVolume;

	public Volume postProcessVolumeRRP;

	public float smoothTime = 1f;

	public bool preventInstantiation;
}
