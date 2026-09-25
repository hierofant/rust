using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace GenerateErosionJobs;

[BurstCompile(FloatMode = FloatMode.Deterministic)]
internal struct CopyArrayJob<T> : IJob where T : unmanaged
{
	[WriteOnly]
	public NativeArray<T> CopyTarget;

	[Unity.Collections.ReadOnly]
	public NativeArray<T> CopySource;

	public void Execute()
	{
		CopyTarget.CopyFrom(CopySource);
	}
}
