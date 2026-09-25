using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UtilityJobs;

[BurstCompile]
public struct DiffVec3Indirect : IJob
{
	public NativeList<int> FoundDiff;

	public NativeArray<UnityEngine.Vector3>.ReadOnly A;

	public NativeArray<UnityEngine.Vector3>.ReadOnly B;

	public NativeArray<int>.ReadOnly Indices;

	public void Execute()
	{
		foreach (int index in Indices)
		{
			if (A[index] != B[index])
			{
				FoundDiff.AddNoResize(index);
			}
		}
	}
}
