using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace AntiHackJobs;

[BurstCompile]
public struct FilterInsideMeshHitsJob : IJobFor
{
	public NativeArray<RaycastHit> Hits;

	public void Execute(int index)
	{
		RaycastHit raycastHit = Hits[index];
		if (raycastHit.colliderInstanceID == 0 || !(Vector3.Dot(Vector3.up, raycastHit.normal) > 0f))
		{
			Hits[index] = default(RaycastHit);
		}
	}
}
