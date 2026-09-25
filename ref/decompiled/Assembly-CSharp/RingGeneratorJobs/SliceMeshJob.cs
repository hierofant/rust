using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace RingGeneratorJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
public struct SliceMeshJob : IJobParallelForBatch
{
	[Unity.Collections.ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float3> Vertices;

	[Unity.Collections.ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int> Indices;

	public float4x4 ToLocal;

	public float PlaneY;

	public NativeList<float4>.ParallelWriter Segments;

	private const float PlaneEpsilon = 0.0001f;

	public void Execute(int startIndex, int count)
	{
		NativeList<float4> list = new NativeList<float4>(count, Allocator.Temp);
		for (int i = startIndex; i < startIndex + count; i++)
		{
			int num = i * 3;
			float3 @float = math.transform(ToLocal, Vertices[Indices[num]]);
			float3 float2 = math.transform(ToLocal, Vertices[Indices[num + 1]]);
			float3 float3 = math.transform(ToLocal, Vertices[Indices[num + 2]]);
			float num2 = @float.y - PlaneY;
			float num3 = float2.y - PlaneY;
			float num4 = float3.y - PlaneY;
			float3 float4 = new float3(num2, num3, num4);
			if (!math.all(float4 > 0.0001f) && !math.all(float4 < -0.0001f) && !math.all(math.abs(float4) <= 0.0001f))
			{
				float2 p = default(float2);
				float2 p2 = default(float2);
				int n = 0;
				AddCrossing(@float, float2, num2, num3, ref p, ref p2, ref n);
				AddCrossing(float2, float3, num3, num4, ref p, ref p2, ref n);
				AddCrossing(float3, @float, num4, num2, ref p, ref p2, ref n);
				if (n == 2)
				{
					float4 value = new float4(p, p2);
					list.Add(in value);
				}
			}
		}
		Segments.AddRangeNoResize(list);
	}

	private static void AddCrossing(float3 a, float3 b, float da, float db, ref float2 p0, ref float2 p1, ref int n)
	{
		if (math.abs(da) <= 0.0001f)
		{
			AddPoint(a.xz, ref p0, ref p1, ref n);
		}
		else if (da > 0f != db > 0f && math.abs(db) > 0.0001f)
		{
			AddPoint(math.lerp(a, b, math.saturate(da / (da - db))).xz, ref p0, ref p1, ref n);
		}
	}

	private static void AddPoint(float2 q, ref float2 p0, ref float2 p1, ref int n)
	{
		if (n == 0)
		{
			p0 = q;
			n = 1;
		}
		else if (n == 1 && math.lengthsq(q - p0) > 1E-08f)
		{
			p1 = q;
			n = 2;
		}
	}
}
