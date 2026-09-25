using System;
using System.Collections.Generic;
using System.Linq;
using ConVar;
using Rust.Ai.Gen2;
using UnityEngine;

public class SpawnGroup : BaseMonoBehaviour, IServerComponent, ISpawnPointUser, ISpawnGroup
{
	[Serializable]
	public class SpawnEntry
	{
		public GameObjectRef prefab;

		public int weight = 1;

		public bool mobile;
	}

	[InspectorFlags]
	public MonumentTier Tier = (MonumentTier)(-1);

	public List<SpawnEntry> prefabs;

	public int maxPopulation = 5;

	public int numToSpawnPerTickMin = 1;

	public int numToSpawnPerTickMax = 2;

	public float respawnDelayMin = 10f;

	public float respawnDelayMax = 20f;

	public bool wantsInitialSpawn = true;

	public bool temporary;

	public bool forceInitialSpawn;

	public bool preventDuplicates;

	public bool isSpawnerActive = true;

	public bool shouldBlockSpawnedEntitySaving = true;

	public SpawnGroupResetBehavior resetBehavior;

	public BoxCollider setFreeIfMovedBeyond;

	public string category;

	[NonSerialized]
	public MonumentInfo Monument;

	public bool fillOnSpawn;

	public BaseSpawnPoint[] spawnPoints;

	public List<SpawnPointInstance> spawnInstances = new List<SpawnPointInstance>();

	public LocalClock spawnClock = new LocalClock();

	public int currentPopulation => spawnInstances.Count;

	public List<SpawnPointInstance> SpawnInstances => spawnInstances;

	public int ObjectsAdded { get; private set; }

	public int ObjectsRemoved { get; private set; }

	public int ObjectsActive => spawnInstances.Count;

	public int SpawnPointCount => spawnPoints.Length;

	protected virtual bool BlockSpawnedEntitySaving => shouldBlockSpawnedEntitySaving;

	protected virtual bool AllowOverlappingSpawns => false;

	public bool DoesGroupContainNPCs()
	{
		foreach (SpawnEntry prefab in prefabs)
		{
			GameObject gameObject = prefab.prefab?.Get();
			if (!(gameObject == null))
			{
				BaseCombatEntity component = gameObject.GetComponent<BaseCombatEntity>();
				if (!(component == null) && component.IsNpc)
				{
					return true;
				}
			}
		}
		return false;
	}

	public virtual bool WantsInitialSpawn()
	{
		return wantsInitialSpawn;
	}

	public virtual bool WantsTimedSpawn()
	{
		return respawnDelayMax != float.PositiveInfinity;
	}

	public float GetSpawnDelta()
	{
		return (respawnDelayMax + respawnDelayMin) * 0.5f / SpawnHandler.PlayerScale(ConVar.Spawn.player_scale);
	}

	public float GetSpawnVariance()
	{
		return (respawnDelayMax - respawnDelayMin) * 0.5f / SpawnHandler.PlayerScale(ConVar.Spawn.player_scale);
	}

	protected void Awake()
	{
		if (TerrainMeta.TopologyMap == null)
		{
			return;
		}
		int topology = TerrainMeta.TopologyMap.GetTopology(base.transform.position);
		int num = 469762048;
		int num2 = MonumentInfo.TierToMask(Tier);
		if (num2 == num || (num2 & topology) != 0)
		{
			spawnPoints = GetComponentsInChildren<BaseSpawnPoint>();
			if (WantsTimedSpawn())
			{
				spawnClock.Add(GetSpawnDelta(), GetSpawnVariance(), Spawn);
			}
			if (!temporary && (bool)SingletonComponent<SpawnHandler>.Instance)
			{
				SingletonComponent<SpawnHandler>.Instance.SpawnGroups.Add(this);
			}
			if (forceInitialSpawn)
			{
				Invoke(SpawnInitial, 1f);
			}
			Monument = FindMonument();
		}
	}

	protected void OnDestroy()
	{
		if ((bool)SingletonComponent<SpawnHandler>.Instance)
		{
			SingletonComponent<SpawnHandler>.Instance.SpawnGroups.Remove(this);
		}
		else if (AI.logIssues)
		{
			Debug.LogWarning(GetType().Name + ": SpawnHandler instance not found.");
		}
	}

