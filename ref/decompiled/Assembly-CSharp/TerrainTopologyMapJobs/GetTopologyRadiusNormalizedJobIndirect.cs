using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace TerrainTopologyMapJobs;

[BurstCompile(FloatMode = FloatMode.Deterministic)]
internal struct GetTopologyRadiusNormalizedJobIndirect : IJobParallelFor
{
	public float OneOverSizeX;

	public NativeArray<int>.ReadOnly Src;

	public int Res;

	public NativeArray<UnityEngine.Vector2>.ReadOnly WorldNXZ;

	public NativeArray<float>.ReadOnly Radii;

	[WriteOnly]
	public NativeArray<int> Topologies;

	public void Execute(int index)
	{
		float x = WorldNXZ[index].x;
		float y = WorldNXZ[index].y;
		Topologies[index] = TerrainTopologyMapJobUtil.GetTopologyRadius(Src, Res, OneOverSizeX, Radii[index], x, y);
	}
}
