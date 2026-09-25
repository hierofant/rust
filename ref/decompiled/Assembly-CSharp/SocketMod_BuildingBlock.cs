using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class SocketMod_BuildingBlock : SocketMod
{
	public float sphereRadius = 1f;

	public LayerMask layerMask;

	public QueryTriggerInteraction queryTriggers;

	public bool wantsCollide;

	protected override Translate.Phrase ErrorPhrase => ConstructionErrors.MustPlaceOnConstruction;

	public override bool DoCheck(ref Construction.Placement place)
	{
		Vector3 pos = place.position + place.rotation * worldPosition;
		bool contained = GetContained(pos);
		if (!contained || !wantsCollide)
		{
			if (!contained)
			{
				return !wantsCollide;
			}
			return false;
		}
		return true;
	}

	protected virtual bool GetContained(Vector3 pos)
	{
		List<BuildingBlock> obj = Pool.Get<List<BuildingBlock>>();
		Vis.Entities(pos, sphereRadius, obj, layerMask.value, queryTriggers);
		bool result = obj.Count > 0;
		Pool.FreeUnmanaged(ref obj);
		return result;
	}
}
