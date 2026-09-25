using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GamePhysicsJobs;

[BurstCompile]
public struct SelectNearestHitsJob : IJob
{
	[WriteOnly]
	public NativeArray<RaycastHit> Results;

	[Unity.Collections.ReadOnly]
	public NativeArray<RaycastHit>.ReadOnly Hits;

	[Unity.Collections.ReadOnly]
	public int HitsPerBatch;

	public void Execute()
	{
		if (HitsPerBatch < 1)
		{
			Debug.LogError($"Invalid HitsPerBatch: {HitsPerBatch}");
			return;
		}
		int num = Hits.Length / HitsPerBatch;
		for (int i = 0; i < num; i++)
		{
			int num2 = -1;
			float num3 = float.MaxValue;
			for (int j = 0; j < HitsPerBatch; j++)
			{
				RaycastHit raycastHit = Hits[i * HitsPerBatch + j];
				if (raycastHit.normal == Vector3.zero)
				{
					break;
				}
				if (raycastHit.distance < num3)
				{
					num3 = raycastHit.distance;
					num2 = j;
				}
			}
			if (num2 != -1)
			{
				Results[i] = Hits[i * HitsPerBatch + num2];
			}
			else
			{
				Results[i] = default(RaycastHit);
			}
		}
	}
}
