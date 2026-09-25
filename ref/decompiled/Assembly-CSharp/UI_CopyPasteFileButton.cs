using Facepunch.Flexbox;
using Rust.UI;
using UnityEngine;

public class UI_CopyPasteFileButton : MonoBehaviour
{
	[SerializeField]
	private StyleAsset style;

	[SerializeField]
	private StyleAsset evenStyle;

	[SerializeField]
	private RustButton button;

	[Space]
	[SerializeField]
	private bool showThumbnailOnHover;

	[SerializeField]
	private GameObject thumbnailGroup;

	[SerializeField]
	private CoverImage thumbnailImage;

	[SerializeField]
	private GameObject missingThumbnail;

	[SerializeField]
	private RustText fileNameText;

	[SerializeField]
	private RustText dateText;

	[SerializeField]
	private RustText fileSizeText;

	[SerializeField]
	private GameObject entityCount;

	[SerializeField]
	private RustText entityCountText;

	[SerializeField]
	[Space]
	private GameObject deleteButton;

	[SerializeField]
	private FlexTransition hoverTransition;
}