	public void Fill()
	{
		if (isSpawnerActive)
		{
			Spawn(maxPopulation);
		}
	}

	public void Clear()
	{
		for (int num = spawnInstances.Count - 1; num >= 0; num--)
		{
			SpawnPointInstance spawnPointInstance = spawnInstances[num];
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(spawnPointInstance.gameObject);
			if (setFreeIfMovedBeyond != null && !setFreeIfMovedBeyond.bounds.Contains(baseEntity.transform.position))
			{
				spawnPointInstance.Retire();
			}
			else if ((bool)baseEntity)
			{
				baseEntity.Kill();
			}
		}
		spawnInstances.Clear();
	}

	public bool HasSpawnedAny()
	{
		return spawnInstances.Count > 0;
	}

	public bool HasSpawned(uint prefabID)
	{
		foreach (SpawnPointInstance spawnInstance in spawnInstances)
		{
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(spawnInstance.gameObject);
			if ((bool)baseEntity && baseEntity.prefabID == prefabID)
			{
				return true;
			}
		}
		return false;
	}

	public virtual void SpawnInitial()
	{
		if (wantsInitialSpawn && isSpawnerActive)
		{
			if (fillOnSpawn)
			{
				Spawn(maxPopulation);
			}
			else
			{
				Spawn();
			}
		}
	}

	public void SpawnRepeating()
	{
		for (int i = 0; i < spawnClock.events.Count; i++)
		{
			LocalClock.TimedEvent value = spawnClock.events[i];
			if (UnityEngine.Time.time > value.time)
			{
				value.delta = GetSpawnDelta();
				value.variance = GetSpawnVariance();
				spawnClock.events[i] = value;
			}
		}
		spawnClock.Tick();
	}

	public void ObjectSpawned(SpawnPointInstance instance)
	{
		spawnInstances.Add(instance);
		ObjectsAdded++;
	}

	public void ObjectRetired(SpawnPointInstance instance)
	{
		spawnInstances.Remove(instance);
		ObjectsRemoved++;
	}

	public void DelayedSpawn()
	{
		Invoke(Spawn, 1f);
	}

	public void Spawn()
	{
		if (isSpawnerActive)
		{
			Spawn(UnityEngine.Random.Range(numToSpawnPerTickMin, numToSpawnPerTickMax + 1));
		}
	}

	protected virtual void Spawn(int numToSpawn)
	{
		if (!AI.scientist_spawners_enabled && prefabs != null)
		{
			foreach (SpawnEntry prefab2 in prefabs)
			{
				if (prefab2?.prefab != null && prefab2.prefab.isValid)
				{
					BaseEntity entity = prefab2.prefab.GetEntity();
					if (entity is ScientistNPC || entity is TunnelDweller || entity is UnderwaterDweller || entity is ScientistNPC2)
					{
						base.enabled = false;
						return;
					}
				}
			}
		}
		numToSpawn = Mathf.Min(numToSpawn, maxPopulation - currentPopulation);
		for (int i = 0; i < numToSpawn; i++)
		{
			GameObjectRef prefab = GetPrefab();
			if (prefab == null || !prefab.isValid)
			{
				continue;
			}
			Vector3 pos;
			Quaternion rot;
			BaseSpawnPoint spawnPoint = GetSpawnPoint(prefab, out pos, out rot);
			if (!spawnPoint)
			{
				continue;
			}
			BaseEntity baseEntity = GameManager.server.CreateEntity(prefab.resourcePath, pos, rot, startActive: false);
			if ((bool)baseEntity)
			{
				if (baseEntity.enableSaving && BlockSpawnedEntitySaving && !(spawnPoint is SpaceCheckingSpawnPoint))
				{
					baseEntity.enableSaving = false;
				}
				SpawnPointInstance spawnPointInstance = baseEntity.gameObject.AddComponent<SpawnPointInstance>();
				spawnPointInstance.parentSpawnPointUser = this;
				spawnPointInstance.parentSpawnPoint = spawnPoint;
				spawnPointInstance.Entity = baseEntity;
				spawnPointInstance.blockSpawnHandlerRespawns = true;
				PoolableEx.AwakeFromInstantiate(baseEntity.gameObject);
				baseEntity.Spawn();
				PostSpawnProcess(baseEntity, spawnPoint);
				spawnPointInstance.Notify();
			}
		}
	}

