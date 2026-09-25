using UnityEngine;

public class UI_FriendsListContextMenu : MonoBehaviour
{
	public UI_FriendsList FriendsList;

	[SerializeField]
	private CanvasGroup canvasGroup;

	public GameObject SendMessageButton;

	public GameObject InviteToGameButton;

	public GameObject JoinGameButton;

	public GameObject AddFriendSteamButton;

	public GameObject AddFriendDiscordButton;

	public GameObject PromotePartyLeaderButton;

	public GameObject KickPartyMemberButton;

	public GameObject InviteToPartyButton;

	public GameObject LeavePartyButton;
}
