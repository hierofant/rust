using Facepunch.Flexbox;
using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_ServerBrowser_NoResults_Controller : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup _noServersFound;

	[SerializeField]
	private RustButton _button;

	[SerializeField]
	private FlexTransition _transition;
}
