using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace AntiHackJobs;

[BurstCompile]
public struct AdjustDistBasedOnNormalsJob : IJob
{
	public NativeArray<(float Dist, float Budget)> DistAndBudget;

	public NativeArray<Vector3>.ReadOnly Start;

	public NativeArray<Vector3>.ReadOnly End;

	public NativeArray<Vector3>.ReadOnly Normals;

	public NativeArray<float>.ReadOnly DeltaTime;

	public NativeArray<int>.ReadOnly Indices;

	[Unity.Collections.ReadOnly]
	public float SlopeSpeed;

	public void Execute()
	{
		Span<(float, float)> span = DistAndBudget;
		for (int i = 0; i < Indices.Length; i++)
		{
			int index = Indices[i];
			Vector3 v = End[index] - Start[index];
			float num = Mathf.Max(0f, Vector3.Dot(Normals[index].XZ3D(), v.XZ3D())) * SlopeSpeed * DeltaTime[index];
			ref(float, float) reference = ref span[index];
			reference.Item1 = Mathf.Max(0f, reference.Item1 - num);
		}
	}
}
