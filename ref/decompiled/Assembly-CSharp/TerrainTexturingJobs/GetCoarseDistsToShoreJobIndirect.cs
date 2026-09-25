using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace TerrainTexturingJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
public struct GetCoarseDistsToShoreJobIndirect : IJob
{
	[WriteOnly]
	public NativeArray<float> Dists;

	public NativeArray<Vector3>.ReadOnly Positions;

	public NativeArray<int>.ReadOnly Indices;

	[Unity.Collections.ReadOnly]
	public TerrainTexturing.ShoreVectorQueryStructure QueryStructure;

	public void Execute()
	{
		for (int i = 0; i < Indices.Length; i++)
		{
			int index = Indices[i];
			Vector3 pos = Positions[index];
			Dists[index] = QueryStructure.GetCoarseDistanceToShore(pos);
		}
	}
}
