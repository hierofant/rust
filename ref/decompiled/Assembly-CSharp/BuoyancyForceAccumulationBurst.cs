using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public static class BuoyancyForceAccumulationBurst
{
	public struct InstanceInput
	{
		public int pointStartIndex;

		public int pointCount;

		public float buoyancyScale;

		public float rigidBodyMass;

		public float wavesEffect;

		public bool scaleForceWithMass;

		public bool flowForceDisabled;

		public float flowMovementScale;

		public float3 worldCom;
	}

	public struct InstanceOutput
	{
		public float3 netForce;

		public float3 netTorque;

		public int numSubmerged;
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void Compute_000058FA_0024PostfixBurstDelegate(in NativeArray<InstanceInput> instances, in NativeArray<float3> allPositions3D, in NativeArray<float> pointShoreDistance, in NativeArray<WaterLevel.WaterInfo> pointWaterInfo, in NativeArray<float> pointSize, in NativeArray<float> pointBuoyancyForce, in NativeArray<float> pointRandomOffset, in NativeArray<float> pointWaveFrequency, in NativeArray<float> pointWaveScale, in NativeArray<float3> pointFlowDirection, float time, ref NativeArray<InstanceOutput> results);

	internal static class Compute_000058FA_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<Compute_000058FA_0024PostfixBurstDelegate>(Compute).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in NativeArray<InstanceInput> instances, in NativeArray<float3> allPositions3D, in NativeArray<float> pointShoreDistance, in NativeArray<WaterLevel.WaterInfo> pointWaterInfo, in NativeArray<float> pointSize, in NativeArray<float> pointBuoyancyForce, in NativeArray<float> pointRandomOffset, in NativeArray<float> pointWaveFrequency, in NativeArray<float> pointWaveScale, in NativeArray<float3> pointFlowDirection, float time, ref NativeArray<InstanceOutput> results)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref NativeArray<InstanceInput>, ref NativeArray<float3>, ref NativeArray<float>, ref NativeArray<WaterLevel.WaterInfo>, ref NativeArray<float>, ref NativeArray<float>, ref NativeArray<float>, ref NativeArray<float>, ref NativeArray<float>, ref NativeArray<float3>, float, ref NativeArray<InstanceOutput>, void>)functionPointer)(ref instances, ref allPositions3D, ref pointShoreDistance, ref pointWaterInfo, ref pointSize, ref pointBuoyancyForce, ref pointRandomOffset, ref pointWaveFrequency, ref pointWaveScale, ref pointFlowDirection, time, ref results);
					return;
				}
			}
			Compute_0024BurstManaged(in instances, in allPositions3D, in pointShoreDistance, in pointWaterInfo, in pointSize, in pointBuoyancyForce, in pointRandomOffset, in pointWaveFrequency, in pointWaveScale, in pointFlowDirection, time, ref results);
		}
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(Compute_000058FA_0024PostfixBurstDelegate))]
	public static void Compute(in NativeArray<InstanceInput> instances, in NativeArray<float3> allPositions3D, in NativeArray<float> pointShoreDistance, in NativeArray<WaterLevel.WaterInfo> pointWaterInfo, in NativeArray<float> pointSize, in NativeArray<float> pointBuoyancyForce, in NativeArray<float> pointRandomOffset, in NativeArray<float> pointWaveFrequency, in NativeArray<float> pointWaveScale, in NativeArray<float3> pointFlowDirection, float time, ref NativeArray<InstanceOutput> results)
	{
		Compute_000058FA_0024BurstDirectCall.Invoke(in instances, in allPositions3D, in pointShoreDistance, in pointWaterInfo, in pointSize, in pointBuoyancyForce, in pointRandomOffset, in pointWaveFrequency, in pointWaveScale, in pointFlowDirection, time, ref results);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void Compute_0024BurstManaged(in NativeArray<InstanceInput> instances, in NativeArray<float3> allPositions3D, in NativeArray<float> pointShoreDistance, in NativeArray<WaterLevel.WaterInfo> pointWaterInfo, in NativeArray<float> pointSize, in NativeArray<float> pointBuoyancyForce, in NativeArray<float> pointRandomOffset, in NativeArray<float> pointWaveFrequency, in NativeArray<float> pointWaveScale, in NativeArray<float3> pointFlowDirection, float time, ref NativeArray<InstanceOutput> results)
	{
		for (int i = 0; i < instances.Length; i++)
		{
			InstanceInput instanceInput = instances[i];
			int pointStartIndex = instanceInput.pointStartIndex;
			int pointCount = instanceInput.pointCount;
			float wavesEffect = instanceInput.wavesEffect;
			bool flag = wavesEffect < 1f;
			float3 worldCom = instanceInput.worldCom;
			float3 zero = float3.zero;
			float3 zero2 = float3.zero;
			int num = 0;
			for (int j = 0; j < pointCount; j++)
			{
				int index = pointStartIndex + j;
				float3 @float = allPositions3D[index];
				WaterLevel.WaterInfo waterInfo = pointWaterInfo[index];
				if (!waterInfo.isValid)
				{
					continue;
				}
				float surfaceLevel = waterInfo.surfaceLevel;
				float num2 = waterInfo.currentDepth;
				if (flag)
				{
					num2 = math.lerp(num2, surfaceLevel - @float.y, wavesEffect);
				}
				if (@float.y >= surfaceLevel)
				{
					continue;
				}
				num++;
				float end = pointSize[index];
				float num3 = pointBuoyancyForce[index];
				float num4 = pointRandomOffset[index];
				float num5 = pointWaveFrequency[index];
				float num6 = pointWaveScale[index];
				float num7 = math.saturate(math.unlerp(0f, end, num2));
				float num8 = 1f + Mathf.PerlinNoise(num4 + time * num5, 0f) * num6;
				float num9 = num3 * instanceInput.buoyancyScale;
				if (instanceInput.scaleForceWithMass)
				{
					num9 *= instanceInput.rigidBodyMass;
				}
				float3 float2 = new float3(0f, num8 * num7 * num9, 0f);
				if (!waterInfo.artificalWater && !instanceInput.flowForceDisabled && (waterInfo.topology & 0x10000) == 0)
				{
					float x = math.abs(pointShoreDistance[index]);
					float num10 = math.saturate(math.unlerp(60f, 0f, x));
					if (num10 > 1E-06f)
					{
						num10 = math.pow(num10, 0.5f);
						float2 xz = pointFlowDirection[index].xz;
						float num11 = num9 * 0.025f * num10 * instanceInput.flowMovementScale;
						float2.x += xz.x * num11;
						float2.z += xz.y * num11;
					}
				}
				zero += float2;
				zero2 += math.cross(@float - worldCom, float2);
			}
			results[i] = new InstanceOutput
			{
				netForce = zero,
				netTorque = zero2,
				numSubmerged = num
			};
		}
	}
}
