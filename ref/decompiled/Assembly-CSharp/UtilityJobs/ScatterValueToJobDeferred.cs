using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UtilityJobs;

[BurstCompile]
public struct ScatterValueToJobDeferred<T> : IJob where T : unmanaged
{
	[WriteOnly]
	public NativeArray<T> Results;

	[Unity.Collections.ReadOnly]
	public T Value;

	public NativeArray<int> Indices;

	public void Execute()
	{
		for (int i = 0; i < Indices.Length; i++)
		{
			Results[Indices[i]] = Value;
		}
	}
}
