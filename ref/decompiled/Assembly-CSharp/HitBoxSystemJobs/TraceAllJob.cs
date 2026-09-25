using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace HitBoxSystemJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
internal struct TraceAllJob : IJobFor
{
	public NativeArray<HitboxSystem.HitboxShape.JobStruct>.ReadOnly Shapes;

	[WriteOnly]
	public NativeArray<bool> DidHits;

	[WriteOnly]
	public NativeArray<RaycastHit> Hits;

	public Ray ray;

	public float maxDist;

	public float forgiveness;

	public void Execute(int index)
	{
		HitboxSystem.HitboxShape.JobStruct shape = Shapes[index];
		RaycastHit hit;
		bool value = Trace(in shape, ray, out hit, forgiveness, maxDist);
		DidHits[index] = value;
		Hits[index] = hit;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool Trace(in HitboxSystem.HitboxShape.JobStruct shape, Ray ray, out RaycastHit hit, float forgivness = 0f, float maxDistance = float.PositiveInfinity)
	{
		ray.origin = shape.inverseTransform.MultiplyPoint3x4(ray.origin);
		ray.direction = shape.inverseTransform.MultiplyVector(ray.direction);
		if (shape.type == HitboxDefinition.Type.BOX)
		{
			if (!new AABB(Vector3.zero, shape.size).Trace(ray, out hit, forgivness, maxDistance))
			{
				return false;
			}
		}
		else if (!new Capsule(Vector3.zero, shape.size.x, shape.size.y * 0.5f).Trace(ray, out hit, forgivness, maxDistance))
		{
			return false;
		}
		hit.point = shape.transform.MultiplyPoint3x4(hit.point);
		hit.normal = shape.transform.MultiplyVector(hit.normal);
		return true;
	}
}
