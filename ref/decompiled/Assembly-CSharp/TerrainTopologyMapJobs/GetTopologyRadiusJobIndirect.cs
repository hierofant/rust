using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace TerrainTopologyMapJobs;

[BurstCompile(FloatMode = FloatMode.Deterministic)]
internal struct GetTopologyRadiusJobIndirect : IJobParallelFor
{
	public float WorldX;

	public float WorldZ;

	public float OneOverSizeX;

	public float OneOverSizeZ;

	public NativeArray<int>.ReadOnly Src;

	public int Res;

	public NativeArray<Vector3>.ReadOnly WorldPositions;

	public NativeArray<float>.ReadOnly Radii;

	[WriteOnly]
	public NativeArray<int> Topologies;

	public void Execute(int index)
	{
		Vector3 vector = WorldPositions[index];
		float normX = (vector.x - WorldX) * OneOverSizeX;
		float normZ = (vector.z - WorldZ) * OneOverSizeZ;
		Topologies[index] = TerrainTopologyMapJobUtil.GetTopologyRadius(Src, Res, OneOverSizeX, Radii[index], normX, normZ);
	}
}
