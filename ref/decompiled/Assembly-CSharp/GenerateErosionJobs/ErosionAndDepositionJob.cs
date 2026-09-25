using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace GenerateErosionJobs;

[BurstCompile(FloatMode = FloatMode.Deterministic)]
internal struct ErosionAndDepositionJob : IJobParallelFor
{
	public NativeArray<float> SedimentMap;

	public NativeArray<float>.ReadOnly MinTerrainHeightMap;

	public NativeArray<float>.ReadOnly TerrainHeightMapSrcFloat;

	public NativeArray<float> TerrainHeightMapDstFloat;

	public NativeArray<float> WaterMap;

	public NativeArray<float2>.ReadOnly VelocityMap;

	public NativeArray<float>.ReadOnly AngleMap;

	public float DT;

	private const float SedimentCapacityConst = 0.0015f;

	private const float DissolveRateConstant = 0.15f;

	public void Execute(int index)
	{
		float x = math.max(0.01047198f, AngleMap[index]);
		float2 x2 = VelocityMap[index];
		float num = 0.0015f * math.sin(x) * math.length(x2);
		ref float reference = ref BurstUtil.Get(in WaterMap, index);
		float num2 = 1f - math.smoothstep(0f, 10f, reference);
		ref float reference2 = ref BurstUtil.Get(in SedimentMap, index);
		float num3 = DT * 0.15f * (num - reference2) * num2;
		float x3 = math.select(-1f, 1f, num > reference2) * num3;
		x3 = math.max(x3, 0f);
		float num4 = TerrainHeightMapSrcFloat[index];
		float x4 = num4 - x3;
		x4 = math.max(x4, MinTerrainHeightMap[index]);
		x3 = x4 - (num4 - x3);
		TerrainHeightMapDstFloat[index] = x4;
		reference2 += x3;
		reference += x3;
	}
}
