using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace BasePlayerJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
internal struct UpdateWaterCache : IJob
{
	[WriteOnly]
	public NativeArray<BasePlayer.CachedState> States;

	[Unity.Collections.ReadOnly]
	public NativeArray<float>.ReadOnly Factors;

	[Unity.Collections.ReadOnly]
	public NativeArray<WaterLevel.WaterInfo>.ReadOnly Infos;

	[Unity.Collections.ReadOnly]
	public NativeArray<int>.ReadOnly Indices;

	public unsafe void Execute()
	{
		for (int i = 0; i < Indices.Length; i++)
		{
			int index = Indices[i];
			ref BasePlayer.CachedState reference = ref UnsafeUtility.ArrayElementAsRef<BasePlayer.CachedState>(States.GetUnsafePtr(), index);
			reference.WaterFactor = Factors[index];
			reference.WaterInfo = Infos[index];
			reference.IsSwimming = BasePlayer.IsSwimming(reference.WaterFactor);
		}
	}
}
