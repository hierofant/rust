using Unity.Burst;
using Unity.Jobs;
using UnityEngine;

namespace UtilityJobs;

[BurstCompile]
internal struct BakePhysicsMeshJob : IJob
{
	public int MeshId;

	public bool Convex;

	public void Execute()
	{
		Physics.BakeMesh(MeshId, Convex);
	}
}
