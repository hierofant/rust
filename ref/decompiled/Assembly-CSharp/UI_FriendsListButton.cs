using Rust.UI;
using UnityEngine;

[RequireComponent(typeof(RustButton))]
public class UI_FriendsListButton : UI_FriendsListBase
{
	public GameObject OnlineTag;

	public RustFlexText OnlineCount;

	public GameObject Callout;

	public GameObject Notification;

	public CanvasGroup NotificationCanvasGroup;

	public ChatEntry ChatEntry;

	public float NotificationDuration = 5f;

	public UI_MainMenuChat chat;
}
