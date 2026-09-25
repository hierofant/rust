using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace AntiHackJobs;

[BurstCompile]
public struct TransformStartEndTicksJob : IJobFor
{
	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<Vector3> Starts;

	[NativeDisableParallelForRestriction]
	[WriteOnly]
	public NativeArray<Vector3> Ends;

	public NativeArray<Matrix4x4>.ReadOnly Matrices;

	public TickInterpolatorCache.ReadOnlyState TickCache;

	public NativeArray<int>.ReadOnly Indices;

	public void Execute(int jobInd)
	{
		int num = Indices[jobInd];
		Matrix4x4 matrix4x = Matrices[jobInd];
		bool flag = matrix4x[15] == 0f;
		TickInterpolatorCache.PlayerTickIterator playerTickIterator = TickInterpolatorCache.GetPlayerTickIterator(TickCache, num);
		Starts[num] = (flag ? playerTickIterator.StartPoint : matrix4x.MultiplyPoint3x4(playerTickIterator.StartPoint));
		Ends[num] = (flag ? playerTickIterator.EndPoint : matrix4x.MultiplyPoint3x4(playerTickIterator.EndPoint));
	}
}
