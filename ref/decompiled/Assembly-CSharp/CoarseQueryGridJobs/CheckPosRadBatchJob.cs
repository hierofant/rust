using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace CoarseQueryGridJobs;

[BurstCompile]
public struct CheckPosRadBatchJob : IJob
{
	[WriteOnly]
	public NativeList<int> OverlapIndices;

	[NativeDisableContainerSafetyRestriction]
	public CoarseQueryGrid Grid;

	[Unity.Collections.ReadOnly]
	public NativeArray<UnityEngine.Vector3>.ReadOnly Pos;

	[Unity.Collections.ReadOnly]
	public NativeArray<float>.ReadOnly Radii;

	public void Execute()
	{
		for (int i = 0; i < Pos.Length; i++)
		{
			if (Grid.Check(Pos[i], Radii[i]))
			{
				OverlapIndices.AddNoResize(i);
			}
		}
	}
}
