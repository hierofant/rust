using System;
using Facepunch;
using ProtoBuf;
using UnityEngine;

public class BuriedItem : Pool.IPooled
{
	public int? ItemId { get; private set; }

	public ulong UID { get; private set; }

	public ItemOwnershipShare? OwnershipShare { get; private set; }

	public ulong? SkinId { get; private set; }

	public long ExpiryTime { get; set; }

	public Vector2 Location { get; private set; }

	public float? Condition { get; private set; }

	public static BuriedItem Create(Item item, Vector3 worldPosition, long expiryTime)
	{
		if (item.info == null || (object)item.info == null)
		{
			Debug.LogError($"Tried to create a buried item with an item that has no ItemDefinition! UID: {item.uid}, ItemId: {item.info?.itemid}");
			return null;
		}
		BuriedItem buriedItem = Pool.Get<BuriedItem>();
		buriedItem.ItemId = item.info.itemid;
		buriedItem.ExpiryTime = expiryTime;
		buriedItem.Location = new Vector2(worldPosition.x, worldPosition.z);
		buriedItem.Condition = (item.hasCondition ? new float?(item.condition) : null);
		buriedItem.UID = item.uid.Value;
		if (item.ownershipShares != null && item.ownershipShares.Count > 0)
		{
			buriedItem.OwnershipShare = item.ownershipShares[0];
		}
		if (item.skin != 0L)
		{
			buriedItem.SkinId = item.skin;
		}
		return buriedItem;
	}

	public static BuriedItem Create(ProtoBuf.BuriedItems.StoredBuriedItem storedBuriedItem)
	{
		BuriedItem buriedItem = Pool.Get<BuriedItem>();
		buriedItem.ItemId = storedBuriedItem.itemId;
		buriedItem.SkinId = storedBuriedItem.skinId;
		buriedItem.Location = storedBuriedItem.location;
		buriedItem.ExpiryTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + storedBuriedItem.expiryTimeDiff;
		buriedItem.Condition = ((storedBuriedItem.condition < 0f) ? null : new float?(storedBuriedItem.condition));
		buriedItem.UID = storedBuriedItem.uid;
		if (storedBuriedItem.ownership != null)
		{
			buriedItem.OwnershipShare = new ItemOwnershipShare
			{
				amount = storedBuriedItem.ownership.amount,
				reason = storedBuriedItem.ownership.reason,
				username = storedBuriedItem.ownership.username
			};
		}
		return buriedItem;
	}

	public void EnterPool()
	{
		ItemId = null;
		OwnershipShare = null;
		SkinId = null;
	}

	public void LeavePool()
	{
	}
}
