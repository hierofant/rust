using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace TerrainTopologyMapJobs;

[BurstCompile(FloatMode = FloatMode.Deterministic)]
public struct GetTopologyByUVJobIndirect : IJob
{
	[WriteOnly]
	public NativeArray<int> Topologies;

	[Unity.Collections.ReadOnly]
	public NativeArray<UnityEngine.Vector2>.ReadOnly UV;

	[Unity.Collections.ReadOnly]
	public NativeArray<int>.ReadOnly Indices;

	[Unity.Collections.ReadOnly]
	public NativeArray<int>.ReadOnly Data;

	[Unity.Collections.ReadOnly]
	public int Res;

	public void Execute()
	{
		int max = Res - 1;
		for (int i = 0; i < Indices.Length; i++)
		{
			int index = Indices[i];
			int num = Math.Clamp((int)(UV[index].x * (float)Res), 0, max);
			int index2 = Math.Clamp((int)(UV[index].y * (float)Res), 0, max) * Res + num;
			Topologies[index] = Data[index2];
		}
	}
}
