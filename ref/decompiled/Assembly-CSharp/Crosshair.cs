using UnityEngine;
using UnityEngine.UI;

public class Crosshair : BaseMonoBehaviour
{
	public static bool Enabled = true;

	public Image Image;

	public bool IsSettingsPreview;

	public Canvas reticleCanvas;

	public RectTransform reticleTransform;

	public CanvasGroup reticleAlpha;

	public Image imageCenter;

	public Image imageCenterOutline;

	public Image imageTop;

	public Image imageBottom;

	public Image imageLeft;

	public Image imageRight;

	public Outline[] outlineComponents;

	public RectTransform hitNotifyMarker;

	public CanvasGroup hitNotifyAlpha;

	public static Crosshair instance;

	public static float lastHitTime = 0f;

	public float crosshairAlpha = 1f;

	public float aimconeMultiplier = 3f;

	public float aimconeLerpSpeed = 15f;

	public GameObjectRef pointsSplashEffect;
}
