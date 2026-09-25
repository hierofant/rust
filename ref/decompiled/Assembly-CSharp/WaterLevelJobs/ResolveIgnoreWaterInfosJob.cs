using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace WaterLevelJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
public struct ResolveIgnoreWaterInfosJob : IJob
{
	[WriteOnly]
	public NativeArray<WaterLevel.WaterInfo> Infos;

	[WriteOnly]
	public NativeArray<float> WaterHeights;

	[Unity.Collections.ReadOnly]
	public NativeArray<int>.ReadOnly Indices;

	[Unity.Collections.ReadOnly]
	public NativeArray<bool>.ReadOnly Results;

	public unsafe void Execute()
	{
		for (int i = 0; i < Infos.Length; i++)
		{
			int index = Indices[i];
			if (Results[index])
			{
				UnsafeUtility.ArrayElementAsRef<WaterLevel.WaterInfo>(Infos.GetUnsafePtr(), index).isValid = false;
				WaterHeights[index] = -1000f;
			}
		}
	}
}
