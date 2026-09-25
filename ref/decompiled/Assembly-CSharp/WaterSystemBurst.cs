using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;

[BurstCompile]
public class WaterSystemBurst
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void GetHeightArray_Burst_000071FD_0024PostfixBurstDelegate(in NativeArray<Vector2>.ReadOnly posUV, ref NativeArray<float> shore, ref NativeArray<float> terrainHeight, in TerrainTexturing.ShoreData shoreData, in TerrainHeightMap.HeightMapQueryStructure terrainHeightMapData, in bool hasHeightMap, in bool hasTerrainTexturing);

	internal static class GetHeightArray_Burst_000071FD_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<GetHeightArray_Burst_000071FD_0024PostfixBurstDelegate>(GetHeightArray_Burst).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in NativeArray<Vector2>.ReadOnly posUV, ref NativeArray<float> shore, ref NativeArray<float> terrainHeight, in TerrainTexturing.ShoreData shoreData, in TerrainHeightMap.HeightMapQueryStructure terrainHeightMapData, in bool hasHeightMap, in bool hasTerrainTexturing)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref NativeArray<Vector2>.ReadOnly, ref NativeArray<float>, ref NativeArray<float>, ref TerrainTexturing.ShoreData, ref TerrainHeightMap.HeightMapQueryStructure, ref bool, ref bool, void>)functionPointer)(ref posUV, ref shore, ref terrainHeight, ref shoreData, ref terrainHeightMapData, ref hasHeightMap, ref hasTerrainTexturing);
					return;
				}
			}
			GetHeightArray_Burst_0024BurstManaged(in posUV, ref shore, ref terrainHeight, in shoreData, in terrainHeightMapData, in hasHeightMap, in hasTerrainTexturing);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void ComputeOceanSimHeight_Burst_000071FE_0024PostfixBurstDelegate(in NativeArray<Vector2> pos, in NativeArray<Vector2> posUV, ref NativeArray<float> shore, ref NativeArray<float> waterHeight, in bool isDeepSea, in bool hasWaterAndTopology, in TerrainTopologyMap.TopologyQueryStructure topologyQueryStructure, in float OceanLevel, in float MaxOceanLevel, in bool hasWaterSystem, in TerrainWaterMap.WaterMapQueryStructure waterMapQueryStruct);

	internal static class ComputeOceanSimHeight_Burst_000071FE_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<ComputeOceanSimHeight_Burst_000071FE_0024PostfixBurstDelegate>(ComputeOceanSimHeight_Burst).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in NativeArray<Vector2> pos, in NativeArray<Vector2> posUV, ref NativeArray<float> shore, ref NativeArray<float> waterHeight, in bool isDeepSea, in bool hasWaterAndTopology, in TerrainTopologyMap.TopologyQueryStructure topologyQueryStructure, in float OceanLevel, in float MaxOceanLevel, in bool hasWaterSystem, in TerrainWaterMap.WaterMapQueryStructure waterMapQueryStruct)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref NativeArray<Vector2>, ref NativeArray<Vector2>, ref NativeArray<float>, ref NativeArray<float>, ref bool, ref bool, ref TerrainTopologyMap.TopologyQueryStructure, ref float, ref float, ref bool, ref TerrainWaterMap.WaterMapQueryStructure, void>)functionPointer)(ref pos, ref posUV, ref shore, ref waterHeight, ref isDeepSea, ref hasWaterAndTopology, ref topologyQueryStructure, ref OceanLevel, ref MaxOceanLevel, ref hasWaterSystem, ref waterMapQueryStruct);
					return;
				}
			}
			ComputeOceanSimHeight_Burst_0024BurstManaged(in pos, in posUV, ref shore, ref waterHeight, in isDeepSea, in hasWaterAndTopology, in topologyQueryStructure, in OceanLevel, in MaxOceanLevel, in hasWaterSystem, in waterMapQueryStruct);
		}
	}

	[MonoPInvokeCallback(typeof(GetHeightArray_Burst_000071FD_0024PostfixBurstDelegate))]
	[BurstCompile]
	public static void GetHeightArray_Burst(in NativeArray<Vector2>.ReadOnly posUV, ref NativeArray<float> shore, ref NativeArray<float> terrainHeight, in TerrainTexturing.ShoreData shoreData, in TerrainHeightMap.HeightMapQueryStructure terrainHeightMapData, in bool hasHeightMap, in bool hasTerrainTexturing)
	{
		GetHeightArray_Burst_000071FD_0024BurstDirectCall.Invoke(in posUV, ref shore, ref terrainHeight, in shoreData, in terrainHeightMapData, in hasHeightMap, in hasTerrainTexturing);
	}

	[MonoPInvokeCallback(typeof(ComputeOceanSimHeight_Burst_000071FE_0024PostfixBurstDelegate))]
	[BurstCompile]
	public static void ComputeOceanSimHeight_Burst(in NativeArray<Vector2> pos, in NativeArray<Vector2> posUV, ref NativeArray<float> shore, ref NativeArray<float> waterHeight, in bool isDeepSea, in bool hasWaterAndTopology, in TerrainTopologyMap.TopologyQueryStructure topologyQueryStructure, in float OceanLevel, in float MaxOceanLevel, in bool hasWaterSystem, in TerrainWaterMap.WaterMapQueryStructure waterMapQueryStruct)
	{
		ComputeOceanSimHeight_Burst_000071FE_0024BurstDirectCall.Invoke(in pos, in posUV, ref shore, ref waterHeight, in isDeepSea, in hasWaterAndTopology, in topologyQueryStructure, in OceanLevel, in MaxOceanLevel, in hasWaterSystem, in waterMapQueryStruct);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void GetHeightArray_Burst_0024BurstManaged(in NativeArray<Vector2>.ReadOnly posUV, ref NativeArray<float> shore, ref NativeArray<float> terrainHeight, in TerrainTexturing.ShoreData shoreData, in TerrainHeightMap.HeightMapQueryStructure terrainHeightMapData, in bool hasHeightMap, in bool hasTerrainTexturing)
	{
		if (hasTerrainTexturing)
		{
			for (int i = 0; i < posUV.Length; i++)
			{
				shore[i] = shoreData.GetCoarseDistanceToShore(posUV[i]);
			}
		}
		else
		{
			NativeArrayUtility.Fill(ref shore, 0f);
		}
		if (hasHeightMap)
		{
			for (int j = 0; j < posUV.Length; j++)
			{
				terrainHeight[j] = terrainHeightMapData.GetHeightFromUV(posUV[j]);
			}
		}
		else
		{
			NativeArrayUtility.Fill(ref terrainHeight, 0f);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void ComputeOceanSimHeight_Burst_0024BurstManaged(in NativeArray<Vector2> pos, in NativeArray<Vector2> posUV, ref NativeArray<float> shore, ref NativeArray<float> waterHeight, in bool isDeepSea, in bool hasWaterAndTopology, in TerrainTopologyMap.TopologyQueryStructure topologyQueryStructure, in float OceanLevel, in float MaxOceanLevel, in bool hasWaterSystem, in TerrainWaterMap.WaterMapQueryStructure waterMapQueryStruct)
	{
		if (hasWaterAndTopology)
		{
			for (int i = 0; i < posUV.Length; i++)
			{
				Vector2 uv = posUV[i];
				float num = waterMapQueryStruct.GetHeightFast(uv, isDeepSea);
				if (num < OceanLevel + MaxOceanLevel && (isDeepSea || topologyQueryStructure.GetTopology(uv.x, uv.y, 384)))
				{
					float b = waterHeight[i] + OceanLevel;
					num = Mathf.Max(num, b);
				}
				waterHeight[i] = num;
			}
		}
		else if (hasWaterSystem)
		{
			for (int j = 0; j < pos.Length; j++)
			{
				waterHeight[j] += OceanLevel;
			}
		}
		else
		{
			NativeArrayUtility.Fill(ref waterHeight, OceanLevel);
			NativeArrayUtility.Fill(ref shore, 0f);
		}
	}
}
