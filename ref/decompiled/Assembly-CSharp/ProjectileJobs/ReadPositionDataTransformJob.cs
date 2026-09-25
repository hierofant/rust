using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace ProjectileJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
internal struct ReadPositionDataTransformJob : IJobParallelForTransform
{
	public NativeArray<Vector3> Positions;

	public void Execute(int index, TransformAccess transform)
	{
		if (transform.isValid)
		{
			Positions[index] = transform.position;
		}
	}
}
