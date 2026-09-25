using Rust.UI;
using UnityEngine;

public class RentableShopOpenStoreDialog : UIDialog
{
	[SerializeField]
	private RustText TotalCost;

	[SerializeField]
	private RustButton RentButton;

	[SerializeField]
	private RustText NotOwnedParagraphText;

	[SerializeField]
	private RustText OwnedParagraphText;

	[SerializeField]
	private RustText YouReceiveText;

	[SerializeField]
	private GameObject AvailableBox;

	[SerializeField]
	private GameObject UnavailableBox;

	[SerializeField]
	private RustText TakeOverCooldownText;

	[SerializeField]
	private GameObject TakeOverCooldownComplete;
}
