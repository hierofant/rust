using System;
using System.Collections.Generic;
using System.Threading;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;

public class DisplayingBoxStorage : BoxStorage, IPrivilegeUpdateReceiver
{
	public enum DisplayCategory
	{
		Charcoal,
		Sulfur,
		Ore,
		Stone,
		Wood,
		Metal,
		Components,
		Scrap,
		Explosives,
		Ammo,
		Clothing,
		Armour,
		Weapons,
		Tools,
		Medical,
		Food,
		Misc
	}

	[Serializable]
	public struct DisplayItemOverride
	{
		public ItemDefinition itemDef;

		public DisplayCategory category;
	}

	[Serializable]
	public struct DisplayCategoryWeight
	{
		public DisplayCategory category;

		public float weight;
	}

	public const Flags Flag_HopperAttached = Flags.Reserved10;

	[Header("WARNING: DO NOT REARRANGE, please only replace prefabs with upgraded versions if necessary, don't change the order of existing ones")]
	public GameObjectRef[] itemDisplayPrefabs;

	[Tooltip("Overrides for specific items to display in certain categories.")]
	[Header("Okay you're good now. You can rearrange these.")]
	public List<DisplayItemOverride> displayItemOverrides;

	[Tooltip("Anchors for displaying the conditional prefabs, add as many as you want.")]
	public List<Transform> displayAnchors;

	[Tooltip("Scaling to apply to displayed items, set to (1,1,1) to use prefab's original scale.")]
	public Vector3 displayScaling = Vector3.one;

	[Tooltip("Do the displayed items have a random rotation, snapped to 90 degree increments?")]
	public bool randomSpawnAngle = true;

	[Tooltip("Custom weights to determine how much of the display is allocated to each category.")]
	public List<DisplayCategoryWeight> displayCategoryCustomWeights;

	private Dictionary<ItemCategory, DisplayCategory> displayCategoryDict = new Dictionary<ItemCategory, DisplayCategory>();

	private Dictionary<ItemDefinition, DisplayCategory> itemCategoryOverrideDict = new Dictionary<ItemDefinition, DisplayCategory>();

	private Dictionary<DisplayCategory, float> displayCategoryWeightsDict = new Dictionary<DisplayCategory, float>();

	private EntityRef<BaseEntity> cachedPrivilege;

	private HashSet<BasePlayer> openAccessPlayers = new HashSet<BasePlayer>();

	private float[] cachedResourceProportions;

	private bool dirtyCache = true;

