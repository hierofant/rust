using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class UIConversationScreen : SingletonComponent<UIConversationScreen>, IUIScreen
{
	public RectTransform layoutGroupRectTransform;

	public CanvasGroup canvasGroup;

	public NeedsCursor needsCursor;

	public RustText conversationSpeechBody;

	public RustText conversationProviderName;

	public Image conversationProviderImage;

	public UIDialogueChoice[] dialogueChoices;

	public RectTransform letterBoxTop;

	public RectTransform letterBoxBottom;

	public CanvasGroup responseCanvasGroup;

	public GameObject cancelButton;

	public GameObject conversationPanel;

	public UIMissionInfoConversation missionInfo;

	public Canvas canvas;

	public GraphicRaycaster graphicRaycaster;

	public UIEscapeCapture escapeCapture;

	public RustButton skipTalkingButton;

	[Min(0f)]
	public float layoutGroupWidthWithDialogue;

	[Min(0f)]
	public float layoutGroupWidthWithoutDialogue;
}
