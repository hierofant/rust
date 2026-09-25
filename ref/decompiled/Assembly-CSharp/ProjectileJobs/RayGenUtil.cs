using UnityEngine;
using UnityEngine.Jobs;

namespace ProjectileJobs;

public static class RayGenUtil
{
	public static RayGenOutput GenerateRayGenOutput(TransformAccess transform, in RayGenBatchData data, Vector3 position, float time, float deltaTime, bool isClientDemo)
	{
		Vector3 currentVelocity = data.CurrentVelocity;
		if (data.SwimScale != Vector3.zero)
		{
			Vector3 swimSpeed = data.SwimSpeed;
			Vector3 swimScale = data.SwimScale;
			float num = time + data.SwimRandom;
			Vector3 vector = new Vector3(Mathf.Sin(num * swimSpeed.x) * swimScale.x, Mathf.Cos(num * swimSpeed.y) * swimScale.y, Mathf.Sin(num * swimSpeed.z) * swimScale.z);
			vector = Quaternion.Inverse(transform.rotation) * vector;
			currentVelocity += vector;
		}
		Vector3 vector2 = currentVelocity * deltaTime;
		float magnitude = vector2.magnitude;
		float num2 = 1f / magnitude;
		Vector3 vector3 = vector2 * num2;
		if (isClientDemo && vector3.IsNaNOrInfinity())
		{
			vector3 = Vector3.zero;
		}
		RayGenOutput result = default(RayGenOutput);
		result.Ray = new Ray(position, vector3);
		result.MaxDistance = magnitude;
		return result;
	}
}
