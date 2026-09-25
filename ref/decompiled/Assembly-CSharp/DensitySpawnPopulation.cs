using System.Threading.Tasks;
using ConVar;
using Facepunch;
using Rust.Ai.Gen2;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Rust/Density Spawn Population")]
public class DensitySpawnPopulation : SpawnPopulationBase
{
	[FormerlySerializedAs("TargetDensity")]
	[Header("Spawn Info")]
	[Tooltip("Usually per square km")]
	[SerializeField]
	public float _targetDensity = 1f;

	public int ClusterSizeMin = 1;

	public int ClusterSizeMax = 1;

	public int ClusterDithering;

	public int SpawnAttemptsInitial = 20;

	public int SpawnAttemptsRepeating = 10;

	public bool ScaleWithLargeMaps = true;

	public bool ScaleWithSpawnFilter = true;

	public bool AlignToNormal;

	public SpawnFilter Filter = new SpawnFilter();

	public float FilterCutoff;

	public float FilterRadius;

	public bool FilterOutTutorialIslands;

	public MonumentType[] FilterOutMonuments;

	public float NpcRadiusCheckDistance;

	[Tooltip("Reject spawn positions further than this from actual water (0 = disabled). Topology like Swamp can be painted on terrain with no water body, so water-dependent NPCs need a real water check.")]
	public float MaxDistanceFromWater;

	private int sumToSpawn;

	public virtual float TargetDensity => _targetDensity;

	public override void SubFill(ISpawnHandler spawnHandler, SpawnDistribution distribution, int numToFill, bool initialSpawn)
	{
		float num = Mathf.Max(ClusterSizeMax, distribution.GetGridCellArea() * GetMaximumSpawnDensity());
		UpdateWeights(distribution, GetTargetCount(distribution));
		int num2 = (initialSpawn ? (numToFill * SpawnAttemptsInitial) : (numToFill * SpawnAttemptsRepeating));
		while (numToFill >= ClusterSizeMax && num2 > 0 && sumToSpawn > 0)
		{
			ByteQuadtree.Element node = distribution.SampleNode();
			int f = Random.Range(ClusterSizeMin, ClusterSizeMax + 1);
			f = Mathx.Min(num2, numToFill, f, sumToSpawn);
			for (int i = 0; i < f; i++)
			{
				Vector3 spawnPos;
				Quaternion spawnRot;
				bool flag = distribution.Sample(out spawnPos, out spawnRot, node, AlignToNormal, ClusterDithering, 0f, Filter, FilterCutoff);
				if (flag && FilterOutTutorialIslands && TutorialIsland.WorldBoundsMinusTutorialIslands.size != Vector3.zero)
				{
					flag = TutorialIsland.WorldBoundsMinusTutorialIslands.Contains(spawnPos);
				}
				if (flag && FilterRadius > 0f)
				{
					flag = Filter.GetFactor(spawnPos + Vector3.forward * FilterRadius) > 0f && Filter.GetFactor(spawnPos - Vector3.forward * FilterRadius) > 0f && Filter.GetFactor(spawnPos + Vector3.right * FilterRadius) > 0f && Filter.GetFactor(spawnPos - Vector3.right * FilterRadius) > 0f;
				}
				if (flag && NpcRadiusCheckDistance > 0f)
				{
					using PooledList<BaseNPC2> pooledList = Facepunch.Pool.Get<PooledList<BaseNPC2>>();
					BaseEntity.Query.Server.GetBrainsInSphere(spawnPos, NpcRadiusCheckDistance, pooledList);
					flag = pooledList.Count == 0;
				}
				if (flag && MaxDistanceFromWater > 0f)
				{
					flag = TerrainTexturing.Instance != null && TerrainTexturing.Instance.GetCoarseDistanceToShore(spawnPos) > 0f - MaxDistanceFromWater;
				}
				if (flag && FilterOutMonuments != null && FilterOutMonuments.Length != 0)
				{
					flag = TerrainMeta.Path.FindMonumentWithBoundsOverlap(spawnPos, FilterOutMonuments) == null;
				}
				if (flag)
				{
					if (TryTakeRandomPrefab(out var result))
					{
						Status status = Status.Success;
						if (!GetSpawnPosOverride(result, ref spawnPos, ref spawnRot))
						{
							status = Status.InvalidSpawnPosOverride;
						}
						if (status == Status.Success && (float)distribution.GetCount(spawnPos) >= num)
						{
							status = Status.DensityOverflow;
						}
						if (status == Status.Success)
						{
							status = spawnHandler.TrySpawn(this, result, spawnPos, spawnRot, out var _);
						}
						if (status == Status.Success)
						{
							numToFill--;
						}
						else
						{
							ReturnPrefab(result);
						}
						spawnHandler.ReportAttempt(status, spawnPos);
					}
					else
					{
						spawnHandler.ReportAttempt(Status.PrefabPickFailed, spawnPos);
					}
				}
				else
				{
					spawnHandler.ReportAttempt(Status.InvalidSample, spawnPos);
				}
				num2--;
			}
		}
	}

