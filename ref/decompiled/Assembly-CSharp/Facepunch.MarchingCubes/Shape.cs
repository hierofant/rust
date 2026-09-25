using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Facepunch.MarchingCubes;

[BurstCompile]
public readonly struct Shape
{
	public readonly ShapeType Type;

	public readonly bool IsAdditive;

	public readonly float Smoothing;

	public readonly float3 Position;

	public readonly float3 Extents;

	public readonly quaternion Rotation;

	private readonly quaternion InvRotation;

	public Shape(ShapeType type, float3 position, float3 extents, quaternion rotation, bool isAdditive, float smoothing)
	{
		Type = type;
		Position = position;
		Extents = extents;
		Rotation = rotation;
		InvRotation = math.inverse(rotation);
		IsAdditive = isAdditive;
		Smoothing = smoothing;
	}

	public Bounds GetBounds()
	{
		float num = 4f * Smoothing + 1f;
		switch (Type)
		{
		case ShapeType.Sphere:
			return new Bounds((Vector3)Position, Vector3.one * ((Extents.x + num) * 2f));
		case ShapeType.AABB:
			return new Bounds((Vector3)Position, (Vector3)(Extents + num) * 2f);
		case ShapeType.OBB:
		case ShapeType.SharpOBB:
			return Facepunch.MarchingCubes.SDFBounds.OrientedExtentsBounds(Position, Extents + num, Rotation);
		case ShapeType.Cylinder:
			return Facepunch.MarchingCubes.SDFBounds.OrientedExtentsBounds(Position, new float3(Extents.x, Extents.y, Extents.x) + num, Rotation);
		case ShapeType.Capsule:
			return Facepunch.MarchingCubes.SDFBounds.OrientedExtentsBounds(Position, new float3(Extents.x, Extents.y + Extents.x, Extents.x) + num, Rotation);
		case ShapeType.Cone:
			return Facepunch.MarchingCubes.SDFBounds.OrientedExtentsBounds(Position, new float3(Extents.x, Extents.y, Extents.x) + num, Rotation);
		case ShapeType.HexPrism:
		{
			float num2 = Extents.x * 1.1547005f;
			return Facepunch.MarchingCubes.SDFBounds.OrientedExtentsBounds(Position, new float3(num2, num2, Extents.y) + num, Rotation);
		}
		case ShapeType.Bulge:
			return new Bounds((Vector3)Position, Vector3.one * (Extents.x + 1f) * 2f);
		case ShapeType.Smooth:
			return new Bounds((Vector3)Position, Vector3.one * (Extents.x + 2f) * 2f);
		default:
			return new Bounds((Vector3)Position, Vector3.zero);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal float SphereDistance(float3 p)
	{
		return math.length(p - Position) - Extents.x;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal float AABBDistance(float3 p)
	{
		float3 x = math.abs(Position - p) - Extents;
		return math.length(math.max(x, 0f)) + math.min(math.max(x.x, math.max(x.y, x.z)), 0f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal float OBBDistance(float3 p)
	{
		float num = math.cmin(Extents) * 0.25f;
		float3 x = math.abs(math.rotate(InvRotation, Position - p)) - Extents + num;
		return math.length(math.max(x, 0f)) + math.min(math.max(x.x, math.max(x.y, x.z)), 0f) - num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal float SharpOBBDistance(float3 p)
	{
		float3 x = math.abs(math.rotate(InvRotation, p - Position)) - Extents;
		return math.length(math.max(x, 0f)) + math.min(math.max(x.x, math.max(x.y, x.z)), 0f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal float CylinderDistance(float3 p)
	{
		float3 @float = math.rotate(InvRotation, p - Position);
		float2 x = math.abs(new float2(math.length(@float.xz), @float.y)) - new float2(Extents.x, Extents.y);
		return math.min(math.max(x.x, x.y), 0f) + math.length(math.max(x, 0f));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal float CapsuleDistance(float3 p)
	{
		float3 x = math.rotate(InvRotation, p - Position);
		x.y -= math.clamp(x.y, 0f - Extents.y, Extents.y);
		return math.length(x) - Extents.x;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal float ConeDistance(float3 p)
	{
		float x = Extents.x;
		float y = Extents.y;
		float3 @float = math.rotate(InvRotation, p - Position);
		float2 float2 = new float2(math.length(@float.xz), @float.y);
		float2 float3 = new float2(0f, y);
		float2 float4 = new float2(0f - x, 2f * y);
		float2 float5 = new float2(float2.x - math.min(float2.x, math.select(0f, x, float2.y < 0f)), math.abs(float2.y) - y);
		float2 float6 = float2 - float3 + float4 * math.clamp(math.dot(float3 - float2, float4) / math.dot(float4, float4), 0f, 1f);
		return ((float6.x < 0f && float5.y < 0f) ? (-1f) : 1f) * math.sqrt(math.min(math.dot(float5, float5), math.dot(float6, float6)));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal float HexPrismDistance(float3 p)
	{
		float x = Extents.x;
		float y = Extents.y;
		float3 @float = new float3(-0.8660254f, 0.5f, 0.57735f);
		float3 float2 = math.abs(math.rotate(InvRotation, p - Position));
		float2.xy -= 2f * math.min(math.dot(@float.xy, float2.xy), 0f) * @float.xy;
		float2 x2 = new float2(math.length(float2.xy - new float2(math.clamp(float2.x, (0f - @float.z) * x, @float.z * x), x)) * math.sign(float2.y - x), float2.z - y);
		return math.min(math.max(x2.x, x2.y), 0f) + math.length(math.max(x2, 0f));
	}
}
