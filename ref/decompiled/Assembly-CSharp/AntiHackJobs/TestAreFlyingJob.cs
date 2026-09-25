using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace AntiHackJobs;

[BurstCompile]
public struct TestAreFlyingJob : IJob
{
	public NativeArray<bool> Results;

	public NativeArray<AntiHack.PlayerFlyhackState> PlayerStates;

	public NativeArray<int>.ReadOnly Indices;

	public NativeArray<int>.ReadOnly BatchMap;

	public NativeArray<Vector3>.ReadOnly OldPoses;

	public NativeArray<Vector3>.ReadOnly NewPoses;

	public NativeArray<bool>.ReadOnly PlayersInAir;

	public NativeArray<bool>.ReadOnly WasInAirStates;

	[Unity.Collections.ReadOnly]
	public float ForgivenessVerticalInertia;

	[Unity.Collections.ReadOnly]
	public float ForgivenessVertical;

	[Unity.Collections.ReadOnly]
	public float ForgivenessHorizontalInertia;

	[Unity.Collections.ReadOnly]
	public float ForgivenessHorizontal;

	[Unity.Collections.ReadOnly]
	public float TimeSinceStartup;

	public void Execute()
	{
		Span<AntiHack.PlayerFlyhackState> span = PlayerStates;
		for (int i = 0; i < OldPoses.Length; i++)
		{
			int index = BatchMap[i];
			if (Results[index])
			{
				continue;
			}
			int index2 = Indices[index];
			ref AntiHack.PlayerFlyhackState reference = ref span[index2];
			if (PlayersInAir[i])
			{
				bool flag = false;
				float num = ((reference.PauseTime > 0f) ? ForgivenessVerticalInertia : ForgivenessVertical);
				float num2 = ((reference.PauseTime > 0f) ? ForgivenessHorizontalInertia : ForgivenessHorizontal);
				Vector3 v = NewPoses[i] - OldPoses[i];
				float num3 = Mathf.Abs(v.y);
				float num4 = v.Magnitude2D();
				if (v.y >= 0f)
				{
					reference.VerticalDistance += v.y;
					flag = true;
				}
				if (num3 < num4)
				{
					reference.HorizontalDistance += num4;
					flag = true;
				}
				if (flag)
				{
					float num5 = BasePlayer.GetJumpHeight() + num;
					if (reference.VerticalDistance > num5)
					{
						Results[index] = true;
					}
					float num6 = 5f + num2;
					if (reference.HorizontalDistance > num6)
					{
						Results[index] = true;
					}
				}
			}
			else
			{
				if (WasInAirStates[index])
				{
					reference.LastInAirTime = TimeSinceStartup;
				}
				reference.HorizontalDistance = 0f;
				reference.VerticalDistance = 0f;
			}
		}
	}
}
