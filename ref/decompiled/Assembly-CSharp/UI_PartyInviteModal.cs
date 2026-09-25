using System.Threading;
using Rust.UI;
using UnityEngine;

public class UI_PartyInviteModal : SingletonComponent<UI_PartyInviteModal>
{
	public RustText InviteLabel;

	public RustButton AcceptButton;

	public static Translate.Phrase InvitePhrase = new Translate.Phrase("party.invite", "{0} has invited you to a party");

	private ulong pendingLobbyId;

	private TimeSince age;

	public HttpImage ProfilePicture;

	public RectTransform ProgressBar;

	private CancellationTokenSource cancel;

	private ulong lastUserId;

	public bool IsShown => base.gameObject.activeInHierarchy;

	public void Show(string username, ulong userId, ulong lobbyId)
	{
	}

	public void OnClientStartup()
	{
		if (lastUserId != 0L)
		{
			age = 0f;
		}
	}

	public void Hide()
	{
	}

	[UnityEvent]
	public void OnAcceptButtonClicked()
	{
	}
}
