using System;
using System.Collections.Generic;
using Facepunch;
using ProtoBuf;
using Spatial;
using UnityEngine;

public class BuriedItems : PointEntity
{
	[ServerVar(Help = "Time in seconds before an item expires.")]
	public static float expiryTime = 86400f;

	private const int CellSize = 128;

	private const float WorldSize = 8096f;

	private const float QuerySize = 64f;

	[ServerVar]
	public static int maxBuriedItems = 32;

	[ServerVar(Help = "Metal detector loot weight is 100.")]
	public static int buriedItemWeight = 100;

	[ServerVar(Help = "[0.0 to 1.0]")]
	public static float buryItemChance = 0.85f;

	private Grid<BuriedItem> grid = new Grid<BuriedItem>(128);

	private readonly SortedList<long, BuriedItem> itemExpiryTracking = new SortedList<long, BuriedItem>(128);

	private readonly Dictionary<ulong, BuriedItem> uidItemMapping = new Dictionary<ulong, BuriedItem>(128);

	private static readonly System.Random Random = new System.Random();

	private (long lastExpiryTime, long modifiedExpiryTime)? lastExpiryTime;

	public static BuriedItems Instance { get; private set; }

	public override void ServerInit()
	{
		base.ServerInit();
		Clear();
		Instance = this;
	}

	public void Register(Item item, Vector3 worldPosition)
	{
		if (item != null && item.uid.IsValid && item.info.allowBurying && Random.NextDouble() <= (double)buryItemChance && itemExpiryTracking.Count < maxBuriedItems)
		{
			TimeSpan timeSpan = TimeSpan.FromSeconds(expiryTime);
			long num = DateTimeOffset.UtcNow.Add(timeSpan).ToUnixTimeMilliseconds();
			BuriedItem buriedItem = BuriedItem.Create(item, worldPosition, num);
			if (buriedItem != null)
			{
				Add(buriedItem);
			}
		}
	}

	private void Add(BuriedItem buriedItem)
	{
		HandleDuplicateExpiryTimes(buriedItem);
		if (itemExpiryTracking.TryAdd(buriedItem.ExpiryTime, buriedItem))
		{
			grid.Add(buriedItem, buriedItem.Location.x, buriedItem.Location.y);
			uidItemMapping.Add(buriedItem.UID, buriedItem);
		}
		else
		{
			Debug.LogError($"Failed to add buried item with expiry time {buriedItem.ExpiryTime} to the expiry tracking list, retrying.");
			Add(buriedItem);
		}
	}

	private void HandleDuplicateExpiryTimes(BuriedItem buriedItem)
	{
		long num = buriedItem.ExpiryTime;
		if (lastExpiryTime.HasValue && lastExpiryTime.Value.lastExpiryTime == num)
		{
			long num2;
			for (num2 = lastExpiryTime.Value.modifiedExpiryTime + 1; itemExpiryTracking.ContainsKey(num2); num2++)
			{
			}
			lastExpiryTime = (num, num2);
			buriedItem.ExpiryTime = num2;
		}
		else
		{
			lastExpiryTime = (num, num);
		}
	}

	private void PruneExpiredItems()
	{
		if (itemExpiryTracking.Count == 0)
		{
			return;
		}
		long num = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		if (itemExpiryTracking.Values[0].ExpiryTime > num)
		{
			return;
		}
		using PooledList<BuriedItem> pooledList = Pool.Get<PooledList<BuriedItem>>();
		foreach (var (num3, item) in itemExpiryTracking)
		{
			if (num3 <= num)
			{
				pooledList.Add(item);
				continue;
			}
			break;
		}
		if (pooledList.Count <= 0)
		{
			return;
		}
		if (pooledList.Count == itemExpiryTracking.Count)
		{
			itemExpiryTracking.Clear();
		}
		foreach (BuriedItem item2 in pooledList)
		{
			UnregisterItem(item2);
		}
	}

