using LeTai.TrueShadow;
using UnityEngine;
using UnityEngine.UI;

namespace Rust.UI.MainMenu;

public class UI_SettingsGestureWidget : UI_SettingsGestureDraggable
{
	[SerializeField]
	private StyleAsset boundStyle;

	[SerializeField]
	private StyleAsset emptyStyle;

	[SerializeField]
	[Space]
	private Image gestureIcon;

	[SerializeField]
	private GameObject emptyIcon;

	[SerializeField]
	private GameObject hoverImage;

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private TrueShadow shadow;
}