	private List<ulong> cachedAuthPlayers = new List<ulong>();

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("DisplayingBoxStorage.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void PreInitShared()
	{
		base.PreInitShared();
		displayCategoryDict = new Dictionary<ItemCategory, DisplayCategory>();
		displayCategoryDict.Add(ItemCategory.Ammunition, DisplayCategory.Ammo);
		displayCategoryDict.Add(ItemCategory.Weapon, DisplayCategory.Weapons);
		displayCategoryDict.Add(ItemCategory.Component, DisplayCategory.Components);
		displayCategoryDict.Add(ItemCategory.Medical, DisplayCategory.Medical);
		displayCategoryDict.Add(ItemCategory.Tool, DisplayCategory.Tools);
		displayCategoryDict.Add(ItemCategory.Food, DisplayCategory.Food);
		displayCategoryDict.Add(ItemCategory.Attire, DisplayCategory.Clothing);
		itemCategoryOverrideDict = new Dictionary<ItemDefinition, DisplayCategory>();
		foreach (DisplayItemOverride displayItemOverride in displayItemOverrides)
		{
			itemCategoryOverrideDict.Add(displayItemOverride.itemDef, displayItemOverride.category);
		}
		displayCategoryWeightsDict = new Dictionary<DisplayCategory, float>();
		foreach (DisplayCategoryWeight displayCategoryCustomWeight in displayCategoryCustomWeights)
		{
			displayCategoryWeightsDict.Add(displayCategoryCustomWeight.category, displayCategoryCustomWeight.weight);
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.displayingBoxStorage != null)
		{
			cachedPrivilege.uid = info.msg.displayingBoxStorage.privelegeEntityId;
		}
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		base.OnDeployed(parent, deployedBy, fromItem);
		FindPrivilege();
	}

	private void FindPrivilege()
	{
		IPrivilege privilege = GetPrivilege(useFallbackVisEntities: false);
		if (privilege != null && privilege is BaseEntity entity)
		{
			cachedPrivilege.Set(entity);
		}
	}

	public override void OnParentChanging(BaseEntity oldParent, BaseEntity newParent)
	{
		base.OnParentChanging(oldParent, newParent);
		FindPrivilege();
	}

	public override bool CanUseNetworkCache(Connection connection)
	{
		return false;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.displayingBoxStorage = Pool.Get<ProtoBuf.DisplayingBoxStorage>();
		bool flag = info.forConnection?.player != null && CanShowToPlayer(info.forConnection.player as BasePlayer);
		if ((info.forDisk || flag) && cachedPrivilege.IsSet)
		{
			info.msg.displayingBoxStorage.privelegeEntityId = cachedPrivilege.uid;
		}
		if (!info.forDisk && flag)
		{
			using (TimeWarning.New("DisplayingBoxStorage.SaveResourceProportions"))
			{
				List<float> list = Pool.Get<List<float>>();
				GetResourceProportions(list);
				info.msg.displayingBoxStorage.resources = list;
			}
		}
	}

	public override bool PlayerOpenLoot(BasePlayer player, string panelToOpen = "", bool doPositionChecks = true)
	{
		bool num = base.PlayerOpenLoot(player, panelToOpen, doPositionChecks);
		if (num && cachedPrivilege.IsSet)
		{
			BaseEntity baseEntity = cachedPrivilege.Get(base.isServer);
			if (baseEntity == null || baseEntity.IsDestroyed)
			{
				openAccessPlayers.Add(player);
			}
		}
		return num;
	}

	private bool CanShowToPlayer(BasePlayer player)
	{
		using (TimeWarning.New("DisplayingBoxStorage.CanShowToPlayer"))
		{
			BaseLock baseLock = GetSlot(Slot.Lock) as BaseLock;
			using (TimeWarning.New("DisplayingBoxStorage.CanShowToPlayer.LockCheck"))
			{
				if (baseLock != null && baseLock.IsLocked() && baseLock.GetPlayerLockPermission(player))
				{
					return true;
				}
			}
			using (TimeWarning.New("DisplayingBoxStorage.CanShowToPlayer.TCCheck"))
			{
				if (cachedPrivilege.IsValid(base.isServer))
				{
					BaseEntity baseEntity = cachedPrivilege.Get(base.isServer);
					if (baseEntity != null)
					{
						if (baseEntity.IsDestroyed || !(baseEntity is IPrivilege privilege))
						{
							return openAccessPlayers.Contains(player);
						}
						return privilege.IsAuthed(player);
					}
				}
			}
			return baseLock == null;
		}
	}

	protected override void OnInventoryDirty()
	{
		base.OnInventoryDirty();
		dirtyCache = true;
	}

	private bool GetResourceProportions(List<float> proportions)
	{
		using (TimeWarning.New("DisplayingBoxStorage.GetResourceProportions"))
		{
			if (!dirtyCache)
			{
				proportions.AddRange(cachedResourceProportions);
				return true;
			}
			if (Volatile.Read(ref cachedResourceProportions) == null)
			{
				float[] value = new float[17];
				Interlocked.CompareExchange(ref cachedResourceProportions, value, null);
			}
			Dictionary<DisplayCategory, int> categorySlotCache = Pool.Get<Dictionary<DisplayCategory, int>>();
			BuildCategorySlotCache(ref categorySlotCache);
			for (int i = 0; i < 17; i++)
			{
				DisplayCategory category = (DisplayCategory)i;
				cachedResourceProportions[i] = GetResourceCategoryProportion(ref categorySlotCache, category);
			}
			Pool.FreeUnmanaged(ref categorySlotCache);
			dirtyCache = false;
			proportions.AddRange(cachedResourceProportions);
			return false;
		}
	}

	private float GetResourceCategoryProportion(ref Dictionary<DisplayCategory, int> categorySlotCache, DisplayCategory category)
	{
		int num = 0;
		if (categorySlotCache.TryGetValue(category, out var value))
		{
			num = value;
		}
		float num2 = (float)num / (float)base.inventory.capacity;
		if (displayCategoryWeightsDict.TryGetValue(category, out var value2))
		{
			num2 *= value2;
		}
		return num2;
	}

	private void BuildCategorySlotCache(ref Dictionary<DisplayCategory, int> categorySlotCache)
	{
		categorySlotCache.Clear();
		foreach (Item item in base.inventory.itemList)
		{
			DisplayCategory categoryForItem = GetCategoryForItem(item.info);
			categorySlotCache[categoryForItem] = categorySlotCache.GetValueOrDefault(categoryForItem, 0) + 1;
		}
	}

	private DisplayCategory GetCategoryForItem(ItemDefinition item)
	{
		if (itemCategoryOverrideDict.TryGetValue(item, out var value))
		{
			return value;
		}
		if (displayCategoryDict.TryGetValue(item.category, out var value2))
		{
			return value2;
		}
		return DisplayCategory.Misc;
	}

	protected override void OnChildAdded(BaseEntity child)
	{
		base.OnChildAdded(child);
		if (base.isServer)
		{
			if (child is Hopper)
			{
				SetFlagLocal(Flags.Reserved10, b: true);
			}
			SendNetworkUpdate();
		}
	}

	protected override void OnChildRemoved(BaseEntity child)
	{
		base.OnChildAdded(child);
		if (base.isServer)
		{
			if (child is Hopper)
			{
				SetFlagLocal(Flags.Reserved10, b: false);
			}
			SendNetworkUpdate();
		}
	}

	public void OnPrivilegeUpdated(BaseEntity privilegeEntity, HashSet<ulong> authorizedPlayers)
	{
		if (!cachedPrivilege.IsSet && GetBuilding() != null)
		{
			FindPrivilege();
		}
		if (cachedPrivilege.IsSet && privilegeEntity != null && privilegeEntity.net.ID != cachedPrivilege.uid)
		{
			return;
		}
		using (TimeWarning.New("DisplayingBoxStorage.OnPrivilegeUpdated"))
		{
			HashSet<ulong> obj = Pool.Get<HashSet<ulong>>();
			foreach (ulong authorizedPlayer in authorizedPlayers)
			{
				if (!cachedAuthPlayers.Contains(authorizedPlayer))
				{
					obj.Add(authorizedPlayer);
				}
			}
			HashSet<ulong> obj2 = Pool.Get<HashSet<ulong>>();
			foreach (ulong cachedAuthPlayer in cachedAuthPlayers)
			{
				if (!authorizedPlayers.Contains(cachedAuthPlayer))
				{
					obj2.Add(cachedAuthPlayer);
				}
			}
			ProtoBuf.DisplayingBoxStorage displayingBoxStorage = null;
			foreach (ulong item in obj2)
			{
				if (displayingBoxStorage == null)
				{
					displayingBoxStorage = Pool.Get<ProtoBuf.DisplayingBoxStorage>();
				}
				ClientRPC(RpcTarget.Player("UpdateAuthState", BasePlayer.FindByID(item)), displayingBoxStorage);
			}
			Pool.FreeUnmanaged(ref obj2);
			foreach (ulong item2 in obj)
			{
				if (displayingBoxStorage == null)
				{
					displayingBoxStorage = Pool.Get<ProtoBuf.DisplayingBoxStorage>();
					List<float> list = Pool.Get<List<float>>();
					GetResourceProportions(list);
					displayingBoxStorage.resources = list;
					if (cachedPrivilege.IsSet)
					{
						displayingBoxStorage.privelegeEntityId = cachedPrivilege.uid;
					}
				}
				ClientRPC(RpcTarget.Player("UpdateAuthState", BasePlayer.FindByID(item2)), displayingBoxStorage);
			}
			Pool.FreeUnmanaged(ref obj);
			displayingBoxStorage?.Dispose();
			cachedAuthPlayers.Clear();
			cachedAuthPlayers.AddRange(authorizedPlayers);
		}
	}
}
