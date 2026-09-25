using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class SocketMod_TerrainCheck : SocketMod
{
	public bool wantsInTerrain = true;

	public bool preventWorldLayerInMonuments;

	private static Translate.Phrase lastError = new Translate.Phrase();

	protected override Translate.Phrase ErrorPhrase => lastError;

	public static bool IsInTerrain(Vector3 vPoint, bool worldLayerInMonuments)
	{
		if (TerrainMeta.OutOfBounds(vPoint) && !DeepSeaManager.IsInsideDeepSea(vPoint))
		{
			Ray ray = new Ray(vPoint + Vector3.up * 3f, Vector3.down);
			if (TerrainMeta.IsPointWithinTutorialBounds(vPoint))
			{
				return Physics.Raycast(ray, 3f, 65536);
			}
			return false;
		}
		List<RaycastHit> obj = Pool.Get<List<RaycastHit>>();
		GamePhysics.TraceAllUnordered(new Ray(vPoint + Vector3.up * 3f, Vector3.down), 0f, obj, 3f, 65536);
		using (List<RaycastHit>.Enumerator enumerator = obj.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				RaycastHit current = enumerator.Current;
				if (worldLayerInMonuments)
				{
					Pool.FreeUnmanaged(ref obj);
					return true;
				}
				if (current.collider.gameObject.HasCustomTag(GameObjectTag.BlockBarricadePlacement))
				{
					lastError = ConstructionErrors.CantPlaceOnMonument;
					Pool.FreeUnmanaged(ref obj);
					return false;
				}
				if (current.collider.gameObject.HasCustomTag(GameObjectTag.AllowBarricadePlacement))
				{
					Pool.FreeUnmanaged(ref obj);
					return true;
				}
				bool num = ColliderEx.GetMonument(current.collider) != null;
				bool flag = current.collider != null && GameObjectEx.ToBaseEntity(current.collider) is NPCDwelling;
				if (!num && !flag)
				{
					Pool.FreeUnmanaged(ref obj);
					return true;
				}
				lastError = ConstructionErrors.CantPlaceOnMonument;
				Pool.FreeUnmanaged(ref obj);
				return false;
			}
		}
		Pool.FreeUnmanaged(ref obj);
		if ((!TerrainMeta.Collision || !TerrainMeta.Collision.GetIgnore(vPoint)) && TerrainMeta.SampleTerrainMeshHeight(vPoint) > vPoint.y)
		{
			return true;
		}
		if (DeepSeaManager.IsInsideDeepSea(vPoint))
		{
			Ray ray2 = new Ray(vPoint + Vector3.up * 3f, Vector3.down);
			if (DeepSeaManager.IsInsideDeepSea(vPoint) && GamePhysics.Trace(ray2, 0f, out var hitInfo, 3f, 8388608))
			{
				return GameObjectEx.ToBaseEntity(hitInfo.collider) is DeepSeaIsland;
			}
		}
		return false;
	}

	public override bool DoCheck(ref Construction.Placement place)
	{
		Vector3 vPoint = place.position + place.rotation * worldPosition;
		lastError = null;
		if (IsInTerrain(vPoint, !preventWorldLayerInMonuments) == wantsInTerrain)
		{
			return true;
		}
		if (lastError == null)
		{
			lastError = ConstructionErrors.NotInTerrain;
		}
		return false;
	}
}
