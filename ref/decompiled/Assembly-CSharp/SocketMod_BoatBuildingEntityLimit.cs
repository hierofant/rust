using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class SocketMod_BoatBuildingEntityLimit : SocketMod
{
	public BaseEntity LimitedEntityType;

	public int Limit = 1;

	protected override Translate.Phrase ErrorPhrase => ConstructionErrors.BoatBuildingEntityLimit;

	public override bool DoCheck(ref Construction.Placement place)
	{
		Vector3 point = place.position + place.rotation * worldPosition;
		return IsBelowLimit(point);
	}

	private bool IsBelowLimit(Vector3 point)
	{
		List<TriggerBoatBuildingArea> obj = Pool.Get<List<TriggerBoatBuildingArea>>();
		Vis.Components(point, 3f, obj, 262144);
		int num = 0;
		foreach (TriggerBoatBuildingArea item in obj)
		{
			BoatBuildingStation boatBuildingStation = GameObjectEx.ToBaseEntity(item.gameObject) as BoatBuildingStation;
			if (boatBuildingStation == null || boatBuildingStation.isServer != isServer)
			{
				continue;
			}
			List<BaseEntity> obj2 = BoatBuildingStation.GetEntitiesInBuildArea<BaseEntity>(boatBuildingStation.BuildArea, 256, isServer);
			foreach (BaseEntity item2 in obj2)
			{
				if (!(item2 == null) && item2.isServer == isServer && item2.GetType() == LimitedEntityType.GetType())
				{
					num++;
					if (num >= Limit)
					{
						Pool.FreeUnmanaged(ref obj2);
						Pool.FreeUnmanaged(ref obj);
						return false;
					}
				}
			}
			Pool.FreeUnmanaged(ref obj2);
		}
		Pool.FreeUnmanaged(ref obj);
		return true;
	}
}
