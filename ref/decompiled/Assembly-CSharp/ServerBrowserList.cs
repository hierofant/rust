using System;
using Facepunch;
using UnityEngine;

public class ServerBrowserList : ServerBrowserListBase, VirtualScroll.IDataSource, VirtualScroll.IVisualUpdate
{
	public enum QueryType
	{
		RegularInternet,
		Friends,
		History,
		LAN,
		Favourites,
		None
	}

	[Serializable]
	public struct ServerKeyvalues
	{
		public string key;

		public string value;
	}

	[Serializable]
	public struct Rules
	{
		public string tag;

		public ServerBrowserList serverList;

		public bool keepInList;

		public string CompressedTag { get; set; }
	}

	public QueryType queryType;

	public static string VersionTag = "v" + 2633;

	public ServerKeyvalues[] keyValues = new ServerKeyvalues[0];

	public bool legacyUIDisplay = true;

	public bool UseOfficialServers;

	public VirtualScroll VirtualScroll;

	public FlexVirtualScroll VirtualScrollFlex;

	public bool prioritizePremiumServers;

	public bool prioritizeSecureServers;

	public Rules[] rules;

	public bool hideOfficialServers;

	public bool excludeEmptyServersUsingQuery;

	public bool alwaysIncludeEmptyServers;

	public bool clampPlayerCountsToTrustedValues = true;

	public bool replacePingWithTimeSinceLastPlayed;

	private static string[] pingStrings = new string[3] { ".", "..", "..." };

	public void OnVisualUpdate(int i, GameObject obj)
	{
	}

	public int GetItemCount()
	{
		return 0;
	}

	public float GetItemSize(int i)
	{
		return 0f;
	}

	public void SetItemData(int i, GameObject obj)
	{
	}

	private void VisualUpdate(int i, GameObject obj)
	{
	}

	private void VisualUpdateLegacy(int i, GameObject obj)
	{
	}
}
