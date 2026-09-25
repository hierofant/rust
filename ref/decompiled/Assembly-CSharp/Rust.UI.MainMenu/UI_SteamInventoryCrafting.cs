using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_SteamInventoryCrafting : MonoBehaviour
{
	[SerializeField]
	private RustText clothAmountText;

	[SerializeField]
	private RustText woodAmountText;

	[SerializeField]
	private RustText metalAmountText;

	[SerializeField]
	[Space]
	private GameObject craftingLoading;

	[SerializeField]
	private CanvasGroup craftingButtonParent;

	[SerializeField]
	private GameObject craftingButtonPrefab;

	[SerializeField]
	public UI_SteamInventoryNewItemModal newItemModal;

	[SerializeField]
	private UI_SteamInventoryCraftingModal craftingModal;

	[SerializeField]
	[Space]
	private UI_SteamInventoryPlayerItemModal bagOpeningModal;

	[SerializeField]
	private UI_SteamInventoryPlayerItemModal playerItemModal;

	[SerializeField]
	private UI_Popup deleteWarningPopup;

	[SerializeField]
	private UI_Popup craftingWarningPopup;
}
