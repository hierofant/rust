using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace CoarseQueryGridJobs;

[BurstCompile]
public struct CheckPosRadBatchJobIndirect : IJob
{
	[WriteOnly]
	public NativeList<int> OverlapIndices;

	[NativeDisableContainerSafetyRestriction]
	public CoarseQueryGrid Grid;

	[Unity.Collections.ReadOnly]
	public NativeArray<UnityEngine.Vector3>.ReadOnly Pos;

	[Unity.Collections.ReadOnly]
	public NativeArray<float>.ReadOnly Radii;

	[Unity.Collections.ReadOnly]
	public NativeArray<int>.ReadOnly Indices;

	public void Execute()
	{
		for (int i = 0; i < Indices.Length; i++)
		{
			int num = Indices[i];
			if (Grid.Check(Pos[num], Radii[num]))
			{
				OverlapIndices.AddNoResize(num);
			}
		}
	}
}
