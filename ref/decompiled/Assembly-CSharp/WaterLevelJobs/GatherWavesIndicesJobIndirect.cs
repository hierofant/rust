using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace WaterLevelJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
public struct GatherWavesIndicesJobIndirect : IJob
{
	[WriteOnly]
	public NativeArray<int> WaveIndices;

	[WriteOnly]
	public NativeReference<int> WaveIndexCount;

	public NativeArray<UnityEngine.Vector3>.ReadOnly Positions;

	public NativeArray<int>.ReadOnly Topologies;

	public NativeArray<float>.ReadOnly Heights;

	public NativeArray<int>.ReadOnly Indices;

	public NativeArray<float>.ReadOnly WaterLevels;

	public void Execute()
	{
		int value = 0;
		for (int i = 0; i < Indices.Length; i++)
		{
			int num = Indices[i];
			bool num2 = Heights[num] < WaterLevels[num];
			bool flag = (Topologies[num] & 0x180) != 0;
			if (num2 && flag)
			{
				WaveIndices[value++] = num;
			}
		}
		WaveIndexCount.Value = value;
	}
}
