using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace AntiHackJobs;

[BurstCompile]
public struct GatherHitColliderIndicesJob : IJob
{
	public NativeList<int> Results;

	public NativeArray<UnityEngine.ColliderHit>.ReadOnly Hits;

	[Unity.Collections.ReadOnly]
	public int ResultsPerQuery;

	public void Execute()
	{
		int num = Hits.Length / ResultsPerQuery;
		for (int i = 0; i < num; i++)
		{
			int num2 = i * ResultsPerQuery;
			for (int j = 0; j < ResultsPerQuery && Hits[num2 + j].instanceID != 0; j++)
			{
				Results.AddNoResize(num2 + j);
			}
		}
	}
}
