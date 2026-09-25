using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace WaterLevelJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
public struct ApplyHeadQueryResultsJobIndirect : IJob
{
	public NativeArray<float> WaterHeights;

	[WriteOnly]
	public NativeArray<WaterLevel.WaterInfo> Infos;

	[Unity.Collections.ReadOnly]
	public NativeArray<bool>.ReadOnly ValidInfos;

	[Unity.Collections.ReadOnly]
	public NativeArray<Vector3>.ReadOnly Starts;

	[Unity.Collections.ReadOnly]
	public NativeArray<int>.ReadOnly Indices;

	public unsafe void Execute()
	{
		for (int i = 0; i < Indices.Length; i++)
		{
			int index = Indices[i];
			float a = WaterHeights[index];
			if (ValidInfos[index])
			{
				UnsafeUtility.ArrayElementAsRef<WaterLevel.WaterInfo>(Infos.GetUnsafePtr(), index).isValid = false;
				a = -1000f;
			}
			else
			{
				a = Mathf.Min(a, Starts[index].y);
			}
			WaterHeights[index] = a;
		}
	}
}
