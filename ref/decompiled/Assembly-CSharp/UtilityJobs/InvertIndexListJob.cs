using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UtilityJobs;

[BurstCompile]
public struct InvertIndexListJob : IJob
{
	public NativeList<int> Indices;

	public NativeArray<bool> WorkBuffer;

	public void Execute()
	{
		for (int i = 0; i < WorkBuffer.Length; i++)
		{
			WorkBuffer[i] = false;
		}
		foreach (int index in Indices)
		{
			WorkBuffer[index] = true;
		}
		Indices.Clear();
		for (int j = 0; j < WorkBuffer.Length; j++)
		{
			if (!WorkBuffer[j])
			{
				Indices.AddNoResize(j);
			}
		}
	}
}
