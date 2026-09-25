using System;
using Facepunch;
using UnityEngine;

public class ItemSkinDirectory : ScriptableObject
{
	[Serializable]
	public struct Skin
	{
		public int id;

		public int itemid;

		public string name;

		public bool isSkin;

		private SteamInventoryItem _invItem;

		public SteamInventoryItem invItem
		{
			get
			{
				if (_invItem == null && !string.IsNullOrEmpty(name))
				{
					_invItem = FileSystem.Load<SteamInventoryItem>(name);
				}
				return _invItem;
			}
		}
	}

	private static ItemSkinDirectory _Instance;

	public Skin[] skins;

	public static ItemSkinDirectory Instance
	{
		get
		{
			if (_Instance == null)
			{
				_Instance = FileSystem.Load<ItemSkinDirectory>("assets/skins.asset");
				if (_Instance == null)
				{
					throw new Exception("Couldn't load assets/skins.asset");
				}
				if (_Instance.skins == null || _Instance.skins.Length == 0)
				{
					throw new Exception("Loaded assets/skins.asset but something is wrong");
				}
			}
			return _Instance;
		}
	}

	public static Skin[] ForItem(ItemDefinition item)
	{
		using PooledList<Skin> pooledList = Pool.Get<PooledList<Skin>>();
		for (int i = 0; i < Instance.skins.Length; i++)
		{
			Skin item2 = Instance.skins[i];
			if (item2.isSkin && item2.itemid == item.itemid)
			{
				pooledList.Add(item2);
			}
		}
		return pooledList.ToArray();
	}

	public static Skin FindByInventoryDefinitionId(int id)
	{
		for (int i = 0; i < Instance.skins.Length; i++)
		{
			Skin result = Instance.skins[i];
			if (result.id == id)
			{
				return result;
			}
		}
		return default(Skin);
	}

	public static bool TryGetItemFromDefinitionID(int id, out ItemDefinition result)
	{
		result = null;
		Skin skin = FindByInventoryDefinitionId(id);
		if (skin.invItem != null)
		{
			result = skin.invItem.itemDefinition;
			if (skin.invItem is ItemSkin itemSkin && itemSkin.Redirect != null)
			{
				result = itemSkin.Redirect;
			}
		}
		return result != null;
	}
}
