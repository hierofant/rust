using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GamePhysicsJobs;

[BurstCompile]
public struct GatherPosFromOBBsJob : IJob
{
	[WriteOnly]
	public NativeArray<Vector3> Posi;

	[Unity.Collections.ReadOnly]
	public NativeArray<OBB>.ReadOnly OBBs;

	public void Execute()
	{
		for (int i = 0; i < OBBs.Length; i++)
		{
			Posi[i] = OBBs[i].position;
		}
	}
}
