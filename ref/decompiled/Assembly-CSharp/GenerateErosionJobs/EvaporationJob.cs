using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace GenerateErosionJobs;

[BurstCompile(FloatMode = FloatMode.Deterministic)]
internal struct EvaporationJob : IJobParallelFor
{
	public NativeArray<float> WaterMap;

	public float DT;

	public float EvaporationRate;

	public void Execute(int index)
	{
		WaterMap[index] *= 1f - EvaporationRate * DT;
	}
}
