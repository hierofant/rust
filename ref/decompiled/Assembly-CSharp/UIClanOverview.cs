using Rust.UI;
using UnityEngine.UI;

public class UIClanOverview : BaseMonoBehaviour, IUIPlayerRefreshCallback
{
	public static readonly Translate.Phrase SetMotdFailure = new TokenisedPhrase("clan.set_motd.fail", "Failed to update the message of the day.");

	public UIClans UiClans;

	public RawImage MotdAuthorAvatar;

	public RustText MotdAuthorName;

	public RustText MotdTime;

	public RustInput MotdInput;

	public RustText MotdCharacterLimit;

	public RustButton MotdSaveButton;

	public RustButton MotdCancelButton;
}
