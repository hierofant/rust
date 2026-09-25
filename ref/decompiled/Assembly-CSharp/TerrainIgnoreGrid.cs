using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class TerrainIgnoreGrid : ICoarseQueryGridProvider, IDisposable
{
	private CoarseQueryGrid _queryGrid;

	private const int CellSize = 8;

	public TerrainIgnoreGrid()
	{
		_queryGrid = new CoarseQueryGrid(8, (int)(World.Size + 1000), -5f);
	}

	public CoarseQueryGrid GetQueryGrid()
	{
		return _queryGrid;
	}

	public void AddTrigger(TerrainCollisionTrigger trigger)
	{
		_queryGrid.AddStatic(trigger.volume.trigger.bounds);
	}

	public void RemoveTrigger(TerrainCollisionTrigger trigger)
	{
		_queryGrid.RemoveStatic(trigger.volume.trigger.bounds);
	}

	public bool Check(Vector3 pos, float radius)
	{
		using (TimeWarning.New("TerrainIgnoreGrid.Check(pos,rad)"))
		{
			return _queryGrid.CheckJob(pos, radius);
		}
	}

	public JobHandle Check(NativeArray<Vector3>.ReadOnly starts, NativeArray<float>.ReadOnly radii, NativeList<int> results)
	{
		return _queryGrid.CheckJob(starts, radii, results);
	}

	public JobHandle CheckIndirect(NativeArray<Vector3>.ReadOnly pos, NativeArray<float>.ReadOnly radii, NativeArray<int>.ReadOnly indices, NativeList<int> results)
	{
		return _queryGrid.CheckJobIndirect(pos, radii, indices, results);
	}

	public bool Check(Vector3 pos)
	{
		using (TimeWarning.New("TerraingIgnoreGrid.Check(pos)"))
		{
			return _queryGrid.CheckJob(pos, 0f);
		}
	}

	public void Dispose()
	{
		_queryGrid.Dispose();
	}
}
