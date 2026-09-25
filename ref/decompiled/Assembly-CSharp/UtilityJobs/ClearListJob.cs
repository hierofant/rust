using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UtilityJobs;

[BurstCompile]
public struct ClearListJob<T> : IJob where T : unmanaged
{
	[WriteOnly]
	public NativeList<T> List;

	public void Execute()
	{
		List.Clear();
	}
}
