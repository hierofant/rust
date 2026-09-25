using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GamePhysicsJobs;

[BurstCompile]
public struct SelectNearestNHitsJob : IJob
{
	[WriteOnly]
	public NativeArray<RaycastHit> Results;

	[Unity.Collections.ReadOnly]
	public NativeArray<RaycastHit>.ReadOnly Hits;

	[Unity.Collections.ReadOnly]
	public int HitsPerBatch;

	[Unity.Collections.ReadOnly]
	public int SelectCount;

	public void Execute()
	{
		if (HitsPerBatch < 1)
		{
			Debug.LogError($"Invalid HitsPerBatch: {HitsPerBatch}");
			return;
		}
		if (SelectCount > HitsPerBatch)
		{
			Debug.LogError($"Invalid SelectCount: {SelectCount}");
			return;
		}
		int num = Hits.Length / HitsPerBatch;
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < SelectCount; j++)
			{
				RaycastHit value = Hits[i * HitsPerBatch + j];
				if (value.normal == Vector3.zero)
				{
					Results[i * SelectCount + j] = default(RaycastHit);
					break;
				}
				Results[i * SelectCount + j] = value;
			}
		}
	}
}
