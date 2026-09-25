using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class SocketMod_BoatBuildingNettingPoint : SocketMod
{
	private static Translate.Phrase lastError = new Translate.Phrase();

	protected override Translate.Phrase ErrorPhrase => lastError;

	public static bool IsOnBoatBuildingNetting(Vector3 vPoint)
	{
		List<RaycastHit> obj = Pool.Get<List<RaycastHit>>();
		GamePhysics.TraceAllUnordered(new Ray(vPoint + Vector3.up * 3f, Vector3.down), 0f, obj, 3f, 8388608);
		foreach (RaycastHit item in obj)
		{
			if (item.collider.gameObject.CompareTag("BoatBuildingNetting"))
			{
				Pool.FreeUnmanaged(ref obj);
				return true;
			}
		}
		Pool.FreeUnmanaged(ref obj);
		return false;
	}

	public override bool DoCheck(ref Construction.Placement place)
	{
		Vector3 vPoint = place.position + place.rotation * worldPosition;
		lastError = null;
		if (IsOnBoatBuildingNetting(vPoint))
		{
			return true;
		}
		if (lastError == null)
		{
			lastError = ConstructionErrors.MustPlaceOnNetting;
		}
		return false;
	}
}
