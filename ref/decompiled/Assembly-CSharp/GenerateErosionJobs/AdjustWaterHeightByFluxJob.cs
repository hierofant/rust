using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace GenerateErosionJobs;

[BurstCompile(FloatMode = FloatMode.Deterministic)]
internal struct AdjustWaterHeightByFluxJob : IJobParallelFor
{
	public NativeArray<float> WaterMap;

	public NativeArray<float2> VelocityMap;

	public NativeArray<float4>.ReadOnly FluxMap;

	public int Res;

	public float DT;

	public float InvGridCellSquareSize;

	public void Execute(int index)
	{
		int num = index % Res;
		int num2 = index / Res;
		ref float2 reference = ref BurstUtil.Get(in VelocityMap, index);
		if (num == 0 || num2 == 0 || num == Res - 1 || num2 == Res - 1)
		{
			reference = float2.zero;
			return;
		}
		float4 x = FluxMap[index];
		float num3 = math.csum(x);
		int4x2 int4x = new int4x2(new int4(num - 1, num + 1, num, num), new int4(num2, num2, num2 + 1, num2 - 1));
		int4x.c0 = int4x.c1 * Res + int4x.c0;
		float y = FluxMap[int4x.c0.x].y;
		float x2 = FluxMap[int4x.c0.y].x;
		float w = FluxMap[int4x.c0.z].w;
		float z = FluxMap[int4x.c0.w].z;
		float num4 = y + x2 + w + z;
		float num5 = DT * (num4 - num3);
		BurstUtil.Get(in WaterMap, index) += num5 * InvGridCellSquareSize;
		float2 @float = default(float2);
		@float.x = y - x.x + x.y - x2;
		@float.y = w - x.z + x.w - z;
		float2 float2 = @float;
		float2 *= 0.5f;
		reference += float2 * DT;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int ToIndex([AssumeRange(0L, 2147483647L)] int x, [AssumeRange(0L, 2147483647L)] int y, [AssumeRange(0L, 2147483647L)] int res)
	{
		return y * res + x;
	}
}
