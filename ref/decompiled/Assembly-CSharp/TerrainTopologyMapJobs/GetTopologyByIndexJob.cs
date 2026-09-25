using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace TerrainTopologyMapJobs;

[BurstCompile]
public struct GetTopologyByIndexJob : IJob
{
	[WriteOnly]
	public NativeArray<int> Topologies;

	[Unity.Collections.ReadOnly]
	public NativeArray<Vector2i> Indices;

	[Unity.Collections.ReadOnly]
	public NativeArray<int> Data;

	[Unity.Collections.ReadOnly]
	public int Res;

	public void Execute()
	{
		for (int i = 0; i < Indices.Length; i++)
		{
			int index = Indices[i].y * Res + Indices[i].x;
			Topologies[i] = Data[index];
		}
	}
}
