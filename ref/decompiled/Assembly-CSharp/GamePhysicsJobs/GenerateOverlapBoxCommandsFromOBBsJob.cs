using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GamePhysicsJobs;

[BurstCompile]
public struct GenerateOverlapBoxCommandsFromOBBsJob : IJob
{
	[WriteOnly]
	public NativeArray<OverlapBoxCommand> BoxCommands;

	[Unity.Collections.ReadOnly]
	public NativeArray<OBB>.ReadOnly OBBs;

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
		for (int i = 0; i < OBBs.Length; i++)
		{
			QueryParameters queryParameters = new QueryParameters(LayerMasks[i], HitMultipleFaces, TriggerInteraction, HitBackfaces);
			BoxCommands[i] = new OverlapBoxCommand(OBBs[i].position, OBBs[i].extents, OBBs[i].rotation, queryParameters);
		}
	}
}
