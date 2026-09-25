using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace TerrainTopologyMapJobs;

[BurstCompile(FloatMode = FloatMode.Deterministic)]
public struct GetTopologyByPosJob : IJob
{
	[WriteOnly]
	public NativeArray<int> Topologies;

	[Unity.Collections.ReadOnly]
	public NativeArray<Vector3> Pos;

	[Unity.Collections.ReadOnly]
	public NativeArray<int> Data;

	[Unity.Collections.ReadOnly]
	public int Res;

	[Unity.Collections.ReadOnly]
	public Vector2 DataOrigin;

	[Unity.Collections.ReadOnly]
	public Vector2 DataScale;

	public void Execute()
	{
		int max = Res - 1;
		for (int i = 0; i < Pos.Length; i++)
		{
			float num = (Pos[i].x - DataOrigin.x) * DataScale.x;
			float num2 = (Pos[i].z - DataOrigin.y) * DataScale.y;
			int num3 = Math.Clamp((int)(num * (float)Res), 0, max);
			int index = Math.Clamp((int)(num2 * (float)Res), 0, max) * Res + num3;
			Topologies[i] = Data[index];
		}
	}
}
