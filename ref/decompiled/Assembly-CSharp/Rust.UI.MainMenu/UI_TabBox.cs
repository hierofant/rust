using UnityEngine;
using UnityEngine.UI;

namespace Rust.UI.MainMenu;

public class UI_TabBox : MonoBehaviour
{
	[SerializeField]
	private bool _autoClose = true;

	[SerializeField]
	private RustText _filterEnabledText;

	[SerializeField]
	private RustButton _collapseButton;

	[SerializeField]
	private Image _spacerImage;

	public static readonly Translate.Phrase FiltersPhrase = new Translate.Phrase("tabbox.filters", "{0} filters");

	public static readonly Translate.Phrase EnabledPhrase = new Translate.Phrase("tabbox.filters.enabled", "enabled");

	public static readonly Translate.Phrase DisabledPhrase = new Translate.Phrase("tabbox.filters.disabled", "disabled");
}
