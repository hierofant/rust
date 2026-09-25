using Facepunch.Flexbox;
using UnityEngine;
using UnityEngine.UI;

public class LootPanelVendingMachine : LootPanel, IVendingMachineInterface
{
	public TmProEmojiRedirector vendingMachineName;

	public GameObject tableHeader;

	public GameObject noSellOrders;

	public FlexTransition noSellOrdersTransition;

	public GameObjectRef sellOrderPrefab;

	public GameObject sellOrderContainer;

	public FlexTransition sellOrderTransition;

	[Header("Busy Overlay")]
	public GameObject busyOverlay;

	public FlexTransition busyTransition;

	public Image busyItemImage;
}
