using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace CoarseQueryGridJobs;

[BurstCompile]
public struct CheckBoundsJob : IJob
{
	public NativeReference<bool> Result;

	[NativeDisableContainerSafetyRestriction]
	public CoarseQueryGrid Grid;

	public Bounds CheckBounds;

	public void Execute()
	{
		Result.Value = Grid.Check(CheckBounds);
	}
}
