using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class PickupVolume : PrefabAttribute
{
	public Bounds bounds = new Bounds(Vector3.zero, Vector3.one);

	protected override Type GetIndexedType()
	{
		return typeof(PickupVolume);
	}

	public static bool Check(Vector3 position, Quaternion rotation, PickupVolume[] volumes, BaseEntity ignoreEntity = null)
	{
		for (int i = 0; i < volumes.Length; i++)
		{
			if (volumes[i].CheckInternal(position, rotation, 256, ignoreEntity))
			{
				return true;
			}
		}
		return false;
	}

	protected bool CheckInternal(Vector3 position, Quaternion rotation, int mask = -1, BaseEntity ignoreEntity = null)
	{
		position += rotation * (worldRotation * bounds.center + worldPosition);
		if (CheckOBB(new OBB(position, bounds.size, rotation * worldRotation), mask, this, ignoreEntity))
		{
			return true;
		}
		return false;
	}

	private static bool CheckOBB(OBB obb, int layerMask, PickupVolume volume)
	{
		return CheckOBB(obb, layerMask, volume, null);
	}

	private static bool CheckOBB(OBB obb, int layerMask, PickupVolume volume, BaseEntity ignoredEntity = null)
	{
		List<Collider> obj = Pool.Get<List<Collider>>();
		GamePhysics.OverlapOBB(obb, obj, layerMask, QueryTriggerInteraction.Collide);
		bool result = CheckFlags(obj, volume, ignoredEntity);
		Pool.FreeUnmanaged(ref obj);
		return result;
	}

	public static bool CheckFlags(List<Collider> colliders, PickupVolume volume, BaseEntity ignoredEntity = null)
	{
		foreach (Collider collider in colliders)
		{
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(collider);
			if ((!(baseEntity != null) || !(ignoredEntity != null) || !(baseEntity.net.ID == ignoredEntity.net.ID)) && baseEntity != null && baseEntity != ignoredEntity)
			{
				return true;
			}
		}
		return false;
	}
}
