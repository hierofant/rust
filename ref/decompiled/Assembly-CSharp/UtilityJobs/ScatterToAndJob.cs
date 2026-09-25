using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UtilityJobs;

[BurstCompile]
public struct ScatterToAndJob : IJob
{
	public NativeArray<bool> To;

	public NativeArray<bool>.ReadOnly From;

	public NativeArray<int>.ReadOnly Indices;

	public void Execute()
	{
		for (int i = 0; i < Indices.Length; i++)
		{
			To[Indices[i]] &= From[i];
		}
	}
}
