using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class WaterVisibilityGrid : ICoarseQueryGridProvider, IDisposable
{
	private CoarseQueryGrid _queryGrid;

	private const int CellSize = 8;

	private readonly ListHashSet<WaterVisibilityTrigger> _dynamicListSet;

	public WaterVisibilityGrid()
	{
		_queryGrid = new CoarseQueryGrid(8, (int)(World.Size + 1000), -5f);
		_dynamicListSet = new ListHashSet<WaterVisibilityTrigger>();
	}

	public CoarseQueryGrid GetQueryGrid()
	{
		_queryGrid.PrepareForDynamicPopulate(_dynamicListSet.Count);
		foreach (WaterVisibilityTrigger value in _dynamicListSet.Values)
		{
			if (!(value == null) && !(value.volume == null) && !(value.volume.trigger == null))
			{
				_queryGrid.AddDynamic(value.volume.trigger.bounds);
			}
		}
		return _queryGrid;
	}

	public void AddTrigger(WaterVisibilityTrigger trigger)
	{
		if (trigger.IsDynamic)
		{
			_dynamicListSet.Add(trigger);
		}
		else
		{
			_queryGrid.AddStatic(trigger.volume.trigger.bounds);
		}
	}

	public void RemoveTrigger(WaterVisibilityTrigger trigger)
	{
		if (trigger.IsDynamic)
		{
			_dynamicListSet.Remove(trigger);
		}
		else
		{
			_queryGrid.RemoveStatic(trigger.volume.trigger.bounds);
		}
	}

	public bool Check(Bounds bounds)
	{
		return GetQueryGrid().CheckJob(bounds);
	}

	public bool Check(Vector3 worldPosition, float radius)
	{
		return GetQueryGrid().CheckJob(worldPosition, radius);
	}

	public JobHandle Check(NativeArray<Vector3>.ReadOnly positions, NativeArray<float>.ReadOnly radii, NativeList<int> results)
	{
		return GetQueryGrid().CheckJob(positions, radii, results);
	}

	public JobHandle CheckIndirect(NativeArray<Vector3>.ReadOnly positions, NativeArray<float>.ReadOnly radii, NativeArray<int>.ReadOnly indices, NativeList<int> results)
	{
		return GetQueryGrid().CheckJobIndirect(positions, radii, indices, results);
	}

	public bool Check(Vector3 start, Vector3 end, float radius)
	{
		return GetQueryGrid().CheckJob(start, end, radius);
	}

	public JobHandle CheckIndirect(NativeArray<Vector3>.ReadOnly starts, NativeArray<Vector3>.ReadOnly ends, NativeArray<float>.ReadOnly radii, NativeArray<int>.ReadOnly indices, NativeList<int> results)
	{
		return GetQueryGrid().CheckJobIndirect(starts, ends, radii, indices, results);
	}

	public void Dispose()
	{
		_queryGrid.Dispose();
	}
}
