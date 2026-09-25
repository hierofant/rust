using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace ProjectileJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
internal struct GenerateRaysJob : IJobParallelForTransform
{
	public NativeHashSet<int>.ReadOnly Indices;

	public NativeArray<RayGenBatchData>.ReadOnly Data;

	public NativeArray<Vector3> PositionData;

	public NativeArray<RayGenOutput> Out;

	public float Time;

	public float DeltaTime;

	public bool IsClientDemo;

	public void Execute(int index, TransformAccess transform)
	{
		if (transform.isValid && Indices.Contains(index))
		{
			RayGenBatchData data = Data[index];
			Vector3 vector = (PositionData[index] = transform.position);
			Vector3 position2 = vector;
			Out[index] = RayGenUtil.GenerateRayGenOutput(transform, in data, position2, Time, DeltaTime, IsClientDemo);
		}
	}
}
