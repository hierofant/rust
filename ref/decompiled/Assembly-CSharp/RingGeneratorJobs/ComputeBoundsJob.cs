using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace RingGeneratorJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
public struct ComputeBoundsJob : IJob
{
	[Unity.Collections.ReadOnly]
	public NativeArray<float4> Segments;

	[WriteOnly]
	public NativeArray<float2> Bounds;

	public void Execute()
	{
		float2 @float = new float2(float.MaxValue, float.MaxValue);
		float2 float2 = new float2(float.MinValue, float.MinValue);
		for (int i = 0; i < Segments.Length; i++)
		{
			float4 float3 = Segments[i];
			@float = math.min(@float, math.min(float3.xy, float3.zw));
			float2 = math.max(float2, math.max(float3.xy, float3.zw));
		}
		Bounds[0] = @float;
		Bounds[1] = float2;
	}
}
