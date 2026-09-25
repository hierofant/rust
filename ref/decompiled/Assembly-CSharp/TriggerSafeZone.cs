using System;
using System.Collections.Generic;
using UnityEngine;

public class TriggerSafeZone : TriggerBase
{
	public bool IncludePlayerBoats;

	public static List<TriggerSafeZone> allSafeZones = new List<TriggerSafeZone>();

	public float maxDepth = 20f;

	public float maxAltitude = -1f;

	[NonSerialized]
	public ApartmentBuilding Apartment;

	public Collider triggerCollider { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		triggerCollider = GetComponent<Collider>();
		base.InterestLayers = (int)base.InterestLayers | 0x200;
	}

	protected void OnEnable()
	{
		allSafeZones.Add(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		allSafeZones.Remove(this);
	}

	internal override GameObject InterestedInObject(GameObject obj)
	{
		obj = base.InterestedInObject(obj);
		if (obj == null)
		{
			return null;
		}
		BaseEntity baseEntity = GameObjectEx.ToBaseEntity(obj);
		if (baseEntity == null)
		{
			return null;
		}
		if (baseEntity.isClient)
		{
			return null;
		}
		if (IncludePlayerBoats && baseEntity is BoatBuildingBlock)
		{
			return PlayerBoat.GetParentPlayerBoat(baseEntity)?.gameObject;
		}
		return baseEntity.gameObject;
	}

	public bool PassesHeightChecks(Vector3 entPos)
	{
		Vector3 position = base.transform.position;
		float num = Mathf.Abs(position.y - entPos.y);
		if (maxDepth != -1f && entPos.y < position.y && num > maxDepth)
		{
			return false;
		}
		if (maxAltitude != -1f && entPos.y > position.y && num > maxAltitude)
		{
			return false;
		}
		return true;
	}

	public float GetSafeLevel(Vector3 pos)
	{
		if (!PassesHeightChecks(pos))
		{
			return 0f;
		}
		return 1f;
	}

	private static bool CheckIntersects(in OBB bounds, Collider trigger)
	{
		if (trigger is SphereCollider sphereCollider)
		{
			return Vector3.Distance(bounds.ClosestPoint(sphereCollider.transform.position), sphereCollider.transform.position) < sphereCollider.radius;
		}
		if (trigger is BoxCollider boxCollider)
		{
			return bounds.Intersects(new OBB(trigger.transform, new Bounds(boxCollider.center, boxCollider.size)));
		}
		throw new NotSupportedException("Unsupported safezone collider type: " + trigger.GetType().Name);
	}

	public static bool IsBoundsInsideSafeZone(OBB worldSpaceBound, bool checkCombatZones = true)
	{
		BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(serverside: true);
		if (activeGameMode != null && !activeGameMode.safeZone)
		{
			return false;
		}
		bool flag = false;
		foreach (TriggerSafeZone allSafeZone in allSafeZones)
		{
			if (CheckIntersects(in worldSpaceBound, allSafeZone.triggerCollider))
			{
				flag = true;
				break;
			}
		}
		if (flag && checkCombatZones)
		{
			foreach (TriggerSafeZoneOverride allHostileZone in TriggerSafeZoneOverride.allHostileZones)
			{
				if (allHostileZone.IsCombatActive && CheckIntersects(in worldSpaceBound, allHostileZone.triggerCollider))
				{
					flag = false;
					break;
				}
			}
		}
		return flag;
	}
}
