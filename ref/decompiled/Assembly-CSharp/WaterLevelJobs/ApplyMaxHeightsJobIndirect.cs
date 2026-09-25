using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace WaterLevelJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
public struct ApplyMaxHeightsJobIndirect : IJob
{
	public NativeArray<float> Heights;

	[Unity.Collections.ReadOnly]
	public NativeArray<int>.ReadOnly Topologies;

	[Unity.Collections.ReadOnly]
	public NativeArray<int>.ReadOnly Indices;

	[Unity.Collections.ReadOnly]
	public NativeArray<float>.ReadOnly WaterLevels;

	[Unity.Collections.ReadOnly]
	public float OceanLevel;

	public void Execute()
	{
		for (int i = 0; i < Indices.Length; i++)
		{
			int index = Indices[i];
			bool num = Heights[index] < WaterLevels[index];
			bool flag = (Topologies[index] & 0x180) != 0;
			if (num && flag)
			{
				Heights[index] = Math.Max(Heights[index], OceanLevel);
			}
		}
	}
}
