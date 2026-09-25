using Rust.UI;
using UnityEngine;

public class UI_VendingSellOrderEntry : MonoBehaviour
{
	[Header("Offer")]
	public VirtualItemIcon offerIcon;

	public RustText offerAmountText;

	[Header("Cost")]
	public VirtualItemIcon costIcon;

	public RustText costAmountText;

	[Header("Quantity")]
	public RustButton minusButton;

	public RustInput quantityInput;

	public RustButton plusButton;

	[Header("Buy")]
	public RustButton buyButton;

	[Header("Increased/Decreased Pricing")]
	public GameObject increaseHolder;

	public GameObject decreaseHolder;

	public RustText increasedText;

	public RustText decreasedText;

	[Header("Sold Out")]
	public GameObject soldOutOverlay;

	public GameObject soldOutSection;

	public GameObject inStockSection;
}
