using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace TerrainTexturingJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
internal struct ProcessTopologyJob : IJobParallelFor
{
	public NativeArray<Vector4> vectors;

	public NativeArray<int>.ReadOnly topologies;

	public void Execute(int index)
	{
		Vector4 value = vectors[index];
		int num = topologies[index];
		if (((uint)num & 0x180u) != 0)
		{
			value.w = 1f;
		}
		else if (((uint)num & 0x32000u) != 0)
		{
			value.w = 2f;
		}
		else if (((uint)num & 0xC000u) != 0)
		{
			value.w = 3f;
		}
		vectors[index] = value;
	}
}
