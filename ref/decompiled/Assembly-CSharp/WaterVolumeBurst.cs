using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public class WaterVolumeBurst
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate bool TestBurst_00006524_0024PostfixBurstDelegate(in Vector3 position, out WaterLevel.WaterInfo info, float queryRadius = 100f);

	internal static class TestBurst_00006524_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<TestBurst_00006524_0024PostfixBurstDelegate>(TestBurst).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static bool Invoke(in Vector3 position, out WaterLevel.WaterInfo info, float queryRadius = 100f)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<ref Vector3, ref WaterLevel.WaterInfo, float, bool>)functionPointer)(ref position, ref info, queryRadius);
				}
			}
			return TestBurst_0024BurstManaged(in position, out info, queryRadius);
		}
	}

	[MonoPInvokeCallback(typeof(TestBurst_00006524_0024PostfixBurstDelegate))]
	[BurstCompile]
	public static bool TestBurst(in Vector3 position, out WaterLevel.WaterInfo info, float queryRadius = 100f)
	{
		return TestBurst_00006524_0024BurstDirectCall.Invoke(in position, out info, queryRadius);
	}

	private static bool CheckCutOffPlanesBurst(in WaterVolumeBurstData data, in Vector3 pos, out float bottomCutY)
	{
		int length = data.cutOffPlanePoses.Length;
		bottomCutY = float.MaxValue;
		bool flag = true;
		for (int i = 0; i < length; i++)
		{
			float3 xyz = math.mul(math.inverse(data.cutOffPlaneMatrices[i]), new float4(pos, 1f)).xyz;
			Vector3 position = data.cutOffPlanePoses[i].position;
			if (math.dot(data.cutOffPlanePoses[i].up, data.bounds.up) < -0.1f)
			{
				bottomCutY = math.min(bottomCutY, position.y);
			}
			if (xyz.y > 0f)
			{
				flag = false;
				break;
			}
		}
		if (!flag)
		{
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static bool TestBurst_0024BurstManaged(in Vector3 position, out WaterLevel.WaterInfo info, float queryRadius = 100f)
	{
		NativeList<WaterVolumeBurstData> nativeList = WaterVolume.WaterVolumeBoundsGrid.Data.Query(Allocator.Temp, position.x, position.z, queryRadius);
		for (int i = 0; i < nativeList.Length; i++)
		{
			OBB bounds = nativeList[i].bounds;
			if (bounds.Contains(position))
			{
				WaterVolumeBurstData data = nativeList[i];
				if (CheckCutOffPlanesBurst(in data, in position, out var bottomCutY))
				{
					Vector3 vector = new Plane(bounds.up, bounds.position).ClosestPointOnPlane(position);
					float y = (vector + bounds.up * bounds.extents.y).y;
					float y2 = (vector + -bounds.up * bounds.extents.y).y;
					y2 = math.max(y2, bottomCutY);
					info = default(WaterLevel.WaterInfo);
					info.isValid = true;
					info.artificalWater = !nativeList[i].naturalSource;
					info.currentDepth = Mathf.Max(0f, y - position.y);
					info.overallDepth = Mathf.Max(0f, y - y2);
					info.surfaceLevel = y;
					return true;
				}
			}
		}
		info = default(WaterLevel.WaterInfo);
		return false;
	}
}
