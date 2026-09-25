using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace WaterLevelJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
public struct ResolveWaterInfosJobIndirect : IJob
{
	public NativeArray<WaterLevel.WaterInfo> Infos;

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

	public unsafe void Execute()
	{
		for (int i = 0; i < Indices.Length; i++)
		{
			int index = Indices[i];
			Vector3 vector = Starts[index];
			Vector3 vector2 = Ends[index];
			float num = Radii[index];
			float num2 = Mathf.Min(vector.y, vector2.y) - num;
			float num3 = WaterHeights[index];
			float num4 = TerrainHeights[index];
			ref WaterLevel.WaterInfo reference = ref UnsafeUtility.ArrayElementAsRef<WaterLevel.WaterInfo>(Infos.GetUnsafePtr(), index);
			reference.currentDepth = Mathf.Max(0f, num3 - num2);
			reference.overallDepth = Mathf.Max(0f, num3 - num4);
			reference.surfaceLevel = num3;
			reference.terrainHeight = num4;
		}
	}
}
