using UnityEngine;
using UnityEngine.UI;

namespace Rust.UI.MainMenu;

public class UI_SettingsGestureWheel : MonoBehaviour
{
	[SerializeField]
	private RustText wheelHeaderText;

	[SerializeField]
	private RustButton leftArrowButton;

	[SerializeField]
	private RustButton rightArrowButton;

	[SerializeField]
	[Space]
	private UI_SettingsGestureWidget[] gestureWidgets;

	[SerializeField]
	private RustText gestureTitleText;

	[SerializeField]
	private RustText gestureDescriptionText;

	[SerializeField]
	private Image gestureIcon;

	[Space]
	[SerializeField]
	private RectTransform wheelCenter;

	[SerializeField]
	private float radius = 100f;

	[SerializeField]
	private float initialAngleOffset;

	[SerializeField]
	[Space]
	private UI_SettingsGestureButton gestureButtonPrefab;

	[SerializeField]
	[Space]
	private RectTransform ownedButtonsParent;

	[SerializeField]
	private RectTransform lockedPacksParent;

	[SerializeField]
	private UI_SettingsGesturePack packPrefab;

	[SerializeField]
	[Space]
	public RectTransform dragAndDropCanvas;

	[SerializeField]
	private CanvasGroup assignOverlay;

	[SerializeField]
	private Canvas buttonListCanvas;

	[SerializeField]
	[Space]
	private GameObject hoveredGestureTexts;

	[SerializeField]
	private GameObject emptyWheelTexts;
}
