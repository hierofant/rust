using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace WaterLevelJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
public struct ResolveWaterInfosSimpleJobIndirect : IJob
{
	[WriteOnly]
	public NativeArray<WaterLevel.WaterInfo> Infos;

	[Unity.Collections.ReadOnly]
	public NativeArray<Vector3>.ReadOnly Poses;

	[Unity.Collections.ReadOnly]
	public NativeArray<float>.ReadOnly WaterHeights;

	[Unity.Collections.ReadOnly]
	public NativeArray<float>.ReadOnly TerrainHeights;

	[Unity.Collections.ReadOnly]
	public NativeArray<bool>.ReadOnly UseVolumeDepths;

	[Unity.Collections.ReadOnly]
	public NativeArray<int>.ReadOnly Indices;

	public unsafe void Execute()
	{
		for (int i = 0; i < Indices.Length; i++)
		{
			int index = Indices[i];
			Vector3 vector = Poses[index];
			float num = WaterHeights[index];
			float num2 = TerrainHeights[index];
			ref WaterLevel.WaterInfo reference = ref UnsafeUtility.ArrayElementAsRef<WaterLevel.WaterInfo>(Infos.GetUnsafePtr(), index);
			reference.currentDepth = Mathf.Max(0f, num - vector.y);
			if (!UseVolumeDepths[index])
			{
				reference.overallDepth = Mathf.Max(0f, num - num2);
			}
			reference.surfaceLevel = num;
			reference.terrainHeight = num2;
		}
	}
}
