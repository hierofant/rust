using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class DeployVolumeEntityBoundsReverse : DeployVolume
{
	public Bounds bounds = new Bounds(Vector3.zero, Vector3.one);

	public int layer;

	protected override bool Check(Vector3 position, Quaternion rotation, int mask = -1)
	{
		position += rotation * bounds.center;
		OBB test = new OBB(position, bounds.size, rotation);
		List<BaseEntity> obj = Pool.Get<List<BaseEntity>>();
		Vis.Entities(position, test.extents.magnitude, obj, (int)layers & mask);
		foreach (BaseEntity item in obj)
		{
			DeployVolume[] array = PrefabAttribute.server.FindAll<DeployVolume>(item.prefabID);
			List<DeployVolume> obj2 = Pool.Get<List<DeployVolume>>();
			DeployVolume[] array2 = array;
			foreach (DeployVolume deployVolume in array2)
			{
				if (DeployVolume.ShouldApplyVolumeForEntity(deployVolume, item))
				{
					obj2.Add(deployVolume);
				}
			}
			if (DeployVolume.Check(item.transform.position, item.transform.rotation, obj2, test, 1 << layer))
			{
				Pool.FreeUnmanaged(ref obj2);
				Pool.FreeUnmanaged(ref obj);
				return true;
			}
			Pool.FreeUnmanaged(ref obj2);
		}
		Pool.FreeUnmanaged(ref obj);
		return false;
	}

	protected override bool Check(Vector3 position, Quaternion rotation, List<Type> types, TypeFilterMode filterMode, BaseEntity ignoredEntity = null, int mask = -1, bool ignoreChildrenOfEntity = false)
	{
		return Check(position, rotation, mask);
	}

	protected override bool Check(Vector3 position, Quaternion rotation, OBB test, int mask = -1)
	{
		return false;
	}

	protected override void AttributeSetup(GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		bounds = rootObj.GetComponent<BaseEntity>().bounds;
		layer = rootObj.layer;
	}
}
