using Facepunch.Flexbox;
using Rust.UI;
using Rust.UI.MainMenu;
using UnityEngine;
using UnityEngine.UI;

public class UI_ServerAdmin : UI_Window
{
	public static UI_ServerAdmin Instance;

	[SerializeField]
	[Header("Player List")]
	private GameObjectRef playerEntryPrefab;

	[SerializeField]
	private RectTransform playerInfoParent;

	[SerializeField]
	private FlexElement playerInfoParentFlex;

	[SerializeField]
	private Scrollbar playerListScrollbar;

	[SerializeField]
	private ScrollRect playerListScrollRect;

	[SerializeField]
	private RustText playerCountText;

	[SerializeField]
	private RustInput playerNameFilter;

	[SerializeField]
	private UI_ServerAdminPlayerInfo playerInfoPanel;

	[SerializeField]
	private RustInput playerListSearchInput;

	[SerializeField]
	[Header("Server Info")]
	private GameObjectRef serverInfoEntryPrefab;

	[SerializeField]
	private RectTransform serverInfoParent;

	[Header("Convars")]
	[SerializeField]
	private GameObjectRef convarInfoEntryPrefab;

	[SerializeField]
	private GameObjectRef convarInfoLongEntryPrefab;

	[SerializeField]
	private RectTransform convarInfoParent;

	[SerializeField]
	[Header("UGC")]
	private FlexVirtualScroll ugcVirtualScroll;

	[SerializeField]
	private FlexElement ugcContentFlex;

	[SerializeField]
	private Scrollbar ugcScrollbar;

	[SerializeField]
	private RustInput ugcNameFilter;

	[SerializeField]
	private UI_ServerAdminUGCFilterPanel ugcFilterPanel;

	[SerializeField]
	private GameObject expandedUgcRoot;

	[SerializeField]
	private RawImage expandedUgcImage;

	[SerializeField]
	private UI_ServerAdminExpandedUgc expandedUgcController;
}
