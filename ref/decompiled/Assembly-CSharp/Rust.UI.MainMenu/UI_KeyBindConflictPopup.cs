using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_KeyBindConflictPopup : UI_Popup
{
	[Space]
	[SerializeField]
	private RustText keyText;

	[SerializeField]
	private RustText bindText;

	public static readonly Translate.Phrase TitlePhrase = new Translate.Phrase("keybinds.conflict.title", "Conflict");

	public static readonly Translate.Phrase MessagePhrase = new Translate.Phrase("keybinds.conflict.message", "This key is already bound to another action");

	public static readonly Translate.Phrase ReplacePhrase = new Translate.Phrase("keybinds.conflict.replace", "Replace");

	public static readonly Translate.Phrase CancelPhrase = new Translate.Phrase("keybinds.conflict.cancel", "Cancel");
}
