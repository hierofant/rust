using ConVar;
using Rust.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIChat : PriorityListComponent<UIChat>
{
	public GameObject inputArea;

	public GameObject chatArea;

	public TMP_InputField inputField;

	public TextMeshProUGUI channelLabel;

	public ScrollRect scrollRect;

	public CanvasGroup canvasGroup;

	public bool allowOpeningWhileCursorVisible;

	public GameObjectRef chatItemPlayer;

	public GameObject userPopup;

	public EmojiGallery emojiGallery;

	public GameObject dmPicker;

	public GameObject dmNameSection;

	public RustText dmName;

	public CanvasGroup backgroundCanvasGroup;

	public int maxMessageCount = 16;

	public bool fadeOutMessages = true;

	public bool enableSingleChannel;

	public Chat.ChatChannel singleChannel;

	public UI_FriendsList friendsList;

	[Header("Disable Settings")]
	[Tooltip("Disable the text input field rather than hiding it.")]
	public bool useDisable;

	public RustInput rustInput;

	[FormerlySerializedAs("forceOpen")]
	public bool isMainMenuChat;
}
