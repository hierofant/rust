using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace TerrainTopologyMapJobs;

[BurstCompile(FloatMode = FloatMode.Deterministic)]
internal struct GetTopologyRadiusJob : IJob
{
	public int Res;

	public NativeArray<int>.ReadOnly Src;

	public NativeReference<int> Topo;

	public int x_mid;

	public int z_mid;

	public int x_min;

	public int x_max;

	public int z_min;

	public int z_max;

	public float radius;

	public void Execute()
	{
		Topo.Value = TerrainTopologyMapJobUtil.GetTopologyRadius(Src, Res, radius, x_min, x_mid, x_max, z_min, z_mid, z_max);
	}
}
