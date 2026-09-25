using Rust.UI;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class UIVideoPlayer : UIDialog
{
	public AspectRatioFitter aspectRatioFitter;

	public GameObject closeButton;

	public VideoPlayer videoPlayer;

	public RawImage videoCanvas;

	public CanvasGroup controlsCanvas;

	public RustSlider volumeSlider;

	public RectTransform volumeSliderRect;

	public RustIcon volumeIcon;

	public Icons[] volumeLoudnessIcons;

	public RectTransform volumeKnob;

	public RectTransform videoProgressBar;

	public GameObject loadingIndicator;

	public float audioDuckingAmount = 0.333f;

	public float timeoutAfter = 5f;
}
