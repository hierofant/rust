using System.Collections.Generic;
using ConVar;
using Facepunch;
using UnityEngine;

public class SocketMod_BoatBuildingBlockHeight : SocketMod_BuildingBlock
{
	public float MaxHeight = 5f;

	protected override bool GetContained(Vector3 pos)
	{
		float maxHeight = Env.oceanlevel + MaxHeight;
		return !ValidPlacementHeight(pos, sphereRadius, layerMask.value, queryTriggers, maxHeight);
	}

	public static bool ValidPlacementHeight(Vector3 pos, float sphereRadius, int layerMask, QueryTriggerInteraction queryTriggers, float maxHeight)
	{
		List<BoatBuildingBlock> obj = Facepunch.Pool.Get<List<BoatBuildingBlock>>();
		Vis.Entities(pos, sphereRadius, obj, layerMask, queryTriggers);
		bool num = obj.Count > 0;
		Facepunch.Pool.FreeUnmanaged(ref obj);
		if (!num)
		{
			return true;
		}
		return pos.y <= maxHeight;
	}
}
