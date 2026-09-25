using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_SteamInventory : UI_Page
{
	public static UI_SteamInventory Instance;

	[SerializeField]
	private FlexVirtualScroll virtualScrollFlex;

	[SerializeField]
	[Space]
	private UI_SteamInventoryCrafting crafting;

	[SerializeField]
	private UI_SteamInventoryItem inventoryItemPrefab;

	[SerializeField]
	private RectTransform inventoryItemParent;

	[Space]
	public GameObject loadingOverlay;

	[SerializeField]
	private GameObject noConnectionOverlay;

	[SerializeField]
	private UI_StoreTakeover takeovers;

	[SerializeField]
	private RustButton refreshButton;

	[SerializeField]
	private RustInput searchBar;

	private static readonly Translate.Phrase inventoryRetryPhrase = new Translate.Phrase("inventory.retry", "Retry");

	private static readonly Translate.Phrase inventoryConnectionIssuePhrase = new Translate.Phrase("inventory.connection_issue", "Your Steam inventory failed to load. Steam might be down?");

	public static UI_SteamInventoryCrafting Crafting => Instance?.crafting;
}
