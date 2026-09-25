using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class DeployVolumeRequireBoatBuildingVolume : DeployVolume
{
	public List<Transform> Points = new List<Transform>();

	protected override bool Check(Vector3 position, Quaternion rotation, int mask = -1)
	{
		List<TriggerBoatBuildingArea> obj = Pool.Get<List<TriggerBoatBuildingArea>>();
		Vis.Components(position, 3f, obj, 262144);
		using (List<TriggerBoatBuildingArea>.Enumerator enumerator = obj.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				TriggerBoatBuildingArea current = enumerator.Current;
				Vector3 position2 = current.transform.position;
				Vector3 lossyScale = current.transform.lossyScale;
				Quaternion rotation2 = current.transform.rotation;
				OBB oBB = new OBB(position2, lossyScale, rotation2);
				foreach (Transform point in Points)
				{
					Vector3 target = position + rotation * point.position;
					if (!oBB.Contains(target))
					{
						Pool.FreeUnmanaged(ref obj);
						return true;
					}
				}
				Pool.FreeUnmanaged(ref obj);
				return false;
			}
		}
		Pool.FreeUnmanaged(ref obj);
		return true;
	}

	protected override bool Check(Vector3 position, Quaternion rotation, List<Type> types, TypeFilterMode filterMode, BaseEntity ignoredEntity = null, int mask = -1, bool ignoreChildrenOfEntity = false)
	{
		return Check(position, rotation, mask);
	}

	protected override bool Check(Vector3 position, Quaternion rotation, OBB obb, int mask = -1)
	{
		return false;
	}

	protected override void AttributeSetup(GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
	}
}
