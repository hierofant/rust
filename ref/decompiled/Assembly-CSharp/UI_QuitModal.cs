using Rust.UI.MainMenu;
using UnityEngine;

public class UI_QuitModal : UI_Window, IClientComponent
{
	public static UI_QuitModal Instance;

	[SerializeField]
	private UIEscapeCapture _capture;
}
