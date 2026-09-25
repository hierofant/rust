using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class UI_Autocomplete : MonoBehaviour
{
	public ScrollRect scrollRect;

	[Header("Buttons")]
	public UI_AutoCompleteButton[] buttons;

	public StyleAsset buttonStyle;

	public StyleAsset hoverStyle;

	public StyleAsset buttonDarkStyle;

	public bool IsShowing { get; private set; }
}
