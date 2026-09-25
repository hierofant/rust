using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace AntiHackJobs;

[BurstCompile]
public struct GatherValidWaterIndicesJob : IJob
{
	[WriteOnly]
	public NativeArray<bool> Results;

	public NativeArray<WaterLevel.WaterInfo>.ReadOnly WaterInfos;

	public void Execute()
	{
		for (int i = 0; i < WaterInfos.Length; i++)
		{
			Results[i] = WaterInfos[i].isValid;
		}
	}
}
