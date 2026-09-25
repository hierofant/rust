using Rust.UI;
using UnityEngine;

public class UICameraOverlay : SingletonComponent<UICameraOverlay>
{
	public static readonly Translate.Phrase FocusOffText = new Translate.Phrase("camera.infinite_focus", "Infinite Focus");

	public static readonly Translate.Phrase FocusAutoText = new Translate.Phrase("camera.auto_focus", "Auto Focus");

	public static readonly Translate.Phrase FocusManualText = new Translate.Phrase("camera.manual_focus", "Manual Focus");

	public static readonly Translate.Phrase FlashOn = new Translate.Phrase("camera.flash_is_on", "Flash [ON]");

	public static readonly Translate.Phrase FlashOff = new Translate.Phrase("camera.flash_is_off", "Flash [OFF]");

	public Canvas Canvas;

	public CanvasGroup CanvasGroup;

	public RustText FocusModeLabel;

	public RustText FlashLabel;

	protected override void Awake()
	{
		base.Awake();
		Hide();
	}

	public void Show()
	{
		if (Canvas != null)
		{
			Canvas.enabled = true;
		}
		CanvasGroup.alpha = 1f;
	}

	public void Hide()
	{
		if (Canvas != null)
		{
			Canvas.enabled = false;
		}
		CanvasGroup.alpha = 0f;
	}

	public void SetFlash(bool flashEnabled)
	{
		FlashLabel.SetPhrase(flashEnabled ? FlashOn : FlashOff);
	}

	public void SetFocusMode(CameraFocusMode mode)
	{
		switch (mode)
		{
		case CameraFocusMode.Auto:
			FocusModeLabel.SetPhrase(FocusAutoText);
			break;
		case CameraFocusMode.Manual:
			FocusModeLabel.SetPhrase(FocusManualText);
			break;
		default:
			FocusModeLabel.SetPhrase(FocusOffText);
			break;
		}
	}
}
