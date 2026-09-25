using Facepunch.Flexbox;
using UnityEngine;
using UnityEngine.UI;

namespace Rust.UI.MainMenu;

public class UI_ConnectModal : UI_Window
{
	[SerializeField]
	[Header("References")]
	private RustText _title;

	[SerializeField]
	private RustText _description;

	[SerializeField]
	private HttpImage _headerImage;

	[SerializeField]
	private GameObject _headerImageLoading;

	[SerializeField]
	private ServerBrowserTagList _tagController;

	[SerializeField]
	private RustText _mapTypeText;

	[SerializeField]
	private RustButton _websiteButton;

	[SerializeField]
	private Tooltip _websiteTooltip;

	[SerializeField]
	private GameObject _descriptionLoading;

	[SerializeField]
	private GameObject _connectToServerButton;

	[SerializeField]
	private GameObject _needsPremiumButton;

	[SerializeField]
	private GameObject _mapButton;

	[SerializeField]
	private UI_ServerMap _map;

	[SerializeField]
	[Header("References - System Config")]
	private GameObject _requiredSystemConfigSection;

	[SerializeField]
	private UI_TagToggle _tpmCheck;

	[SerializeField]
	private UI_TagToggle _secureBootCheck;

	[SerializeField]
	private UI_TagToggle _kernelCodeIntegrityCheck;

	[SerializeField]
	private UI_TagToggle _iommuCheck;

	[Header("References - Friends")]
	[SerializeField]
	private RustText _friendsText;

	[SerializeField]
	private GameObject _friendsObject;

	[SerializeField]
	private Tooltip _friendsTooltip;

	[Header("Info Box References")]
	[SerializeField]
	private RustText _playerCount;

	[SerializeField]
	private GameObject _pingObject;

	[SerializeField]
	private RustText _pingText;

	[SerializeField]
	private RustText _regionText;

	[SerializeField]
	private GameObject _queuedPlayersObject;

	[SerializeField]
	private RustText _queuedPlayersCount;

	[SerializeField]
	private GameObject _lastPlayedObject;

	[SerializeField]
	private RustText _lastPlayedText;

	[SerializeField]
	private GameObject _wipedObject;

	[SerializeField]
	private RustText _wipedText;

	[SerializeField]
	[Header("Nexus")]
	private GameObject _zoneCountObject;

	[SerializeField]
	private RustText _zoneCountText;

	[SerializeField]
	private UINexusMapWidget _nexusMapWidget;

	[SerializeField]
	private GameObject _zoneListSection;

	[SerializeField]
	private FlexTransition _zoneListReveal;

	[SerializeField]
	private RectTransform _zoneListParent;

	[SerializeField]
	private GameObject _descriptionSection;

	[SerializeField]
	private RustButton _zoneListToggle;

	[SerializeField]
	private GameObjectRef _zoneListItem;

	[SerializeField]
	private ScrollRect _scrollRect;

	[SerializeField]
	private RectMask2D _scrollMask;

	public static Translate.Phrase lastPlayedPhrase = new Translate.Phrase("connection.modal.lastplayed.ago", "{0} ago");

	public static Translate.Phrase serverAgePhrase = new Translate.Phrase("connection.modal.serverage.old", "{0} old");

	public static Translate.Phrase loadingError = new Translate.Phrase("connection.modal.error", "Error loading server");

	public static Translate.Phrase nexusZonesPhrase = new Translate.Phrase("nexus.zones", "{0} zones");
}
