using Facepunch.Flexbox;
using Rust.UI;
using UnityEngine;

public class UI_VendingMachineAdminDialog : UIDialog
{
	[SerializeField]
	private RustInput shopNameInput;

	[SerializeField]
	private UI_SellOrderCreator sellOrderCreator;

	[SerializeField]
	private EmojiGallery emojiGallery;

	[SerializeField]
	private GameObjectRef statsPanelRef;

	[Space]
	[SerializeField]
	private UI_FakeInventory fakeInventory;

	[SerializeField]
	[Space]
	private Transform existingSellOrderParent;

	[SerializeField]
	private GameObjectRef existingSellOrderPrefab;

	[SerializeField]
	private GameObject existingOrdersLoadingThingy;

	[SerializeField]
	private RustButton removeAllExistingOrdersButton;

	[SerializeField]
	private GameObject noExistingOrders;

	[SerializeField]
	private FlexTransition transition;

	[SerializeField]
	[Space]
	private UI_TagToggle droneAccessTag;

	[SerializeField]
	private UI_TagToggle stockTag;

	[SerializeField]
	private UI_TagToggle broadcastingTag;

	[SerializeField]
	private RustButton skinModeToggle;
}
