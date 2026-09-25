using UnityEngine;

[DefaultExecutionOrder(-9999)]
public class UI_Error : MonoBehaviour
{
	public UI_ErrorEntry[] entries;

	public Canvas canvas;

	public CanvasGroup canvasGroup;

	[Tooltip("How long (seconds) the panel stays fully visible after the last error.")]
	public float visibleDuration = 25f;

	[Tooltip("How long (seconds) the lerp to zero alpha takes after visibleDuration expires.")]
	public float fadeDuration = 5f;
}
