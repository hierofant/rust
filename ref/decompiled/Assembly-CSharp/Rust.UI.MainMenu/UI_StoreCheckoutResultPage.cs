using Facepunch.Flexbox;
using UnityEngine;
using UnityEngine.UI;

namespace Rust.UI.MainMenu;

public class UI_StoreCheckoutResultPage : UI_Window
{
	[SerializeField]
	[Space]
	private FlexTransition crossFadeTransition;

	[SerializeField]
	private CoverImage coverBackground;

	[SerializeField]
	private Sprite defaultBackground;

	[SerializeField]
	private RustText itemNameText;

	[SerializeField]
	private RustText itemSubtitleText;

	[Space]
	[SerializeField]
	private UI_StoreCheckoutResultButton carrouselButtonPrefab;

	[SerializeField]
	private RectTransform buttonsParent;

	[SerializeField]
	[Space]
	private GraphicRaycaster footerGraphicRaycaster;

	[SerializeField]
	private CanvasGroup arrowButtons;

	[SerializeField]
	private ScrollRect scrollRect;

	[SerializeField]
	private FlexElement scrollContentFlex;

	[SerializeField]
	private CanvasGroup leftArrow;

	[SerializeField]
	private CanvasGroup rightArrow;

	[SerializeField]
	private bool autoCycleEnabled = true;

	[SerializeField]
	private float autoCycleInterval = 10f;

	[SerializeField]
	private UI_StoreTakeover localTakeovers;

	[Space]
	[Header("Skin Viewer")]
	[SerializeField]
	private CoverImage skinViewerImage;

	[SerializeField]
	private GameObject icon3D;

	[SerializeField]
	private GameObject icon2D;

	[SerializeField]
	private GameObject loadingOverlay;

	[SerializeField]
	protected HttpImage iconImage;

	[Header("Intro Sequence")]
	[SerializeField]
	private CanvasGroup fadeOverlay;

	[SerializeField]
	private CanvasGroup inventoryButton;
}
