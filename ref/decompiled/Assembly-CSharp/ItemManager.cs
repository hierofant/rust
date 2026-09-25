#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Facepunch;
using ProtoBuf;
using Rust;
using UnityEngine;

public class ItemManager
{
	public class ItemLookup
	{
		public ItemDefinition Scrap = FindItemDefinition("scrap");

		public ItemDefinition Hoodie = FindItemDefinition("hoodie");

		public ItemDefinition Pants = FindItemDefinition("pants");

		public ItemDefinition HazmatSuit = FindItemDefinition("hazmatsuit");

		public ItemDefinition Rock = FindItemDefinition("rock");

		public ItemDefinition MasterKey = FindItemDefinition("apartment.master_key");

		public ItemDefinition Fertilizer = FindItemDefinition("fertilizer");
	}

	private struct ItemRemove
	{
		public Item item;

		public float time;
	}

	[ServerVar(Help = "(Generated) When enabled, ItemManager uses object pooling for item instances to reduce GC allocations from frequent item creation and destruction")]
	public static bool EnablePooling = true;

	public static List<ItemDefinition> itemList;

	public static Dictionary<int, ItemDefinition> itemDictionary;

	public static Dictionary<string, ItemDefinition> itemDictionaryByName;

	public static List<ItemBlueprint> bpList;

	public static int[] defaultBlueprints;

	public static ItemDefinition blueprintBaseDef;

	public static Dictionary<ItemDefinition, ItemBlueprint> itemToBlueprint;

	public static Dictionary<ItemDefinition, List<ItemDefinition>> redirectPerItem;

	public static Dictionary<ItemDefinition, List<ItemBlueprint>> ingredientToBlueprints;

	private static List<ItemRemove> ItemRemoves = new List<ItemRemove>();

	public static ItemLookup Items { get; private set; }

	public static void InvalidateWorkshopSkinCache()
	{
		if (itemList == null)
		{
			return;
		}
		foreach (ItemDefinition item in itemList)
		{
			item.InvalidateWorkshopSkinCache();
		}
	}

