using Unity.Mathematics;
using UnityEngine;

namespace Facepunch.MarchingCubes;

internal static class SDFBounds
{
	public static Bounds OrientedExtentsBounds(float3 position, float3 halfExtents, quaternion rotation)
	{
		float3 @float = math.rotate(rotation, math.right()) * halfExtents.x;
		float3 float2 = math.rotate(rotation, math.up()) * halfExtents.y;
		float3 float3 = math.rotate(rotation, math.forward()) * halfExtents.z;
		Bounds result = new Bounds((Vector3)position, Vector3.zero);
		result.Encapsulate((Vector3)(position + float2 + @float + float3));
		result.Encapsulate((Vector3)(position + float2 + @float - float3));
		result.Encapsulate((Vector3)(position + float2 - @float + float3));
		result.Encapsulate((Vector3)(position + float2 - @float - float3));
		result.Encapsulate((Vector3)(position - float2 + @float + float3));
		result.Encapsulate((Vector3)(position - float2 + @float - float3));
		result.Encapsulate((Vector3)(position - float2 - @float + float3));
		result.Encapsulate((Vector3)(position - float2 - @float - float3));
		return result;
	}
}
