using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace WaterLevelJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
public struct ToUVJobIndirect : IJob
{
	[WriteOnly]
	public NativeArray<Vector2> UV;

	[Unity.Collections.ReadOnly]
	public NativeArray<Vector3>.ReadOnly Pos;

	[Unity.Collections.ReadOnly]
	public NativeArray<int>.ReadOnly Indices;

	[Unity.Collections.ReadOnly]
	public Vector2 TerrainPos;

	[Unity.Collections.ReadOnly]
	public Vector2 TerrainOneOverSize;

	public void Execute()
	{
		for (int i = 0; i < Indices.Length; i++)
		{
			int index = Indices[i];
			Vector3 vector = Pos[index];
			Vector2 vector2 = new Vector2(vector.x, vector.z);
			UV[index] = (vector2 - TerrainPos) * TerrainOneOverSize;
		}
	}
}
