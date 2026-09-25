using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace WaterLevelJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
public struct InitialValidateInfoJobIndirect : IJob
{
	[WriteOnly]
	public NativeArray<WaterLevel.WaterInfo> Results;

	[Unity.Collections.ReadOnly]
	public NativeArray<Vector3>.ReadOnly Starts;

	[Unity.Collections.ReadOnly]
	public NativeArray<Vector3>.ReadOnly Ends;

	[Unity.Collections.ReadOnly]
	public NativeArray<float>.ReadOnly Radii;

	[Unity.Collections.ReadOnly]
	public NativeArray<float>.ReadOnly WaterHeights;

	[Unity.Collections.ReadOnly]
	public NativeArray<float>.ReadOnly TerrainHeights;

	[Unity.Collections.ReadOnly]
	public NativeArray<int>.ReadOnly Indices;

	public void Execute()
	{
		for (int i = 0; i < Indices.Length; i++)
		{
			int index = Indices[i];
			Vector3 vector = Starts[index];
			Vector3 vector2 = Ends[index];
			float num = Radii[index];
			float minY = Mathf.Min(vector.y, vector2.y) - num;
			float maxY = Mathf.Max(vector.y, vector2.y) + num;
			Results[index] = WaterLevel.InitialValidate(minY, maxY, WaterHeights[index], TerrainHeights[index]);
		}
	}
}
