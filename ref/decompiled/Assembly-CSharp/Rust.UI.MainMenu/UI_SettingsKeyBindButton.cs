using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_SettingsKeyBindButton : MonoBehaviour
{
	[HideInInspector]
	public string currentBind;

	public RustButton button;

	public StyleAsset boundStyle;

	public StyleAsset notBoundStyle;

	public RustText text;

	public static readonly Translate.Phrase pressAKeyPhrase = new Translate.Phrase("keybinds.presskey", "Press a key");
}
