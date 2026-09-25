using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GamePhysicsJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
public struct ValidateOverlapBoxCommandsJob : IJob
{
	[WriteOnly]
	public NativeList<int> InvalidIndices;

	[Unity.Collections.ReadOnly]
	public NativeArray<OverlapBoxCommand>.ReadOnly Commands;

	public void Execute()
	{
		for (int i = 0; i < Commands.Length; i++)
		{
			OverlapBoxCommand overlapBoxCommand = Commands[i];
			if (overlapBoxCommand.halfExtents.IsNaNOrInfinity())
			{
				InvalidIndices.AddNoResize(i);
			}
			else if (overlapBoxCommand.halfExtents.x <= 0f || overlapBoxCommand.halfExtents.y <= 0f || overlapBoxCommand.halfExtents.z <= 0f)
			{
				InvalidIndices.AddNoResize(i);
			}
		}
	}
}
