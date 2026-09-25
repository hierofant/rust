using BasePlayerJobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace AntiHackJobs;

[BurstCompile]
public struct CalculateRDCsJob : IJobFor
{
	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<RDC> RDCs;

	public NativeArray<BasePlayer.CachedState>.ReadOnly States;

	public NativeArray<ModelState.Flag>.ReadOnly MsFlags;

	public NativeArray<float>.ReadOnly Ducking;

	public NativeArray<int>.ReadOnly Indices;

	public void Execute(int jobInd)
	{
		int index = Indices[jobInd];
		BasePlayer.CachedState cachedState = States[index];
		float running = (((MsFlags[index] & ModelState.Flag.Sprinting) != 0) ? 1 : 0);
		float ducking = ((BasePlayer.IsDucked(Ducking[index]) || cachedState.IsSwimming) ? 1 : 0);
		float crawling = (BasePlayer.IsCrawling(cachedState.PlayerFlags) ? 1 : 0);
		RDCs[index] = new RDC
		{
			Running = running,
			Ducking = ducking,
			Crawling = crawling
		};
	}
}
