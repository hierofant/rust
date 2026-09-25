using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;

namespace UnityEngine.Jobs;

public static class JobEx
{
	public static int GetBatchSize(int length)
	{
		return Mathf.Max(length / JobsUtility.JobWorkerCount, 64);
	}

	public static JobHandle ScheduleParallel<T>(this ref T jobData, int arrayLength, JobHandle dependsOn) where T : struct, IJobFor
	{
		return IJobForExtensions.ScheduleParallel(jobData, arrayLength, GetBatchSize(arrayLength), dependsOn);
	}

	public static JobHandle ScheduleParallelByRef<T>(this ref T jobData, int arrayLength, JobHandle dependsOn) where T : struct, IJobFor
	{
		return IJobForExtensions.ScheduleParallelByRef(ref jobData, arrayLength, GetBatchSize(arrayLength), dependsOn);
	}

	public static JobHandle ScheduleParallelReadOnly<T>(this ref T jobData, TransformAccessArray transforms, JobHandle dependsOn = default(JobHandle)) where T : struct, IJobParallelForTransform
	{
		return jobData.ScheduleReadOnly(transforms, GetBatchSize(transforms.length), dependsOn);
	}

	public static JobHandle ScheduleParallelReadOnlyByRef<T>(this ref T jobData, TransformAccessArray transforms, JobHandle dependsOn = default(JobHandle)) where T : struct, IJobParallelForTransform
	{
		return IJobParallelForTransformExtensions.ScheduleReadOnlyByRef(ref jobData, transforms, GetBatchSize(transforms.length), dependsOn);
	}
}
