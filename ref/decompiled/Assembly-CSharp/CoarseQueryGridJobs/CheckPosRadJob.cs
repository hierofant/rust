using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace CoarseQueryGridJobs;

[BurstCompile]
public struct CheckPosRadJob : IJob
{
	public NativeReference<bool> Result;

	[NativeDisableContainerSafetyRestriction]
	public CoarseQueryGrid Grid;

	public Vector3 CheckPos;

	public float CheckRad;

	public void Execute()
	{
		Result.Value = Grid.Check(CheckPos, CheckRad);
	}
}
