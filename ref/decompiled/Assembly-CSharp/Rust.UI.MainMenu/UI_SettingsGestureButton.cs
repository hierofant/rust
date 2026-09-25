using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Rust.UI.MainMenu;

public class UI_SettingsGestureButton : UI_SettingsGestureDraggable
{
	[SerializeField]
	private StyleAsset unlockedStyle;

	[SerializeField]
	private StyleAsset lockedStyle;

	[SerializeField]
	[Space]
	private GameObject boundLine;

	[SerializeField]
	private GameObject dragIcon;

	[SerializeField]
	private Image gestureIcon;

	[SerializeField]
	private RustText gestureNameText;

	[SerializeField]
	[Space]
	private RectTransform videoPreview;

	[SerializeField]
	private VideoPlayer videoPlayer;

	[SerializeField]
	private RawImage videoRawTexture;

	[SerializeField]
	[Space]
	private GameObject deleteButton;
}
