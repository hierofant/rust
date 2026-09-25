using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace AntiHackJobs;

[BurstCompile]
public struct CacheInAirStateJob : IJob
{
	[WriteOnly]
	public NativeArray<AntiHack.PlayerFlyhackState> PlayerStates;

	public NativeArray<bool>.ReadOnly PlayersInAir;

	public NativeArray<int>.ReadOnly Indices;

	public NativeArray<int>.ReadOnly BatchMap;

	public NativeArray<UnityEngine.Vector3>.ReadOnly OldPoses;

	public void Execute()
	{
		Span<AntiHack.PlayerFlyhackState> span = PlayerStates;
		for (int i = 0; i < PlayersInAir.Length; i++)
		{
			int index = Indices[BatchMap[i]];
			ref AntiHack.PlayerFlyhackState reference = ref span[index];
			if (!reference.IsInAir)
			{
				reference.LastGroundedPosition = OldPoses[i];
			}
			reference.IsInAir = PlayersInAir[i];
		}
	}
}
