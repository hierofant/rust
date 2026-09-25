using Rust.UI;
using Rust.UI.MainMenu;
using UnityEngine;
using UnityEngine.UI;

public class UI_Loadout : UI_Window
{
	[SerializeField]
	[Space]
	protected RectTransform buttonsParents;

	[SerializeField]
	protected ScrollRect scrollRect;

	[SerializeField]
	protected FlexVirtualScroll virtualScrollFlex;

	[SerializeField]
	private RustText sortButtonText;

	[SerializeField]
	private GameObject openFolderButton;

	[SerializeField]
	private UI_Popup_LoadoutSave savePopup;

	[SerializeField]
	private GameObject nothingToShowContainer;

	[SerializeField]
	private RustInput searchInputField;
}
