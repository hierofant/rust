using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace BasePlayerJobs;

[BurstCompile]
public struct RecacheTransforms : IJobParallelForTransform
{
	public NativeArray<Vector3> LocalPos;

	public NativeArray<Vector3> Pos;

	public NativeArray<Quaternion> LocalRots;

	public NativeArray<Quaternion> Rots;

	public void Execute(int index, TransformAccess transf)
	{
		LocalPos[index] = transf.localPosition;
		Pos[index] = transf.position;
		LocalRots[index] = transf.localRotation;
		Rots[index] = transf.rotation;
	}
}
