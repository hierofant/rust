using Rust.UI;
using Rust.UI.MainMenu;
using UnityEngine;
using UnityEngine.UI;

public class UI_PremiumModal : UI_Window
{
	[Space]
	public RustText UsernameLabel;

	public RustText MoneyLabel;

	public RustText ActiveStatusLabel;

	public RawImage ProfilePicture;

	public RustButton RefreshButton;

	public Translate.Phrase ActivePhrase;

	public Translate.Phrase InactivePhrase;

	public Translate.Phrase SearchingPhrase;

	public static readonly Translate.Phrase ErrorPhrase = new Translate.Phrase("premium.error", "Error");
}
