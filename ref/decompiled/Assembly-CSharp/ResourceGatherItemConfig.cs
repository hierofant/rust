using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Resource Gather Item Config")]
public class ResourceGatherItemConfig : BaseScriptableObject
{
	[Serializable]
	public class ResourceGatherItemData
	{
		public ResourceDispenser.GatherType resourceType;

		public ItemData[] hotspotBypassItems;
	}

	[Serializable]
	public class ItemData
	{
		public ItemDefinition item;

		public float hotspotGatherBonusScale = 1f;
	}

	private static ResourceGatherItemConfig _instance;

	public ResourceGatherItemData[] itemDataEntries;

	private Dictionary<ResourceDispenser.GatherType, Dictionary<ItemDefinition, ItemData>> _hotspotBypassItemsLookup;

	public static ResourceGatherItemConfig instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = FileSystem.Load<ResourceGatherItemConfig>("assets/prefabs/resource/resourcegatheritemconfig.asset");
			}
			if (_instance == null)
			{
				Debug.LogError("Failed to load ResourceGatherItemConfig");
			}
			return _instance;
		}
	}

	private Dictionary<ResourceDispenser.GatherType, Dictionary<ItemDefinition, ItemData>> hotspotBypassItemsLookup
	{
		get
		{
			if (_hotspotBypassItemsLookup == null)
			{
				FillHotspotBypassItemsLookup();
			}
			return _hotspotBypassItemsLookup;
		}
	}

	public bool CanItemBypassHotspotGathering(ResourceDispenser.GatherType resourceType, ItemDefinition item, out ItemData itemData)
	{
		itemData = null;
		if (item == null)
		{
			return false;
		}
		if (hotspotBypassItemsLookup.TryGetValue(resourceType, out var value) && value.TryGetValue(item, out itemData))
		{
			return true;
		}
		return false;
	}

	private void FillHotspotBypassItemsLookup()
	{
		_hotspotBypassItemsLookup = new Dictionary<ResourceDispenser.GatherType, Dictionary<ItemDefinition, ItemData>>();
		ResourceGatherItemData[] array = itemDataEntries;
		foreach (ResourceGatherItemData resourceGatherItemData in array)
		{
			if (_hotspotBypassItemsLookup.ContainsKey(resourceGatherItemData.resourceType))
			{
				continue;
			}
			Dictionary<ItemDefinition, ItemData> dictionary = new Dictionary<ItemDefinition, ItemData>();
			ItemData[] hotspotBypassItems = resourceGatherItemData.hotspotBypassItems;
			foreach (ItemData itemData in hotspotBypassItems)
			{
				dictionary.Add(itemData.item, itemData);
				if (ItemManager.redirectPerItem.TryGetValue(itemData.item, out var value))
				{
					for (int k = 0; k < value.Count; k++)
					{
						ItemDefinition itemDefinition = value[k];
						ItemData value2 = new ItemData
						{
							item = itemDefinition,
							hotspotGatherBonusScale = itemData.hotspotGatherBonusScale
						};
						dictionary.Add(itemDefinition, value2);
					}
				}
			}
			_hotspotBypassItemsLookup.Add(resourceGatherItemData.resourceType, dictionary);
		}
	}
}
