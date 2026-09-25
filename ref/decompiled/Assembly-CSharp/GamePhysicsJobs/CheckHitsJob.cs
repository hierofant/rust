using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace GamePhysicsJobs;

[BurstCompile]
public struct CheckHitsJob : IJob
{
	[WriteOnly]
	public NativeArray<bool> Results;

	[Unity.Collections.ReadOnly]
	public NativeArray<UnityEngine.ColliderHit>.ReadOnly Hits;

	public void Execute()
	{
		for (int i = 0; i < Hits.Length; i++)
		{
			Results[i] = Hits[i].instanceID != 0;
		}
	}
}
