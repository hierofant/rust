using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UtilityJobs;

[BurstCompile]
public struct FlipBoolJob : IJob
{
	public NativeArray<bool> Values;

	public void Execute()
	{
		for (int i = 0; i < Values.Length; i++)
		{
			Values[i] = !Values[i];
		}
	}
}
