using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UtilityJobs;

[BurstCompile]
public struct FillJob<T> : IJob where T : unmanaged
{
	[WriteOnly]
	public NativeArray<T> Values;

	[Unity.Collections.ReadOnly]
	public T Value;

	public void Execute()
	{
		for (int i = 0; i < Values.Length; i++)
		{
			Values[i] = Value;
		}
	}
}
