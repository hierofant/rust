using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Rust.UI.MainMenu;

public class UI_Settings : UI_Page
{
	[Serializable]
	private class SettingTab
	{
		public string name;

		public UI_SettingsTabButton tabButton;

		public GameObject content;

		[ReadOnly]
		public List<SettingEntry> settings;

		public RustButton button => tabButton.button;

		public CanvasGroup canvasGroup => tabButton.canvasGroup;
	}

	private class SettingEntry
	{
		[NonSerialized]
		public SettingTab ownerTab;

		public string convarName;

		public string nameToken;

		public UI_SettingsTweakBase tweakUIBase;

		private CanvasGroup _canvasGroup;

		private RustText _labelText;

		private RustButton _button;

		private string searchIndex;

		public GameObject gameObject => tweakUIBase?.gameObject;

		public CanvasGroup canvasGroup
		{
			get
			{
				if (_canvasGroup == null)
				{
					_canvasGroup = gameObject.GetComponent<CanvasGroup>();
				}
				return _canvasGroup;
			}
		}

		public RustText labelText
		{
			get
			{
				if (_labelText == null)
				{
					_labelText = gameObject.GetComponentInChildren<RustText>();
				}
				return _labelText;
			}
		}

		public RustButton button
		{
			get
			{
				if (_button == null)
				{
					_button = gameObject.GetComponent<RustButton>();
				}
				return _button;
			}
		}

		public SettingEntry(string convarName, UI_SettingsTweakBase tweakUIBase)
		{
			this.convarName = convarName;
			this.tweakUIBase = tweakUIBase;
		}

		public string GetSettingName()
		{
			return Translate.Get(nameToken);
		}

		public string GetTooltip()
		{
			return Translate.Get(tweakUIBase.tooltip.token);
		}

		public void BuildSearchIndex()
		{
			searchIndex = string.Join(" ", GetSettingName(), GetTooltip());
		}

		public bool Matches(string query)
		{
			if (string.IsNullOrWhiteSpace(query))
			{
				return false;
			}
			string[] array = query.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
			foreach (string text in array)
			{
				int startIndex = 0;
				while (true)
				{
					int num = searchIndex.IndexOf(text, startIndex, StringComparison.OrdinalIgnoreCase);
					if (num < 0)
					{
						break;
					}
					if (!RustText.IsInsideTag(searchIndex, num))
					{
						return true;
					}
					startIndex = num + text.Length;
				}
			}
			return false;
		}
	}

	[SerializeField]
	private List<SettingTab> tabs;

	[SerializeField]
	private UI_SearchBar searchBar;

	[SerializeField]
	private GameObject searchButtonGroup;

	[SerializeField]
	[Space]
	private GameObject gestureGroup;

	[SerializeField]
	private CanvasGroup tooltipGroup;

	[SerializeField]
	private RustText tooltipNameText;

	[SerializeField]
	private RustText tooltipDescText;

	[SerializeField]
	private CanvasGroup tooltipImageGroup;

	[SerializeField]
	private CoverImage tooltipImage;

	[SerializeField]
	private CoverVideo tooltipVideo;

	[SerializeField]
	[Space]
	private ScrollRect scrollRect;

	[SerializeField]
	private CanvasGroup scrollbar;

	[SerializeField]
	private RectMask2D mask;

	[SerializeField]
	private UI_Popup safeModePopupPrefab;

	[SerializeField]
	private UI_SettingsGestureWheel gestureWheel;

	[SerializeField]
	private GameObject previewCrosshair;

	[SerializeField]
	private UI_Popup_CrosshairImportExport crosshairImportPopupPrefab;

	[SerializeField]
	private UI_Popup_CrosshairImportExport crosshairExportPopupPrefab;
}
