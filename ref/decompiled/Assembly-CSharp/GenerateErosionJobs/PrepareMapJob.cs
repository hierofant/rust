using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace GenerateErosionJobs;

[BurstCompile(FloatMode = FloatMode.Deterministic)]
internal struct PrepareMapJob : IJobParallelForBatch
{
	public NativeArray<short>.ReadOnly HeightMapAsShort;

	[WriteOnly]
	public NativeArray<float> HeightMapAsFloat;

	public NativeList<int>.ParallelWriter OceanIndicesWriter;

	public float TerrainPositionY;

	public float TerrainSizeY;

	public float OceanLevel;

	public void Execute(int startIndex, int count)
	{
		NativeList<int> list = new NativeList<int>(count, Allocator.Temp);
		for (int i = startIndex; i < startIndex + count; i++)
		{
			float num2 = (HeightMapAsFloat[i] = TerrainPositionY + BitUtility.Short2Float(HeightMapAsShort[i]) * TerrainSizeY);
			if (num2 <= OceanLevel)
			{
				list.Add(in i);
			}
		}
		OceanIndicesWriter.AddRangeNoResize(list);
	}
}
