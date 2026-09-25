using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace GamePhysicsJobs;

[BurstCompile]
public struct ValidateOverlapSphereCommandsJob : IJob
{
	[WriteOnly]
	public NativeList<int> InvalidIndices;

	[Unity.Collections.ReadOnly]
	public NativeArray<UnityEngine.OverlapSphereCommand>.ReadOnly Commands;

	public void Execute()
	{
		for (int i = 0; i < Commands.Length; i++)
		{
			if (Commands[i].radius < 0f)
			{
				InvalidIndices.AddNoResize(i);
			}
		}
	}
}
