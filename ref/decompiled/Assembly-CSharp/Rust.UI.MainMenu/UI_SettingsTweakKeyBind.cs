using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_SettingsTweakKeyBind : UI_SettingsTweakBase
{
	[Space]
	public GameObject blockingCanvas;

	public RustButton button;

	public RustText labelText;

	public UI_SettingsKeyBindButton btnA;

	public UI_SettingsKeyBindButton btnB;

	public CanvasGroup resetButton;

	public string bindString;

	public static bool IsBinding { get; private set; }
}
