using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace WaterLevelJobs;

[BurstCompile]
public struct SelectMaxWaterLevelJobIndirect : IJob
{
	public NativeArray<float> Heights;

	[Unity.Collections.ReadOnly]
	public NativeArray<float>.ReadOnly DynamicHeights;

	[Unity.Collections.ReadOnly]
	public NativeArray<int>.ReadOnly Indices;

	[Unity.Collections.ReadOnly]
	public float OceanLevel;

	public void Execute()
	{
		for (int i = 0; i < Indices.Length; i++)
		{
			int index = Indices[i];
			float a = Heights[index];
			float b = OceanLevel + DynamicHeights[index];
			Heights[index] = Mathf.Max(a, b);
		}
	}
}
