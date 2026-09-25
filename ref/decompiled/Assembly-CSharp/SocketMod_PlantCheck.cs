using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class SocketMod_PlantCheck : SocketMod
{
	public bool CanBePotted = true;

	public float sphereRadius = 1f;

	public LayerMask layerMask;

	public QueryTriggerInteraction queryTriggers;

	public bool wantsCollide;

	public override bool DoCheck(ref Construction.Placement place)
	{
		if (!CanBePotted && place.transform != null)
		{
			PlanterBox planterBox = GameObjectEx.ToBaseEntity(place.transform) as PlanterBox;
			if (planterBox != null && planterBox.PlantPot)
			{
				return false;
			}
		}
		Vector3 position = place.position + place.rotation * worldPosition;
		List<BaseEntity> obj = Pool.Get<List<BaseEntity>>();
		Vis.Entities(position, sphereRadius, obj, layerMask.value, queryTriggers);
		if (isServer)
		{
			List<BaseEntity> obj2 = Pool.Get<List<BaseEntity>>();
			BaseEntity.Query.Server.GetInSphere(position, sphereRadius, obj2);
			obj.AddRange(obj2);
			Pool.FreeUnmanaged(ref obj2);
		}
		bool result = !wantsCollide;
		foreach (BaseEntity item in obj)
		{
			if (!(item == null) && !place.ShouldIgnoreEntity(item))
			{
				GrowableEntity component = item.GetComponent<GrowableEntity>();
				if ((bool)component && wantsCollide)
				{
					result = true;
					break;
				}
				if ((bool)component && !wantsCollide)
				{
					result = false;
					break;
				}
			}
		}
		Pool.FreeUnmanaged(ref obj);
		return result;
	}
}
