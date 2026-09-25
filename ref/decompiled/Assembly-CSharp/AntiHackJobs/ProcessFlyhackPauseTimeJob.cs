using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace AntiHackJobs;

[BurstCompile]
public struct ProcessFlyhackPauseTimeJob : IJob
{
	[WriteOnly]
	public NativeArray<AntiHack.PlayerFlyhackState> PlayerStates;

	public NativeArray<int>.ReadOnly Indices;

	public NativeArray<float>.ReadOnly DeltaTimes;

	public void Execute()
	{
		Span<AntiHack.PlayerFlyhackState> span = PlayerStates;
		for (int i = 0; i < Indices.Length; i++)
		{
			int index = Indices[i];
			ref AntiHack.PlayerFlyhackState reference = ref span[index];
			reference.PauseTime = Mathf.Max(0f, reference.PauseTime - DeltaTimes[index]);
		}
	}
}
