using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UtilityJobs;

[BurstCompile]
public struct ClearArrayJob<T> : IJob where T : unmanaged
{
	[WriteOnly]
	public NativeArray<T> Array;

	public void Execute()
	{
		NativeArrayEx.MemClear(in Array);
	}
}
