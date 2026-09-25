using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GamePhysicsJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
public struct FindSphereCmdsInCapsuleCmdsJob : IJob
{
	[WriteOnly]
	public NativeList<int> SphereIndices;

	[Unity.Collections.ReadOnly]
	public NativeArray<OverlapCapsuleCommand>.ReadOnly Commands;

	public void Execute()
	{
		for (int i = 0; i < Commands.Length; i++)
		{
			OverlapCapsuleCommand overlapCapsuleCommand = Commands[i];
			if ((overlapCapsuleCommand.point1 - overlapCapsuleCommand.point0).magnitude / 2f <= 0f)
			{
				SphereIndices.AddNoResize(i);
			}
		}
	}
}
