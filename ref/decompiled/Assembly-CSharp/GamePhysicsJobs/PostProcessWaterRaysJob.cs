using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GamePhysicsJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
internal struct PostProcessWaterRaysJob : IJob
{
	public NativeArray<RaycastHit> hits;

	public NativeArray<Ray> rays;

	public NativeList<Vector2i> WaterIndices;

	public NativeArray<bool> hitsSub;

	public NativeArray<Vector3> positionsSub;

	public NativeArray<Vector3> normalsSub;

	public void Execute()
	{
		int num = 0;
		for (int i = 0; i < WaterIndices.Length; i++)
		{
			Vector2i vector2i = WaterIndices[i];
			int x = vector2i.x;
			int y = vector2i.y;
			int index = num++;
			if (hitsSub[index])
			{
				Vector3 vector = positionsSub[index];
				Ray ray = rays[index];
				RaycastHit raycastHit = default(RaycastHit);
				raycastHit.point = vector;
				raycastHit.normal = normalsSub[index];
				raycastHit.distance = (vector - ray.origin).magnitude;
				RaycastHit value = raycastHit;
				hits[x++] = value;
			}
			if (x < y)
			{
				hits[x] = default(RaycastHit);
			}
		}
	}
}
