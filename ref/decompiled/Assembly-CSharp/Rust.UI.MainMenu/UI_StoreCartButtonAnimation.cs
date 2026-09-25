using Facepunch.Flexbox;
using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_StoreCartButtonAnimation : BaseMonoBehaviour
{
	[SerializeField]
	private CanvasGroup loading;

	[SerializeField]
	private GameObject addToCartGroup;

	[Space]
	[SerializeField]
	private FlexElement inCartGroup;

	[SerializeField]
	private FlexElement inCartTextParent;

	[SerializeField]
	private RustFlexText inCartText;

	[SerializeField]
	private FlexGraphicTransform inCartIcon;
}
