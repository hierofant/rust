using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace RingGeneratorJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
public struct SilhouetteSweepJob : IJobParallelFor
{
	[Unity.Collections.ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float4> Segments;

	[Unity.Collections.ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float2> Bounds;

	[Unity.Collections.ReadOnly]
	public NativeArray<float2> Directions;

	[WriteOnly]
	public NativeArray<float> BestT;

	public void Execute(int index)
	{
		float2 @float = (Bounds[0] + Bounds[1]) * 0.5f;
		float2 float2 = Directions[index];
		float num = -1f;
		for (int i = 0; i < Segments.Length; i++)
		{
			float4 float3 = Segments[i];
			float2 xy = float3.xy;
			float2 float4 = float3.zw - xy;
			float num2 = float2.x * float4.y - float2.y * float4.x;
			if (!(math.abs(num2) < 1E-09f))
			{
				float2 float5 = xy - @float;
				float num3 = (float5.x * float4.y - float5.y * float4.x) / num2;
				float num4 = (float5.x * float2.y - float5.y * float2.x) / num2;
				if (num3 > 0f && num4 >= 0f && num4 <= 1f && num3 > num)
				{
					num = num3;
				}
			}
		}
		BestT[index] = num;
	}
}
