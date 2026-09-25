using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public static class TerrainWaterFlowMapBurst
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void GetFlowDirections_00006EC7_0024PostfixBurstDelegate(in NativeArray<Vector3> positions3D, ref NativeArray<float3> results, in NativeArray<byte> source, in int res);

	internal static class GetFlowDirections_00006EC7_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<GetFlowDirections_00006EC7_0024PostfixBurstDelegate>(GetFlowDirections).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in NativeArray<Vector3> positions3D, ref NativeArray<float3> results, in NativeArray<byte> source, in int res)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref NativeArray<Vector3>, ref NativeArray<float3>, ref NativeArray<byte>, ref int, void>)functionPointer)(ref positions3D, ref results, ref source, ref res);
					return;
				}
			}
			GetFlowDirections_0024BurstManaged(in positions3D, ref results, in source, in res);
		}
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(GetFlowDirections_00006EC7_0024PostfixBurstDelegate))]
	public static void GetFlowDirections(in NativeArray<Vector3> positions3D, ref NativeArray<float3> results, in NativeArray<byte> source, in int res)
	{
		GetFlowDirections_00006EC7_0024BurstDirectCall.Invoke(in positions3D, ref results, in source, in res);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void GetFlowDirections_0024BurstManaged(in NativeArray<Vector3> positions3D, ref NativeArray<float3> results, in NativeArray<byte> source, in int res)
	{
		TerrainMeta.BurstData data = TerrainMeta.sharedBurstData.Data;
		Vector3 position = data.Position;
		Vector3 oneOverSize = data.OneOverSize;
		for (int i = 0; i < positions3D.Length; i++)
		{
			Vector3 vector = positions3D[i];
			float num = (vector.x - position.x) * oneOverSize.x;
			float num2 = (vector.z - position.z) * oneOverSize.z;
			int valueToClamp = (int)(num * (float)(res - 1));
			int valueToClamp2 = (int)(num2 * (float)(res - 1));
			valueToClamp = math.clamp(valueToClamp, 0, res - 1);
			valueToClamp2 = math.clamp(valueToClamp2, 0, res - 1);
			float x = TerrainWaterFlowMap.ByteToAngle(source[valueToClamp2 * res + valueToClamp]);
			results[i] = new float3(math.sin(x), 0f, math.cos(x));
		}
	}
}
