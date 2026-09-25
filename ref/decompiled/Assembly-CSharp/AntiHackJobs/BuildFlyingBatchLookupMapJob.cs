using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace AntiHackJobs;

[BurstCompile]
public struct BuildFlyingBatchLookupMapJob : IJob
{
	[WriteOnly]
	public NativeArray<int> Lookup;

	public NativeArray<AntiHack.FlyingBatch>.ReadOnly Batches;

	public void Execute()
	{
		int num = 0;
		for (int i = 0; i < Batches.Length; i++)
		{
			AntiHack.FlyingBatch flyingBatch = Batches[i];
			for (int j = 0; j < flyingBatch.Count; j++)
			{
				int index = num + j;
				Lookup[index] = i;
			}
			num += flyingBatch.Count;
		}
	}
}
