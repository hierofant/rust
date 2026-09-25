#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Linq;
using ConVar;
using Development.Attributes;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public sealed class ItemContainer : IAmmoContainer, Facepunch.Pool.IPooled
{
	[Flags]
	public enum Flag
	{
		IsPlayer = 1,
		Clothing = 2,
		Belt = 4,
		SingleType = 8,
		IsLocked = 0x10,
		ShowSlotsOnIcon = 0x20,
		NoBrokenItems = 0x40,
		NoItemInput = 0x80,
		ContentsHidden = 0x100,
		IsArmor = 0x200,
		DroppedItemContainer = 0x400
	}

	[Flags]
	public enum ContentsType
	{
		Generic = 1,
		Liquid = 2
	}

	public enum LimitStack
	{
		None,
		Existing,
		All
	}

	public enum CanAcceptResult
	{
		CanAccept,
		CannotAccept,
		CannotAcceptRightNow
	}

	public const int BackpackSlotIndex = 7;

	public Flag flags;

	public ContentsType allowedContents;

	public ItemDefinition[] onlyAllowedItems;

	public HashSet<ItemDefinition> blockedItems;

	public List<ItemSlot> availableSlots = new List<ItemSlot>();

	public int capacity = 2;

	public ItemContainerId uid;

	public bool dirty;

	public List<Item> itemList = new List<Item>();

	public float temperature = 15f;

	public Item parent;

	public BasePlayer playerOwner;

	public BaseEntity entityOwner;

	public bool isServer;

	public int maxStackSize;

	public bool allowItemsToIncreaseToMaxStackSize;

	public int containerVolume;

	public Func<BasePlayer, Item, int, bool> canAcceptItem;

	public Func<Item, int, bool> slotIsReserved;

	public Action<Item, bool> onItemAddedRemoved;

	public Action<Item, int, int> onItemPositionChanged;

	public Action<Item, bool> onItemContentsChanged;

	public Action<Item, int> onItemAddedToStack;

	public Action<Item, int> onItemRemovedFromStack;

	public Action<Item> onPreItemRemove;

	public Action<Item, float> onItemRadiationChanged;

	public Action<Item, Item> onItemParentChanged;

	private ListHashSet<Item> itemsWithOnCycle;

	public bool HasAvailableSlotsDefined => !CollectionEx.IsEmpty(availableSlots);

	public bool HasLimitedAllowedItems
	{
		get
		{
			if (onlyAllowedItems != null)
			{
				return onlyAllowedItems.Length != 0;
			}
			return false;
		}
	}

	public Vector3 dropPosition
	{
		get
		{
			if ((bool)playerOwner)
			{
				return playerOwner.GetDropPosition();
			}
			if ((bool)entityOwner)
			{
				return entityOwner.GetDropPosition();
			}
			if (parent != null)
			{
				BaseEntity worldEntity = parent.GetWorldEntity();
				if (worldEntity != null)
				{
					return worldEntity.GetDropPosition();
				}
			}
			Debug.LogWarning("ItemContainer.dropPosition dropped through");
			return Vector3.zero;
		}
	}

	public Vector3 dropVelocity
	{
		get
		{
			if ((bool)playerOwner)
			{
				return playerOwner.GetDropVelocity();
			}
			if ((bool)entityOwner)
			{
				return entityOwner.GetDropVelocity();
			}
			if (parent != null)
			{
				BaseEntity worldEntity = parent.GetWorldEntity();
				if (worldEntity != null)
				{
					return worldEntity.GetDropVelocity();
				}
			}
			Debug.LogWarning("ItemContainer.dropVelocity dropped through");
			return Vector3.zero;
		}
	}

	public int OnCycleCount => itemsWithOnCycle?.Count ?? 0;

	public event Action onDirty;

	public void UpdateAvailableSlots(List<ItemSlot> newSlots)
	{
		availableSlots.Clear();
		if (newSlots != null)
		{
			availableSlots.AddRange(newSlots);
		}
	}

	public bool HasFlag(Flag f)
	{
		return (flags & f) == f;
	}

	public void SetFlag(Flag f, bool b)
	{
		if (b)
		{
			flags |= f;
		}
		else
		{
			flags &= ~f;
		}
	}

	public bool IsLocked()
	{
		return HasFlag(Flag.IsLocked);
	}

	public bool PlayerItemInputBlocked()
	{
		if (HasFlag(Flag.NoItemInput))
		{
			return !ConVar.Server.bypassiteminputrestriction;
		}
		return false;
	}

	void Facepunch.Pool.IPooled.EnterPool()
	{
		flags = (Flag)0;
		allowedContents = (ContentsType)0;
		onlyAllowedItems = null;
		blockedItems = null;
		availableSlots.Clear();
		capacity = 2;
		if (uid.IsValid && Network.Net.sv != null)
		{
			Network.Net.sv.ReturnUID(uid.Value);
		}
		uid = default(ItemContainerId);
		temperature = 15f;
		parent = null;
		playerOwner = null;
		entityOwner = null;
		isServer = false;
		maxStackSize = 0;
		containerVolume = 0;
		this.onDirty = null;
		canAcceptItem = null;
		slotIsReserved = null;
		onItemAddedRemoved = null;
		onItemContentsChanged = null;
		onItemAddedToStack = null;
		onItemRemovedFromStack = null;
		onPreItemRemove = null;
		onItemRadiationChanged = null;
		onItemParentChanged = null;
		Clear();
		dirty = false;
	}

	void Facepunch.Pool.IPooled.LeavePool()
	{
	}

	public float GetTemperature(int slot)
	{
		if (entityOwner is BaseOven baseOven)
		{
			return baseOven.GetTemperature(slot);
		}
		return temperature;
	}

	public void ServerInitialize(Item parentItem, int iMaxCapacity)
	{
		parent = parentItem;
		capacity = iMaxCapacity;
		uid = default(ItemContainerId);
		isServer = true;
		if (allowedContents == (ContentsType)0)
		{
			allowedContents = ContentsType.Generic;
		}
		onItemRadiationChanged = (Action<Item, float>)Delegate.Combine(onItemRadiationChanged, new Action<Item, float>(BubbleUpRadiationChanged));
		MarkDirty();
	}

	public void GiveUID()
	{
		Assert.IsTrue(!uid.IsValid, "Calling GiveUID - but already has a uid!");
		uid = new ItemContainerId(Network.Net.sv.TakeUID());
	}

	public void MarkDirty()
	{
		dirty = true;
		parent?.MarkDirty();
		this.onDirty?.Invoke();
	}

	public DroppedItemContainer Drop(string prefab, Vector3 pos, Quaternion rot, float destroyPercent)
	{
		if (itemList == null || itemList.Count == 0)
		{
			return null;
		}
		BaseEntity baseEntity = GameManager.server.CreateEntity(prefab, pos, rot);
		if (baseEntity == null)
		{
			return null;
		}
		DroppedItemContainer droppedItemContainer = baseEntity as DroppedItemContainer;
		if (droppedItemContainer != null)
		{
			droppedItemContainer.TakeFrom(new ItemContainer[1] { this }, destroyPercent);
		}
		droppedItemContainer.Spawn();
		return droppedItemContainer;
	}

	public static DroppedItemContainer Drop(string prefab, Vector3 pos, Quaternion rot, params ItemContainer[] containers)
	{
		int num = 0;
		foreach (ItemContainer itemContainer in containers)
		{
			num += ((itemContainer.itemList != null) ? itemContainer.itemList.Count : 0);
		}
		if (num == 0)
		{
			return null;
		}
		BaseEntity baseEntity = GameManager.server.CreateEntity(prefab, pos, rot);
		if (baseEntity == null)
		{
			return null;
		}
		DroppedItemContainer droppedItemContainer = baseEntity as DroppedItemContainer;
		if (droppedItemContainer != null)
		{
			droppedItemContainer.TakeFrom(containers, 0f);
		}
		droppedItemContainer.Spawn();
		return droppedItemContainer;
	}

	public BaseEntity GetEntityOwner(bool returnHeldEntity = false)
	{
		ItemContainer itemContainer = this;
		for (int i = 0; i < 10; i++)
		{
			if (itemContainer.entityOwner != null)
			{
				return itemContainer.entityOwner;
			}
			if (itemContainer.playerOwner != null)
			{
				return itemContainer.playerOwner;
			}
			if (returnHeldEntity)
			{
				BaseEntity baseEntity = itemContainer.parent?.GetHeldEntity();
				if (baseEntity != null)
				{
					return baseEntity;
				}
			}
			ItemContainer itemContainer2 = itemContainer.parent?.parent;
			if (itemContainer2 == null || itemContainer2 == itemContainer)
			{
				return null;
			}
			itemContainer = itemContainer2;
		}
		return null;
	}

	public void OnChanged()
	{
		for (int i = 0; i < itemList.Count; i++)
		{
			itemList[i].OnChanged();
		}
	}

	public Item FindItemByUID(ItemId iUID)
	{
		for (int i = 0; i < itemList.Count; i++)
		{
			Item item = itemList[i];
			if (item.IsValid())
			{
				Item item2 = item.FindItem(iUID);
				if (item2 != null)
				{
					return item2;
				}
			}
		}
		return null;
	}

	public bool IsFull(bool checkForPartialStacks = false)
	{
		if (!checkForPartialStacks)
		{
			return itemList.Count >= capacity;
		}
		if (itemList.Count < capacity)
		{
			return false;
		}
		foreach (Item item in itemList)
		{
			if (item.IsValid() && item.amount < item.MaxStackable())
			{
				return false;
			}
		}
		return true;
	}

	public bool HasSpaceFor(Item item)
	{
		if (!IsFull())
		{
			return true;
		}
		return HasPartialStack(item);
	}

	public bool IsEmpty()
	{
		return itemList.Count == 0;
	}

	public bool HasPartialStack(Item toStack, out int slot)
	{
		slot = -1;
		foreach (Item item in itemList)
		{
			if (item.info.itemid == toStack.info.itemid && item.amount < item.MaxStackable() && toStack.CanStack(item))
			{
				slot = item.position;
				return true;
			}
		}
		return false;
	}

	public float GetRadioactiveMaterialInContainer()
	{
		float num = 0f;
		foreach (Item item in itemList)
		{
			num += item.radioactivity;
		}
		return num;
	}

	public bool HasPartialStack(Item toStack)
	{
		int slot;
		return HasPartialStack(toStack, out slot);
	}

	public bool CanAccept(Item item)
	{
		if (IsFull())
		{
			return false;
		}
		return true;
	}

	public int GetMaxTransferAmount(ItemDefinition def)
	{
		int num = ContainerMaxStackSize();
		foreach (Item item in itemList)
		{
			if (item.info == def)
			{
				num -= item.amount;
				if (num <= 0)
				{
					return 0;
				}
			}
		}
		return num;
	}

	public void SetOnlyAllowedItem(ItemDefinition def)
	{
		SetOnlyAllowedItems(def);
	}

	public void SetOnlyAllowedItems(ItemDefinition[] baseItems, params ItemDefinition[] additionalItems)
	{
		onlyAllowedItems = (from item in baseItems.Concat(additionalItems)
			where item != null
			select item).ToArray();
	}

	public void SetOnlyAllowedItems(params ItemDefinition[] defs)
	{
		int num = 0;
		ItemDefinition[] array = defs;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != null)
			{
				num++;
			}
		}
		onlyAllowedItems = new ItemDefinition[num];
		int num2 = 0;
		array = defs;
		foreach (ItemDefinition itemDefinition in array)
		{
			if (itemDefinition != null)
			{
				onlyAllowedItems[num2] = itemDefinition;
				num2++;
			}
		}
	}

	public void SetBlacklist(ItemDefinition[] defs)
	{
		if (defs != null && defs.Length != 0)
		{
			blockedItems = new HashSet<ItemDefinition>(defs);
		}
	}

	public bool Insert(Item item)
	{
		if (itemList.Contains(item))
		{
			return false;
		}
		if (IsFull())
		{
			return false;
		}
		itemList.Add(item);
		item.parent = this;
		if (!FindPosition(item))
		{
			return false;
		}
		MarkDirty();
		if (onItemAddedRemoved != null)
		{
			onItemAddedRemoved(item, arg2: true);
		}
		if (item.HasOnCycle)
		{
			if (itemsWithOnCycle == null)
			{
				itemsWithOnCycle = Facepunch.Pool.Get<ListHashSet<Item>>();
			}
			itemsWithOnCycle.Add(item);
		}
		ItemContainer itemContainer = parent?.parent;
		if (itemContainer != null && itemContainer.onItemContentsChanged != null)
		{
			itemContainer.onItemContentsChanged(item, arg2: true);
		}
		Interface.CallHook("OnItemAddedToContainer", this, item);
		return true;
	}

	public bool SlotTaken(Item item, int i)
	{
		if (slotIsReserved != null && slotIsReserved(item, i))
		{
			return true;
		}
		return GetSlot(i) != null;
	}

	public Item GetSlot(int slot)
	{
		if (slot == -1)
		{
			return null;
		}
		_ = itemList.Count;
		foreach (Item item in itemList)
		{
			if (item.position == slot)
			{
				return item;
			}
		}
		return null;
	}

	public bool TryGetItemAtSlot(int slot, out Item item)
	{
		item = GetSlot(slot);
		return item != null;
	}

	public bool QuickIndustrialPreCheck(Item toTransfer, Vector2i range, int fakeSlots, out int foundSlot)
	{
		int num = range.y - range.x + 1;
		int count = itemList.Count;
		int num2 = 0;
		foundSlot = -1;
		for (int i = 0; i < count; i++)
		{
			Item item = itemList[i];
			int position = item.position;
			if (position < range.x || position > range.y)
			{
				continue;
			}
			num2++;
			if (item.amount >= item.info.stackable || item.IsRemoved())
			{
				continue;
			}
			if (toTransfer.IsBlueprint())
			{
				if (item.blueprintTarget != toTransfer.blueprintTarget)
				{
					continue;
				}
			}
			else if (item.info != toTransfer.info)
			{
				continue;
			}
			if (item.instanceData == null || item.CanStack(toTransfer))
			{
				foundSlot = position;
				return true;
			}
		}
		return num2 + fakeSlots < num;
	}

	public bool FindPosition(Item item)
	{
		int position = item.position;
		item.position = -1;
		if (position >= 0 && !SlotTaken(item, position))
		{
			item.position = position;
			return true;
		}
		for (int i = 0; i < capacity; i++)
		{
			if (!SlotTaken(item, i))
			{
				item.position = i;
				return true;
			}
		}
		return false;
	}

	public bool HasItem(ItemDefinition searchFor)
	{
		if (searchFor == null)
		{
			return false;
		}
		foreach (Item item in itemList)
		{
			if (item != null && item.info == searchFor)
			{
				return true;
			}
		}
		return false;
	}

	public void SetLocked(bool isLocked, bool lockSubItems = false)
	{
		SetFlag(Flag.IsLocked, isLocked);
		if (lockSubItems)
		{
			SetSubItemsLocked(isLocked);
		}
		MarkDirty();
	}

	private void SetSubItemsLocked(bool isLocked)
	{
		if (itemList == null)
		{
			return;
		}
		foreach (Item item in itemList)
		{
			if (item != null && item.contents != null)
			{
				item.contents.SetLocked(isLocked);
			}
		}
	}

	public bool Remove(Item item)
	{
		if (!itemList.Contains(item))
		{
			return false;
		}
		onPreItemRemove?.Invoke(item);
		itemList.Remove(item);
		item.parent = null;
		onItemParentChanged?.Invoke(parent, item);
		onItemAddedRemoved?.Invoke(item, arg2: false);
		if (item.HasOnCycle)
		{
			itemsWithOnCycle.Remove(item);
			if (CollectionEx.IsEmpty(itemsWithOnCycle))
			{
				Facepunch.Pool.FreeUnmanaged(ref itemsWithOnCycle);
			}
		}
		ItemContainer itemContainer = parent?.parent;
		if (itemContainer != null && itemContainer.onItemContentsChanged != null)
		{
			itemContainer.onItemContentsChanged(item, arg2: false);
		}
		MarkDirty();
		Interface.CallHook("OnItemRemovedFromContainer", this, item);
		return true;
	}

	public void UseAmount(ItemDefinition def, ref int amount)
	{
		int num = 60;
		int num2 = 0;
		while (amount > 0 && num2 < num)
		{
			num2++;
			Item item = FindItemByItemID(def.itemid);
			if (item != null)
			{
				int num3 = Mathf.Min(amount, item.amount);
				item.UseItem(num3);
				amount -= num3;
			}
		}
	}

	public void Clear()
	{
		if (itemList.Count == 0)
		{
			return;
		}
		foreach (Item item in itemList)
		{
			onPreItemRemove?.Invoke(item);
			item.parent = null;
			item.Remove();
			onItemAddedRemoved?.Invoke(item, arg2: false);
			ItemContainer itemContainer = parent?.parent;
			if (itemContainer != null && itemContainer.onItemContentsChanged != null)
			{
				itemContainer.onItemContentsChanged(item, arg2: false);
			}
		}
		itemList.Clear();
		if (itemsWithOnCycle != null)
		{
			itemsWithOnCycle.Clear();
			Facepunch.Pool.FreeUnmanaged(ref itemsWithOnCycle);
		}
		MarkDirty();
	}

	public void Kill()
	{
		parent = null;
		this.onDirty = null;
		canAcceptItem = null;
		slotIsReserved = null;
		onItemAddedRemoved = null;
		onItemContentsChanged = null;
		onPreItemRemove = null;
		if (uid.IsValid && Network.Net.sv != null)
		{
			Network.Net.sv.ReturnUID(uid.Value);
			uid = default(ItemContainerId);
		}
		Clear();
	}

	public int GetAmount(int itemid, bool onlyUsableAmounts = false, bool redirectAllowed = false)
	{
		int num = 0;
		foreach (Item item in itemList)
		{
			if (item.info.MatchesItemId(itemid, redirectAllowed))
			{
				num += item.amount;
			}
		}
		return num;
	}

	public int GetAmount(int blueprintBaseId, int itemId, bool onlyUsableAmounts = false)
	{
		int num = 0;
		foreach (Item item in itemList)
		{
			if (item.info.itemid == blueprintBaseId && item.blueprintTarget == itemId)
			{
				num += item.amount;
			}
		}
		return num;
	}

	public int GetOkConditionAmount(int itemid, bool onlyUsableAmounts = false, bool redirectAllowed = false)
	{
		int num = 0;
		foreach (Item item in itemList)
		{
			if (item.info.MatchesItemId(itemid, redirectAllowed) && !(item.condition <= 0f))
			{
				num += item.amount;
			}
		}
		return num;
	}

	public Item FindItemByItemID(int itemid)
	{
		for (int i = 0; i < itemList.Count; i++)
		{
			Item item = itemList[i];
			if (item.info.itemid == itemid)
			{
				return item;
			}
		}
		return null;
	}

	public Item FindItemByItemName(string name)
	{
		ItemDefinition itemDefinition = ItemManager.FindItemDefinition(name);
		if (itemDefinition == null)
		{
			return null;
		}
		for (int i = 0; i < itemList.Count; i++)
		{
			if (itemList[i].info == itemDefinition)
			{
				return itemList[i];
			}
		}
		return null;
	}

	public Item FindBySubEntityID(NetworkableId subEntityID)
	{
		if (!subEntityID.IsValid)
		{
			return null;
		}
		foreach (Item item in itemList)
		{
			if (item.instanceData != null && item.instanceData.subEntity == subEntityID)
			{
				return item;
			}
		}
		return null;
	}

	[PoolAnalyzerNonCaching]
	public void FindItemsByItemID(List<Item> list, int itemid)
	{
		for (int i = 0; i < itemList.Count; i++)
		{
			Item item = itemList[i];
			if (item.info.itemid == itemid)
			{
				list.Add(item);
			}
		}
	}

	[PoolAnalyzerNonCaching]
	public void FindItemsByItemID(int itemId, BufferList<Item> found)
	{
		for (int i = 0; i < itemList.Count; i++)
		{
			Item item = itemList[i];
			if (item.info.itemid == itemId)
			{
				found.Add(item);
			}
		}
	}

	public ProtoBuf.ItemContainer Save(bool bIncludeContainer = true, bool stripBelt = false)
	{
		bool flag = stripBelt && ConVar.AntiHack.hotbar_network_mode == 1;
		ItemId itemId = ((flag && playerOwner != null) ? playerOwner.svActiveItemID : default(ItemId));
		ProtoBuf.ItemContainer itemContainer = Facepunch.Pool.Get<ProtoBuf.ItemContainer>();
		itemContainer.contents = Facepunch.Pool.Get<List<ProtoBuf.Item>>();
		itemContainer.UID = uid;
		itemContainer.slots = capacity;
		itemContainer.temperature = temperature;
		itemContainer.allowedContents = (int)allowedContents;
		if (HasLimitedAllowedItems)
		{
			itemContainer.allowedItems = Facepunch.Pool.Get<List<int>>();
			for (int i = 0; i < onlyAllowedItems.Length; i++)
			{
				if (onlyAllowedItems[i] != null)
				{
					itemContainer.allowedItems.Add(onlyAllowedItems[i].itemid);
				}
			}
		}
		itemContainer.flags = (int)flags;
		itemContainer.maxStackSize = maxStackSize;
		itemContainer.allowItemsToIncreaseToMaxStackSize = allowItemsToIncreaseToMaxStackSize;
		itemContainer.volume = containerVolume;
		if (availableSlots != null && availableSlots.Count > 0)
		{
			itemContainer.availableSlots = Facepunch.Pool.Get<List<int>>();
			for (int j = 0; j < availableSlots.Count; j++)
			{
				itemContainer.availableSlots.Add((int)availableSlots[j]);
			}
		}
		for (int k = 0; k < itemList.Count; k++)
		{
			Item item = itemList[k];
			if (item.IsValid() && (!flag || (!(item.uid != itemId) && item.GetHeldEntity() is HeldEntity)))
			{
				itemContainer.contents.Add(item.Save(bIncludeContainer));
			}
		}
		return itemContainer;
	}

	public void Load(ProtoBuf.ItemContainer container)
	{
		using (TimeWarning.New("ItemContainer.Load"))
		{
			uid = container.UID;
			capacity = container.slots;
			temperature = container.temperature;
			flags = (Flag)container.flags;
			allowedContents = ((container.allowedContents == 0) ? ContentsType.Generic : ((ContentsType)container.allowedContents));
			if (container.allowedItems != null && container.allowedItems.Count > 0)
			{
				onlyAllowedItems = new ItemDefinition[container.allowedItems.Count];
				for (int i = 0; i < container.allowedItems.Count; i++)
				{
					onlyAllowedItems[i] = ItemManager.FindItemDefinition(container.allowedItems[i]);
				}
			}
			else
			{
				onlyAllowedItems = null;
			}
			maxStackSize = container.maxStackSize;
			allowItemsToIncreaseToMaxStackSize = container.allowItemsToIncreaseToMaxStackSize;
			containerVolume = container.volume;
			availableSlots.Clear();
			if (container.availableSlots != null)
			{
				for (int j = 0; j < container.availableSlots.Count; j++)
				{
					availableSlots.Add((ItemSlot)container.availableSlots[j]);
				}
			}
			List<Item> obj = itemList;
			itemList = Facepunch.Pool.Get<List<Item>>();
			itemsWithOnCycle?.Clear();
			using (TimeWarning.New("container.contents"))
			{
				foreach (ProtoBuf.Item content in container.contents)
				{
					Item created = null;
					foreach (Item item in obj)
					{
						if (item.uid == content.UID)
						{
							created = item;
							break;
						}
					}
					created = ItemManager.Load(content, created, isServer);
					if (created != null)
					{
						created.parent = this;
						created.position = content.slot;
						Insert(created);
					}
				}
			}
			using (TimeWarning.New("Delete old items"))
			{
				foreach (Item item2 in obj)
				{
					if (!itemList.Contains(item2))
					{
						item2.Remove();
					}
				}
			}
			Facepunch.Pool.Free(ref obj, freeElements: false);
			if (itemsWithOnCycle != null && CollectionEx.IsEmpty(itemsWithOnCycle))
			{
				Facepunch.Pool.Free(ref itemsWithOnCycle, freeElements: false);
			}
			dirty = true;
		}
	}

	public BasePlayer GetOwnerPlayer()
	{
		return playerOwner;
	}

	public int ContainerMaxStackSize()
	{
		if (maxStackSize <= 0)
		{
			return int.MaxValue;
		}
		return maxStackSize;
	}

	[PoolAnalyzerNonCaching]
	public int Take(List<Item> collect, int itemid, int iAmount, bool takeBroken = true, bool redirectAllowed = false)
	{
		int num = 0;
		if (iAmount == 0)
		{
			return num;
		}
		List<Item> obj = Facepunch.Pool.Get<List<Item>>();
		List<Item> obj2 = Facepunch.Pool.Get<List<Item>>();
		for (int i = 0; i < ((!redirectAllowed) ? 1 : 2); i++)
		{
			if (num >= iAmount)
			{
				break;
			}
			bool flag = i == 1;
			foreach (Item item2 in itemList)
			{
				if (!item2.info.MatchesItemId(itemid, redirectAllowed) || item2.info.itemid != itemid != flag || (!takeBroken && item2.info.condition.enabled && item2.condition <= 0f))
				{
					continue;
				}
				int num2 = iAmount - num;
				if (num2 <= 0)
				{
					continue;
				}
				if (item2.amount > num2)
				{
					num += num2;
					if (collect != null)
					{
						Item item = item2.SplitItem(num2);
						if (item != null)
						{
							item.CollectedForCrafting(playerOwner);
							collect.Add(item);
						}
					}
					else
					{
						item2.UseItem(num2);
					}
					break;
				}
				if (item2.amount <= num2)
				{
					num += item2.amount;
					if (collect != null)
					{
						collect.Add(item2);
						obj.Add(item2);
					}
					else
					{
						obj2.Add(item2);
					}
				}
				if (num == iAmount)
				{
					break;
				}
			}
		}
		foreach (Item item3 in obj)
		{
			item3.RemoveFromContainer();
		}
		foreach (Item item4 in obj2)
		{
			item4.Remove();
		}
		Facepunch.Pool.Free(ref obj, freeElements: false);
		Facepunch.Pool.Free(ref obj2, freeElements: false);
		ItemManager.DoRemoves();
		return num;
	}

	public bool TryTakeOne(int itemid, out Item item)
	{
		item = null;
		foreach (Item item3 in itemList)
		{
			if (item3.info.itemid == itemid)
			{
				if (item3.amount > 1)
				{
					item3.MarkDirty();
					item3.amount--;
					Item item2 = ItemManager.CreateByItemID(itemid, 1, 0uL, 0uL);
					item2.amount = 1;
					item2.CollectedForCrafting(playerOwner);
					item = item2;
				}
				else
				{
					item = item3;
				}
				break;
			}
		}
		if (item != null)
		{
			item.RemoveFromContainer();
			return true;
		}
		return false;
	}

	public bool GiveItem(Item item, ItemContainer container = null, GiveItemOptions options = GiveItemOptions.None)
	{
		if (item == null)
		{
			return false;
		}
		if (container != null && item.MoveToContainer(container))
		{
			return true;
		}
		return item.MoveToContainer(this);
	}

	public void OnCycle(float delta)
	{
		if (itemsWithOnCycle == null)
		{
			return;
		}
		using (TimeWarning.New("ItemContainer.OnCycle"))
		{
			foreach (Item item in itemsWithOnCycle)
			{
				if (item.IsValid())
				{
					item.OnCycle(delta);
				}
			}
		}
	}

	public Item FindAmmo(AmmoTypes ammoType)
	{
		for (int i = 0; i < itemList.Count; i++)
		{
			Item item = itemList[i].FindAmmo(ammoType);
			if (item != null)
			{
				return item;
			}
		}
		return null;
	}

	[PoolAnalyzerNonCaching]
	public void FindAmmo(List<Item> list, AmmoTypes ammoType)
	{
		for (int i = 0; i < itemList.Count; i++)
		{
			itemList[i].FindAmmo(list, ammoType);
		}
	}

	public bool HasAmmo(AmmoTypes ammoType)
	{
		for (int i = 0; i < itemList.Count; i++)
		{
			if (itemList[i].HasAmmo(ammoType))
			{
				return true;
			}
		}
		return false;
	}

	public int GetAmmoAmount(ItemDefinition specificAmmo)
	{
		int num = 0;
		for (int i = 0; i < itemList.Count; i++)
		{
			num += ((itemList[i].info == specificAmmo) ? itemList[i].amount : 0);
		}
		return num;
	}

	public int GetAmmoAmount(AmmoTypes ammoType)
	{
		int num = 0;
		for (int i = 0; i < itemList.Count; i++)
		{
			num += itemList[i].GetAmmoAmount(ammoType);
		}
		return num;
	}

	public int TotalItemAmount()
	{
		int num = 0;
		for (int i = 0; i < itemList.Count; i++)
		{
			num += itemList[i].amount;
		}
		return num;
	}

	public bool HasAny(ItemDefinition itemDef)
	{
		for (int i = 0; i < itemList.Count; i++)
		{
			Item item = itemList[i];
			if (item.info == itemDef && item.amount > 0)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAnyWithSpace(ItemDefinition itemDef)
	{
		for (int i = 0; i < itemList.Count; i++)
		{
			Item item = itemList[i];
			if (item.info == itemDef && item.amount > 0 && item.amount < item.info.stackable)
			{
				return true;
			}
		}
		return false;
	}

	public int GetTotalItemAmount(Item item, int slotStartInclusive, int slotEndInclusive)
	{
		int num = 0;
		foreach (Item item2 in itemList)
		{
			if (item2.position < slotStartInclusive || item2.position > slotEndInclusive)
			{
				continue;
			}
			if (item.IsBlueprint())
			{
				if (item2.IsBlueprint() && item2.blueprintTarget == item.blueprintTarget)
				{
					num += item2.amount;
				}
			}
			else if (item2.info == item.info || item2.info.isRedirectOf == item.info || item.info.isRedirectOf == item2.info)
			{
				num += item2.amount;
			}
			else if (item2.info.isRedirectOf != null && item2.info.isRedirectOf == item.info.isRedirectOf)
			{
				num += item2.amount;
			}
		}
		return num;
	}

	public int TotalItemAmount(ItemDefinition itemDef)
	{
		int num = 0;
		for (int i = 0; i < itemList.Count; i++)
		{
			Item item = itemList[i];
			if (item.info == itemDef)
			{
				num += item.amount;
			}
		}
		return num;
	}

	public int GetTotalCategoryAmount(ItemCategory category, int slotStartInclusive, int slotEndInclusive)
	{
		int num = 0;
		for (int i = slotStartInclusive; i <= slotEndInclusive; i++)
		{
			Item slot = GetSlot(i);
			if (slot != null && slot.info.category == category)
			{
				num += slot.amount;
			}
		}
		return num;
	}

	public void AddItem(ItemDefinition itemToCreate, int amount, ulong skin = 0uL, LimitStack limitStack = LimitStack.Existing)
	{
		for (int i = 0; i < itemList.Count; i++)
		{
			if (amount == 0)
			{
				return;
			}
			if (itemList[i].info != itemToCreate)
			{
				continue;
			}
			int num = itemList[i].MaxStackable();
			if (num <= itemList[i].amount && limitStack != 0)
			{
				continue;
			}
			MarkDirty();
			itemList[i].amount += amount;
			amount -= amount;
			if (itemList[i].amount > num && limitStack != 0)
			{
				amount = itemList[i].amount - num;
				if (amount > 0)
				{
					itemList[i].amount -= amount;
				}
			}
		}
		if (amount == 0)
		{
			return;
		}
		int num2 = ((limitStack != LimitStack.All) ? int.MaxValue : ((!allowItemsToIncreaseToMaxStackSize) ? Mathf.Min(itemToCreate.stackable, ContainerMaxStackSize()) : Mathf.Max(itemToCreate.stackable, ContainerMaxStackSize())));
		if (num2 <= 0)
		{
			return;
		}
		while (amount > 0)
		{
			int num3 = Mathf.Min(amount, num2);
			Item item = ItemManager.Create(itemToCreate, num3, skin, isServerSide: true, 0uL);
			amount -= num3;
			if (!item.MoveToContainer(this))
			{
				item.Remove();
			}
		}
	}

	public bool TryMoveAllItems(ItemContainer target)
	{
		for (int num = itemList.Count - 1; num >= 0; num--)
		{
			if (!itemList[num].MoveToContainer(target))
			{
				return false;
			}
		}
		return true;
	}

	public void MoveAllItems(ItemContainer target, BasePlayer notifyTarget = null)
	{
		for (int num = itemList.Count - 1; num >= 0; num--)
		{
			Item item = itemList[num];
			int amount = item.amount;
			if (item.MoveToContainer(target) && notifyTarget != null)
			{
				notifyTarget.Command("note.inv", item.info.itemid, amount);
			}
		}
	}

	public void RemoveFractionOfContainer(List<Item> removedItems, float removeFraction)
	{
		removeFraction = Mathf.Clamp01(removeFraction);
		if (itemList.Count == 0 || removeFraction == 0f)
		{
			return;
		}
		List<Item> obj = Facepunch.Pool.Get<List<Item>>();
		Dictionary<ItemDefinition, float> dict = Facepunch.Pool.Get<Dictionary<ItemDefinition, float>>();
		for (int num = itemList.Count - 1; num >= 0; num--)
		{
			Item item = itemList[num];
			if (item.amount == 1)
			{
				obj.Add(item);
			}
			else
			{
				float num2 = (float)item.amount * removeFraction;
				int num3 = Mathf.FloorToInt(num2);
				float num4 = num2 - (float)num3;
				if (num4 > 0f)
				{
					if (!dict.TryGetValue(item.info, out var value))
					{
						float num5 = num4;
						if (UnityEngine.Random.Range(0f, 1f) < num5)
						{
							num4 = -1f + num5;
							num3++;
						}
						else
						{
							num4 += removeFraction;
						}
						dict.Add(item.info, num4);
					}
					else
					{
						num4 += value;
						if (num4 > 1f)
						{
							num3++;
							num4 -= 1f;
						}
						dict[item.info] = num4;
					}
				}
				if (num3 > 0)
				{
					if (num3 == item.amount)
					{
						removedItems.Add(item);
						item.RemoveFromContainer();
					}
					else
					{
						removedItems.Add(item.SplitItem(num3));
					}
				}
			}
		}
		int num6 = Mathf.RoundToInt((float)obj.Count * removeFraction);
		obj.Shuffle((uint)UnityEngine.Random.Range(0, 1000000000));
		for (int i = 0; i < num6; i++)
		{
			Item item2 = obj[i];
			removedItems.Add(item2);
			item2.RemoveFromContainer();
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
		Facepunch.Pool.FreeUnmanaged(ref dict);
	}

	public void RemoveItemsFromContainer(List<Item> removedItems, ItemDefinition[] targetItems)
	{
		for (int num = itemList.Count - 1; num >= 0; num--)
		{
			Item item = itemList[num];
			foreach (ItemDefinition itemDefinition in targetItems)
			{
				if (!(itemDefinition == null) && (item.info == itemDefinition || item.info.isRedirectOf == itemDefinition))
				{
					item.RemoveFromContainer();
					removedItems.Add(item);
					break;
				}
			}
		}
	}

	public void MergeAllStacks()
	{
		List<Item> obj = Facepunch.Pool.Get<List<Item>>();
		obj.AddRange(itemList);
		foreach (Item item in obj)
		{
			item.RemoveFromContainer();
		}
		foreach (Item item2 in obj)
		{
			item2.MoveToContainer(this);
		}
		Facepunch.Pool.Free(ref obj, freeElements: false);
	}

	public void OnMovedToWorld()
	{
		for (int i = 0; i < itemList.Count; i++)
		{
			itemList[i].OnMovedToWorld();
		}
	}

	public void OnRemovedFromWorld()
	{
		for (int i = 0; i < itemList.Count; i++)
		{
			itemList[i].OnRemovedFromWorld();
		}
	}

	public uint ContentsDeepHash(bool ignoreBackContents = false)
	{
		uint num = 0u;
		for (int i = 0; i < capacity; i++)
		{
			Item slot = GetSlot(i);
			if (slot == null)
			{
				continue;
			}
			num = CRC.Compute32(num, slot.info.itemid);
			num = CRC.Compute32(num, slot.skin);
			num = CRC.Compute32(num, slot.uid.Value);
			if ((slot.IsBackpack() && ignoreBackContents) || slot.contents == null)
			{
				continue;
			}
			for (int j = 0; j < slot.contents.capacity; j++)
			{
				if (slot.contents.GetSlot(j) != null)
				{
					num = CRC.Compute32(num, slot.info.itemid);
				}
			}
		}
		return num;
	}

	public ItemContainer FindContainer(ItemContainerId id)
	{
		if (id == uid)
		{
			return this;
		}
		for (int i = 0; i < itemList.Count; i++)
		{
			Item item = itemList[i];
			if (item.contents != null)
			{
				ItemContainer itemContainer = item.contents.FindContainer(id);
				if (itemContainer != null)
				{
					return itemContainer;
				}
			}
		}
		return null;
	}

	public CanAcceptResult CanAcceptItem(BasePlayer player, Item item, int targetPos)
	{
		if (canAcceptItem != null && !canAcceptItem(player, item, targetPos))
		{
			return CanAcceptResult.CannotAccept;
		}
		if (isServer && availableSlots != null && availableSlots.Count > 0)
		{
			if (item.info.occupySlots == (ItemSlot)0 || item.info.occupySlots == ItemSlot.None)
			{
				return CanAcceptResult.CannotAccept;
			}
			if (item.isBroken)
			{
				return CanAcceptResult.CannotAccept;
			}
			int num = 0;
			foreach (ItemSlot availableSlot in availableSlots)
			{
				num |= (int)availableSlot;
			}
			if (((uint)num & (uint)item.info.occupySlots) != (uint)item.info.occupySlots)
			{
				return CanAcceptResult.CannotAcceptRightNow;
			}
		}
		if ((allowedContents & item.info.itemType) != item.info.itemType)
		{
			return CanAcceptResult.CannotAccept;
		}
		if (HasLimitedAllowedItems)
		{
			bool flag = false;
			for (int i = 0; i < onlyAllowedItems.Length; i++)
			{
				if (onlyAllowedItems[i] == item.info)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return CanAcceptResult.CannotAccept;
			}
		}
		object obj = Interface.CallHook("CanAcceptItem", this, item, targetPos, player);
		if (obj is CanAcceptResult)
		{
			return (CanAcceptResult)obj;
		}
		if (blockedItems != null && blockedItems.Contains(item.info))
		{
			return CanAcceptResult.CannotAccept;
		}
		if (item.GetItemVolume() > containerVolume)
		{
			return CanAcceptResult.CannotAccept;
		}
		return CanAcceptResult.CanAccept;
	}

	public bool HasBackpackItem()
	{
		foreach (Item item in itemList)
		{
			if (!item.isBroken && item.IsBackpack())
			{
				return true;
			}
		}
		return false;
	}

	public void BubbleUpRadiationChanged(Item item, float amount)
	{
		if (parent != null && parent.parent != null)
		{
			parent?.parent?.onItemRadiationChanged?.Invoke(parent, 0f);
		}
	}
}
