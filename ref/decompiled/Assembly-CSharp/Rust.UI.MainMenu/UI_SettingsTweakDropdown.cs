using System;
using UnityEngine;
using UnityEngine.UI;

namespace Rust.UI.MainMenu;

public class UI_SettingsTweakDropdown : UI_SettingsTweakConvar
{
	[Serializable]
	public class NameValue
	{
		public string value;

		public Color imageColor;

		public Translate.Phrase label;

		public string untranslatedLabel;

		public bool rightToLeft;

		public bool useColorInsteadOfText;
	}

	public RustText Current;

	public Image CurrentColor;

	public RustButton Opener;

	public RectTransform Dropdown;

	public RectTransform DropdownContainer;

	public GameObject DropdownItemPrefab;

	public NameValue[] nameValues;

	public bool forceEnglish;

	[HideInInspector]
	public int currentValue;
}
