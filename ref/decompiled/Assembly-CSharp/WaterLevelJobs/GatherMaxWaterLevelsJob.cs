using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace WaterLevelJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
public struct GatherMaxWaterLevelsJob : IJob
{
	[WriteOnly]
	public NativeArray<float> WaterLevels;

	public NativeArray<Vector3>.ReadOnly Positions;

	public Bounds DeepSeaBounds;

	public float waterLevelMain;

	public float waterLevelDeep;

	public void Execute()
	{
		for (int i = 0; i < Positions.Length; i++)
		{
			WaterLevels[i] = (DeepSeaBounds.Contains(Positions[i]) ? waterLevelDeep : waterLevelMain);
		}
	}
}
