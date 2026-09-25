using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace TerrainTopologyMapJobs;

[BurstCompile(FloatMode = FloatMode.Deterministic)]
public struct GetTopologyByUVJob : IJob
{
	[WriteOnly]
	public NativeArray<int> Topologies;

	[Unity.Collections.ReadOnly]
	public NativeArray<Vector2> UV;

	[Unity.Collections.ReadOnly]
	public NativeArray<int> Data;

	[Unity.Collections.ReadOnly]
	public int Res;

	public void Execute()
	{
		int max = Res - 1;
		for (int i = 0; i < UV.Length; i++)
		{
			int num = Math.Clamp((int)(UV[i].x * (float)Res), 0, max);
			int index = Math.Clamp((int)(UV[i].y * (float)Res), 0, max) * Res + num;
			Topologies[i] = Data[index];
		}
	}
}
