using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class SatelliteSpectatorScreenUI : MonoBehaviour
{
	[Header("Map")]
	[SerializeField]
	private RawImage mapImage;

	[SerializeField]
	private RectTransform mapContentRect;

	[SerializeField]
	private float mapZoom = 2f;

	[Header("Markers (optional — created procedurally if unset)")]
	[SerializeField]
	private RectTransform reticleRect;

	[SerializeField]
	private RectTransform impactCircleRect;

	[SerializeField]
	private RectTransform lockedCrashRect;

	[SerializeField]
	[Header("Text")]
	private RustText statusText;

	[SerializeField]
	private RustText driftText;
}
