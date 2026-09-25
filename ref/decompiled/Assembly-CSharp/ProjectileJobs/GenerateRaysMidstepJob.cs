using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace ProjectileJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
internal struct GenerateRaysMidstepJob : IJobParallelForTransform
{
	public NativeArray<Vector3>.ReadOnly PositionData;

	public NativeHashSet<int>.ReadOnly Indices;

	public NativeArray<RayGenBatchData>.ReadOnly Data;

	public NativeArray<RayGenOutput> Out;

	public float Time;

	public float DeltaTime;

	public bool IsClientDemo;

	public void Execute(int index, TransformAccess transform)
	{
		if (transform.isValid && Indices.Contains(index))
		{
			RayGenBatchData data = Data[index];
			Vector3 position = PositionData[index];
			Out[index] = RayGenUtil.GenerateRayGenOutput(transform, in data, position, Time, DeltaTime, IsClientDemo);
		}
	}
}
