using System;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace ProjectileJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
internal struct BatchUpdateVelocityEndJob : IJobParallelForTransform
{
	public struct BatchData
	{
		public int DebugStableIndex;

		public Vector3 CurrentPosition;

		public Vector3 CurrentVelocity;

		public float TumbleSpeed;

		public Vector3 TumbleAxis;
	}

	public NativeHashSet<int>.ReadOnly BatchedIndices;

	public NativeArray<BatchData>.ReadOnly BatchedData;

	public float DeltaTime;

	public void Execute(int index, TransformAccess transform)
	{
		if (transform.isValid && BatchedIndices.Contains(index))
		{
			BatchData batchData = BatchedData[index];
			if (index != batchData.DebugStableIndex)
			{
				throw new Exception($"{batchData.DebugStableIndex} {index}");
			}
			transform.SetPositionAndRotation(rotation: (!(batchData.TumbleSpeed > 0f)) ? Quaternion.LookRotation(batchData.CurrentVelocity) : (transform.rotation * Quaternion.AngleAxis(batchData.TumbleSpeed * DeltaTime, batchData.TumbleAxis)), position: batchData.CurrentPosition);
		}
	}
}
