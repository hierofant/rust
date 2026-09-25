using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace WaterLevelJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
public struct UpdateWaterHeightsJobIndirect : IJob
{
	[WriteOnly]
	public NativeArray<float> WaterHeights;

	[Unity.Collections.ReadOnly]
	public NativeArray<WaterLevel.WaterInfo> Infos;

	[Unity.Collections.ReadOnly]
	public NativeArray<int> Indices;

	public void Execute()
	{
		for (int i = 0; i < Indices.Length; i++)
		{
			int index = Indices[i];
			if (Infos[index].isValid)
			{
				WaterHeights[index] = Infos[index].surfaceLevel;
			}
		}
	}
}
