using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace ServerOcclusionJobs;

[BurstCompile(FloatMode = FloatMode.Fast, OptimizeFor = OptimizeFor.Performance, DisableSafetyChecks = true)]
public struct CalculatePathBetweenGridsJob : IJob
{
	public ServerOcclusion.SubGrid From;

	public ServerOcclusion.SubGrid To;

	public NativeReference<bool> PathBlocked;

	public GridDefinition Grid;

	public int BlockedGridThreshold;

	public int NeighbourThreshold;

	public bool UseNeighbourThresholds;

	public void Execute()
	{
		int3 from = new int3(From.x, From.y, From.z);
		int3 to = new int3(To.x, To.y, To.z);
		PathBlocked.Value = Algorithm.Trace(from, to, in Grid, BlockedGridThreshold, NeighbourThreshold, UseNeighbourThresholds);
	}
}
