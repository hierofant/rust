using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace WaterLevelJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
public struct GatherInvalidInfosJobIndirect : IJob
{
	[WriteOnly]
	public NativeArray<int> InvalidIndices;

	[WriteOnly]
	public NativeReference<int> InvalidIndexCount;

	[Unity.Collections.ReadOnly]
	public NativeArray<WaterLevel.WaterInfo>.ReadOnly Infos;

	[Unity.Collections.ReadOnly]
	public NativeArray<int>.ReadOnly Indices;

	public void Execute()
	{
		int value = 0;
		for (int i = 0; i < Indices.Length; i++)
		{
			int num = Indices[i];
			if (!Infos[num].isValid)
			{
				InvalidIndices[value++] = num;
			}
		}
		InvalidIndexCount.Value = value;
	}
}
