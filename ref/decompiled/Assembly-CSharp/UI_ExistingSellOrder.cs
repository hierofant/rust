using Facepunch.Flexbox;
using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class UI_ExistingSellOrder : FacepunchBehaviour
{
	[Header("Background Style")]
	public Image background;

	public StyleAsset backgroundStyle;

	[Header("Background")]
	public VirtualItemIcon offerIcon;

	public RustText amountText;

	public VirtualItemIcon costIcon;

	public RustText costText;

	public FlexTransition transition;
}
