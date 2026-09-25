using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace AntiHackJobs;

[BurstCompile]
public struct DetermineInAirJob : IJob
{
	[WriteOnly]
	public NativeArray<bool> Results;

	public NativeArray<AntiHack.PlayerFlyhackState> PlayerFlyStates;

	public NativeArray<BasePlayer.CachedState>.ReadOnly PlayerStates;

	public NativeArray<AntiHack.FlyingBatch>.ReadOnly FlyingBatches;

	public NativeArray<ModelState.Flag>.ReadOnly PlayerMSFlags;

	public NativeArray<UnityEngine.Vector3>.ReadOnly OldPoses;

	public NativeArray<bool>.ReadOnly WaterValidStates;

	public NativeArray<bool>.ReadOnly ElevatorValidStates;

	public NativeArray<int>.ReadOnly Indices;

	[Unity.Collections.ReadOnly]
	public bool verifyGrounded;

	public void Execute()
	{
		Span<AntiHack.PlayerFlyhackState> span = PlayerFlyStates;
		int num = 0;
		for (int i = 0; i < Indices.Length; i++)
		{
			AntiHack.FlyingBatch flyingBatch = FlyingBatches[i];
			int index = Indices[i];
			BasePlayer.CachedState cachedState = PlayerStates[index];
			ModelState.Flag flag = PlayerMSFlags[index];
			_ = ref span[index];
			for (int j = 0; j < flyingBatch.Count; j++)
			{
				bool flag2 = (flag & ModelState.Flag.OnGround) != 0;
				if (verifyGrounded)
				{
					if (cachedState.IsOnLadder)
					{
						Results[num] = false;
					}
					else if (WaterValidStates[num])
					{
						Results[num] = false;
					}
					else if (ElevatorValidStates[num])
					{
						Results[num] = false;
					}
				}
				else
				{
					Results[num] = !cachedState.IsOnLadder && !cachedState.IsSwimming && !flag2;
				}
				num++;
			}
		}
	}
}
