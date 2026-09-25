using Rust.UI;
using Rust.UI.MainMenu;
using UnityEngine;
using UnityEngine.UI;

public class UI_Console_CommandList : UI_Window
{
	public static UI_Console_CommandList Instance;

	[Space]
	public FlexVirtualScroll VirtualScroll;

	public RustInput CommandSearchInput;

	public RustButton ShowClientButton;

	public RustButton ShowServerButton;

	public Image DarkenImage;

	public float DarkenAlpha = 0.5f;

	public float DarkenSpeed = 8f;
}
