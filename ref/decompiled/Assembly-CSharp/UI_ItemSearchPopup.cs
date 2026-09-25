using Facepunch.Flexbox;
using Rust.UI;
using UnityEngine;

public class UI_ItemSearchPopup : MonoBehaviour
{
	public RustInput searchInput;

	public CanvasGroup canvasGroup;

	public UI_ItemSearchEntry[] entries;

	public StyleAsset buttonStyle;

	public StyleAsset buttonDarkStyle;

	public FlexTransition openTransition;
}
