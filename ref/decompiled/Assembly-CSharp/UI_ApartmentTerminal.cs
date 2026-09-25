using Rust.UI;
using Rust.UI.MainMenu;
using UnityEngine;
using UnityEngine.UI;

public class UI_ApartmentTerminal : UI_Window
{
	[SerializeField]
	private RectTransform plotList;

	[SerializeField]
	private GameObjectRef plotRowPrefab;

	[Space]
	[SerializeField]
	private RustText timeText;

	[SerializeField]
	private RustText availableText;

	[SerializeField]
	private RustText occupiedText;

	[SerializeField]
	[Header("Opening")]
	[Space]
	private RectTransform crtScreen;

	[SerializeField]
	private GameObject splashScreen;

	[SerializeField]
	private GameObject mainMenu;

	[SerializeField]
	private RustText bootLogText;

	[SerializeField]
	private RustText subtitleText;

	[Space]
	[Header("CCTV")]
	[SerializeField]
	private RawImage feedImage;

	[SerializeField]
	private Camera feedCamera;

	[SerializeField]
	private GameObject feedNoSignal;

	[Header("Tabs")]
	[Space]
	[SerializeField]
	private GameObject apartmentsPanel;

	[SerializeField]
	private GameObject shopsPanel;

	[SerializeField]
	[Space]
	[Header("Shops")]
	private RectTransform shopList;

	[SerializeField]
	private GameObjectRef shopRowPrefab;

	[SerializeField]
	private RustText shopsAvailableText;

	[SerializeField]
	private RustText shopsOccupiedText;

	private static readonly string[] BootLines = new string[9] { "APRT-OS v1.4  (C) COBALT SYSTEMS", "", "> POST .................. OK", "> MEM CHECK 640K ........ OK", "> TENANT REGISTRY ....... MOUNTED", "> NET LINK .............. ESTABLISHED", "> AUTHENTICATING ........ OK", "", "READY." };

	private static readonly Translate.Phrase occupiedPhrase = new Translate.Phrase("apartment.occupied-plots", "{0} Occupied");

	private static readonly Translate.Phrase availablePhrase = new Translate.Phrase("apartment.available-plots", "{0} Available");

	private static readonly Translate.Phrase supplierPhrase = new Translate.Phrase("apartment.terminal.supplier", "[SUPPLIER OF AFFORDABLE LIVING SPACES]");

	private static readonly Translate.Phrase apartmentsTabPhrase = new Translate.Phrase("apartment.terminal.tab.apartments", "Apartments");

	private static readonly Translate.Phrase shopsTabPhrase = new Translate.Phrase("apartment.terminal.tab.shops", "Shops");

	private static readonly Translate.Phrase shopNumberPhrase = new Translate.Phrase("apartment.shop.number", "Shop {0}");
}
