using UnityEngine;

namespace Rust.UI.MainMenu;

public abstract class UI_SettingsGestureDraggable : BaseMonoBehaviour
{
	[SerializeField]
	public RustButton button;

	[SerializeField]
	private GameObject draggedPrefab;

	public GestureConfig gestureConfig { get; private set; }

	public UI_SettingsGestureWheel wheel { get; private set; }
}
