using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UtilityJobs;

[BurstCompile]
public struct CheckAnyBoolInGroupJob : IJob
{
	[WriteOnly]
	public NativeArray<bool> Results;

	public NativeArray<bool>.ReadOnly Hits;

	[Unity.Collections.ReadOnly]
	public int GroupSize;

	public void Execute()
	{
		for (int i = 0; i < Hits.Length; i++)
		{
			if (Hits[i])
			{
				Results[i / GroupSize] = true;
			}
		}
	}
}
