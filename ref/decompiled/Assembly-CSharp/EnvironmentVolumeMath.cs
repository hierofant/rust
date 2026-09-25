using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public static class EnvironmentVolumeMath
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void CalculateTransformationBoundsBurst_0000602A_0024PostfixBurstDelegate(in float4x4 transformationMatrix, in bool capsule, out Bounds bounds);

	internal static class CalculateTransformationBoundsBurst_0000602A_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<CalculateTransformationBoundsBurst_0000602A_0024PostfixBurstDelegate>(CalculateTransformationBoundsBurst).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in float4x4 transformationMatrix, in bool capsule, out Bounds bounds)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref float4x4, ref bool, ref Bounds, void>)functionPointer)(ref transformationMatrix, ref capsule, ref bounds);
					return;
				}
			}
			CalculateTransformationBoundsBurst_0024BurstManaged(in transformationMatrix, in capsule, out bounds);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void MultiplyPoint3X4_0000602B_0024PostfixBurstDelegate(in float4x4 transformationMatrix, in float3 point, out float3 result);

	internal static class MultiplyPoint3X4_0000602B_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<MultiplyPoint3X4_0000602B_0024PostfixBurstDelegate>(MultiplyPoint3X4).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in float4x4 transformationMatrix, in float3 point, out float3 result)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref float4x4, ref float3, ref float3, void>)functionPointer)(ref transformationMatrix, ref point, ref result);
					return;
				}
			}
			MultiplyPoint3X4_0024BurstManaged(in transformationMatrix, in point, out result);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void UpdateVolumeTransformationAndBoundsBurst_0000602C_0024PostfixBurstDelegate(in float3 size, in float3 center, in float4x4 localToWorldMatrix, in bool isCapsule, out float4x4 volumeTransformation, out float4x4 volumeTransformationInverse, out float3 volumePosition, out Bounds volumeBounds);

	internal static class UpdateVolumeTransformationAndBoundsBurst_0000602C_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<UpdateVolumeTransformationAndBoundsBurst_0000602C_0024PostfixBurstDelegate>(UpdateVolumeTransformationAndBoundsBurst).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in float3 size, in float3 center, in float4x4 localToWorldMatrix, in bool isCapsule, out float4x4 volumeTransformation, out float4x4 volumeTransformationInverse, out float3 volumePosition, out Bounds volumeBounds)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref float3, ref float3, ref float4x4, ref bool, ref float4x4, ref float4x4, ref float3, ref Bounds, void>)functionPointer)(ref size, ref center, ref localToWorldMatrix, ref isCapsule, ref volumeTransformation, ref volumeTransformationInverse, ref volumePosition, ref volumeBounds);
					return;
				}
			}
			UpdateVolumeTransformationAndBoundsBurst_0024BurstManaged(in size, in center, in localToWorldMatrix, in isCapsule, out volumeTransformation, out volumeTransformationInverse, out volumePosition, out volumeBounds);
		}
	}

	[MonoPInvokeCallback(typeof(CalculateTransformationBoundsBurst_0000602A_0024PostfixBurstDelegate))]
	[BurstCompile]
	private static void CalculateTransformationBoundsBurst(in float4x4 transformationMatrix, in bool capsule, out Bounds bounds)
	{
		CalculateTransformationBoundsBurst_0000602A_0024BurstDirectCall.Invoke(in transformationMatrix, in capsule, out bounds);
	}

	[MonoPInvokeCallback(typeof(MultiplyPoint3X4_0000602B_0024PostfixBurstDelegate))]
	[BurstCompile]
	private static void MultiplyPoint3X4(in float4x4 transformationMatrix, in float3 point, out float3 result)
	{
		MultiplyPoint3X4_0000602B_0024BurstDirectCall.Invoke(in transformationMatrix, in point, out result);
	}

	[MonoPInvokeCallback(typeof(UpdateVolumeTransformationAndBoundsBurst_0000602C_0024PostfixBurstDelegate))]
	[BurstCompile]
	public static void UpdateVolumeTransformationAndBoundsBurst(in float3 size, in float3 center, in float4x4 localToWorldMatrix, in bool isCapsule, out float4x4 volumeTransformation, out float4x4 volumeTransformationInverse, out float3 volumePosition, out Bounds volumeBounds)
	{
		UpdateVolumeTransformationAndBoundsBurst_0000602C_0024BurstDirectCall.Invoke(in size, in center, in localToWorldMatrix, in isCapsule, out volumeTransformation, out volumeTransformationInverse, out volumePosition, out volumeBounds);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void CalculateTransformationBoundsBurst_0024BurstManaged(in float4x4 transformationMatrix, in bool capsule, out Bounds bounds)
	{
		float3 @float = new float3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
		float3 float2 = new float3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
		ReadOnlySpan<float3> readOnlySpan = stackalloc float3[8]
		{
			new float3(-0.5f, -0.5f, -0.5f),
			new float3(0.5f, -0.5f, -0.5f),
			new float3(0.5f, 0.5f, -0.5f),
			new float3(-0.5f, 0.5f, -0.5f),
			new float3(-0.5f, -0.5f, 0.5f),
			new float3(0.5f, -0.5f, 0.5f),
			new float3(0.5f, 0.5f, 0.5f),
			new float3(-0.5f, 0.5f, 0.5f)
		};
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			MultiplyPoint3X4(in transformationMatrix, in readOnlySpan[i], out var result);
			@float = math.min(@float, result);
			float2 = math.max(float2, result);
		}
		if (capsule)
		{
			float num = math.abs(float2.y - @float.y) * 0.5f;
			@float.y -= num;
			float2.y += num;
		}
		bounds = new Bounds(Vector3.zero, Vector3.one);
		bounds.SetMinMax((Vector3)@float, (Vector3)float2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void MultiplyPoint3X4_0024BurstManaged(in float4x4 transformationMatrix, in float3 point, out float3 result)
	{
		result.x = transformationMatrix.c0.x * point.x + transformationMatrix.c1.x * point.y + transformationMatrix.c2.x * point.z + transformationMatrix.c3.x;
		result.y = transformationMatrix.c0.y * point.x + transformationMatrix.c1.y * point.y + transformationMatrix.c2.y * point.z + transformationMatrix.c3.y;
		result.z = transformationMatrix.c0.z * point.x + transformationMatrix.c1.z * point.y + transformationMatrix.c2.z * point.z + transformationMatrix.c3.z;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void UpdateVolumeTransformationAndBoundsBurst_0024BurstManaged(in float3 size, in float3 center, in float4x4 localToWorldMatrix, in bool isCapsule, out float4x4 volumeTransformation, out float4x4 volumeTransformationInverse, out float3 volumePosition, out Bounds volumeBounds)
	{
		float3 scales = size + new float3(0.001f, 0.001f, 0.001f);
		float4x4 b = math.mul(float4x4.Translate(center), float4x4.Scale(scales));
		volumeTransformation = math.mul(localToWorldMatrix, b);
		volumeTransformationInverse = math.inverse(volumeTransformation);
		volumePosition = Float4x4Ex.ToPosition(volumeTransformation);
		CalculateTransformationBoundsBurst(in volumeTransformation, in isCapsule, out volumeBounds);
	}
}
