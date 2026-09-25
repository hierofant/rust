using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace TerrainTexturingJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
internal struct FillAsOceanTopologyJob : IJobParallelFor
{
	public NativeArray<Vector4> vectors;

	public void Execute(int index)
	{
		BurstUtil.Get(in vectors, index).w = 1f;
	}
}
