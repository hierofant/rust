using System.Collections;
using ConVar;
using UnityEngine;

public class DeepSeaIsland : BaseEntity, IDeepSeaSpawner
{
	public enum IslandType
	{
		Horseshoe,
		Blob,
		Round,
		Line
	}

	public MeshTerrainRoot meshTerrain;

	public MonumentNavMesh monumentNavMesh;

	public GameObjectRef MapMarker;

	public IslandType Variant;

	private SpawnGroup[] _spawnGroups = new SpawnGroup[0];

	public override void ServerInit()
	{
		base.ServerInit();
		DeepSeaManager.ServerIslands.Add(this);
		if (MapMarker.isValid)
		{
			BaseEntity baseEntity = GameManager.server.CreateEntity(MapMarker.resourcePath, base.transform.position, base.transform.rotation);
			if (baseEntity is UIDeepSeaIslandMapMarker uIDeepSeaIslandMapMarker)
			{
				uIDeepSeaIslandMapMarker.IslandType = Variant;
			}
			baseEntity.Spawn();
		}
		if (CollectionEx.IsEmpty(_spawnGroups))
		{
			GetAllSpawnGroups();
		}
		BakedShoreVectors bakedShoreVectors = PrefabAttribute.server.Find<BakedShoreVectors>(prefabID);
		if (bakedShoreVectors != null)
		{
			Invoke(delegate
			{
				TerrainMeta.Texturing.ApplyBakedDeepSeaVectors(bakedShoreVectors, base.transform);
			}, 0f);
		}
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		DeepSeaManager.ServerIslands.Remove(this);
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
				yield return DeepSeaManager.WaitSpawnGroupInterval;
			}
		}
		yield return TriggerDwellingSpawnGroups();
	}

	private IEnumerator TriggerDwellingSpawnGroups()
	{
		SpawnGroup[] spawnGroups = _spawnGroups;
		foreach (SpawnGroup spawnGroup in spawnGroups)
		{
			foreach (SpawnPointInstance spawnInstance in spawnGroup.SpawnInstances)
			{
				BaseEntity entity = spawnInstance.Entity;
				if (!(entity != null) || !(entity is NPCDwelling { spawnGroups: var spawnGroups2 }))
				{
					continue;
				}
				foreach (SpawnGroup spawnGroup2 in spawnGroups2)
				{
					bool flag = DeepSeaManager.IsRespawnVariant(spawnGroup2);
					DeepSeaManager.ApplyPopulationScale(spawnGroup2, flag ? DeepSea.loot_respawn_scale : DeepSea.loot_scale);
					if (!flag && !spawnGroup2.HasSpawnedAny() && !spawnGroup2.wantsInitialSpawn)
					{
						spawnGroup2.Fill();
						yield return null;
					}
				}
			}
		}
	}

	public void GenerateNavMesh()
	{
		if (monumentNavMesh == null || !AI.move)
		{
			if (!AI.move)
			{
				Invoke(GenerateNavMesh, 5f);
			}
		}
		else
		{
			StartCoroutine(UpdateNavMesh());
		}
	}

	public IEnumerator UpdateNavMesh()
	{
		yield return StartCoroutine(monumentNavMesh.UpdateNavMeshAndWait());
	}

	public override void AdminKill()
	{
	}

	public override void PreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		base.PreProcess(preProcess, rootObj, name, serverside, clientside, bundling);
		meshTerrain = GetComponentInChildren<MeshTerrainRoot>();
		if (!clientside)
		{
			GetAllSpawnGroups();
		}
	}

	public void GetAllSpawnGroups()
	{
		_spawnGroups = GetComponentsInChildren<SpawnGroup>();
	}
}
