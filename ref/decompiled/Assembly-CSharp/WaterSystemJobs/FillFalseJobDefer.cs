using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace WaterSystemJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
internal struct FillFalseJobDefer : IJobParallelForDefer
{
	[Unity.Collections.ReadOnly]
	public NativeList<Ray> rays;

	public NativeArray<bool> HitResults;

	public void Execute(int index)
	{
		HitResults[index] = false;
	}
}
