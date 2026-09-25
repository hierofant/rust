using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GamePhysicsJobs;

[BurstCompile]
public struct FilterRaycastHitsJob : IJob
{
	public NativeList<RaycastHit> ColliderHits;

	public NativeList<int> ColliderIndices;

	public NativeList<Vector3> WaterHits;

	public NativeList<int> WaterIndices;

	public NativeArray<RaycastHit>.ReadOnly Hits;

	public int HitsPerBatch;

	public void Execute()
	{
		int num = Hits.Length / HitsPerBatch;
		for (int i = 0; i < num; i++)
		{
			int num2 = i * HitsPerBatch;
			int num3 = num2 + HitsPerBatch;
			for (int j = num2; j < num3; j++)
			{
				RaycastHit value = Hits[j];
				if (value.normal == Vector3.zero)
				{
					break;
				}
				if (value.colliderInstanceID != 0)
				{
					ColliderHits.AddNoResize(value);
					ColliderIndices.AddNoResize(j);
				}
				else
				{
					WaterHits.AddNoResize(value.point);
					WaterIndices.AddNoResize(j);
				}
			}
		}
	}
}
