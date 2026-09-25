using UnityEngine;
using UnityEngine.UI;

namespace Rust.UI.MainMenu;

public class UI_SettingsTweakColour : UI_SettingsTweakConvar
{
	public Image BackgroundImage;

	public RustButton Opener;

	public RectTransform Dropdown;

	public RectTransform DropdownContainer;

	public GameObject DropdownItemPrefab;

	public AccessibilityColourCollection forColourCollection;

	public AccessibilityMaterialCollection forMaterialCollection;

	public int currentValue;
}