	public static void Initialize()
	{
		if (itemList != null)
		{
			return;
		}
		itemToBlueprint = new Dictionary<ItemDefinition, ItemBlueprint>();
		redirectPerItem = new Dictionary<ItemDefinition, List<ItemDefinition>>();
		ingredientToBlueprints = new Dictionary<ItemDefinition, List<ItemBlueprint>>();
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		GameObject[] array = FileSystem.LoadAllFromBundle<GameObject>("items.preload.bundle", "l:ItemDefinition");
		if (array.Length == 0)
		{
			throw new Exception("items.preload.bundle has no items!");
		}
		if (stopwatch.Elapsed.TotalSeconds > 1.0)
		{
			UnityEngine.Debug.Log("Loading Items Took: " + stopwatch.Elapsed.TotalMilliseconds / 1000.0 + " seconds");
		}
		List<ItemDefinition> list = (from x in array
			select x.GetComponent<ItemDefinition>() into x
			where x != null
			select x).ToList();
		List<ItemBlueprint> list2 = (from x in array
			select x.GetComponent<ItemBlueprint>() into x
			where x != null && x.userCraftable
			select x).ToList();
		Dictionary<int, ItemDefinition> dictionary = new Dictionary<int, ItemDefinition>();
		Dictionary<string, ItemDefinition> dictionary2 = new Dictionary<string, ItemDefinition>(StringComparer.OrdinalIgnoreCase);
		for (int i = 0; i < list.Count; i++)
		{
			ItemDefinition itemDefinition = list[i];
			if (dictionary.ContainsKey(itemDefinition.itemid))
			{
				ItemDefinition itemDefinition2 = dictionary[itemDefinition.itemid];
				UnityEngine.Debug.LogWarning("Item ID duplicate " + itemDefinition.itemid + " (" + itemDefinition.name + ") - have you given your items unique shortnames?", itemDefinition.gameObject);
				UnityEngine.Debug.LogWarning("Other item is " + itemDefinition2.name, itemDefinition2);
				list.RemoveAt(i);
				i--;
			}
			else
			{
				dictionary.Add(itemDefinition.itemid, itemDefinition);
				dictionary2.Add(itemDefinition.shortname, itemDefinition);
			}
		}
		itemList = list;
		itemDictionary = dictionary;
		itemDictionaryByName = dictionary2;
		Items = new ItemLookup();
		foreach (ItemDefinition item in list)
		{
			item.Initialize(list);
			if (string.IsNullOrEmpty(item.shortname))
			{
				UnityEngine.Debug.LogWarning($"{item} has a null short name! id: {item.itemid} {item.displayName.english}");
				continue;
			}
			ItemBlueprint component = item.GetComponent<ItemBlueprint>();
			if (component != null)
			{
				itemToBlueprint.Add(item, component);
			}
		}
		stopwatch.Stop();
		if (stopwatch.Elapsed.TotalSeconds > 1.0)
		{
			UnityEngine.Debug.Log("Building Items Took: " + stopwatch.Elapsed.TotalMilliseconds / 1000.0 + " seconds / Items: " + list.Count + " / Blueprints: " + list2.Count);
		}
		defaultBlueprints = (from x in list2
			where !x.NeedsSteamItem && !x.NeedsSteamDLC && x.defaultBlueprint
			select x.targetItem.itemid).ToArray();
		itemList = list;
		bpList = list2;
		itemDictionary = dictionary;
		itemDictionaryByName = dictionary2;
		blueprintBaseDef = FindItemDefinition("blueprintbase");
		foreach (ItemDefinition item2 in itemList)
		{
			if (item2 != null && item2.isRedirectOf != null)
			{
				if (!redirectPerItem.TryGetValue(item2.isRedirectOf, out var value))
				{
					value = new List<ItemDefinition>();
					redirectPerItem[item2.isRedirectOf] = value;
				}
				value.Add(item2);
			}
		}
		foreach (ItemBlueprint bp in bpList)
		{
			foreach (ItemAmount ingredient in bp.ingredients)
			{
				if (!ingredientToBlueprints.TryGetValue(ingredient.itemDef, out var value2))
				{
					value2 = new List<ItemBlueprint>();
					ingredientToBlueprints[ingredient.itemDef] = value2;
				}
				value2.Add(bp);
			}
		}
		CalculateApartmentItemTaxes();
	}

	private static void CalculateApartmentItemTaxes()
	{
		Dictionary<ItemDefinition, int> dictionary = new Dictionary<ItemDefinition, int>();
		ListHashSet<ItemDefinition> listHashSet = new ListHashSet<ItemDefinition>();
		ListHashSet<ItemDefinition> listHashSet2 = new ListHashSet<ItemDefinition>();
		foreach (ItemDefinition item in itemList)
		{
			dictionary[item] = 0;
			listHashSet.Add(item);
		}
		for (int i = 0; i < 100; i++)
		{
			if (listHashSet.Count == 0)
			{
				break;
			}
			foreach (ItemDefinition item2 in listHashSet)
			{
				if (!ingredientToBlueprints.TryGetValue(item2, out var value))
				{
					continue;
				}
				foreach (ItemBlueprint item3 in value)
				{
					dictionary[item3.targetItem] = i + 1;
					listHashSet2.TryAdd(item3.targetItem);
				}
			}
			ListHashSet<ItemDefinition> listHashSet3 = listHashSet;
			listHashSet = listHashSet2;
			listHashSet2 = listHashSet3;
			listHashSet2.Clear();
		}
		IGrouping<int, KeyValuePair<ItemDefinition, int>>[] array = (from x in dictionary
			group x by x.Value into x
			orderby x.Key
			select x).ToArray();
		for (int j = 0; j < array.Length; j++)
		{
			foreach (KeyValuePair<ItemDefinition, int> item4 in array[j])
			{
				ItemDefinition key = item4.Key;
				ItemBlueprint blueprint = key.Blueprint;
				ItemModApartmentTax component = key.GetComponent<ItemModApartmentTax>();
				if (component != null && component.ScrapPerStack != 0f)
				{
					key.ApartmentTaxPerStack = component.ScrapPerStack;
				}
				else
				{
					if (blueprint == null)
					{
						continue;
					}
					float num = 0f;
					foreach (ItemAmount ingredient in blueprint.ingredients)
					{
						float num2 = (float)(blueprint.targetItem.stackable / blueprint.amountToCreate) * ingredient.amount / (float)ingredient.itemDef.stackable;
						num += ingredient.itemDef.ApartmentTaxPerStack * num2;
					}
					blueprint.targetItem.ApartmentTaxPerStack = num;
				}
			}
		}
		foreach (ItemDefinition item5 in itemList)
		{
			if (!(item5.ApartmentTaxPerStack > 0f))
			{
				Rarity rarity2 = ((item5.despawnRarity > item5.rarity) ? item5.despawnRarity : item5.rarity);
				item5.ApartmentTaxPerStack = GetRarityTaxPerStack(rarity2);
			}
		}
		static float GetRarityTaxPerStack(Rarity rarity)
		{
			return rarity switch
			{
				Rarity.Uncommon => 3f, 
				Rarity.Rare => 8f, 
				Rarity.VeryRare => 15f, 
				_ => 0f, 
			};
		}
	}

