using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace UtilityJobs;

[BurstCompile]
public struct AddVectorJob : IJob
{
	[WriteOnly]
	public NativeArray<Vector3> Results;

	public NativeArray<Vector3>.ReadOnly Inputs;

	[Unity.Collections.ReadOnly]
	public Vector3 Modification;

	public void Execute()
	{
		for (int i = 0; i < Inputs.Length; i++)
		{
			Results[i] = Inputs[i] + Modification;
		}
	}
}
