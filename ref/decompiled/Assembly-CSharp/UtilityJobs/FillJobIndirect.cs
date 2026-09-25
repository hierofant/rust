using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UtilityJobs;

[BurstCompile]
public struct FillJobIndirect<T> : IJob where T : unmanaged
{
	[WriteOnly]
	public NativeArray<T> Values;

	[Unity.Collections.ReadOnly]
	public T Value;

	public NativeArray<int>.ReadOnly Indices;

	public void Execute()
	{
		foreach (int index in Indices)
		{
			Values[index] = Value;
		}
	}
}
