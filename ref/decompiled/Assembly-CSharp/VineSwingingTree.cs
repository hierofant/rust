using System.Collections.Generic;
using Facepunch;
using ProtoBuf;
using Rust;
using UnityEngine;

public class VineSwingingTree : TreeEntity
{
	public GameObjectRef StumpPrefab;

	public MeshRenderer[] BranchRenderers;

	public GameObject[] BranchRoots;

	public MeshRenderer BranchHighlightRenderer;

	public float VineSpawnHeight = 15f;

	public float VineSpawnRadius = 5f;

	public VineLaunchPoint[] LaunchPoints;

	public Collider[] ClimbColliders;

	public Collider StumpCollider;

	public List<EntityRef<VineMountable>> SpawnedVines = new List<EntityRef<VineMountable>>();

	public override bool IncludeInNavmesh => true;

	public VineMountable GetSpawnedVine(VineLaunchPoint point)
	{
		int index = point.Index();
		EnsureVineArrayLength(index);
		return SpawnedVines[index].Get(base.isServer);
	}

	private void EnsureVineArrayLength(int index)
	{
		if (SpawnedVines.Count <= index)
		{
			while (SpawnedVines.Count <= index)
			{
				SpawnedVines.Add(default(EntityRef<VineMountable>));
			}
		}
	}

	public void SetSpawnedVine(VineLaunchPoint point, VineMountable vine)
	{
		int index = point.Index();
		EnsureVineArrayLength(index);
		EntityRef<VineMountable> value = default(EntityRef<VineMountable>);
		value.Set(vine);
		SpawnedVines[index] = value;
	}

	public Vector3 GetVineSpawnPos(List<VineLaunchPoint> possibleDestinations)
	{
		if (possibleDestinations.Count == 0)
		{
			return Vector3.zero;
		}
		Vector3 zero = Vector3.zero;
		foreach (VineLaunchPoint possibleDestination in possibleDestinations)
		{
			zero += possibleDestination.transform.position;
		}
		zero /= (float)possibleDestinations.Count;
		Vector3 vector = base.transform.position + base.transform.up * VineSpawnHeight;
		zero = zero.WithY(vector.y);
		Vector3 normalized = (zero - vector).normalized;
		return vector + normalized * VineSpawnRadius;
	}

	public override void InitShared()
	{
		base.InitShared();
		GameObject[] branchRoots = BranchRoots;
		foreach (GameObject gameObject in branchRoots)
		{
			if (gameObject != null)
			{
				gameObject.SetActive(value: true);
			}
		}
		if (StumpCollider != null)
		{
			StumpCollider.enabled = false;
		}
	}

	public void RefreshVineState()
	{
		if (Rust.Application.isLoading)
		{
			Invoke(RefreshVineState, 0.25f);
			return;
		}
		VineLaunchPoint[] launchPoints = LaunchPoints;
		for (int i = 0; i < launchPoints.Length; i++)
		{
			launchPoints[i].SpawnVineIfPossible(this);
		}
	}

	public override void PostMapEntitySpawn()
	{
		base.PostMapEntitySpawn();
		RefreshVineState();
	}

	public override void ServerInit()
	{
		base.ServerInit();
		VineLaunchPoint[] launchPoints = LaunchPoints;
		for (int i = 0; i < launchPoints.Length; i++)
		{
			launchPoints[i].ServerInit();
		}
		Invoke(RefreshVineState, 0.25f);
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		VineLaunchPoint[] launchPoints = LaunchPoints;
		foreach (VineLaunchPoint vineLaunchPoint in launchPoints)
		{
			if (vineLaunchPoint != null)
			{
				vineLaunchPoint.DoServerDestroy();
			}
		}
		if (StumpPrefab.isValid)
		{
			VineSwingingTreeStump obj = base.gameManager.CreateEntity(StumpPrefab.resourcePath, base.transform.position, base.transform.rotation) as VineSwingingTreeStump;
			obj.InitializeTree(this);
			obj.Spawn();
		}
	}

	public void NotifyNearbyTreesSpawned()
	{
		using PooledList<VineSwingingTree> pooledList = Pool.Get<PooledList<VineSwingingTree>>();
		Vis.Entities(base.transform.position, 64f, pooledList, 1073741824);
		foreach (VineSwingingTree item in pooledList)
		{
			if (!item.isClient && !(item == this))
			{
				item.RefreshVineState();
			}
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.vineTree = Pool.Get<VineTree>();
		info.msg.vineTree.spawnedVines = Pool.Get<List<NetworkableId>>();
		foreach (EntityRef<VineMountable> spawnedVine in SpawnedVines)
		{
			info.msg.vineTree.spawnedVines.Add(spawnedVine.uid);
		}
	}

	protected override void OnFallServer()
	{
		base.OnFallServer();
		ToggleClimbColliders(state: false);
		GameObject[] branchRoots = BranchRoots;
		foreach (GameObject gameObject in branchRoots)
		{
			if (gameObject != null)
			{
				gameObject.SetActive(value: false);
			}
		}
		using PooledList<Collider> pooledList = Pool.Get<PooledList<Collider>>();
		GetComponentsInChildren(pooledList);
		foreach (Collider item in pooledList)
		{
			if (!item.isTrigger)
			{
				item.enabled = false;
			}
		}
		VineLaunchPoint[] launchPoints = LaunchPoints;
		for (int i = 0; i < launchPoints.Length; i++)
		{
			launchPoints[i].DoServerDestroy();
		}
		if (StumpCollider != null)
		{
			StumpCollider.enabled = true;
		}
	}

	public override void OnAttacked(HitInfo info)
	{
		base.OnAttacked(info);
		if (info.InitiatorPlayer == null || base.isClient)
		{
			return;
		}
		using PooledList<VineMountable> pooledList = Pool.Get<PooledList<VineMountable>>();
		VineMountable.pointGrid.Query(base.transform.position.x, base.transform.position.z, 10f, pooledList);
		foreach (VineMountable item in pooledList)
		{
			if (!item.IsOn())
			{
				VineLaunchPoint vineLaunchPoint = item.currentLocation.Get(isServer: true);
				if (vineLaunchPoint != null && vineLaunchPoint.ParentTree == this && item.AttackedByPlayer(info.InitiatorPlayer))
				{
					break;
				}
			}
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.vineTree == null || info.msg.vineTree.spawnedVines == null)
		{
			return;
		}
		SpawnedVines.Clear();
		foreach (NetworkableId spawnedVine in info.msg.vineTree.spawnedVines)
		{
			SpawnedVines.Add(new EntityRef<VineMountable>(spawnedVine));
		}
	}

	private void ToggleClimbColliders(bool state)
	{
		Collider[] climbColliders = ClimbColliders;
		foreach (Collider collider in climbColliders)
		{
			if (collider != null)
			{
				collider.gameObject.SetActive(state);
			}
		}
	}
}
