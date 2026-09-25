using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace AntiHackJobs;

[BurstCompile]
public struct BuildBatchLookupMapJob : IJob
{
	[WriteOnly]
	public NativeArray<int> Lookup;

	public NativeArray<AntiHack.Batch>.ReadOnly Batches;

	public void Execute()
	{
		int num = 0;
		for (int i = 0; i < Batches.Length; i++)
		{
			AntiHack.Batch batch = Batches[i];
			for (int j = 0; j < batch.Count; j++)
			{
				int index = num + j;
				Lookup[index] = i;
			}
			num += batch.Count;
		}
	}
}
