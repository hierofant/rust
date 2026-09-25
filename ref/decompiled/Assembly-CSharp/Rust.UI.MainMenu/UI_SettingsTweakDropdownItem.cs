using UnityEngine;
using UnityEngine.UI;

namespace Rust.UI.MainMenu;

public class UI_SettingsTweakDropdownItem : MonoBehaviour
{
	public Image Image;

	public RustText Text;

	[Space]
	public StyleAsset DefaultStyle;

	public StyleAsset SelectedStyle;

	public RustButton Button;

	[UnityEvent]
	public void SetSelected(bool selected)
	{
		Button.Styles = (selected ? SelectedStyle : DefaultStyle);
		Button.ApplyStyles();
	}
}
