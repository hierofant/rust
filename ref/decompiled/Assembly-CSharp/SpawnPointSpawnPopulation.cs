using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Spawn Point Spawn Population")]
public class SpawnPointSpawnPopulation : SpawnPopulationBase
{
	[SerializeField]
	private BaseSpawnPoint.SpawnPointType spawnPointType;

	private SpawnFilter Filter = new SpawnFilter();

	public override void SubFill(ISpawnHandler spawnHandler, SpawnDistribution distribution, int numToFill, bool initialSpawn)
	{
		if (numToFill == 0)
		{
			return;
		}
		if (!TryGetSpawnPoints(out var result))
		{
			Debug.LogWarning(base.name + " couldn't find any spawn points of type: " + spawnPointType, this);
			return;
		}
		foreach (BaseSpawnPoint item in result)
		{
			Prefab<Spawnable> prefab = Prefabs[Random.Range(0, Prefabs.Length)];
			if (item != null && item.IsAvailableTo(prefab.Object))
			{
				item.GetLocation(out var pos, out var rot);
				GameObject spawned;
				Status status = spawnHandler.TrySpawn(this, prefab, pos, rot, out spawned);
				spawnHandler.ReportAttempt(status, pos);
				numToFill--;
				if (numToFill == 0)
				{
					break;
				}
			}
		}
	}

	public override int EstimateMaxAttempts(int toSpawn)
	{
		return toSpawn;
	}

	public override byte[] GetBaseMapValues(int populationRes)
	{
		return new byte[0];
	}

	public override SpawnFilter GetSpawnFilter()
	{
		return Filter;
	}

	public override int GetTargetCount(SpawnDistribution distribution)
	{
		if (TryGetSpawnPoints(out var result))
		{
			return result.Count;
		}
		return 0;
	}

	private bool TryGetSpawnPoints(out List<BaseSpawnPoint> result)
	{
		return BaseSpawnPoint.spawnPoints.TryGetValue(spawnPointType, out result);
	}
}
