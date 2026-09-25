using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GamePhysicsJobs;

[BurstCompile]
public struct GenerateOverlapBoxCommandsJob : IJob
{
	[WriteOnly]
	public NativeArray<OverlapBoxCommand> BoxCommands;

	[Unity.Collections.ReadOnly]
	public NativeArray<Vector3>.ReadOnly Centers;

	[Unity.Collections.ReadOnly]
	public NativeArray<Vector3>.ReadOnly Extents;

	[Unity.Collections.ReadOnly]
	public NativeArray<int>.ReadOnly LayerMasks;

	[Unity.Collections.ReadOnly]
	public QueryTriggerInteraction TriggerInteraction;

	[Unity.Collections.ReadOnly]
	public bool HitMultipleFaces;

	[Unity.Collections.ReadOnly]
	public bool HitBackfaces;

	public void Execute()
	{
		for (int i = 0; i < Centers.Length; i++)
		{
			QueryParameters queryParameters = new QueryParameters(LayerMasks[i], HitMultipleFaces, TriggerInteraction, HitBackfaces);
			BoxCommands[i] = new OverlapBoxCommand(Centers[i], Extents[i], Quaternion.identity, queryParameters);
		}
	}
}
