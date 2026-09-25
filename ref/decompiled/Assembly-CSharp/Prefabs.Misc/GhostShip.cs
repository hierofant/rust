using System.Collections;
using ConVar;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace Prefabs.Misc;

public class GhostShip : JunkPileWater, IDeepSeaSpawner
{
	public GameObjectRef hackableLockedCratePrefab;

	public Transform[] crateSpawnPoints;

	public GameObjectRef mapMarkerPrefab;

	public BoatGroupSpawner boatGroupSpawner;

	private SpawnGroup[] _spawnGroups = new SpawnGroup[0];

	private BaseEntity spawnedMapMarker;

	private NavMeshDataInstance navMeshInst;

	private Matrix4x4 navMeshTransf;

	public override Matrix4x4 WorldToNavMeshSpace
	{
		get
		{
			if (!navMeshInst.valid)
			{
				return base.WorldToNavMeshSpace;
			}
			return navMeshTransf * base.transform.worldToLocalMatrix;
		}
	}

	public override Matrix4x4 NavMeshToWorldSpace
	{
		get
		{
			if (!navMeshInst.valid)
			{
				return base.WorldToNavMeshSpace;
			}
			return base.transform.localToWorldMatrix * navMeshTransf.inverse;
		}
	}

	protected override void StartTimeout()
	{
	}

	public override void ServerInit()
	{
		base.ServerInit();
		DeepSeaManager.ServerGhostShips.Add(this);
		if (mapMarkerPrefab.isValid)
		{
			spawnedMapMarker = base.gameManager.CreateEntity(mapMarkerPrefab.resourcePath, base.transform.position, base.transform.rotation);
			spawnedMapMarker.Spawn();
		}
		if (AI.useUnityNavmesh)
		{
			NavMeshSurface componentInChildren = GetComponentInChildren<NavMeshSurface>();
			if ((bool)componentInChildren && componentInChildren.navMeshData != null)
			{
				Vector3 position = base.transform.position;
				Quaternion rotation = base.transform.rotation;
				navMeshInst = NavMesh.AddNavMeshData(componentInChildren.navMeshData, position, rotation);
				navMeshInst.owner = this;
				navMeshTransf = Matrix4x4.TRS(position, rotation, Vector3.one);
				if (SingletonComponent<DynamicNavMesh>.Instance != null)
				{
					SingletonComponent<DynamicNavMesh>.Instance.IgnoreRoots.Add(base.transform);
				}
			}
		}
		if (CollectionEx.IsEmpty(_spawnGroups))
		{
			GetAllSpawnGroups();
		}
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		DeepSeaManager.ServerGhostShips.Remove(this);
		if (AI.useUnityNavmesh)
		{
			if (navMeshInst.valid)
			{
				NavMesh.RemoveNavMeshData(navMeshInst);
			}
			if (SingletonComponent<DynamicNavMesh>.Instance != null)
			{
				SingletonComponent<DynamicNavMesh>.Instance.IgnoreRoots.Remove(base.transform);
			}
		}
		if (spawnedMapMarker != null)
		{
			spawnedMapMarker.Kill();
		}
		spawnedMapMarker = null;
	}

	public void SpawnHackableLockedCrate()
	{
		if (crateSpawnPoints != null)
		{
			Transform random = ArrayEx.GetRandom(crateSpawnPoints);
			if (!(random == null))
			{
				BaseEntity baseEntity = GameManager.server.CreateEntity(hackableLockedCratePrefab.resourcePath, random.position, random.rotation);
				baseEntity.Spawn();
				baseEntity.SetParent(this, worldPositionStays: true);
			}
		}
	}

	public override void OnEntityMessage(BaseEntity from, string msg)
	{
		if (from is HackableLockedCrate && msg == "HackingStarted")
		{
			boatGroupSpawner.SpawnBoatGroup(BoatAI.AILoadMode.KillBoat);
		}
	}

	public override bool ShouldJunkpileBeDestroyedBy(PlayerBoat boat)
	{
		return false;
	}

	public IEnumerator TriggerSpawnGroups()
	{
		SpawnGroup[] spawnGroups = _spawnGroups;
		foreach (SpawnGroup spawnGroup in spawnGroups)
		{
			bool flag = DeepSeaManager.IsRespawnVariant(spawnGroup);
			DeepSeaManager.ApplyPopulationScale(spawnGroup, flag ? DeepSea.loot_respawn_scale : DeepSea.loot_scale);
			if (!flag && (!spawnGroup.HasSpawnedAny() || !spawnGroup.wantsInitialSpawn))
			{
				spawnGroup.Spawn();
				yield return new WaitForSeconds(DeepSea.spawngroups_spawninterval);
			}
		}
	}

	public void GetAllSpawnGroups()
	{
		_spawnGroups = GetComponentsInChildren<SpawnGroup>();
	}

	public override bool ShouldChildrenInheritNetworkGroup()
	{
		return false;
	}

	private void OnDrawGizmos()
	{
		if (crateSpawnPoints == null)
		{
			return;
		}
		Transform[] array = crateSpawnPoints;
		foreach (Transform transform in array)
		{
			if (!(transform == null))
			{
				Gizmos.color = Color.yellow;
				Gizmos.DrawSphere(transform.position, 0.1f);
			}
		}
	}
}
