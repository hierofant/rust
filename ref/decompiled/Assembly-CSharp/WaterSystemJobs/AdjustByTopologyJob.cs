using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace WaterSystemJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
internal struct AdjustByTopologyJob : IJobParallelForDefer
{
	[Unity.Collections.ReadOnly]
	public NativeList<Ray> rays;

	public NativeArray<bool> hitResults;

	public NativeArray<Vector3> hitNormals;

	public NativeArray<Vector3>.ReadOnly hitPositions;

	[Unity.Collections.ReadOnly]
	public NativeArray<int> TopologyData;

	[Unity.Collections.ReadOnly]
	public int TopologyRes;

	[Unity.Collections.ReadOnly]
	public Vector2 DataOrigin;

	[Unity.Collections.ReadOnly]
	public Vector2 DataScale;

	public void Execute(int index)
	{
		if (hitResults[index])
		{
			hitNormals[index] = Vector3.up;
			float num = (hitPositions[index].x - DataOrigin.x) * DataScale.x;
			float num2 = (hitPositions[index].z - DataOrigin.y) * DataScale.y;
			int num3 = Math.Clamp((int)(num * (float)TopologyRes), 0, TopologyRes - 1);
			int index2 = Math.Clamp((int)(num2 * (float)TopologyRes), 0, TopologyRes - 1) * TopologyRes + num3;
			hitResults[index] = (TopologyData[index2] & 0x180) != 0;
		}
	}
}
