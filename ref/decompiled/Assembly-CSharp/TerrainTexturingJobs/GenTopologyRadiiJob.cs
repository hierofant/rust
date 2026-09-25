using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace TerrainTexturingJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
internal struct GenTopologyRadiiJob : IJobParallelFor
{
	public NativeArray<float>.ReadOnly heights;

	public NativeArray<float> radii;

	public void Execute(int index)
	{
		float value = heights[index];
		float t = Mathf.InverseLerp(4f, 0f, value);
		float value2 = Mathf.Lerp(8f, 16f, t);
		radii[index] = value2;
	}
}
