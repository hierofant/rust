using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace AntiHackJobs;

[BurstCompile]
public struct ProgressSpeedingStatesJob : IJobFor
{
	public NativeArray<AntiHack.PlayerSpeedhackState> SpeedStates;

	public NativeArray<float>.ReadOnly DeltaTime;

	public NativeArray<int>.ReadOnly Indices;

	public void Execute(int jobInd)
	{
		int index = Indices[jobInd];
		ref AntiHack.PlayerSpeedhackState reference = ref ((Span<AntiHack.PlayerSpeedhackState>)SpeedStates)[index];
		float num = DeltaTime[index];
		reference.PauseTime = Mathf.Max(0f, reference.PauseTime - num);
		reference.ExtraSpeedTime = Mathf.Max(0f, reference.ExtraSpeedTime - num);
	}
}
