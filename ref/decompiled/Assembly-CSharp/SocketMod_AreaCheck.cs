using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class SocketMod_AreaCheck : SocketMod
{
	public Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 0.1f);

	public LayerMask layerMask;

	public bool wantsInside = true;

	public bool ignoreAntiLargeVehicleCheck;

	private Translate.Phrase lastError = new Translate.Phrase();

	protected override Translate.Phrase ErrorPhrase => lastError;

	public static bool IsInArea(Vector3 position, OBB obb, LayerMask layerMask, out bool foundParent, bool wantsInside = true, bool shouldParent = false, BaseEntity parentEntity = null, BaseEntity ignoredEntity = null)
	{
		List<Collider> obj = Pool.Get<List<Collider>>();
		GamePhysics.OverlapOBB(obb, obj, layerMask.value, QueryTriggerInteraction.UseGlobal);
		foundParent = false;
		if (ignoredEntity != null)
		{
			for (int num = obj.Count - 1; num >= 0; num--)
			{
				BaseEntity baseEntity = GameObjectEx.ToBaseEntity(obj[num]);
				if (!(baseEntity == null))
				{
					if (baseEntity.isServer != ignoredEntity.isServer)
					{
						obj.RemoveAt(num);
					}
					else if (baseEntity == ignoredEntity)
					{
						obj.RemoveAt(num);
					}
				}
			}
		}
		if (shouldParent && wantsInside)
		{
			for (int i = 0; i < obj.Count; i++)
			{
				if (GameObjectEx.ToBaseEntity(obj[i]) == parentEntity)
				{
					foundParent = true;
					break;
				}
				if (parentEntity is PlayerBoat playerBoat)
				{
					if (playerBoat.WorldSpaceBounds().Contains(position))
					{
						foundParent = true;
					}
					break;
				}
			}
		}
		bool result = obj.Count > 0;
		Pool.FreeUnmanaged(ref obj);
		return result;
	}

	private bool SocketCanTargetBoats()
	{
		if (baseSocket == null || baseSocket.socketMods == null)
		{
			if (wantsInside)
			{
				return AcceptsLargeVehicles(this);
			}
			return false;
		}
		SocketMod[] socketMods = baseSocket.socketMods;
		foreach (SocketMod socketMod in socketMods)
		{
			if (socketMod is SocketMod_BoatBuildingBlock { wantsCollide: not false })
			{
				return true;
			}
			if (socketMod is SocketMod_AreaCheck { wantsInside: not false } socketMod_AreaCheck && AcceptsLargeVehicles(socketMod_AreaCheck))
			{
				return true;
			}
		}
		return false;
	}

	private static bool AcceptsLargeVehicles(SocketMod_AreaCheck check)
	{
		if (!check.ignoreAntiLargeVehicleCheck)
		{
			return (check.layerMask.value & 0x8000000) != 0;
		}
		return true;
	}

	public bool DoCheck(Vector3 position, Quaternion rotation, BaseEntity entity = null)
	{
		Vector3 position2 = position + rotation * worldPosition;
		Quaternion rotation2 = rotation * worldRotation;
		bool foundParent;
		return IsInArea(position, new OBB(position2, rotation2, bounds), layerMask, out foundParent, wantsInside, shouldParent: false, null, entity) == wantsInside;
	}

	public override bool DoCheck(ref Construction.Placement place)
	{
		Vector3 position = place.position + place.rotation * worldPosition;
		Quaternion rotation = place.rotation * worldRotation;
		bool foundParent;
		bool flag = IsInArea(position, new OBB(position, rotation, bounds), layerMask, out foundParent, wantsInside, !place.parentPassed && place.shouldParent, (place.transform != null) ? GameObjectEx.ToBaseEntity(place.transform) : null, place.ignoredEntity) == wantsInside;
		place.parentPassed |= foundParent;
		if (!flag)
		{
			lastError = ConstructionErrors.NotStableEnough;
			if ((int)layerMask == 2097152 || (int)layerMask == 136314880)
			{
				lastError = (wantsInside ? ConstructionErrors.MustPlaceOnConstruction : ConstructionErrors.CantPlaceOnConstruction);
			}
		}
		else if (!ignoreAntiLargeVehicleCheck && wantsInside && ((int)layerMask & 0x8000000) == 0)
		{
			flag = !GamePhysics.CheckSphere(place.position, 5f, 134217728);
			if (!flag)
			{
				lastError = ConstructionErrors.InvalidAreaVehicleLarge;
			}
		}
		if (flag)
		{
			return true;
		}
		return false;
	}
}
