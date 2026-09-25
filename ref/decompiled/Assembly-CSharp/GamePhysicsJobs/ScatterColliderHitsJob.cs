using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GamePhysicsJobs;

[BurstCompile]
public struct ScatterColliderHitsJob : IJob
{
	[WriteOnly]
	public NativeArray<ColliderHit> To;

	[Unity.Collections.ReadOnly]
	public NativeArray<ColliderHit>.ReadOnly From;

	[Unity.Collections.ReadOnly]
	public NativeArray<int>.ReadOnly Indices;

	[Unity.Collections.ReadOnly]
	public int MaxHitsPerRay;

	public void Execute()
	{
		for (int i = 0; i < Indices.Length; i++)
		{
			int srcIndex = i * MaxHitsPerRay;
			int dstIndex = Indices[i] * MaxHitsPerRay;
			NativeArray<ColliderHit>.Copy(From, srcIndex, To, dstIndex, MaxHitsPerRay);
		}
	}
}
