using Facepunch.Flexbox;
using Rust.UI;
using UnityEngine;

public class UI_FriendsList : UI_FriendsListBase
{
	public GameObjectRef FriendPrefab;

	public FriendStyleDef FriendStyle;

	public RustInput SearchInput;

	public RustButton ShowPendingToggle;

	public RustButton ShowOfflineToggle;

	public FlexElement RootElement;

	public GameObject PendingSection;

	public RustText PendingCount;

	public RectTransform PendingContainer;

	public GameObject InGameSection;

	public RustText InGameCount;

	public RectTransform InGameContainer;

	public GameObject OnlineSection;

	public RustText OnlineCount;

	public RectTransform OnlineContainer;

	public GameObject OfflineSection;

	public RustText OfflineCount;

	public RectTransform OfflineContainer;

	public GameObject LinkDiscordSection;

	public UIChat Chat;

	public UI_FriendsListContextMenu ContextMenu;

	public UIParty Party;

	public UIParty FooterParty;
}