	public override int EstimateMaxAttempts(int toSpawn)
	{
		return toSpawn * SpawnAttemptsRepeating;
	}

	public void UpdateWeights(SpawnDistribution distribution, int targetCount)
	{
		float num = 0f;
		for (int i = 0; i < Prefabs.Length; i++)
		{
			Prefab<Spawnable> prefab = Prefabs[i];
			float prefabWeight = GetPrefabWeight(prefab);
			num += prefabWeight;
		}
		int num2 = Mathf.CeilToInt((float)targetCount / num);
		sumToSpawn = 0;
		for (int j = 0; j < Prefabs.Length; j++)
		{
			Prefab<Spawnable> prefab2 = Prefabs[j];
			float num3 = GetPrefabWeight(prefab2);
			if (prefab2.Weight != null && prefab2.Weight.IsActiveInEra())
			{
				num3 *= prefab2.Weight.Scale;
			}
			int count = distribution.GetCount(prefab2.ID);
			int num4 = Mathf.Max(Mathf.FloorToInt(num3 * (float)num2 - (float)count), 0);
			numToSpawn[j] = num4;
			sumToSpawn += num4;
		}
	}

	protected virtual float GetPrefabWeight(Prefab<Spawnable> prefab)
	{
		if (!prefab.Parameters)
		{
			return 1f;
		}
		return prefab.Parameters.Count;
	}

	public bool TryTakeRandomPrefab(out Prefab<Spawnable> result)
	{
		int num = Random.Range(0, sumToSpawn);
		for (int i = 0; i < Prefabs.Length; i++)
		{
			if ((num -= numToSpawn[i]) < 0)
			{
				numToSpawn[i]--;
				sumToSpawn--;
				result = Prefabs[i];
				return true;
			}
		}
		result = null;
		return false;
	}

	public void ReturnPrefab(Prefab<Spawnable> prefab)
	{
		if (prefab == null)
		{
			return;
		}
		for (int i = 0; i < Prefabs.Length; i++)
		{
			if (Prefabs[i] == prefab)
			{
				numToSpawn[i]++;
				sumToSpawn++;
			}
		}
	}

	public float GetCurrentSpawnDensity()
	{
		if (ScaleWithServerPopulation)
		{
			return TargetDensity * SpawnHandler.PlayerLerp(Spawn.min_density, Spawn.max_density) * 1E-06f;
		}
		return TargetDensity * Spawn.max_density * 1E-06f;
	}

	public float GetMaximumSpawnDensity()
	{
		if (ScaleWithServerPopulation)
		{
			return 2f * TargetDensity * SpawnHandler.PlayerLerp(Spawn.min_density, Spawn.max_density) * 1E-06f;
		}
		return 2f * TargetDensity * Spawn.max_density * 1E-06f;
	}

	public virtual bool GetSpawnPosOverride(Prefab<Spawnable> prefab, ref Vector3 newPos, ref Quaternion newRot)
	{
		return true;
	}

	public override byte[] GetBaseMapValues(int populationRes)
	{
		byte[] baseValues = new byte[populationRes * populationRes];
		SpawnFilter filter = Filter;
		float cutoff = FilterCutoff;
		Parallel.For(0, populationRes, delegate(int z)
		{
			for (int i = 0; i < populationRes; i++)
			{
				float normX = ((float)i + 0.5f) / (float)populationRes;
				float normZ = ((float)z + 0.5f) / (float)populationRes;
				float factor = filter.GetFactor(normX, normZ);
				baseValues[z * populationRes + i] = (byte)((factor > cutoff) ? (255f * factor) : 0f);
			}
		});
		return baseValues;
	}

	public override int GetTargetCount(SpawnDistribution distribution)
	{
		float num = TerrainMeta.Size.x * TerrainMeta.Size.z;
		float num2 = GetCurrentSpawnDensity();
		if (!ScaleWithLargeMaps)
		{
			num = Mathf.Min(num, 16000000f);
		}
		if (ScaleWithSpawnFilter)
		{
			num2 *= distribution.Density;
		}
		float num3 = 1f;
		if (Prefabs != null && Prefabs.Length != 0)
		{
			float num4 = 0f;
			Prefab<Spawnable>[] prefabs = Prefabs;
			foreach (Prefab<Spawnable> prefab in prefabs)
			{
				num4 = ((prefab == null || !(prefab.Weight != null) || !prefab.Weight.IsActiveInEra()) ? (num4 + 1f) : (num4 + prefab.Weight.Scale));
			}
			num3 = num4 / (float)Prefabs.Length;
		}
		return Mathf.RoundToInt(num * num2 * num3);
	}

	public override SpawnFilter GetSpawnFilter()
	{
		return Filter;
	}
}
