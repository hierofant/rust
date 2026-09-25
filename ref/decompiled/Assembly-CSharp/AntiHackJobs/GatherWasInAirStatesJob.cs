using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace AntiHackJobs;

[BurstCompile]
public struct GatherWasInAirStatesJob : IJob
{
	[WriteOnly]
	public NativeArray<bool> Results;

	public NativeArray<AntiHack.PlayerFlyhackState>.ReadOnly PlayerStates;

	public NativeArray<int>.ReadOnly Indices;

	public void Execute()
	{
		for (int i = 0; i < Indices.Length; i++)
		{
			int index = Indices[i];
			AntiHack.PlayerFlyhackState playerFlyhackState = PlayerStates[index];
			Results[i] = playerFlyhackState.IsInAir;
		}
	}
}