	protected virtual void PostSpawnProcess(BaseEntity entity, BaseSpawnPoint spawnPoint)
	{
		if (entity is HumanNPC humanNPC)
		{
			Vector3 position = spawnPoint.transform.position;
			bool num = TerrainMeta.BiomeMap.GetBiomeMaxType(position) == 16;
			bool flag = EnvironmentManager.Check(position, EnvironmentType.TrainTunnels | EnvironmentType.UnderwaterLab | EnvironmentType.Submarine);
			bool topology = TerrainMeta.TopologyMap.GetTopology(position, 128);
			if (num && !flag && !topology && SingletonComponent<SpawnHandler>.Instance.JungleLoadouts != null && SingletonComponent<SpawnHandler>.Instance.JungleLoadouts.Length != 0)
			{
				humanNPC.EquipLoadout(SingletonComponent<SpawnHandler>.Instance.JungleLoadouts);
			}
		}
	}

	protected GameObjectRef GetPrefab()
	{
		float num = prefabs.Sum((SpawnEntry x) => (!preventDuplicates || !HasSpawned(x.prefab.resourceID)) ? x.weight : 0);
		if (num == 0f)
		{
			return null;
		}
		float num2 = UnityEngine.Random.Range(0f, num);
		foreach (SpawnEntry prefab in prefabs)
		{
			int num3 = ((!preventDuplicates || !HasSpawned(prefab.prefab.resourceID)) ? prefab.weight : 0);
			if ((num2 -= (float)num3) <= 0f)
			{
				return prefab.prefab;
			}
		}
		return prefabs[prefabs.Count - 1].prefab;
	}

	protected virtual BaseSpawnPoint GetSpawnPoint(GameObjectRef prefabRef, out Vector3 pos, out Quaternion rot)
	{
		pos = Vector3.zero;
		rot = Quaternion.identity;
		bool flag = DoesRequireNavmeshToSpawn(prefabRef.Get());
		GameObjectEx.ToBaseEntity(base.transform);
		int num = UnityEngine.Random.Range(0, spawnPoints.Length);
		for (int i = 0; i < spawnPoints.Length; i++)
		{
			BaseSpawnPoint baseSpawnPoint = spawnPoints[(num + i) % spawnPoints.Length];
			if (!(baseSpawnPoint == null) && (baseSpawnPoint.IsAvailableTo(prefabRef.Get()) || AllowOverlappingSpawns) && !baseSpawnPoint.HasPlayersIntersecting())
			{
				baseSpawnPoint.GetLocation(out pos, out rot);
				if (!flag)
				{
					return baseSpawnPoint;
				}
				if (RustNavMeshHelpers.SamplePosition(pos, out var hitWS, 2f, -1))
				{
					pos = hitWS.position;
					return baseSpawnPoint;
				}
				if (AI.logIssues)
				{
					Debug.LogWarning($"Failed to spawn {prefabRef.Get()} at {pos} - no navmesh found");
				}
			}
		}
		return null;
	}

	private static bool DoesRequireNavmeshToSpawn(GameObject prefab)
	{
		if (!AI.npc_check_spawner_is_on_navmesh)
		{
			return false;
		}
		if (prefab.TryGetComponent<BaseNavigator>(out var component) && !component.CanUseNavMesh)
		{
			return false;
		}
		if (prefab.TryGetComponent<RustNavMeshAgent>(out var _))
		{
			return true;
		}
		return false;
	}

	private MonumentInfo FindMonument()
	{
		return GetComponentInParent<MonumentInfo>();
	}

	protected virtual void OnDrawGizmos()
	{
		Gizmos.color = new Color(1f, 1f, 0f, 1f);
		Gizmos.DrawSphere(base.transform.position, 0.25f);
	}

	public void SetIsSpawningActive(bool isActive)
	{
		isSpawnerActive = isActive;
	}
}
