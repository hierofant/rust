using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class SocketMod_BoatBuildingBlock : SocketMod_BuildingBlock
{
	public enum BoatBuildFailReason
	{
		None,
		NotOnHull,
		CannotBePlacedOnBoat
	}

	public bool RequireHull;

	public bool RequireNoParentBoat = true;

	private BoatBuildFailReason lastFailReason;

	protected override Translate.Phrase ErrorPhrase => lastFailReason switch
	{
		BoatBuildFailReason.NotOnHull => ConstructionErrors.RequiresHull, 
		BoatBuildFailReason.CannotBePlacedOnBoat => ConstructionErrors.CannotPlaceOnBoat, 
		_ => ConstructionErrors.MustPlaceOnBoat, 
	};

	protected override bool GetContained(Vector3 pos)
	{
		bool flag = Contained(pos, sphereRadius, layerMask.value, queryTriggers, RequireHull, RequireNoParentBoat, out lastFailReason);
		if (flag && !wantsCollide)
		{
			lastFailReason = BoatBuildFailReason.CannotBePlacedOnBoat;
			return true;
		}
		return flag;
	}

	public static bool Contained(Vector3 pos, float sphereRadius, int layerMask, QueryTriggerInteraction queryTriggers, bool requireHull, bool requireNoParentBoat, out BoatBuildFailReason failReason)
	{
		failReason = BoatBuildFailReason.None;
		List<BoatBuildingBlock> obj = Pool.Get<List<BoatBuildingBlock>>();
		Vis.Entities(pos, sphereRadius, obj, layerMask, queryTriggers);
		bool flag = obj.Count > 0;
		if (flag && (requireHull || requireNoParentBoat))
		{
			flag = false;
			foreach (BoatBuildingBlock item in obj)
			{
				if (requireHull && !item.Hull)
				{
					failReason = BoatBuildFailReason.NotOnHull;
				}
				else if (!requireNoParentBoat || !(item.GetParentEntity() is PlayerBoat))
				{
					flag = true;
					break;
				}
			}
		}
		Pool.FreeUnmanaged(ref obj);
		return flag;
	}
}
