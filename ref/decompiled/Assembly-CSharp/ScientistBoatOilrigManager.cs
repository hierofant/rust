using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class ScientistBoatOilrigManager : MonoBehaviour
{
	private BoatGroupSpawner _spawner;

	private HashSet<RHIB> _spawnedBoats = new HashSet<RHIB>();

	public void AIDestroyed(RHIB rhib)
	{
		_spawnedBoats.Remove(rhib);
	}

	public void OnPuzzleReset()
	{
		if (_spawnedBoats == null)
		{
			_spawnedBoats = new HashSet<RHIB>();
		}
		if (_spawnedBoats.Count > 0)
		{
			using PooledList<RHIB> pooledList = Pool.Get<PooledList<RHIB>>();
			pooledList.AddRange(_spawnedBoats);
			foreach (RHIB item in pooledList)
			{
				if (item != null && !item.IsDestroyed)
				{
					item.AdminKillNoLoot();
				}
			}
			_spawnedBoats.Clear();
		}
		if (_spawner == null)
		{
			_spawner = GetComponent<BoatGroupSpawner>();
		}
		_spawner.SpawnBoatGroup(_spawnedBoats, BoatAI.AILoadMode.KillBoat, spawnsPT: false, this);
	}
}
