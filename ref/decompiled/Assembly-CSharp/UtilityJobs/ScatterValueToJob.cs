using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UtilityJobs;

[BurstCompile]
public struct ScatterValueToJob<T> : IJob where T : unmanaged
{
	[WriteOnly]
	public NativeArray<T> Results;

	[Unity.Collections.ReadOnly]
	public T Value;

	public NativeArray<int>.ReadOnly Indices;

	public void Execute()
	{
		for (int i = 0; i < Indices.Length; i++)
		{
			Results[Indices[i]] = Value;
		}
	}
}
