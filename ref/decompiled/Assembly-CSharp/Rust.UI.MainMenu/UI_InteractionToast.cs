using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_InteractionToast : UI_Window
{
	[Header("Interaction Modal")]
	[SerializeField]
	private RustText _text;

	[SerializeField]
	private GameObject _informationPanel;

	[SerializeField]
	private RustText _informationTitle;

	[SerializeField]
	private RustText _informationText;

	[SerializeField]
	private RustButton _firstButton;

	[SerializeField]
	private RustButton _secondButton;

	[SerializeField]
	private RustIcon _firstIcon;

	[SerializeField]
	private RustIcon _secondIcon;
}
