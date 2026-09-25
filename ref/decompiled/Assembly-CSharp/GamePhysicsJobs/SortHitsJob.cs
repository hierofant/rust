using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GamePhysicsJobs;

[BurstCompile]
public struct SortHitsJob<CompT> : IJobFor where CompT : unmanaged, IComparer<RaycastHit>
{
	[NativeDisableParallelForRestriction]
	public NativeArray<RaycastHit> Hits;

	[Unity.Collections.ReadOnly]
	public CompT Comp;

	[Unity.Collections.ReadOnly]
	public int MaxHitsPerRay;

	public void Execute(int index)
	{
		int num = 0;
		int num2 = index * MaxHitsPerRay;
		for (int i = 0; i < MaxHitsPerRay && !(Hits[num2 + i].normal == Vector3.zero); i++)
		{
			num++;
		}
		if (num > 1)
		{
			Hits.GetSubArray(num2, num).Sort(Comp);
		}
	}
}
