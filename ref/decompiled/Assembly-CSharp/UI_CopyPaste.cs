using Rust.UI;
using Rust.UI.MainMenu;
using UnityEngine;
using UnityEngine.UI;

public class UI_CopyPaste : UI_Window
{
	[Space]
	[SerializeField]
	protected ScrollRect scrollRect;

	[SerializeField]
	protected FlexVirtualScroll gridVirtualScrollFlex;

	[SerializeField]
	protected FlexVirtualScroll listVirtualScrollFlex;

	[SerializeField]
	private RectTransform fileButtonsParent;

	[SerializeField]
	private RectTransform fileLinesParent;

	[Space]
	[SerializeField]
	private GameObject gridModeButtonGo;

	[SerializeField]
	private GameObject listModeButtonGo;

	[SerializeField]
	private GameObject sortButton;

	[SerializeField]
	private RustText sortButtonText;

	[SerializeField]
	private RustButton undoPasteButton;

	[SerializeField]
	private GameObject fileName;

	[SerializeField]
	private RustText undoPasteText;

	[SerializeField]
	private GameObject nothingToShowContainer;

	[SerializeField]
	private RustInput searchInputField;
}
