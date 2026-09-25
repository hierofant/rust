using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace GenerateErosionJobs;

[BurstCompile(FloatMode = FloatMode.Deterministic)]
internal struct RefillOceanJob : IJobParallelFor
{
	public NativeArray<int>.ReadOnly OceanIndices;

	public NativeArray<float>.ReadOnly HeightMap;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float> WaterMap;

	public float OceanLevel;

	public void Execute(int index)
	{
		int index2 = OceanIndices[index];
		float num = HeightMap[index2];
		WaterMap[index2] = OceanLevel - num;
	}
}
