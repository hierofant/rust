using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GamePhysicsJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
public struct GenerateSphereCmdsFromCapsuleCmdsJob : IJob
{
	[WriteOnly]
	public NativeArray<OverlapSphereCommand> SphereCommands;

	[Unity.Collections.ReadOnly]
	public NativeArray<OverlapCapsuleCommand>.ReadOnly Commands;

	[Unity.Collections.ReadOnly]
	public NativeArray<int>.ReadOnly Indices;

	public void Execute()
	{
		for (int i = 0; i < Indices.Length; i++)
		{
			int index = Indices[i];
			OverlapCapsuleCommand overlapCapsuleCommand = Commands[index];
			OverlapSphereCommand value = new OverlapSphereCommand(overlapCapsuleCommand.point0, overlapCapsuleCommand.radius, overlapCapsuleCommand.queryParameters);
			SphereCommands[i] = value;
		}
	}
}
