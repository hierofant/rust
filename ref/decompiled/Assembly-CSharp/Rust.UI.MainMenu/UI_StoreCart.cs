using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_StoreCart : UI_Window
{
	public static readonly Translate.Phrase CartEmptyPhrase = new Translate.Phrase("store.cart", "Cart");

	public static readonly Translate.Phrase CartPhrase = new Translate.Phrase("store.cart.items", "Cart ({0})");

	[Space]
	[SerializeField]
	private StyleAsset emptyStyle;

	[SerializeField]
	private StyleAsset notEmptySyle;

	[SerializeField]
	private RustButton cartButton;

	[SerializeField]
	private Canvas cartButtonCanvas;

	[SerializeField]
	private RustText cartButtonText;

	[SerializeField]
	private RustText itemCountText;

	[SerializeField]
	private RustText totalValueText;

	[Space]
	[SerializeField]
	private RectTransform itemParent;

	[SerializeField]
	private GameObject cartItemPrefab;

	[SerializeField]
	private RustButton checkoutButton;

	[SerializeField]
	private GameObject emptyGroup;

	[SerializeField]
	private GameObject footer;
}
