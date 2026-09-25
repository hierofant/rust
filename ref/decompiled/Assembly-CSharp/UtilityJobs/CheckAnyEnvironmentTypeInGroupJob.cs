using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UtilityJobs;

[BurstCompile]
public struct CheckAnyEnvironmentTypeInGroupJob : IJob
{
	[WriteOnly]
	public NativeArray<bool> Results;

	public NativeArray<EnvironmentType>.ReadOnly Hits;

	[Unity.Collections.ReadOnly]
	public int GroupSize;

	[Unity.Collections.ReadOnly]
	public EnvironmentType TypeToTest;

	public void Execute()
	{
		for (int i = 0; i < Hits.Length; i++)
		{
			if ((Hits[i] & TypeToTest) != 0)
			{
				Results[i] = true;
			}
			else
			{
				Results[i] = false;
			}
		}
	}
}
