using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace TerrainTexturingJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
internal struct GenShoreVecBitMapJob : IJobParallelFor
{
	public NativeArray<float>.ReadOnly waterHeights;

	public NativeArray<float>.ReadOnly terrainHeights;

	[WriteOnly]
	public NativeArray<byte> bitmap;

	public void Execute(int index)
	{
		bool flag = Mathf.Max(waterHeights[index] - terrainHeights[index], 0f) <= 0f;
		bitmap[index] = (byte)(flag ? 255u : 0u);
	}
}
