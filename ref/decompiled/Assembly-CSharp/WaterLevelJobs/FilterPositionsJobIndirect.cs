using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace WaterLevelJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
public struct FilterPositionsJobIndirect : IJob
{
	[WriteOnly]
	public NativeList<int> OverworldIndices;

	[WriteOnly]
	public NativeList<int> DeepSeaIndices;

	public NativeArray<Vector3>.ReadOnly Positions;

	public NativeArray<int>.ReadOnly Indices;

	public Bounds DeepSeaBounds;

	public void Execute()
	{
		for (int i = 0; i < Indices.Length; i++)
		{
			int num = Indices[i];
			if (DeepSeaBounds.Contains(Positions[num]))
			{
				DeepSeaIndices.AddNoResize(num);
			}
			else
			{
				OverworldIndices.AddNoResize(num);
			}
		}
	}
}
