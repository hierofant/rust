using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class UIBinocularOverlay : UIBlackoutOverlay, IShadowGroupVisibility
{
	public RustText RangeText;

	public CanvasGroup RangeGroup;

	public RectTransform RangeTransform;

	public Vector2 RangeScreenPosition = new Vector2(0f, 0f);

	public Material binocularEffectMaterial;

	public Image overlayImage;

	[Header("Day/Night Settings")]
	public float dayFresnel;

	public float nightFresnel;

	public float dayGlare;

	public float nightGlare;

	public float dayCoating;

	public float nightCoating;

	public bool ShouldUpdateShadows => RangeGroup.alpha > 0f;
}
