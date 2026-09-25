using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace GenerateErosionJobs;

[BurstCompile(FloatMode = FloatMode.Deterministic)]
internal struct CalculateOutputFluxJob : IJobParallelFor
{
	public NativeArray<float>.ReadOnly TerrainHeightMapFloatVal;

	public NativeArray<float>.ReadOnly WaterMap;

	public NativeArray<float4> FluxMap;

	public int Res;

	public float DT;

	public float GridCellSquareSize;

	public float PipeLength;

	public float PipeArea;

	private const float Gravity = 10f;

	[SkipLocalsInit]
	public void Execute(int index)
	{
		ref float4 reference = ref BurstUtil.Get(in FluxMap, index);
		int num = index % Res;
		int num2 = index / Res;
		if (num == 0 || num2 == 0 || num == Res - 1 || num2 == Res - 1)
		{
			reference = float4.zero;
			return;
		}
		float num3 = TerrainHeightMapFloatVal[index];
		float num4 = WaterMap[index];
		float num5 = num3 + num4;
		int4x2 int4x = new int4x2(new int4(num - 1, num + 1, num, num), new int4(num2, num2, num2 + 1, num2 - 1));
		int4 @int = math.mad(int4x.c1, Res, int4x.c0);
		float4 @float = default(float4);
		@float.x = BurstUtil.GetReadonly(in TerrainHeightMapFloatVal, @int.x) + BurstUtil.GetReadonly(in WaterMap, @int.x);
		@float.y = BurstUtil.GetReadonly(in TerrainHeightMapFloatVal, @int.y) + BurstUtil.GetReadonly(in WaterMap, @int.y);
		@float.z = BurstUtil.GetReadonly(in TerrainHeightMapFloatVal, @int.z) + BurstUtil.GetReadonly(in WaterMap, @int.z);
		@float.w = BurstUtil.GetReadonly(in TerrainHeightMapFloatVal, @int.w) + BurstUtil.GetReadonly(in WaterMap, @int.w);
		reference = math.max(float4.zero, reference + DT * PipeArea * (10f * (num5 - @float) / PipeLength));
		float num6 = math.min(1f, num4 * GridCellSquareSize / (math.csum(reference) * DT));
		reference *= num6;
	}
}
