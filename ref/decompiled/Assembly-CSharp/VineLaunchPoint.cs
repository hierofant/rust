using System.Collections.Generic;
using Development.Attributes;
using Facepunch;
using Spatial;
using UnityEngine;

[ResetStaticFields]
public class VineLaunchPoint : MonoBehaviour
{
	[Header("References")]
	public GameObjectRef VineMountablePrefab;

	public float MaximumDestinationRange;

	public float MinimumDestinationRange;

	[Header("Arc Settings")]
	public float maxDistanceHeight = -10f;

	public float minDistanceHeight = -4f;

	public int resolution = 30;

	public bool drawArc = true;

	public float angle;

	public float VineSpawnOffset = 0.1f;

	public bool useLevelDirection = true;

	public Transform[] VineArrivalPoints;

	public VineSwingingTree ParentTree;

	public static Grid<VineLaunchPoint> pointGrid = new Grid<VineLaunchPoint>();

	private bool hasDied;

	private VineMountable spawnedVine
	{
		get
		{
			return ParentTree.GetSpawnedVine(this);
		}
		set
		{
			ParentTree.SetSpawnedVine(this, value);
		}
	}

	public int Index()
	{
		if (ParentTree != null)
		{
			for (int i = 0; i < ParentTree.LaunchPoints.Length; i++)
			{
				if (ParentTree.LaunchPoints[i] == this)
				{
					return i;
				}
			}
		}
		return -1;
	}

	public Vector3 GetSwingPointAtTime(float time, VineLaunchPoint forPoint)
	{
		return GetSwingPointAtTime(time, forPoint.transform.position);
	}

	public Vector3 GetSwingPointAtTime(float time, Vector3 forPoint)
	{
		Vector3 position = base.transform.position;
		Vector3 vector = forPoint;
		Vector3 normalized = (vector - position).normalized;
		position += normalized * VineSpawnOffset;
		vector += normalized * (0f - VineSpawnOffset);
		float t = Mathx.RemapValClamped(Vector3.Distance(position, vector), MinimumDestinationRange, MaximumDestinationRange, 0f, 1f);
		Vector3 point = VineUtils.SampleParabola(position, vector, Mathf.Lerp(minDistanceHeight, maxDistanceHeight, t), time, useLevelDirection);
		Vector3 pivot = (position + vector) / 2f;
		return VineUtils.RotateAroundWorldAxis(point, pivot, (position - vector).normalized, angle);
	}

	public void ServerInit()
	{
		Vector3 position = base.transform.position;
		pointGrid.Add(this, position.x, position.z);
		hasDied = false;
	}

	public void DoServerDestroy()
	{
		if (!hasDied)
		{
			hasDied = true;
			pointGrid.Remove(this);
			VineMountable.NotifyVinesLaunchSiteRemoved(this);
		}
	}

	public void SpawnVineIfPossible(VineSwingingTree fromTree)
	{
		hasDied = false;
		VineMountable vineMountable = spawnedVine;
		using PooledList<VineLaunchPoint> pooledList = Pool.Get<PooledList<VineLaunchPoint>>();
		if (vineMountable != null)
		{
			if (GetReceivePoints(pooledList))
			{
				vineMountable.Initialise(this, pooledList, vineMountable.WorldSpaceAnchorPoint);
				vineMountable.SendNetworkUpdate();
			}
			return;
		}
		pooledList.Clear();
		GetReceivePoints(pooledList);
		if (pooledList.Count <= 0)
		{
			return;
		}
		Vector3 vector = base.transform.TransformPoint(Vector3.forward * VineSpawnOffset);
		using PooledList<VineMountable> pooledList2 = Pool.Get<PooledList<VineMountable>>();
		GamePhysics.OverlapSphere(vector, 5f, pooledList2, 134217728, QueryTriggerInteraction.Collide);
		foreach (VineMountable item in pooledList2)
		{
			if (!item.HasFlag(BaseEntity.Flags.Reserved1))
			{
				return;
			}
		}
		VineMountable vineMountable2 = GameManager.server.CreateEntity(VineMountablePrefab.resourcePath, vector, Quaternion.identity) as VineMountable;
		if (FindVacantArrivalPoint(vineMountable2, out var worldPos))
		{
			vineMountable2.transform.position = worldPos;
		}
		spawnedVine = vineMountable2;
		Vector3 vineSpawnPos = fromTree.GetVineSpawnPos(pooledList);
		vineMountable2.Initialise(this, pooledList, vineSpawnPos);
		vineMountable2.Spawn();
		vineMountable2.SendNetworkUpdate();
	}

	public void OnVineKilled()
	{
		spawnedVine = null;
	}

	private bool GetReceivePoints(List<VineLaunchPoint> points)
	{
		Vector3 position = base.transform.position;
		Vector3 forward = base.transform.forward;
		bool result = false;
		using PooledList<VineLaunchPoint> pooledList = Pool.Get<PooledList<VineLaunchPoint>>();
		if (!UnityEngine.Application.isPlaying)
		{
			pooledList.AddRange(Object.FindObjectsByType<VineLaunchPoint>(FindObjectsInactive.Exclude, FindObjectsSortMode.None));
		}
		else
		{
			pointGrid.Query(position.x, position.z, MaximumDestinationRange, pooledList);
		}
		foreach (VineLaunchPoint item in pooledList)
		{
			if (item == this || points.Contains(item))
			{
				continue;
			}
			Vector3 position2 = item.transform.position;
			float num = Vector3.Distance(position, position2.WithY(position.y));
			if (!(num > MaximumDestinationRange) && !(num < MinimumDestinationRange) && !(Vector3.Angle(forward, (position2.WithY(position.y) - position).normalized) > 45f) && !(Vector3.Angle(forward, -item.transform.forward) > 90f))
			{
				if (!GamePhysics.LineOfSightRadius(position, position2, 1084293377, 0.25f, ParentTree))
				{
					return false;
				}
				Vector3 swingPointAtTime = GetSwingPointAtTime(0.5f, position2);
				if (!GamePhysics.LineOfSightRadius(position, swingPointAtTime, 1084293377, 0.25f, ParentTree))
				{
					return false;
				}
				if (!GamePhysics.LineOfSightRadius(position2, swingPointAtTime, 1084293377, 0.25f, ParentTree))
				{
					return false;
				}
				points.Add(item);
				result = true;
			}
		}
		return result;
	}

	public bool FindVacantArrivalPoint(VineMountable forMountable, out Vector3 worldPos)
	{
		if (!forMountable.HasFlag(BaseEntity.Flags.Reserved1))
		{
			worldPos = base.transform.TransformPoint(Vector3.forward * VineSpawnOffset);
			return true;
		}
		worldPos = Vector3.zero;
		using PooledList<VineMountable> pooledList = Pool.Get<PooledList<VineMountable>>();
		Vis.Entities(base.transform.position, 2f, pooledList, 134217728);
		float num = float.MaxValue;
		Transform transform = null;
		Transform[] vineArrivalPoints = VineArrivalPoints;
		foreach (Transform transform2 in vineArrivalPoints)
		{
			Vector3 position = transform2.position;
			bool flag = true;
			foreach (VineMountable item in pooledList)
			{
				if (!item.isClient && !(item == forMountable) && Vector3.Distance(position, item.transform.position.WithY(position.y)) < 0.1f)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				float num2 = Vector3.Distance(forMountable.transform.position.WithY(position.y), position);
				if (num2 < num)
				{
					num = num2;
					transform = transform2;
				}
			}
		}
		bool num3 = transform != null;
		if (num3)
		{
			worldPos = transform.position;
		}
		return num3;
	}
}
