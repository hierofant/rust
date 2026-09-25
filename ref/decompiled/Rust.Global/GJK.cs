#define UNITY_ASSERTIONS
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Assertions;

[BurstCompile]
public static class GJK
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate float Distance_0000009A_0024PostfixBurstDelegate(in OBB a, in OBB b);

	internal static class Distance_0000009A_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<Distance_0000009A_0024PostfixBurstDelegate>(Distance).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static float Invoke(in OBB a, in OBB b)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<ref OBB, ref OBB, float>)functionPointer)(ref a, ref b);
				}
			}
			return Distance_0024BurstManaged(in a, in b);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate float Distance2_0000009B_0024PostfixBurstDelegate(in OBB a, in OBB b);

	internal static class Distance2_0000009B_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<Distance2_0000009B_0024PostfixBurstDelegate>(Distance2).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static float Invoke(in OBB a, in OBB b)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<ref OBB, ref OBB, float>)functionPointer)(ref a, ref b);
				}
			}
			return Distance2_0024BurstManaged(in a, in b);
		}
	}

	private static readonly ProfilerMarker p_Distance = new ProfilerMarker("GJK.Distance");

	private const float MaxIterations = 32f;

	[BurstCompile]
	[MonoPInvokeCallback(typeof(Distance_0000009A_0024PostfixBurstDelegate))]
	public static float Distance(in OBB a, in OBB b)
	{
		return Distance_0000009A_0024BurstDirectCall.Invoke(in a, in b);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(Distance2_0000009B_0024PostfixBurstDelegate))]
	public static float Distance2(in OBB a, in OBB b)
	{
		return Distance2_0000009B_0024BurstDirectCall.Invoke(in a, in b);
	}

	private static float DistanceInternal(in ReadOnlySpan<Vector3> a, in ReadOnlySpan<Vector3> b)
	{
		SolveDistanceSimplex(in a, in b, out var simplex);
		float num = Vector3.SqrMagnitude(simplex.Direction);
		float num2 = 1f / Mathf.Sqrt(num);
		if (num != 0f)
		{
			return simplex.ScaledDistance * num2;
		}
		return 0f;
	}

	private static float Distance2Internal(in ReadOnlySpan<Vector3> a, in ReadOnlySpan<Vector3> b)
	{
		SolveDistanceSimplex(in a, in b, out var simplex);
		float num = Vector3.SqrMagnitude(simplex.Direction);
		if (num != 0f)
		{
			return simplex.ScaledDistance * simplex.ScaledDistance / num;
		}
		return 0f;
	}

	private static void SolveDistanceSimplex(in ReadOnlySpan<Vector3> a, in ReadOnlySpan<Vector3> b, out Simplex simplex)
	{
		Assert.IsTrue(a.Length > 0, "Distance function called with empty LHS collider");
		Assert.IsTrue(b.Length > 0, "Distance function called with empty RHS collider");
		Simplex simplex2 = new Simplex
		{
			size = 1
		};
		Vector3 direction = Vector3.up;
		simplex2.a = GetSupportingVertex(in a, in b, in direction);
		simplex = simplex2;
		simplex.Direction = simplex.a;
		simplex.ScaledDistance = Vector3.SqrMagnitude(simplex.a);
		float num = simplex.ScaledDistance;
		for (int i = 0; (float)i < 32f; i++)
		{
			direction = -simplex.Direction;
			Vector3 supportingVertex = GetSupportingVertex(in a, in b, in direction);
			float num2 = Vector3.Dot(simplex.a - supportingVertex, simplex.Direction);
			if (!(num2 * Mathf.Abs(num2) < 1E-08f * num))
			{
				switch (simplex.size++)
				{
				case 1:
					simplex.b = supportingVertex;
					break;
				case 2:
					simplex.c = supportingVertex;
					break;
				default:
					simplex.d = supportingVertex;
					break;
				}
				simplex.SolveDistance();
				num = Vector3.SqrMagnitude(simplex.Direction);
				if (simplex.size == 4)
				{
					simplex.ScaledDistance = 0f;
					break;
				}
				continue;
			}
			break;
		}
	}

	private static Vector3 Support(in ReadOnlySpan<Vector3> vertices, in Vector3 direction)
	{
		float num = float.MinValue;
		Vector3 result = Vector3.zero;
		ReadOnlySpan<Vector3> readOnlySpan = vertices;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			Vector3 vector = readOnlySpan[i];
			float num2 = Vector3.Dot(vector, direction);
			if (num2 > num)
			{
				num = num2;
				result = vector;
			}
		}
		return result;
	}

	private static Vector3 GetSupportingVertex(in ReadOnlySpan<Vector3> verticesA, in ReadOnlySpan<Vector3> verticesB, in Vector3 direction)
	{
		Vector3 vector = Support(in verticesA, in direction);
		Vector3 direction2 = -direction;
		Vector3 vector2 = Support(in verticesB, in direction2);
		return vector - vector2;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void PopulateVertices(OBB obb, Span<Vector3> vertices)
	{
		Assert.IsTrue(vertices.Length >= 8);
		Vector3 position = obb.position;
		Vector3 right = obb.right;
		Vector3 up = obb.up;
		Vector3 forward = obb.forward;
		Vector3 extents = obb.extents;
		vertices[0] = position + right * extents.x + up * extents.y + forward * extents.z;
		vertices[1] = position + right * extents.x + up * extents.y - forward * extents.z;
		vertices[2] = position + right * extents.x - up * extents.y + forward * extents.z;
		vertices[3] = position + right * extents.x - up * extents.y - forward * extents.z;
		vertices[4] = position - right * extents.x + up * extents.y + forward * extents.z;
		vertices[5] = position - right * extents.x + up * extents.y - forward * extents.z;
		vertices[6] = position - right * extents.x - up * extents.y + forward * extents.z;
		vertices[7] = position - right * extents.x - up * extents.y - forward * extents.z;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static float Distance_0024BurstManaged(in OBB a, in OBB b)
	{
		using (p_Distance.Auto())
		{
			Span<Vector3> span = stackalloc Vector3[8];
			Span<Vector3> span2 = stackalloc Vector3[8];
			PopulateVertices(a, span);
			PopulateVertices(b, span2);
			ReadOnlySpan<Vector3> a2 = span;
			ReadOnlySpan<Vector3> b2 = span2;
			return DistanceInternal(in a2, in b2);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static float Distance2_0024BurstManaged(in OBB a, in OBB b)
	{
		using (p_Distance.Auto())
		{
			Span<Vector3> span = stackalloc Vector3[8];
			Span<Vector3> span2 = stackalloc Vector3[8];
			PopulateVertices(a, span);
			PopulateVertices(b, span2);
			ReadOnlySpan<Vector3> a2 = span;
			ReadOnlySpan<Vector3> b2 = span2;
			return Distance2Internal(in a2, in b2);
		}
	}
}
