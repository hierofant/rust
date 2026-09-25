using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace AntiHackJobs;

[BurstCompile]
public struct CalcDistAndBudgetJob : IJobFor
{
	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<(float Dist, float Budget)> DistAndBudget;

	[WriteOnly]
	public NativeList<int> IndicesForNormalSample;

	public NativeArray<Vector3>.ReadOnly Start;

	public NativeArray<Vector3>.ReadOnly End;

	public NativeArray<BasePlayer.CachedState>.ReadOnly States;

	public NativeArray<float>.ReadOnly Speed;

	public NativeArray<float>.ReadOnly DeltaTime;

	public NativeArray<int>.ReadOnly Indices;

	[Unity.Collections.ReadOnly]
	public bool Use3DMagnitude;

	public void Execute(int jobInd)
	{
		int num = Indices[jobInd];
		bool isSwimming = States[num].IsSwimming;
		Vector3 v = End[num] - Start[num];
		float num2 = ((isSwimming && Use3DMagnitude) ? v.magnitude : v.Magnitude2D());
		float num3 = Speed[jobInd] * DeltaTime[num];
		DistAndBudget[num] = (num2, num3);
		if (!isSwimming && num2 > num3)
		{
			IndicesForNormalSample.AddNoResize(num);
		}
	}
}
