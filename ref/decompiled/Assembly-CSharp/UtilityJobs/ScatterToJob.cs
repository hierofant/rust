using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UtilityJobs;

[BurstCompile]
public struct ScatterToJob<T> : IJob where T : unmanaged
{
	[WriteOnly]
	public NativeArray<T> Results;

	public NativeArray<T>.ReadOnly Source;

	public NativeArray<int>.ReadOnly Indices;

	public void Execute()
	{
		for (int i = 0; i < Indices.Length; i++)
		{
			Results[Indices[i]] = Source[i];
		}
	}
}
