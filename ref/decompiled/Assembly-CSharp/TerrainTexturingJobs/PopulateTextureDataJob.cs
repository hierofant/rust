using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace TerrainTexturingJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
internal struct PopulateTextureDataJob : IJobParallelFor
{
	[WriteOnly]
	public NativeArray<half4> colors;

	public NativeArray<Vector4>.ReadOnly vectors;

	public NativeArray<float>.ReadOnly distances;

	public void Execute(int index)
	{
		ref readonly Vector4 @readonly = ref BurstUtil.GetReadonly(in vectors, index);
		colors[index] = new half4(math.half(@readonly.x), math.half(@readonly.y), math.half(distances[index]), math.half(@readonly.w));
	}
}
