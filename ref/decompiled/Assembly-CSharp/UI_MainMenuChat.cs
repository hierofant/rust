using Rust.UI;
using UnityEngine;

public class UI_MainMenuChat : UIChat
{
	public static UI_MainMenuChat MenuChatInstance;

	[Header("Menu Chat")]
	public RustText placeholderText;

	public GameObject dmTargetIconSteam;

	public GameObject dmTargetIconDiscord;

	public GameObject dmTargetIconDefault;

	[Space]
	public RectTransform windowMovingParent;

	public UIEscapeCapture escapeCapture;

	public CanvasGroup dismisser;

	public override int Priority => 0;
}