	public static Item CreateByName(string strName, int iAmount = 1, ulong skin = 0uL)
	{
		ItemDefinition itemDefinition = itemList.Find((ItemDefinition x) => x.shortname == strName);
		if (itemDefinition == null)
		{
			return null;
		}
		return CreateByItemID(itemDefinition.itemid, iAmount, skin, 0uL);
	}

	public static Item CreateByPartialName(string strName, int iAmount = 1, ulong skin = 0uL)
	{
		ItemDefinition itemDefinition = FindDefinitionByPartialName(strName);
		if (itemDefinition == null)
		{
			return null;
		}
		return CreateByItemID(itemDefinition.itemid, iAmount, skin, 0uL);
	}

	public static ItemDefinition FindDefinitionByPartialName(string strName)
	{
		ItemDefinition itemDefinition = itemList.Find((ItemDefinition x) => x.shortname == strName);
		if (itemDefinition == null)
		{
			itemDefinition = itemList.Find((ItemDefinition x) => x.shortname.Contains(strName, CompareOptions.IgnoreCase));
		}
		return itemDefinition;
	}

	public static Item CreateByItemID(int itemID, int iAmount = 1, ulong skin = 0uL, ulong attachment = 0uL)
	{
		ItemDefinition itemDefinition = FindItemDefinition(itemID);
		if (itemDefinition == null)
		{
			return null;
		}
		return Create(itemDefinition, iAmount, skin, isServerSide: true, attachment);
	}

	public static Item Create(ItemDefinition template, int iAmount = 1, ulong skin = 0uL, bool isServerSide = true, ulong attachment = 0uL)
	{
		UnityEngine.Debug.Assert(isServerSide, "Tried to create client item on server!");
		TrySkinChangeItem(ref template, ref skin);
		if (template == null)
		{
			UnityEngine.Debug.LogWarning("Creating invalid/missing item!");
			return null;
		}
		if (iAmount <= 0)
		{
			UnityEngine.Debug.LogError("Creating item with less than 1 amount! (" + template.displayName.english + ")");
			return null;
		}
		Item item = ((EnablePooling && isServerSide) ? Pool.Get<Item>() : new Item());
		item.isServer = isServerSide;
		item.info = template;
		item.amount = iAmount;
		item.skin = skin;
		item.attachment = attachment;
		item.Initialize(template);
		RustLog.Log(RustLog.EntryType.Item, 1, null, "Created <color={0}>{1}</color>", item.isServer ? "yellow" : "cyan", item);
		return item;
	}

	private static void TrySkinChangeItem(ref ItemDefinition template, ref ulong skinId)
	{
		if (skinId == 0L)
		{
			return;
		}
		ItemSkinDirectory.Skin skin = ItemSkinDirectory.FindByInventoryDefinitionId((int)skinId);
		if (skin.id != 0)
		{
			ItemSkin itemSkin = skin.invItem as ItemSkin;
			if (!(itemSkin == null) && !(itemSkin.Redirect == null))
			{
				template = itemSkin.Redirect;
				skinId = 0uL;
			}
		}
	}

