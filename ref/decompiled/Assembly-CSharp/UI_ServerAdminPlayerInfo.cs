using Rust.UI;
using Rust.UI.MainMenu;
using UnityEngine;
using UnityEngine.UI;

public class UI_ServerAdminPlayerInfo : UI_Window
{
	private static readonly Translate.Phrase MutePhrase = new Translate.Phrase("playerinfo.mute", "Mute ({0})");

	private static readonly Translate.Phrase PermanentPhrase = new Translate.Phrase("playerinfo.mutepermanent", "Permanent");

	[SerializeField]
	[Space]
	private RawImage avatarImage;

	[SerializeField]
	private RustText headerNameText;

	[SerializeField]
	private RustText playerNameText;

	[SerializeField]
	private RustText steamIDText;

	[SerializeField]
	private RustText pingText;

	[SerializeField]
	private GameObject addressGroup;

	[SerializeField]
	private RustText addressText;

	[SerializeField]
	private RustText connectedTimeText;

	[SerializeField]
	private RustText violationLevelText;

	[SerializeField]
	private RustText healthText;

	[SerializeField]
	private RustText positionText;

	[SerializeField]
	private RustText teamIDText;

	[Space]
	[SerializeField]
	private RustInput muteReasonInput;

	[SerializeField]
	private RustInput kickReasonInput;

	[SerializeField]
	private RustInput banReasonInput;

	[SerializeField]
	private RustText muteButtonText;

	[SerializeField]
	private GameObject muteButton;

	[SerializeField]
	private GameObject unmuteButton;
}
