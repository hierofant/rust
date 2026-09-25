using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace GenerateErosionJobs;

[BurstCompile(FloatMode = FloatMode.Deterministic)]
internal struct WaterIncrementationJob : IJobParallelFor
{
	public NativeArray<float> WaterMap;

	public float WaterFillRate;

	public float DT;

	public void Execute(int index)
	{
		WaterMap[index] += WaterFillRate * DT;
	}
}
