using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_CountrySelection : UI_Window
{
	public RustInput searchInput;

	public UI_CountryEntry[] entries;

	public CanvasGroup entriesCanvasGroup;

	public StyleAsset buttonStyle;

	public StyleAsset buttonDarkStyle;

	public RustText selectedCountryText;

	public RustButton autoDetectToggle;

	public RustButton changeCountryButton;

	private static readonly Translate.Phrase automaticPhrase = new Translate.Phrase("countryselect.automatic", "Automatic");
}
