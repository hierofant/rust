using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UtilityJobs;

[BurstCompile]
public struct GenerateAscSeqListJob : IJob
{
	[WriteOnly]
	public NativeList<int> Values;

	[Unity.Collections.ReadOnly]
	public int Start;

	[Unity.Collections.ReadOnly]
	public int Step;

	[Unity.Collections.ReadOnly]
	public int Count;

	public void Execute()
	{
		int num = Start;
		for (int i = 0; i < Count; i++)
		{
			Values.AddNoResize(num);
			num += Step;
		}
	}
}
