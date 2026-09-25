using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;

namespace WaterLevelJobs;

[BurstCompile]
public static class WaterLevelBurst
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void GetBuoyancyWaterInfoBatched_000085F0_0024PostfixBurstDelegate(in NativeArray<Vector3> allPositions, in NativeArray<Vector2> allUVPositions, in NativeArray<float> pointTerrainHeightNativeArray, in NativeArray<float> pointWaterHeightNativeArray, in NativeArray<bool> doDeepWaterChecksStateNativeArray, ref NativeArray<WaterLevel.WaterInfo> pointWaterInfoNativeArray, in NativeArray<int> instancePointCountNativeArray, in int instanceCount, in TerrainTopologyMap.TopologyQueryStructure topologyMap, in NativeArray<bool> waterIgnoreStates, ref NativeArray<bool> needsDeepWaterChecks, bool isDeepSea, out bool hasAnyDeepWaterChecks);

	internal static class GetBuoyancyWaterInfoBatched_000085F0_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<GetBuoyancyWaterInfoBatched_000085F0_0024PostfixBurstDelegate>(GetBuoyancyWaterInfoBatched).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in NativeArray<Vector3> allPositions, in NativeArray<Vector2> allUVPositions, in NativeArray<float> pointTerrainHeightNativeArray, in NativeArray<float> pointWaterHeightNativeArray, in NativeArray<bool> doDeepWaterChecksStateNativeArray, ref NativeArray<WaterLevel.WaterInfo> pointWaterInfoNativeArray, in NativeArray<int> instancePointCountNativeArray, in int instanceCount, in TerrainTopologyMap.TopologyQueryStructure topologyMap, in NativeArray<bool> waterIgnoreStates, ref NativeArray<bool> needsDeepWaterChecks, bool isDeepSea, out bool hasAnyDeepWaterChecks)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref NativeArray<Vector3>, ref NativeArray<Vector2>, ref NativeArray<float>, ref NativeArray<float>, ref NativeArray<bool>, ref NativeArray<WaterLevel.WaterInfo>, ref NativeArray<int>, ref int, ref TerrainTopologyMap.TopologyQueryStructure, ref NativeArray<bool>, ref NativeArray<bool>, bool, ref bool, void>)functionPointer)(ref allPositions, ref allUVPositions, ref pointTerrainHeightNativeArray, ref pointWaterHeightNativeArray, ref doDeepWaterChecksStateNativeArray, ref pointWaterInfoNativeArray, ref instancePointCountNativeArray, ref instanceCount, ref topologyMap, ref waterIgnoreStates, ref needsDeepWaterChecks, isDeepSea, ref hasAnyDeepWaterChecks);
					return;
				}
			}
			GetBuoyancyWaterInfoBatched_0024BurstManaged(in allPositions, in allUVPositions, in pointTerrainHeightNativeArray, in pointWaterHeightNativeArray, in doDeepWaterChecksStateNativeArray, ref pointWaterInfoNativeArray, in instancePointCountNativeArray, in instanceCount, in topologyMap, in waterIgnoreStates, ref needsDeepWaterChecks, isDeepSea, out hasAnyDeepWaterChecks);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void ConstructDeepWaterCommands_000085F1_0024PostfixBurstDelegate(in NativeArray<Vector3> allPositions, in NativeArray<WaterLevel.WaterInfo> pointWaterInfoNativeArray, in NativeArray<bool> needsDeepWaterChecks, out NativeList<RaycastCommand> deepWaterCasts, out NativeList<int> raycastPointIndices, Allocator allocator);

	internal static class ConstructDeepWaterCommands_000085F1_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<ConstructDeepWaterCommands_000085F1_0024PostfixBurstDelegate>(ConstructDeepWaterCommands).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in NativeArray<Vector3> allPositions, in NativeArray<WaterLevel.WaterInfo> pointWaterInfoNativeArray, in NativeArray<bool> needsDeepWaterChecks, out NativeList<RaycastCommand> deepWaterCasts, out NativeList<int> raycastPointIndices, Allocator allocator)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref NativeArray<Vector3>, ref NativeArray<WaterLevel.WaterInfo>, ref NativeArray<bool>, ref NativeList<RaycastCommand>, ref NativeList<int>, Allocator, void>)functionPointer)(ref allPositions, ref pointWaterInfoNativeArray, ref needsDeepWaterChecks, ref deepWaterCasts, ref raycastPointIndices, allocator);
					return;
				}
			}
			ConstructDeepWaterCommands_0024BurstManaged(in allPositions, in pointWaterInfoNativeArray, in needsDeepWaterChecks, out deepWaterCasts, out raycastPointIndices, allocator);
		}
	}

	[MonoPInvokeCallback(typeof(WaterLevelJobs_002EGetBuoyancyWaterInfoBatched_000085F0_0024PostfixBurstDelegate))]
	[BurstCompile]
	public static void GetBuoyancyWaterInfoBatched(in NativeArray<Vector3> allPositions, in NativeArray<Vector2> allUVPositions, in NativeArray<float> pointTerrainHeightNativeArray, in NativeArray<float> pointWaterHeightNativeArray, in NativeArray<bool> doDeepWaterChecksStateNativeArray, ref NativeArray<WaterLevel.WaterInfo> pointWaterInfoNativeArray, in NativeArray<int> instancePointCountNativeArray, in int instanceCount, in TerrainTopologyMap.TopologyQueryStructure topologyMap, in NativeArray<bool> waterIgnoreStates, ref NativeArray<bool> needsDeepWaterChecks, bool isDeepSea, out bool hasAnyDeepWaterChecks)
	{
		GetBuoyancyWaterInfoBatched_000085F0_0024BurstDirectCall.Invoke(in allPositions, in allUVPositions, in pointTerrainHeightNativeArray, in pointWaterHeightNativeArray, in doDeepWaterChecksStateNativeArray, ref pointWaterInfoNativeArray, in instancePointCountNativeArray, in instanceCount, in topologyMap, in waterIgnoreStates, ref needsDeepWaterChecks, isDeepSea, out hasAnyDeepWaterChecks);
	}

	[MonoPInvokeCallback(typeof(WaterLevelJobs_002EConstructDeepWaterCommands_000085F1_0024PostfixBurstDelegate))]
	[BurstCompile]
	public static void ConstructDeepWaterCommands(in NativeArray<Vector3> allPositions, in NativeArray<WaterLevel.WaterInfo> pointWaterInfoNativeArray, in NativeArray<bool> needsDeepWaterChecks, out NativeList<RaycastCommand> deepWaterCasts, out NativeList<int> raycastPointIndices, Allocator allocator)
	{
		ConstructDeepWaterCommands_000085F1_0024BurstDirectCall.Invoke(in allPositions, in pointWaterInfoNativeArray, in needsDeepWaterChecks, out deepWaterCasts, out raycastPointIndices, allocator);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void GetBuoyancyWaterInfoBatched_0024BurstManaged(in NativeArray<Vector3> allPositions, in NativeArray<Vector2> allUVPositions, in NativeArray<float> pointTerrainHeightNativeArray, in NativeArray<float> pointWaterHeightNativeArray, in NativeArray<bool> doDeepWaterChecksStateNativeArray, ref NativeArray<WaterLevel.WaterInfo> pointWaterInfoNativeArray, in NativeArray<int> instancePointCountNativeArray, in int instanceCount, in TerrainTopologyMap.TopologyQueryStructure topologyMap, in NativeArray<bool> waterIgnoreStates, ref NativeArray<bool> needsDeepWaterChecks, bool isDeepSea, out bool hasAnyDeepWaterChecks)
	{
		hasAnyDeepWaterChecks = false;
		int num = 0;
		for (int i = 0; i < instanceCount; i++)
		{
			bool flag = doDeepWaterChecksStateNativeArray[i];
			float num2 = pointTerrainHeightNativeArray[i];
			int num3 = instancePointCountNativeArray[i];
			int num4 = num + num3;
			for (int j = num; j < num4; j++)
			{
				Vector3 position = allPositions[j];
				Vector2 uv = allUVPositions[j];
				float num5 = pointWaterHeightNativeArray[j];
				WaterLevel.WaterInfo value = default(WaterLevel.WaterInfo);
				if (position.y > num5 && WaterVolumeBurst.TestBurst(in position, out var info))
				{
					pointWaterInfoNativeArray[j] = info;
					continue;
				}
				bool flag2 = position.y < num2 - 1f;
				if (flag2 && WaterVolumeBurst.TestBurst(in position, out var info2))
				{
					pointWaterInfoNativeArray[j] = info2;
					continue;
				}
				bool flag3 = flag && (position.y < num5 - 10f || (TerrainMeta.OutOfBoundsBurst(position) && !isDeepSea));
				int topologyFast = topologyMap.GetTopologyFast(uv);
				if ((flag2 || flag3 || (topologyFast & 0x3C180) == 0) && waterIgnoreStates[j])
				{
					pointWaterInfoNativeArray[j] = value;
					continue;
				}
				if (flag3)
				{
					needsDeepWaterChecks[j] = true;
					hasAnyDeepWaterChecks = true;
				}
				value.isValid = true;
				value.currentDepth = Mathf.Max(0f, num5 - position.y);
				value.overallDepth = Mathf.Max(0f, num5 - num2);
				value.surfaceLevel = num5;
				value.terrainHeight = num2;
				value.topology = topologyFast;
				pointWaterInfoNativeArray[j] = value;
			}
			num += num3;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void ConstructDeepWaterCommands_0024BurstManaged(in NativeArray<Vector3> allPositions, in NativeArray<WaterLevel.WaterInfo> pointWaterInfoNativeArray, in NativeArray<bool> needsDeepWaterChecks, out NativeList<RaycastCommand> deepWaterCasts, out NativeList<int> raycastPointIndices, Allocator allocator)
	{
		deepWaterCasts = new NativeList<RaycastCommand>(32, allocator);
		raycastPointIndices = new NativeList<int>(32, Allocator.Temp);
		QueryParameters queryParameters = default(QueryParameters);
		queryParameters.hitTriggers = QueryTriggerInteraction.Collide;
		queryParameters.layerMask = 16;
		QueryParameters queryParameters2 = queryParameters;
		for (int i = 0; i < needsDeepWaterChecks.Length; i++)
		{
			if (needsDeepWaterChecks[i])
			{
				Vector3 from = allPositions[i];
				RaycastCommand value = new RaycastCommand(from, Vector3.up, queryParameters2);
				deepWaterCasts.Add(in value);
				raycastPointIndices.Add(in i);
			}
		}
	}
}
