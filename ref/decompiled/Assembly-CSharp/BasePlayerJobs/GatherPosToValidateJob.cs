using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace BasePlayerJobs;

[BurstCompile]
internal struct GatherPosToValidateJob : IJob
{
	[WriteOnly]
	public NativeArray<BasePlayer.PositionChange> Changes;

	[WriteOnly]
	public NativeList<int> ToValidate;

	[Unity.Collections.ReadOnly]
	public TickInterpolatorCache.ReadOnlyState TickCache;

	[Unity.Collections.ReadOnly]
	public NativeArray<int>.ReadOnly Indices;

	public void Execute()
	{
		for (int i = 0; i < Indices.Length; i++)
		{
			int num = Indices[i];
			Vector3 startPoint = TickInterpolatorCache.GetStartPoint(TickCache, num);
			Vector3 endPoint = TickInterpolatorCache.GetEndPoint(TickCache, num);
			bool num2 = startPoint != endPoint;
			Changes[num] = BasePlayer.PositionChange.Same;
			if (num2)
			{
				ToValidate.AddNoResize(num);
			}
		}
	}
}
