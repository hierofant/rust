using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace ServerOcclusionJobs;

[BurstCompile(FloatMode = FloatMode.Fast, OptimizeFor = OptimizeFor.Performance, DisableSafetyChecks = true)]
public struct CalculatePathsBetweenGridsJob : IJobParallelForBatch
{
	public NativeArray<(ServerOcclusion.SubGrid from, ServerOcclusion.SubGrid to)>.ReadOnly Paths;

	public NativeArray<bool> PathsBlocked;

	public GridDefinition Grid;

	public int BlockedGridThreshold;

	public int NeighbourThreshold;

	public bool UseNeighbourThresholds;

	public void Execute(int startIndex, int count)
	{
		for (int i = startIndex; i < startIndex + count; i++)
		{
			(ServerOcclusion.SubGrid from, ServerOcclusion.SubGrid to) tuple = Paths[i];
			ServerOcclusion.SubGrid item = tuple.from;
			ServerOcclusion.SubGrid item2 = tuple.to;
			int3 from = new int3(item.x, item.y, item.z);
			int3 to = new int3(item2.x, item2.y, item2.z);
			PathsBlocked[i] = Algorithm.Trace(from, to, in Grid, BlockedGridThreshold, NeighbourThreshold, UseNeighbourThresholds);
		}
	}
}
