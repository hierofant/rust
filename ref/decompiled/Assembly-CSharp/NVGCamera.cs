using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

public class NVGCamera : FacepunchBehaviour, IClothingChanged
{
	public static NVGCamera instance;

	public PostProcessVolume postProcessVolume;

	public Volume postProcessVolumeRRP;

	public GameObject lights;
}
