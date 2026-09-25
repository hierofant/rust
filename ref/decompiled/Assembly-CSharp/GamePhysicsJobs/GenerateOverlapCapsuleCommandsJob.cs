using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GamePhysicsJobs;

[BurstCompile]
public struct GenerateOverlapCapsuleCommandsJob : IJob
{
	[WriteOnly]
	public NativeArray<OverlapCapsuleCommand> CapsuleCommands;

	[Unity.Collections.ReadOnly]
	public NativeArray<Vector3>.ReadOnly From;

	[Unity.Collections.ReadOnly]
	public NativeArray<Vector3>.ReadOnly To;

	[Unity.Collections.ReadOnly]
	public NativeArray<int>.ReadOnly LayerMasks;

	[Unity.Collections.ReadOnly]
	public NativeArray<float>.ReadOnly Radiii;

	[Unity.Collections.ReadOnly]
	public QueryTriggerInteraction TriggerInteraction;

	[Unity.Collections.ReadOnly]
	public bool HitMultipleFaces;

	[Unity.Collections.ReadOnly]
	public bool HitBackfaces;

	public void Execute()
	{
		for (int i = 0; i < From.Length; i++)
		{
			QueryParameters queryParameters = new QueryParameters(LayerMasks[i], HitMultipleFaces, TriggerInteraction, HitBackfaces);
			CapsuleCommands[i] = new OverlapCapsuleCommand(From[i], To[i], Radiii[i], queryParameters);
		}
	}
}
