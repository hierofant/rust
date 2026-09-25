using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace GenerateErosionJobs;

[BurstCompile(FloatMode = FloatMode.Deterministic)]
internal struct PopulateDeltaHeightJob : IJobParallelFor
{
	public NativeArray<float>.ReadOnly HeightMapOriginal;

	public NativeArray<float>.ReadOnly HeightMap;

	[WriteOnly]
	public NativeArray<float> DeltaHeightMap;

	public void Execute(int index)
	{
		DeltaHeightMap[index] = HeightMapOriginal[index] - HeightMap[index];
	}
}
