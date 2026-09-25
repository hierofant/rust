using Rust.UI;
using UnityEngine;

public class UI_SellOrderCreator : MonoBehaviour
{
	public RustButton sellMinusButton;

	public RustButton sellPlusButton;

	public VirtualItemIcon sellIcon;

	public RustInput sellAmountInput;

	public RustButton sellClearButton;

	public UI_Spotlight sellSpotlight;

	[Space]
	public RustButton costMinusButton;

	public RustButton costPlusButton;

	public VirtualItemIcon costIcon;

	public RustInput costAmountInput;

	public RustButton costClearButton;

	public UI_Spotlight costSpotlight;

	[Space]
	public RustButton clearAllButton;

	public RustButton createSellOrderButton;

	public GameObject cancelModifyButton;

	public GameObject confirmModifyButton;

	public UI_ItemSearchPopup itemSearchPopup;

	[Space]
	public RustText sellItemName;

	public RustText costItemName;

	public GameObject sellItemAmountHolder;

	public GameObject costItemAmountHolder;

	public GameObject sellClearButtonHolder;

	public GameObject costClearButtonHolder;

	[Space]
	public GameObject suggestionsObject;

	public UI_SuggestionsHolder suggestions;
}
