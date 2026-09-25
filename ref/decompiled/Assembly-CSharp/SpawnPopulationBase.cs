using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ConVar;
using UnityEngine;

public abstract class SpawnPopulationBase : BaseScriptableObject
{
	public enum Status
	{
		Success,
		InvalidSample,
		PrefabPickFailed,
		PrefabRejected,
		InvalidSpawnPosOverride,
		DensityOverflow,
		InvalidEntity
	}

	public interface ISpawnHandler
	{
		Status TrySpawn(SpawnPopulationBase pop, Prefab<Spawnable> prefab, Vector3 pos, Quaternion rot, out GameObject spawned);

		void ReportAttempt(Status status, Vector3 pos);
	}

	public string ResourceFolder = string.Empty;

	public GameObjectRef[] ResourceList;

	public bool EnforcePopulationLimits = true;

	public float SpawnRate = 1f;

	public bool ScaleWithServerPopulation;

	public Prefab<Spawnable>[] Prefabs;

	public int[] numToSpawn;

	private HashSet<Spawnable> spawnables = new HashSet<Spawnable>();

	private int[] spawnCounts = new int[10];

	private int spawnCountsIndex;

	public int FailedFillsInARow { get; private set; }

	public int AvgSpawnCount
	{
		get
		{
			if (spawnCounts[0] == 0)
			{
				return 0;
			}
			int num = 0;
			int i;
			for (i = 0; i < spawnCounts.Length && spawnCounts[i] != 0; i++)
			{
				num += spawnCounts[i];
			}
			return num / i;
		}
	}

	public ICollection<Spawnable> GetSpawnables()
	{
		return spawnables;
	}

	public virtual bool Initialize()
	{
		if (Prefabs == null || Prefabs.Length == 0)
		{
			if (!string.IsNullOrEmpty(ResourceFolder))
			{
				Prefabs = Prefab.Load<Spawnable>("assets/bundled/prefabs/autospawn/" + ResourceFolder, GameManager.server, PrefabAttribute.server, useProbabilities: false);
			}
			if (ResourceList != null && ResourceList.Length != 0)
			{
				List<string> list = new List<string>();
				GameObjectRef[] resourceList = ResourceList;
				foreach (GameObjectRef gameObjectRef in resourceList)
				{
					string resourcePath = gameObjectRef.resourcePath;
					if (string.IsNullOrEmpty(resourcePath))
					{
						Debug.LogWarning(base.name + " resource list contains invalid resource path for GUID " + gameObjectRef.guid, this);
					}
					else
					{
						list.Add(resourcePath);
					}
				}
				Prefabs = Prefab.Load<Spawnable>(list.ToArray(), GameManager.server, PrefabAttribute.server);
			}
			if (Prefabs == null || Prefabs.Length == 0)
			{
				return false;
			}
			numToSpawn = new int[Prefabs.Length];
		}
		return true;
	}

	public float GetCurrentSpawnRate()
	{
		if (ScaleWithServerPopulation)
		{
			return SpawnRate * SpawnHandler.PlayerLerp(Spawn.min_rate, Spawn.max_rate);
		}
		return SpawnRate * Spawn.max_rate;
	}

	public void Fill(SpawnHandler spawnHandler, SpawnDistribution distribution, int numToFill, bool initialSpawn)
	{
		if (GetTargetCount(distribution) == 0)
		{
			return;
		}
		if (!Initialize())
		{
			Debug.LogError("[Spawn] No prefabs to spawn: " + base.name, this);
			return;
		}
		if (Global.developer > 1)
		{
			Debug.Log("[Spawn] Population " + base.name + " needs to spawn " + numToFill);
		}
		int count = distribution.Count;
		SubFill(spawnHandler, distribution, numToFill, initialSpawn);
		if (initialSpawn)
		{
			return;
		}
		if (distribution.Count - count != numToFill)
		{
			FailedFillsInARow++;
			spawnCounts[spawnCountsIndex] = distribution.Count - count;
			spawnCountsIndex = (spawnCountsIndex + 1) % spawnCounts.Length;
			return;
		}
		FailedFillsInARow = 0;
		for (int i = 0; i < spawnCounts.Length; i++)
		{
			spawnCounts[i] = 0;
		}
		spawnCountsIndex = 0;
	}

	public abstract void SubFill(ISpawnHandler spawnHandler, SpawnDistribution distribution, int numToFill, bool initialSpawn);

	public abstract int EstimateMaxAttempts(int toSpawn);

	public void DeleteEntities()
	{
		Spawnable[] array = GetSpawnables().ToArray();
		foreach (Spawnable spawnable in array)
		{
			if (spawnable.gameObject == null)
			{
				Debug.LogWarning("Trying to delete spawnable that has already been destroyed: " + base.name);
				continue;
			}
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(spawnable.gameObject);
			if (!baseEntity.IsValid())
			{
				Debug.LogWarning("Trying to delete spawnable that is invalid: " + base.name);
			}
			else
			{
				baseEntity.Kill();
			}
		}
	}

	public abstract byte[] GetBaseMapValues(int populationRes);

	public abstract int GetTargetCount(SpawnDistribution distribution);

	public abstract SpawnFilter GetSpawnFilter();

	public void GetReportString(StringBuilder sb, bool detailed)
	{
		if (!string.IsNullOrEmpty(ResourceFolder))
		{
			sb.AppendLine(base.name + " (autospawn/" + ResourceFolder + ")");
		}
		else
		{
			sb.AppendLine(base.name);
		}
		if (!detailed)
		{
			return;
		}
		sb.AppendLine("\tPrefabs:");
		if (Prefabs != null)
		{
			Prefab<Spawnable>[] prefabs = Prefabs;
			foreach (Prefab<Spawnable> prefab in prefabs)
			{
				int num = spawnables.Count((Spawnable x) => (x.GetComponent<BaseNetworkable>()?.prefabID ?? 0) == prefab.ID);
				sb.AppendLine($"\t\t{Path.GetFileNameWithoutExtension(prefab.Name)} : {num}");
			}
		}
		else
		{
			sb.AppendLine("\t\tN/A");
		}
	}

	public void AddInstance(Spawnable spawnable)
	{
		spawnables.Add(spawnable);
	}

	public void RemoveInstance(Spawnable spawnable)
	{
		spawnables.Remove(spawnable);
	}
}
