using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace AntiHackJobs;

[BurstCompile]
public struct CheckInsideMeshHitsJob : IJobFor
{
	[WriteOnly]
	public NativeArray<bool> Results;

	public NativeArray<RaycastHit>.ReadOnly Hits;

	public void Execute(int index)
	{
		RaycastHit raycastHit = Hits[index];
		Results[index] = raycastHit.colliderInstanceID != 0 && Vector3.Dot(Vector3.up, raycastHit.normal) > 0f;
	}
}
