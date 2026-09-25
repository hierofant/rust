using Rust.UI;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class SatelliteMapUI : MonoBehaviour
{
	[SerializeField]
	private MapView mapView;

	[SerializeField]
	private ScrollRectZoom mapScrollRectZoom;

	[SerializeField]
	private RectTransform minRangeCircle;

	[SerializeField]
	private RectTransform targetAimRect;

	[SerializeField]
	private RectTransform trueAimRect;

	[SerializeField]
	private UILineRenderer connectingLine;

	[SerializeField]
	private RustText statusText;

	[SerializeField]
	private AnimationCurve targetingEase = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[SerializeField]
	private GameObject errorPopup;

	[SerializeField]
	private CanvasGroup errorPopupGroup;

	[SerializeField]
	private RustText errorPopupText;

	[SerializeField]
	private float errorFadeTime = 0.15f;
}
