using Rust.UI.MainMenu;
using UnityEngine;

public class UI_Friends : UI_Window
{
	public static UI_Friends Instance;

	public CanvasGroup CanvasGroup;

	public RectTransform Body;

	public UI_FriendsListBase FriendsList;

	public UI_FriendsListButton Button;

	public GameObject DiscordSettingsButton;

	public GameObject DiscordSettingsPanel;
}
