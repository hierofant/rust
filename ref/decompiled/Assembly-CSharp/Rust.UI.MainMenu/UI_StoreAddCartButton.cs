using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_StoreAddCartButton : ListComponent<UI_StoreAddCartButton>
{
	[SerializeField]
	private StyleAsset notInCartStyle;

	[SerializeField]
	private StyleAsset inCartStyle;

	[SerializeField]
	private RustButton button;

	[SerializeField]
	private RustText priceText;

	[SerializeField]
	private GameObject discountTag;

	[SerializeField]
	private RustText discountText;

	[SerializeField]
	[Space]
	private Animator animator;

	[SerializeField]
	private UI_StoreCartButtonAnimation animationSequence;

	[SerializeField]
	[Space]
	private SteamInventoryItem autoInitItem;
}
