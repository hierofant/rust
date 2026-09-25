using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_SteamInventoryPlayerItemModal : UI_SteamInventoryItemBaseModal
{
	[Header("Breakdown")]
	[SerializeField]
	private GameObject breakdownGroup;

	[SerializeField]
	private GameObject notScrappableGroup;

	[SerializeField]
	protected UI_SteamInventoryCraftingModal.MaterialGroup woodGroup;

	[SerializeField]
	protected UI_SteamInventoryCraftingModal.MaterialGroup metalGroup;

	[SerializeField]
	protected UI_SteamInventoryCraftingModal.MaterialGroup clothGroup;

	[Header("Breakdown Controls")]
	[SerializeField]
	private GameObject breakdownButtonGroup;

	[SerializeField]
	private GameObject cantBreakdownButtonGroup;

	[SerializeField]
	private GameObject deleteButtonGroup;

	[SerializeField]
	private RustText cantBreakdownReasonText;
}
