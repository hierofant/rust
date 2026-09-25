using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace BasePlayerJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
public struct CalcWaterFactorsJobIndirect : IJob
{
	[WriteOnly]
	public NativeArray<float> Factors;

	[Unity.Collections.ReadOnly]
	public NativeArray<int>.ReadOnly Indices;

	[Unity.Collections.ReadOnly]
	public NativeArray<WaterLevel.WaterInfo>.ReadOnly Infos;

	[Unity.Collections.ReadOnly]
	public NativeArray<UnityEngine.Vector3>.ReadOnly Starts;

	[Unity.Collections.ReadOnly]
	public NativeArray<UnityEngine.Vector3>.ReadOnly Ends;

	[Unity.Collections.ReadOnly]
	public NativeArray<float>.ReadOnly Radii;

	public void Execute()
	{
		for (int i = 0; i < Indices.Length; i++)
		{
			int index = Indices[i];
			ref NativeArray<float> factors = ref Factors;
			WaterLevel.WaterInfo info = Infos[index];
			factors[index] = WaterLevel.Factor(in info, Starts[index], Ends[index], Radii[index]);
		}
	}
}
