using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GamePhysicsJobs;

[BurstCompile]
public struct RemoveInvalidRaycastHitsJob : IJob
{
	public NativeArray<RaycastHit> Hits;

	public NativeArray<bool>.ReadOnly AreValid;

	public int HitsPerBatch;

	public void Execute()
	{
		int num = Hits.Length / HitsPerBatch;
		for (int i = 0; i < num; i++)
		{
			int num2 = i * HitsPerBatch;
			int num3 = num2 + HitsPerBatch;
			int num4 = num2;
			for (int j = num2; j < num3 && !(Hits[j].normal == Vector3.zero); j++)
			{
				if (AreValid[j])
				{
					Hits[num4++] = Hits[j];
				}
			}
			if (num4 < num3)
			{
				Hits[num4] = default(RaycastHit);
			}
		}
	}
}
