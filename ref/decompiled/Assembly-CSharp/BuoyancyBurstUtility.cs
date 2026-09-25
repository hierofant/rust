using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;

[BurstCompile]
public class BuoyancyBurstUtility
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void FillPointData_000058F8_0024PostfixBurstDelegate(in int pointIndexOffset, ref NativeArray<Vector2> pointPositionArray, ref NativeArray<Vector2> pointPositionUVArray, in Matrix4x4 rootToWorld, ref NativeArray<Buoyancy.BuoyancyPointData> pointData, in Bounds deepSeaBounds, in Vector3 terrainPosition, in Vector3 terrainOneOverSize, in bool isDeepSea, ref NativeArray<Vector3> allPositions3D, out int pointCount);

	internal static class FillPointData_000058F8_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<FillPointData_000058F8_0024PostfixBurstDelegate>(FillPointData).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in int pointIndexOffset, ref NativeArray<Vector2> pointPositionArray, ref NativeArray<Vector2> pointPositionUVArray, in Matrix4x4 rootToWorld, ref NativeArray<Buoyancy.BuoyancyPointData> pointData, in Bounds deepSeaBounds, in Vector3 terrainPosition, in Vector3 terrainOneOverSize, in bool isDeepSea, ref NativeArray<Vector3> allPositions3D, out int pointCount)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref int, ref NativeArray<Vector2>, ref NativeArray<Vector2>, ref Matrix4x4, ref NativeArray<Buoyancy.BuoyancyPointData>, ref Bounds, ref Vector3, ref Vector3, ref bool, ref NativeArray<Vector3>, ref int, void>)functionPointer)(ref pointIndexOffset, ref pointPositionArray, ref pointPositionUVArray, ref rootToWorld, ref pointData, ref deepSeaBounds, ref terrainPosition, ref terrainOneOverSize, ref isDeepSea, ref allPositions3D, ref pointCount);
					return;
				}
			}
			FillPointData_0024BurstManaged(in pointIndexOffset, ref pointPositionArray, ref pointPositionUVArray, in rootToWorld, ref pointData, in deepSeaBounds, in terrainPosition, in terrainOneOverSize, in isDeepSea, ref allPositions3D, out pointCount);
		}
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(FillPointData_000058F8_0024PostfixBurstDelegate))]
	public static void FillPointData(in int pointIndexOffset, ref NativeArray<Vector2> pointPositionArray, ref NativeArray<Vector2> pointPositionUVArray, in Matrix4x4 rootToWorld, ref NativeArray<Buoyancy.BuoyancyPointData> pointData, in Bounds deepSeaBounds, in Vector3 terrainPosition, in Vector3 terrainOneOverSize, in bool isDeepSea, ref NativeArray<Vector3> allPositions3D, out int pointCount)
	{
		FillPointData_000058F8_0024BurstDirectCall.Invoke(in pointIndexOffset, ref pointPositionArray, ref pointPositionUVArray, in rootToWorld, ref pointData, in deepSeaBounds, in terrainPosition, in terrainOneOverSize, in isDeepSea, ref allPositions3D, out pointCount);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void FillPointData_0024BurstManaged(in int pointIndexOffset, ref NativeArray<Vector2> pointPositionArray, ref NativeArray<Vector2> pointPositionUVArray, in Matrix4x4 rootToWorld, ref NativeArray<Buoyancy.BuoyancyPointData> pointData, in Bounds deepSeaBounds, in Vector3 terrainPosition, in Vector3 terrainOneOverSize, in bool isDeepSea, ref NativeArray<Vector3> allPositions3D, out int pointCount)
	{
		float x;
		float z;
		float x2;
		float z2;
		if (isDeepSea)
		{
			x = deepSeaBounds.min.x;
			z = deepSeaBounds.min.z;
			x2 = deepSeaBounds.size.Inverse().x;
			z2 = deepSeaBounds.size.Inverse().z;
		}
		else
		{
			x = terrainPosition.x;
			z = terrainPosition.z;
			x2 = terrainOneOverSize.x;
			z2 = terrainOneOverSize.z;
		}
		for (int i = 0; i < pointData.Length; i++)
		{
			Vector3 value = rootToWorld.MultiplyPoint3x4(pointData[i].rootToPoint);
			float x3 = (value.x - x) * x2;
			float y = (value.z - z) * z2;
			pointPositionArray[i + pointIndexOffset] = new Vector2(value.x, value.z);
			pointPositionUVArray[i + pointIndexOffset] = new Vector2(x3, y);
			allPositions3D[i + pointIndexOffset] = value;
		}
		pointCount = pointData.Length;
	}
}
