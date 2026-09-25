using Unity.Jobs;

namespace UnityEngine.Jobs;

public static class ParallelJobEx
{
	public static JobHandle ScheduleParallel<T>(this ref T jobData, int arrayLength, JobHandle dependsOn) where T : struct, IJobParallelFor
	{
		return IJobParallelForExtensions.Schedule(jobData, arrayLength, JobEx.GetBatchSize(arrayLength), dependsOn);
	}

	public static JobHandle ScheduleParallelByRef<T>(this ref T jobData, int arrayLength, JobHandle dependsOn) where T : struct, IJobParallelFor
	{
		return IJobParallelForExtensions.ScheduleByRef(ref jobData, arrayLength, JobEx.GetBatchSize(arrayLength), dependsOn);
	}
}
