using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

[BurstCompile]
public static class QueryVisJobs
{
	[BurstCompile]
	public struct ConstructCommandsJob : IJobParallelForTransform
	{
		[NativeDisableParallelForRestriction]
		public Vector3 cameraPosition;

		[NativeDisableParallelForRestriction]
		public QueryParameters queryParameters;

		[NativeDisableParallelForRestriction]
		[WriteOnly]
		public NativeArray<RaycastCommand> commands;

		public void Execute(int index, TransformAccess transform)
		{
			if (transform.isValid)
			{
				Vector3 vector = cameraPosition;
				Vector3 position = transform.position;
				Vector3 normalized = (position - vector).normalized;
				float distance = Vector3.Distance(vector, position);
				commands[index] = new RaycastCommand(vector, normalized, queryParameters, distance);
			}
		}
	}

	[BurstCompile]
	public struct CheckWaterLevelVisibilityJob : IJobParallelForTransform
	{
		[WriteOnly]
		public NativeArray<bool> blockedByWaterLevel;

		public float waterLevelHeight;

		public bool cameraAboveWater;

		public void Execute(int index, TransformAccess transform)
		{
			bool flag = transform.position.y - 0.5f >= waterLevelHeight;
			bool flag2 = transform.position.y + 0.5f < waterLevelHeight;
			blockedByWaterLevel[index] = (flag2 && cameraAboveWater) || (flag && !cameraAboveWater);
		}
	}

	public const float WaterLevelForgiveness = 0.5f;
}
