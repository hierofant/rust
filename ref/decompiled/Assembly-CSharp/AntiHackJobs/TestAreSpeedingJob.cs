using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace AntiHackJobs;

[BurstCompile]
public struct TestAreSpeedingJob : IJobFor
{
	[WriteOnly]
	public NativeArray<bool> Results;

	public NativeArray<AntiHack.PlayerSpeedhackState> PlayerStates;

	public NativeArray<(float Dist, float Budget)>.ReadOnly DistAndBudget;

	public NativeArray<float>.ReadOnly DeltaTime;

	public NativeArray<int>.ReadOnly Indices;

	[Unity.Collections.ReadOnly]
	public float ForgivenessInertia;

	[Unity.Collections.ReadOnly]
	public float Forgiveness;

	public void Execute(int jobInd)
	{
		int index = Indices[jobInd];
		ref AntiHack.PlayerSpeedhackState reference = ref ((Span<AntiHack.PlayerSpeedhackState>)PlayerStates)[index];
		float num = Mathf.Max((reference.PauseTime > 0f) ? ForgivenessInertia : Forgiveness, 0.1f);
		float num2 = num + Mathf.Max(Forgiveness, 0.1f);
		reference.Distance = Mathf.Clamp(reference.Distance, 0f - num2, num2);
		(float Dist, float Budget) tuple = DistAndBudget[index];
		float item = tuple.Dist;
		float item2 = tuple.Budget;
		float num3 = ((reference.ExtraSpeedTime > 0f) ? (reference.ExtraSpeed * DeltaTime[index]) : 0f);
		reference.Distance = Mathf.Clamp(reference.Distance - item2 - num3, 0f - num2, num2);
		if (reference.Distance > num)
		{
			Results[jobInd] = true;
			return;
		}
		reference.Distance = Mathf.Clamp(reference.Distance + item, 0f - num2, num2);
		if (reference.Distance > num)
		{
			Results[jobInd] = true;
		}
		else
		{
			Results[jobInd] = false;
		}
	}
}
