using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace AntiHackJobs;

[BurstCompile]
public struct InsideTerrainHeightsChecksJob : IJob
{
	[WriteOnly]
	public NativeArray<bool> Results;

	[WriteOnly]
	public NativeList<int> IndicesToCheck;

	[WriteOnly]
	public NativeArray<Vector3> PosiToCheck;

	[WriteOnly]
	public NativeArray<float> RadiiToCheck;

	public NativeArray<Vector3>.ReadOnly Posi;

	public NativeArray<float>.ReadOnly HeightMapHeights;

	public NativeArray<float>.ReadOnly TerrainHeights;

	[Unity.Collections.ReadOnly]
	public float TerrainPadding;

	[Unity.Collections.ReadOnly]
	public float RadiusToCheck;

	public void Execute()
	{
		int num = 0;
		for (int i = 0; i < Posi.Length; i++)
		{
			Vector3 value = Posi[i];
			float num2 = value.y + TerrainPadding;
			float num3 = HeightMapHeights[i];
			float num4 = TerrainHeights[i];
			if (num2 > num3 || num2 > num4)
			{
				Results[i] = false;
				continue;
			}
			IndicesToCheck.AddNoResize(i);
			PosiToCheck[num] = value;
			RadiiToCheck[num] = RadiusToCheck;
			num++;
		}
	}
}
