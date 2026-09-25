using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_ServerMap : UI_Window
{
	[Header("References")]
	[SerializeField]
	private HttpImage _httpImage;

	[SerializeField]
	private GameObject _loadingObject;

	[SerializeField]
	private RustButton _gridButton;
}
