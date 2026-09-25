using Rust.UI.MainMenu;
using UnityEngine;

public class UI_DemosMenuWindow : UI_Window
{
	public static UI_DemosMenuWindow Instance;

	[SerializeField]
	private FlexVirtualScroll virtualScroll;

	[SerializeField]
	private GameObject deleteButton;

	[SerializeField]
	private GameObject playButton;

	[SerializeField]
	private UI_SearchBar searchField;
}
