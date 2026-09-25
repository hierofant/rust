using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GamePhysicsJobs;

[BurstCompile]
public struct GenerateOverlapSphereCommandsJob : IJob
{
	[WriteOnly]
	public NativeArray<OverlapSphereCommand> SphereCommands;

	[Unity.Collections.ReadOnly]
	public NativeArray<Vector3>.ReadOnly Pos;

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
		for (int i = 0; i < Pos.Length; i++)
		{
			QueryParameters queryParameters = new QueryParameters(LayerMasks[i], HitMultipleFaces, TriggerInteraction, HitBackfaces);
			SphereCommands[i] = new OverlapSphereCommand(Pos[i], Radiii[i], queryParameters);
		}
	}
}
