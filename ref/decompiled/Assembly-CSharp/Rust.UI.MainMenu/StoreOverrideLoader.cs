using System;
using System.Collections.Generic;
using Facepunch.Models;
using Newtonsoft.Json;
using UnityEngine;

namespace Rust.UI.MainMenu;

public static class StoreOverrideLoader
{
	private static readonly Dictionary<int, ItemStoreTakeover> Overrides = new Dictionary<int, ItemStoreTakeover>();

	private static readonly Dictionary<int, Manifest.StoreOverrides.StorePageOverride.ElementOverride[]> PageOverrides = new Dictionary<int, Manifest.StoreOverrides.StorePageOverride.ElementOverride[]>();

	public static void Load(TextAsset json)
	{
		Load((json != null) ? json.text : null);
	}

	public static void Load(string json)
	{
		Overrides.Clear();
		PageOverrides.Clear();
		if (string.IsNullOrEmpty(json))
		{
			return;
		}
		try
		{
			Manifest.StoreOverrides storeOverrides = JsonConvert.DeserializeObject<Manifest.StoreOverrides>(json);
			if (storeOverrides != null)
			{
				Load(storeOverrides);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to load store overrides: " + ex.Message);
		}
	}

	public static void Load(Manifest.StoreOverrides content)
	{
		Overrides.Clear();
		PageOverrides.Clear();
		if (content == null)
		{
			return;
		}
		if (content.Items != null)
		{
			foreach (Manifest.StoreOverrides.StoreEntryOverride item in content.Items)
			{
				if (item != null)
				{
					ItemStoreTakeover itemStoreTakeover = default(ItemStoreTakeover);
					itemStoreTakeover.ItemId = item.ItemId;
					itemStoreTakeover.NameOverride = ((!string.IsNullOrEmpty(item.Name)) ? item.Name : null);
					itemStoreTakeover.SubtitleOverride = ((!string.IsNullOrEmpty(item.Subtitle)) ? item.Subtitle : null);
					itemStoreTakeover.HeaderPhrase = ((!string.IsNullOrEmpty(item.Header)) ? item.Header : null);
					itemStoreTakeover.ImageURL = item.ImageUrl;
					itemStoreTakeover.VideoURL = item.VideoUrl;
					itemStoreTakeover.IconUrl = item.IconUrl;
					itemStoreTakeover.IconPortraitUrl = item.IconPortraitUrl;
					itemStoreTakeover.IconSquareUrl = item.IconSquareUrl;
					ItemStoreTakeover value = itemStoreTakeover;
					Overrides[item.ItemId] = value;
				}
			}
		}
		if (content.Pages == null)
		{
			return;
		}
		foreach (Manifest.StoreOverrides.StorePageOverride page in content.Pages)
		{
			if (page != null && page.Elements != null && page.Elements.Length != 0)
			{
				PageOverrides[page.ItemId] = page.Elements;
			}
		}
	}

	public static bool TryGetItem(int itemId, out ItemStoreTakeover takeover)
	{
		return Overrides.TryGetValue(itemId, out takeover);
	}

	public static bool TryGetPageElements(int itemId, out Manifest.StoreOverrides.StorePageOverride.ElementOverride[] elements)
	{
		return PageOverrides.TryGetValue(itemId, out elements);
	}

	public static bool Validate(string json, out string error)
	{
		if (string.IsNullOrEmpty(json))
		{
			error = "JSON is null or empty";
			return false;
		}
		Manifest.StoreOverrides storeOverrides;
		try
		{
			storeOverrides = JsonConvert.DeserializeObject<Manifest.StoreOverrides>(json);
		}
		catch (Exception ex)
		{
			error = "JSON deserialization failed: " + ex.Message;
			return false;
		}
		if (storeOverrides == null)
		{
			error = "JSON deserialized to null";
			return false;
		}
		if (storeOverrides.Items != null)
		{
			for (int i = 0; i < storeOverrides.Items.Count; i++)
			{
				Manifest.StoreOverrides.StoreEntryOverride storeEntryOverride = storeOverrides.Items[i];
				if (storeEntryOverride == null)
				{
					error = $"Items[{i}] is null";
					return false;
				}
				if (storeEntryOverride.ItemId == 0)
				{
					error = $"Items[{i}] has no ItemId";
					return false;
				}
			}
		}
		if (storeOverrides.Pages != null)
		{
			for (int j = 0; j < storeOverrides.Pages.Count; j++)
			{
				Manifest.StoreOverrides.StorePageOverride storePageOverride = storeOverrides.Pages[j];
				if (storePageOverride == null)
				{
					error = $"Pages[{j}] is null";
					return false;
				}
				if (storePageOverride.ItemId == 0)
				{
					error = $"Pages[{j}] has no ItemId";
					return false;
				}
				if (storePageOverride.Elements == null)
				{
					continue;
				}
				for (int k = 0; k < storePageOverride.Elements.Length; k++)
				{
					if (storePageOverride.Elements[k] == null)
					{
						error = $"Pages[{j}].Elements[{k}] is null";
						return false;
					}
				}
			}
		}
		error = null;
		return true;
	}
}
