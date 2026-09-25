using UnityEngine;

namespace Rust.UI.MainMenu.Workshop;

public class UI_WorkshopItemList : MonoBehaviour
{
	[SerializeField]
	private RustButton previousPageButton;

	[SerializeField]
	private RustButton nextPageButton;

	[SerializeField]
	private UI_WorkshopItemButton itemPrefab;

	[SerializeField]
	private UI_WorkshopItemButton myItemPrefab;

	[SerializeField]
	private Transform itemsParent;

	[SerializeField]
	private RustText pageInfoText;

	[SerializeField]
	private Dropdown itemTypeSelector;
}
