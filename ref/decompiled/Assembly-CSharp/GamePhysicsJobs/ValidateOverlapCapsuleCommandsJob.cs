using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GamePhysicsJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
public struct ValidateOverlapCapsuleCommandsJob : IJob
{
	[WriteOnly]
	public NativeList<int> InvalidIndices;

	[Unity.Collections.ReadOnly]
	public NativeArray<OverlapCapsuleCommand>.ReadOnly Commands;

	public void Execute()
	{
		for (int i = 0; i < Commands.Length; i++)
		{
			OverlapCapsuleCommand overlapCapsuleCommand = Commands[i];
			if (overlapCapsuleCommand.radius <= 0f || (overlapCapsuleCommand.point1 - overlapCapsuleCommand.point0).magnitude / 2f <= 0f)
			{
				InvalidIndices.AddNoResize(i);
			}
		}
	}
}
