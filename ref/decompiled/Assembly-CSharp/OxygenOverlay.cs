using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

public class OxygenOverlay : MonoBehaviour
{
	[SerializeField]
	private PostProcessVolume postProcessVolume;

	[SerializeField]
	private Volume postProcessVolumeRRP;

	[SerializeField]
	private float smoothTime = 1f;

	[SerializeField]
	[Tooltip("If true, only show this effect when the player is mounted in a submarine.")]
	private bool submarinesOnly;
}
