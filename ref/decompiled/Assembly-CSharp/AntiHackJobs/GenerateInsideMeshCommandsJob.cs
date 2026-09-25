using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace AntiHackJobs;

[BurstCompile]
public struct GenerateInsideMeshCommandsJob : IJobFor
{
	[WriteOnly]
	public NativeArray<RaycastCommand> Commands;

	public NativeArray<Vector3>.ReadOnly Posi;

	[Unity.Collections.ReadOnly]
	public float Distance;

	public void Execute(int index)
	{
		QueryParameters queryParameters = new QueryParameters(65536, hitMultipleFaces: false, QueryTriggerInteraction.UseGlobal, hitBackfaces: true);
		Commands[index] = new RaycastCommand(Posi[index], Vector3.up, queryParameters, Distance);
	}
}
