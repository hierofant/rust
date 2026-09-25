using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace AntiHackJobs;

[BurstCompile]
public struct FindValidIndicesJob : IJob
{
	[WriteOnly]
	public NativeList<int> ValidIndices;

	public NativeArray<bool> WorkBuffer;

	public NativeArray<int>.ReadOnly InvalidIndices;

	public NativeArray<int>.ReadOnly AllIndices;

	public void Execute()
	{
		foreach (int allIndex in AllIndices)
		{
			WorkBuffer[allIndex] = true;
		}
		foreach (int invalidIndex in InvalidIndices)
		{
			WorkBuffer[invalidIndex] = false;
		}
		foreach (int allIndex2 in AllIndices)
		{
			if (WorkBuffer[allIndex2])
			{
				ValidIndices.AddNoResize(allIndex2);
			}
		}
	}
}
