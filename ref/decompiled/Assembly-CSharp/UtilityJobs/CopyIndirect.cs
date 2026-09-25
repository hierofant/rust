using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UtilityJobs;

[BurstCompile]
public struct CopyIndirect<T> : IJob where T : unmanaged
{
	public NativeArray<T> To;

	public NativeArray<T>.ReadOnly From;

	public NativeArray<int>.ReadOnly Indices;

	public void Execute()
	{
		foreach (int index in Indices)
		{
			To[index] = From[index];
		}
	}
}
