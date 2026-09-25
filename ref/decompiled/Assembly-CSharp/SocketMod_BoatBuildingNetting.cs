using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class SocketMod_BoatBuildingNetting : SocketMod_BuildingBlock
{
	protected override Translate.Phrase ErrorPhrase
	{
		get
		{
			if (!wantsCollide)
			{
				return ConstructionErrors.CantPlaceOnNetting;
			}
			return ConstructionErrors.MustPlaceOnNetting;
		}
	}

	protected override bool GetContained(Vector3 pos)
	{
		List<BoatBuildingNetting> obj = Pool.Get<List<BoatBuildingNetting>>();
		Vis.Components(pos, sphereRadius, obj, layerMask.value, queryTriggers);
		bool result = obj.Count > 0;
		Pool.FreeUnmanaged(ref obj);
		return result;
	}
}