	public static Item Load(ProtoBuf.Item load, Item created, bool isServer)
	{
		if (created == null)
		{
			created = ((EnablePooling && isServer) ? Pool.Get<Item>() : new Item());
		}
		created.isServer = isServer;
		created.Load(load);
		if (created.info == null)
		{
			UnityEngine.Debug.LogWarning("Item loading failed - item is invalid");
			return null;
		}
		if (created.info == blueprintBaseDef && created.blueprintTargetDef == null)
		{
			UnityEngine.Debug.LogWarning("Blueprint item loading failed - invalid item target");
			return null;
		}
		RustLog.Log(RustLog.EntryType.Item, 1, null, "Loaded <color={0}>{1}</color>", created.isServer ? "yellow" : "cyan", created);
		return created;
	}

	public static ItemDefinition FindItemDefinition(int itemID)
	{
		Initialize();
		return itemDictionary.GetValueOrDefault(itemID, null);
	}

	public static ItemDefinition FindItemDefinition(string shortName)
	{
		Initialize();
		return itemDictionaryByName.GetValueOrDefault(shortName, null);
	}

	public static ItemBlueprint FindBlueprint(ItemDefinition item)
	{
		Initialize();
		return itemToBlueprint.GetValueOrDefault(item, null);
	}

	public static List<ItemDefinition> GetItemDefinitions()
	{
		Initialize();
		return itemList;
	}

	public static List<ItemBlueprint> GetBlueprints()
	{
		Initialize();
		return bpList;
	}

	public static void DoRemoves(bool force = false)
	{
		using (TimeWarning.New("DoRemoves"))
		{
			float time = Time.time;
			for (int i = 0; i < ItemRemoves.Count; i++)
			{
				if (force || !(ItemRemoves[i].time > time))
				{
					Item obj = ItemRemoves[i].item;
					ItemRemoves.RemoveAt(i--);
					RustLog.Log(RustLog.EntryType.Item, 1, null, "Removing <color={0}>{1}</color>", obj.isServer ? "yellow" : "cyan", obj);
					obj.DoRemove();
					if (EnablePooling)
					{
						Pool.Free(ref obj);
					}
				}
			}
		}
	}

	public static void Heartbeat()
	{
		DoRemoves();
	}

	public static void RemoveItem(Item item, float fTime = 0f)
	{
		RustLog.Log(RustLog.EntryType.Item, 2, null, "Scheduled removal of <color={0}>{1}</color>", item.isServer ? "yellow" : "cyan", item);
		ItemRemove item2 = default(ItemRemove);
		item2.item = item;
		item2.time = Time.time + fTime;
		ItemRemoves.Add(item2);
	}

	public static IEnumerable<Item> GetAllItems()
	{
		Queue<Item> buffer = new Queue<Item>();
		HashSet<Item> bufferHash = new HashSet<Item>();
		foreach (Item item in GetAllItemsInternal())
		{
			if (item == null)
			{
				continue;
			}
			yield return item;
			if (item.contents == null)
			{
				continue;
			}
			bufferHash.Clear();
			buffer.Enqueue(item);
			Item result;
			while (buffer.TryDequeue(out result))
			{
				if (result.contents?.itemList == null)
				{
					continue;
				}
				foreach (Item child in result.contents.itemList)
				{
					yield return child;
					if (bufferHash.Add(child))
					{
						buffer.Enqueue(child);
					}
				}
			}
		}
	}

	private static IEnumerable<Item> GetAllItemsInternal()
	{
		List<ItemContainer> buffer = new List<ItemContainer>();
		foreach (BaseNetworkable serverEntity in BaseNetworkable.serverEntities)
		{
			if (serverEntity is IInventoryProvider inventoryProvider)
			{
				buffer.Clear();
				inventoryProvider.GetAllInventories(buffer);
				foreach (ItemContainer item in buffer)
				{
					foreach (Item item2 in item.itemList)
					{
						yield return item2;
					}
				}
			}
			else if (serverEntity is DroppedItem droppedItem)
			{
				yield return droppedItem.item;
			}
		}
	}
}