	private void UnregisterItem(BuriedItem buriedItem)
	{
		grid.Remove(buriedItem);
		itemExpiryTracking.Remove(buriedItem.ExpiryTime);
		uidItemMapping.Remove(buriedItem.UID);
		Pool.Free(ref buriedItem);
	}

	public void UnregisterItem(ulong itemUid)
	{
		if (itemUid == 0L || !uidItemMapping.TryGetValue(itemUid, out var value))
		{
			Debug.LogError($"Couldn't find buried item with ID {itemUid}");
		}
		else
		{
			UnregisterItem(value);
		}
	}

	public void Clear()
	{
		uidItemMapping.Clear();
		foreach (BuriedItem value in itemExpiryTracking.Values)
		{
			BuriedItem obj = value;
			Pool.Free(ref obj);
		}
		itemExpiryTracking.Clear();
		grid = new Grid<BuriedItem>(128);
		lastExpiryTime = null;
	}

	public void DoUpdate()
	{
		PruneExpiredItems();
	}

	public void AddItems(List<DiggableEntityLoot.ItemEntry> items, Vector3 digWorldPos)
	{
		List<BuriedItem> obj = Pool.Get<List<BuriedItem>>();
		grid.Query(digWorldPos.x, digWorldPos.z, 64f, obj);
		foreach (BuriedItem item in obj)
		{
			if (item.ItemId.HasValue)
			{
				items.Add(new DiggableEntityLoot.ItemEntry
				{
					Item = ItemManager.FindItemDefinition(item.ItemId.Value),
					Skin = item.SkinId.GetValueOrDefault(),
					Min = 1,
					Max = 1,
					Weight = buriedItemWeight,
					Condition = item.Condition,
					UID = item.UID,
					Owner = item.OwnershipShare
				});
			}
		}
		Pool.Free(ref obj, freeElements: false);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (!info.forDisk)
		{
			return;
		}
		ProtoBuf.BuriedItems buriedItems = Pool.Get<ProtoBuf.BuriedItems>();
		buriedItems.buriedItems = Pool.Get<List<ProtoBuf.BuriedItems.StoredBuriedItem>>();
		foreach (BuriedItem value in itemExpiryTracking.Values)
		{
			if (value.ItemId.HasValue)
			{
				ProtoBuf.BuriedItems.StoredBuriedItem storedBuriedItem = Pool.Get<ProtoBuf.BuriedItems.StoredBuriedItem>();
				storedBuriedItem.itemId = value.ItemId.Value;
				storedBuriedItem.skinId = value.SkinId.GetValueOrDefault();
				storedBuriedItem.expiryTimeDiff = value.ExpiryTime - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
				storedBuriedItem.location = value.Location;
				storedBuriedItem.condition = (value.Condition.HasValue ? value.Condition.Value : (-1f));
				storedBuriedItem.uid = value.UID;
				if (value.OwnershipShare.HasValue)
				{
					ItemOwnershipAmount itemOwnershipAmount = Pool.Get<ItemOwnershipAmount>();
					itemOwnershipAmount.amount = value.OwnershipShare.Value.amount;
					itemOwnershipAmount.username = value.OwnershipShare.Value.username;
					itemOwnershipAmount.reason = value.OwnershipShare.Value.reason;
					storedBuriedItem.ownership = itemOwnershipAmount;
				}
				buriedItems.buriedItems.Add(storedBuriedItem);
			}
		}
		info.msg.buriedItemStorage = buriedItems;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (!info.fromDisk || info.msg.buriedItemStorage == null)
		{
			return;
		}
		Clear();
		foreach (ProtoBuf.BuriedItems.StoredBuriedItem buriedItem2 in info.msg.buriedItemStorage.buriedItems)
		{
			if (buriedItem2.uid != 0L)
			{
				BuriedItem buriedItem = BuriedItem.Create(buriedItem2);
				Add(buriedItem);
			}
		}
	}
}
