using Facepunch.Flexbox;
using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_StoreItemTile : BaseMonoBehaviour
{
	public RustButton button;

	[SerializeField]
	public CanvasGroup animatedParent;

	[SerializeField]
	private RustText skinNameText;

	[SerializeField]
	private RustText itemTypeText;

	[Space]
	[SerializeField]
	private CanvasGroup imageGroup;

	[SerializeField]
	protected HttpImage httpImage;

	[SerializeField]
	private CoverVideo coverVideo;

	[SerializeField]
	public CoverImage coverImage;

	[Header("Header")]
	[SerializeField]
	[Space]
	private GameObject headerTextGroup;

	[SerializeField]
	private RustText headerText;

	[SerializeField]
	private FlexElement paddedContainer;

	[SerializeField]
	private UI_StoreAddCartButton cartButton;

	[Space]
	[SerializeField]
	private GameObject ownedOverlay;

	[SerializeField]
	private GameObject ownedTag;

	[SerializeField]
	private bool disableCartWhenOwned;

	[SerializeField]
	private bool fadeIconWhenOwned = true;
}
