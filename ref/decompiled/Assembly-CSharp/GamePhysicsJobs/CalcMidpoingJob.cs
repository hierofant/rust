using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GamePhysicsJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
public struct CalcMidpoingJob : IJob
{
	[WriteOnly]
	public NativeArray<Vector3> Results;

	[Unity.Collections.ReadOnly]
	public NativeArray<Vector3>.ReadOnly From;

	[Unity.Collections.ReadOnly]
	public NativeArray<Vector3>.ReadOnly To;

	public void Execute()
	{
		for (int i = 0; i < From.Length; i++)
		{
			Vector3 vector = From[i];
			Vector3 vector2 = To[i];
			Results[i] = (vector + vector2) * 0.5f;
		}
	}
}
