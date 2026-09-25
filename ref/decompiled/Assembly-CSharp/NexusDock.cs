using System;
using Facepunch.Extend;
using UnityEngine;

public class NexusDock : SingletonComponent<NexusDock>
{
	[Header("Targets")]
	public Transform FerryWaypoint;

	public Transform[] QueuePoints;

	public Transform Arrival;

	public Transform Docking;

	public Transform Docked;

	public Transform CastingOff;

	public Transform Departure;

	[Header("Ferry")]
	public float WaitTime = 30f;

	[Header("Ejection")]
	public BoxCollider EjectionZone;

	public float TraceHeight = 100f;

	public LayerMask TraceLayerMask = 1503731969;

	public int EjectionAttempts = 25;

	[Range(0f, 1f)]
	public float MinGroundNormal = 0.7f;

	public float MaxGroundVariance = 0.35f;

	private const float SkinWidth = 0.05f;

	private const float MinFootprint = 0.25f;

	private const float CornerInset = 0.8f;

	[NonSerialized]
	public NexusFerry[] QueuedFerries;

	[NonSerialized]
	public NexusFerry CurrentFerry;

	public Transform GetEntryPoint(NexusFerry ferry, out bool entered)
	{
		if (ferry == null)
		{
			throw new ArgumentNullException("ferry");
		}
		CleanupQueuedFerries();
		if (ferry == CurrentFerry)
		{
			entered = true;
			return Arrival;
		}
		int num = QueuedFerries.FindIndex(ferry);
		if (num < 0)
		{
			if (QueuedFerries[0] == null)
			{
				QueuedFerries[0] = ferry;
				entered = false;
				return QueuePoints[0];
			}
			entered = false;
			return FerryWaypoint;
		}
		int num2 = QueuedFerries.Length - 1;
		if (num == num2)
		{
			if (CurrentFerry == null)
			{
				QueuedFerries[num] = null;
				CurrentFerry = ferry;
				entered = true;
				return Arrival;
			}
			entered = false;
			return QueuePoints[num];
		}
		if (num < num2)
		{
			if (QueuedFerries[num + 1] == null)
			{
				QueuedFerries[num] = null;
				QueuedFerries[num + 1] = ferry;
				entered = false;
				return QueuePoints[num + 1];
			}
			entered = false;
			return QueuePoints[num];
		}
		entered = false;
		return QueuePoints[num];
	}

	public bool Depart(NexusFerry ferry)
	{
		if (ferry != CurrentFerry)
		{
			return false;
		}
		CurrentFerry = null;
		return true;
	}

	public bool TryFindEjectionPosition(BaseEntity entity, out Vector3 position)
	{
		position = Vector3.zero;
		if (entity == null)
		{
			Debug.LogError("Cannot find an eject position without an entity to fit", this);
			return false;
		}
		if (EjectionZone == null)
		{
			Debug.LogError("EjectionZone is null, cannot find an eject position", this);
			return false;
		}
		Quaternion quaternion = Quaternion.Euler(0f, entity.transform.eulerAngles.y, 0f);
		Bounds bounds = entity.bounds;
		Vector3 lossyScale = entity.transform.lossyScale;
		Vector3 vector = new Vector3(Mathf.Max(Mathf.Abs(bounds.extents.x * lossyScale.x), 0.25f), Mathf.Max(Mathf.Abs(bounds.extents.y * lossyScale.y), 0.25f), Mathf.Max(Mathf.Abs(bounds.extents.z * lossyScale.z), 0.25f));
		Vector3 vector2 = quaternion * Vector3.Scale(bounds.center, lossyScale);
		Vector3 halfExtents = Vector3.Max(vector - new Vector3(0.05f, 0.05f, 0.05f), new Vector3(0.05f, 0.05f, 0.05f));
		Transform transform = EjectionZone.transform;
		Vector3 size = EjectionZone.size;
		float num = transform.position.y - size.y / 2f;
		bool flag = false;
		Vector3 vector3 = Vector3.zero;
		for (int i = 0; i < EjectionAttempts; i++)
		{
			Vector3 position2 = size.Scale(UnityEngine.Random.value - 0.5f, 0f, UnityEngine.Random.value - 0.5f);
			Vector3 pos = transform.TransformPoint(position2);
			Vector3 center = new Vector3(pos.x + vector2.x, num + TraceHeight + halfExtents.y, pos.z + vector2.z);
			if (!Physics.BoxCast(center, halfExtents, Vector3.down, out var hitInfo, quaternion, TraceHeight + size.y, TraceLayerMask, QueryTriggerInteraction.Ignore) || hitInfo.normal.y < MinGroundNormal)
			{
				continue;
			}
			float num2 = center.y - hitInfo.distance - halfExtents.y;
			if (num2 < pos.y - size.y || num2 > pos.y + size.y)
			{
				continue;
			}
			float waterSurface = WaterLevel.GetWaterSurface(pos, waves: false, volumes: false);
			if (!(num2 < waterSurface))
			{
				Vector3 vector4 = new Vector3(center.x, num2 + vector.y, center.z);
				Vector3 vector5 = vector4 - vector2;
				if (!flag)
				{
					flag = true;
					vector3 = vector5;
				}
				if (IsRestingOnGround(vector4, vector, quaternion, num2, waterSurface) && !GamePhysics.CheckOBBAndEntity(new OBB(vector4 + Vector3.up * MaxGroundVariance, vector * 2f, quaternion), TraceLayerMask, QueryTriggerInteraction.Ignore, entity))
				{
					position = vector5;
					return true;
				}
			}
		}
		if (flag)
		{
			position = vector3;
			return true;
		}
		return false;
	}

	private bool IsRestingOnGround(Vector3 restingCenter, Vector3 extents, Quaternion rotation, float groundHeight, float waterHeight)
	{
		float y = groundHeight + MaxGroundVariance;
		float maxDistance = MaxGroundVariance * 2f;
		for (int i = 0; i < 4; i++)
		{
			float x = (((i & 1) == 0) ? (-1f) : 1f) * extents.x * 0.8f;
			float z = (((i & 2) == 0) ? (-1f) : 1f) * extents.z * 0.8f;
			if (!Physics.Raycast((restingCenter + rotation * new Vector3(x, 0f, z)).WithY(y), Vector3.down, out var hitInfo, maxDistance, TraceLayerMask, QueryTriggerInteraction.Ignore))
			{
				return false;
			}
			if (Mathf.Abs(hitInfo.point.y - groundHeight) > MaxGroundVariance || hitInfo.normal.y < MinGroundNormal || hitInfo.point.y < waterHeight)
			{
				return false;
			}
		}
		return true;
	}

	public void CleanupQueuedFerries()
	{
		Array.Resize(ref QueuedFerries, QueuePoints.Length);
		for (int i = 0; i < QueuedFerries.Length; i++)
		{
			if (!QueuedFerries[i])
			{
				QueuedFerries[i] = null;
			}
		}
		if (!CurrentFerry)
		{
			CurrentFerry = null;
		}
	}
}
