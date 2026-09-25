using Facepunch.Flexbox;
using Rust.UI;
using Rust.UI.MainMenu;
using UnityEngine;
using UnityEngine.UI;

public abstract class UI_DevListBase : UI_Window
{
	[Space]
	[SerializeField]
	protected FlexColumnsElement flexColumns;

	[SerializeField]
	protected ScrollRect scrollRect;

	[SerializeField]
	protected GameObjectRef categoryButtonPrefab;

	[SerializeField]
	protected GameObjectRef itemButtonPrefab;

	[SerializeField]
	[Space]
	protected RectTransform categoryButtonsParent;

	[SerializeField]
	protected RectTransform itemButtonsParent;

	[SerializeField]
	protected RustInput searchInputField;

	[SerializeField]
	protected FlexVirtualScroll virtualScrollFlex;

	[SerializeField]
	[Space]
	protected RustButton favouritesButton;

	[SerializeField]
	protected RustButton recentsButton;

	[SerializeField]
	protected UI_RustButtonGroup buttonGroup;
}
