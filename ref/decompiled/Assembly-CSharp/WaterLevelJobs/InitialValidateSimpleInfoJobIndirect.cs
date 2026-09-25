using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace WaterLevelJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
public struct InitialValidateSimpleInfoJobIndirect : IJob
{
	public NativeArray<WaterLevel.WaterInfo> Results;

	[Unity.Collections.ReadOnly]
	public NativeArray<Vector3>.ReadOnly Poses;

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
			Vector3 vector = Poses[index];
			if (vector.y > WaterHeights[index])
			{
				UnsafeUtility.ArrayElementAsRef<WaterLevel.WaterInfo>(Results.GetUnsafePtr(), index).isValid = false;
			}
			else if (vector.y < TerrainHeights[index] - 1f)
			{
				UnsafeUtility.ArrayElementAsRef<WaterLevel.WaterInfo>(Results.GetUnsafePtr(), index).isValid = false;
			}
		}
	}
}
