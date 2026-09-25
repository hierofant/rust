using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_Store : UI_Page
{
	[Serializable]
	private class StoreTab
	{
		public string name;

		public RustButton button;

		public UI_StoreTabBase page;
	}

	private const string JOURNEY_ID_KEY = "journey_id";

	[Header("Search")]
	[SerializeField]
	private UI_StoreItemGrid searchGrid;

	[SerializeField]
	private RustText searchSubtitleText;

	public static UI_Store Instance;

	[SerializeField]
	private List<StoreTab> tabs;

	[SerializeField]
	private SteamDLCItem[] dlcItems;

	[SerializeField]
	private UI_StoreLimitedItemModal weeklySkinModal;

	[SerializeField]
	private UI_StoreCart cart;

	[SerializeField]
	private RectTransform pageOverlayParent;

	[SerializeField]
	private GameObject loadingOverlay;

	[SerializeField]
	private GameObject noConnectionOverlay;

	public UI_StoreTakeover Takeovers;

	public UI_StoreCheckoutResultPage checkoutResultPagePrefab;

	[SerializeField]
	private GameObject newHeaderButtonTag;

	public static int CurrentWeekID;

	private const string CART_KEY = "STORE_CART_ITEMS";

	private const string KNOWN_OWNEDITEMS_KEY = "STORE_KNOWN_OWNEDITEMS";

	public static Guid JourneyId
	{
		get
		{
			if (Guid.TryParse(PlayerPrefs.GetString("journey_id", ""), out var result))
			{
				return result;
			}
			Guid result2 = Guid.NewGuid();
			PlayerPrefs.SetString("journey_id", result2.ToString());
			PlayerPrefs.Save();
			return result2;
		}
		private set
		{
			PlayerPrefs.SetString("journey_id", value.ToString());
			PlayerPrefs.Save();
		}
	}

	public void EnsureJourneyId()
	{
		_ = JourneyId;
	}

	public void CreateNewJourneyId()
	{
		PlayerPrefs.DeleteKey("journey_id");
		PlayerPrefs.Save();
		EnsureJourneyId();
	}

	private StoreSource ParseSource(string query)
	{
		StoreSource result = default(StoreSource);
		string[] array = query.Split('&');
		foreach (string text in array)
		{
			if (text.StartsWith("source_area="))
			{
				result.source_area = text.Substring(12);
			}
			else if (text.StartsWith("source="))
			{
				result.source = text.Substring(7);
			}
			else if (text.StartsWith("source_id="))
			{
				result.source_id = text.Substring(10);
			}
		}
		return result;
	}
}
