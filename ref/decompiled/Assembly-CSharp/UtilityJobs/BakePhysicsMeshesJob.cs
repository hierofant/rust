using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace UtilityJobs;

[BurstCompile]
internal struct BakePhysicsMeshesJob : IJobParallelFor
{
	public NativeArray<int>.ReadOnly MeshIds;

	public NativeArray<bool>.ReadOnly Convex;

	public void Execute(int index)
	{
		Physics.BakeMesh(MeshIds[index], Convex[index]);
	}
}
