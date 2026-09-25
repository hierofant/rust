using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace ProjectileJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
internal struct PosRotReadJob : IJobParallelForTransform
{
	public NativeArray<Vector3> Positions;

	public NativeArray<Quaternion> Rotations;

	public void Execute(int index, TransformAccess transform)
	{
		if (transform.isValid)
		{
			transform.GetPositionAndRotation(out var position, out var rotation);
			Positions[index] = position;
			Rotations[index] = rotation;
		}
	}
}
