using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Facepunch;
using Rust.Water5;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UtilityJobs;
using WaterLevelJobs;

public static class WaterLevel
{
	public struct WaterInfo
	{
		[MarshalAs(UnmanagedType.U1)]
		public bool isValid;

		public float currentDepth;

		public float overallDepth;

		public float surfaceLevel;

		public float terrainHeight;

		[MarshalAs(UnmanagedType.U1)]
		public bool artificalWater;

		public int topology;

		public static bool ApproxEquals(in WaterInfo left, in WaterInfo right, float epsilon = 1E-05f)
		{
			float num = left.currentDepth - right.currentDepth;
			float num2 = left.overallDepth - right.overallDepth;
			float num3 = left.surfaceLevel - right.surfaceLevel;
			float num4 = left.terrainHeight - right.terrainHeight;
			float num5 = epsilon * epsilon;
			if (left.isValid == right.isValid && num * num < num5 && num2 * num2 < num5 && num3 * num3 < num5 && num4 * num4 < num5 && left.artificalWater == right.artificalWater)
			{
				return left.topology == right.topology;
			}
			return false;
		}
	}

	public const float InvalidWaterHeight = -1000f;

	private static NativeReference<int> CounterRef;

	private static NativeReference<int> DeepCounterRef;

	private static NativeArray<Vector3> Centers;

	private static NativeArray<float> WaterHeights;

	private static NativeArray<float> TerrainHeights;

	private static NativeArray<int> Indices;

	private static NativeArray<int> DeepIndices;

	private static NativeArray<bool> GetIgnoreResults;

	private static NativeArray<Vector3> GetIgnoreHeadStarts;

	private static NativeArray<float> GetIgnoreHeadRadii;

	private static NativeArray<Vector2> UVs;

	private static NativeArray<int> Topologies;

	private static NativeArray<float> ShoreDists;

	private static NativeArray<float> WaveHeights;

	private static NativeArray<float> WaterLevels;

	public static float Factor(Vector3 start, Vector3 end, float radius, bool waves, bool volumes, BaseEntity forEntity = null)
	{
		using (TimeWarning.New("WaterLevel.Factor"))
		{
			WaterInfo info = GetWaterInfo(start, end, radius, waves, volumes, forEntity);
			return Factor(in info, start, end, radius);
		}
	}

	public static float Factor(in WaterInfo info, Vector3 start, Vector3 end, float radius)
	{
		if (!info.isValid)
		{
			return 0f;
		}
		return Mathf.InverseLerp(Mathf.Min(start.y, end.y) - radius, Mathf.Max(start.y, end.y) + radius, info.surfaceLevel);
	}

	public static float Factor(Bounds bounds, bool waves, bool volumes, BaseEntity forEntity = null)
	{
		using (TimeWarning.New("WaterLevel.Factor"))
		{
			if (bounds.size == Vector3.zero)
			{
				bounds.size = new Vector3(0.1f, 0.1f, 0.1f);
			}
			WaterInfo waterInfo = GetWaterInfo(bounds, waves, volumes, forEntity);
			return waterInfo.isValid ? Mathf.InverseLerp(bounds.min.y, bounds.max.y, waterInfo.surfaceLevel) : 0f;
		}
	}

	public static float Factor(in WaterInfo info, Bounds bounds)
	{
		if (bounds.size == Vector3.zero)
		{
			bounds.size = new Vector3(0.1f, 0.1f, 0.1f);
		}
		if (!info.isValid)
		{
			return 0f;
		}
		return Mathf.InverseLerp(bounds.min.y, bounds.max.y, info.surfaceLevel);
	}

	public static bool Test(Vector3 pos, bool waves, bool volumes, BaseEntity forEntity = null)
	{
		using (TimeWarning.New("WaterLevel.Test"))
		{
			return GetWaterInfo(pos, waves, volumes, forEntity).isValid;
		}
	}

	public static bool Test(in WaterInfo info, bool volumes, Vector3 pos, BaseEntity forEntity = null)
	{
		bool flag = pos.y >= info.terrainHeight - 1f && pos.y <= info.surfaceLevel;
		if (!flag && volumes)
		{
			flag = GetWaterInfoFromVolumes(pos, forEntity).isValid;
		}
		return flag;
	}

	public static (float, float) GetWaterAndTerrainSurface(Vector3 pos, bool waves, bool volumes, BaseEntity forEntity = null)
	{
		using (TimeWarning.New("WaterLevel.GetWaterDepth"))
		{
			WaterInfo waterInfo = GetWaterInfo(pos, waves, volumes, forEntity);
			return (waterInfo.surfaceLevel, waterInfo.terrainHeight);
		}
	}

	public static float GetWaterOrTerrainSurface(Vector3 pos, bool waves, bool volumes, BaseEntity forEntity = null)
	{
		using (TimeWarning.New("WaterLevel.GetWaterDepth"))
		{
			WaterInfo waterInfo = GetWaterInfo(pos, waves, volumes, forEntity);
			return Mathf.Max(waterInfo.surfaceLevel, waterInfo.terrainHeight);
		}
	}

	public static float GetWaterSurface(Vector3 pos, bool waves, bool volumes, BaseEntity forEntity = null)
	{
		using (TimeWarning.New("WaterLevel.GetWaterDepth"))
		{
			return GetWaterInfo(pos, waves, volumes, forEntity).surfaceLevel;
		}
	}

	public static float GetWaterDepth(Vector3 pos, bool waves, bool volumes, BaseEntity forEntity = null)
	{
		using (TimeWarning.New("WaterLevel.GetWaterDepth"))
		{
			return GetWaterInfo(pos, waves, volumes, forEntity).currentDepth;
		}
	}

	public static float GetOverallWaterDepth(Vector3 pos, bool waves, bool volumes, BaseEntity forEntity = null)
	{
		using (TimeWarning.New("WaterLevel.GetOverallWaterDepth"))
		{
			return GetWaterInfo(pos, waves, volumes, forEntity).overallDepth;
		}
	}

	public static Vector3 GetWaterFlowDirection(Vector3 worldPosition)
	{
		if (TerrainMeta.WaterFlowMap == null)
		{
			return Vector3.zero;
		}
		return TerrainMeta.WaterFlowMap.GetFlowDirection(worldPosition);
	}

	public static Vector3 GetWaterNormal(Vector3 pos)
	{
		return Vector3.up;
	}

	public static WaterInfo GetBuoyancyWaterInfo(Vector3 pos, Vector2 posUV, float terrainHeight, float waterHeight, bool doDeepwaterChecks, BaseEntity forEntity)
	{
		using (TimeWarning.New("WaterLevel.GetWaterInfo"))
		{
			WaterInfo result = default(WaterInfo);
			if (pos.y > waterHeight)
			{
				return GetWaterInfoFromVolumes(pos, forEntity);
			}
			bool flag = pos.y < terrainHeight - 1f;
			if (flag)
			{
				return GetWaterInfoFromVolumes(pos, forEntity);
			}
			bool flag2 = doDeepwaterChecks && (pos.y < waterHeight - 10f || (TerrainMeta.OutOfBounds(pos) && !DeepSeaManager.IsInsideDeepSea(pos)));
			int num = (TerrainMeta.TopologyMap ? TerrainMeta.TopologyMap.GetTopologyFast(posUV) : 0);
			if ((flag || flag2 || (num & 0x3C180) == 0) && (bool)WaterSystem.Collision && WaterSystem.Collision.GetIgnore(pos))
			{
				return result;
			}
			if (flag2 && Physics.Raycast(pos, Vector3.up, out var hitInfo, 5f, 16, QueryTriggerInteraction.Collide))
			{
				waterHeight = Mathf.Min(waterHeight, hitInfo.collider.bounds.max.y);
			}
			result.isValid = true;
			result.currentDepth = Mathf.Max(0f, waterHeight - pos.y);
			result.overallDepth = Mathf.Max(0f, waterHeight - terrainHeight);
			result.surfaceLevel = waterHeight;
			result.terrainHeight = terrainHeight;
			result.topology = num;
			return result;
		}
	}

	public static WaterInfo GetWaterInfo(Vector3 pos, bool waves, bool volumes, BaseEntity forEntity = null)
	{
		using (TimeWarning.New("WaterLevel.GetWaterInfo"))
		{
			WaterInfo result = default(WaterInfo);
			float num = GetWaterLevel(pos, waves);
			float num2 = (((bool)TerrainMeta.HeightMap && TerrainMeta.HeightMap.isInitialized) ? TerrainMeta.HeightMap.GetHeight(pos) : 0f);
			result.isValid = true;
			if (pos.y > num)
			{
				result.isValid = false;
			}
			else if (pos.y < num2 - 1f)
			{
				result.isValid = false;
			}
			bool flag = false;
			if (!result.isValid && volumes)
			{
				result = GetWaterInfoFromVolumes(pos, forEntity);
				if (result.isValid)
				{
					flag = true;
					num = result.surfaceLevel;
				}
			}
			if (result.isValid && (bool)WaterSystem.Collision && WaterSystem.Collision.GetIgnore(pos))
			{
				result.isValid = false;
				num = -1000f;
			}
			result.currentDepth = Mathf.Max(0f, num - pos.y);
			if (!flag)
			{
				result.overallDepth = Mathf.Max(0f, num - num2);
			}
			result.surfaceLevel = num;
			result.terrainHeight = num2;
			return result;
		}
	}

	public static WaterInfo GetWaterInfo(Bounds bounds, bool waves, bool volumes, BaseEntity forEntity = null)
	{
		using (TimeWarning.New("WaterLevel.GetWaterInfo"))
		{
			WaterInfo result = default(WaterInfo);
			float num = GetWaterLevel(bounds.center, waves);
			float num2 = (TerrainMeta.HeightMap ? TerrainMeta.HeightMap.GetHeight(bounds.center) : 0f);
			result.isValid = true;
			if (bounds.min.y > num)
			{
				result.isValid = false;
			}
			else if (bounds.max.y < num2 - 1f)
			{
				result.isValid = false;
			}
			if (!result.isValid && volumes)
			{
				result = GetWaterInfoFromVolumes(bounds, forEntity);
				if (result.isValid)
				{
					num = result.surfaceLevel;
				}
			}
			if (result.isValid && (bool)WaterSystem.Collision && WaterSystem.Collision.GetIgnore(bounds))
			{
				result.isValid = false;
				num = -1000f;
			}
			result.currentDepth = Mathf.Max(0f, num - bounds.min.y);
			result.overallDepth = Mathf.Max(0f, num - num2);
			result.surfaceLevel = num;
			result.terrainHeight = num2;
			return result;
		}
	}

	public static void InitInternalState(int initCap)
	{
		DisposeInternalState();
		CounterRef = new NativeReference<int>(Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		DeepCounterRef = new NativeReference<int>(Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Centers = new NativeArray<Vector3>(initCap, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		WaterHeights = new NativeArray<float>(initCap, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		TerrainHeights = new NativeArray<float>(initCap, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Indices = new NativeArray<int>(initCap, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		DeepIndices = new NativeArray<int>(initCap, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		GetIgnoreResults = new NativeArray<bool>(initCap, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		GetIgnoreHeadStarts = new NativeArray<Vector3>(initCap, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		GetIgnoreHeadRadii = new NativeArray<float>(initCap, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		UVs = new NativeArray<Vector2>(initCap, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Topologies = new NativeArray<int>(initCap, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		ShoreDists = new NativeArray<float>(initCap, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		WaveHeights = new NativeArray<float>(initCap, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		WaterLevels = new NativeArray<float>(initCap, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
	}

	public static void DisposeInternalState()
	{
		NativeReferenceEx.SafeDispose(ref CounterRef);
		NativeReferenceEx.SafeDispose(ref DeepCounterRef);
		NativeArrayEx.SafeDispose(ref Centers);
		NativeArrayEx.SafeDispose(ref WaterHeights);
		NativeArrayEx.SafeDispose(ref TerrainHeights);
		NativeArrayEx.SafeDispose(ref Indices);
		NativeArrayEx.SafeDispose(ref DeepIndices);
		NativeArrayEx.SafeDispose(ref GetIgnoreResults);
		NativeArrayEx.SafeDispose(ref GetIgnoreHeadStarts);
		NativeArrayEx.SafeDispose(ref GetIgnoreHeadRadii);
		NativeArrayEx.SafeDispose(ref UVs);
		NativeArrayEx.SafeDispose(ref Topologies);
		NativeArrayEx.SafeDispose(ref ShoreDists);
		NativeArrayEx.SafeDispose(ref WaveHeights);
		NativeArrayEx.SafeDispose(ref WaterLevels);
	}

	public static void GetWaterInfos(NativeArray<Vector3>.ReadOnly poses, bool waves, bool volumes, ReadOnlySpan<BaseEntity> entities, NativeArray<WaterInfo> results)
	{
		using (TimeWarning.New("GetWaterInfos"))
		{
			using NativeList<int> values = new NativeList<int>(poses.Length, Allocator.TempJob);
			GenerateAscSeqListJob generateAscSeqListJob = default(GenerateAscSeqListJob);
			generateAscSeqListJob.Values = values;
			generateAscSeqListJob.Start = 0;
			generateAscSeqListJob.Step = 1;
			generateAscSeqListJob.Count = poses.Length;
			GenerateAscSeqListJob jobData = generateAscSeqListJob;
			IJobExtensions.RunByRef(ref jobData);
			FillJob<WaterInfo> fillJob = default(FillJob<WaterInfo>);
			fillJob.Values = results;
			fillJob.Value = new WaterInfo
			{
				isValid = true
			};
			FillJob<WaterInfo> jobData2 = fillJob;
			IJobExtensions.RunByRef(ref jobData2);
			using NativeArray<bool> useVolumeDepths = new NativeArray<bool>(poses.Length, Allocator.TempJob);
			NativeArrayEx.Expand(ref WaterHeights, poses.Length, NativeArrayOptions.UninitializedMemory, copyContents: false);
			GetWaterLevels(poses, values.AsReadOnly(), waves, WaterHeights);
			NativeArrayEx.Expand(ref TerrainHeights, poses.Length, NativeArrayOptions.UninitializedMemory, copyContents: false);
			TerrainMeta.HeightMap?.GetHeightsIndirect(poses, values.AsReadOnly(), TerrainHeights);
			InitialValidateSimpleInfoJobIndirect initialValidateSimpleInfoJobIndirect = default(InitialValidateSimpleInfoJobIndirect);
			initialValidateSimpleInfoJobIndirect.Results = results;
			initialValidateSimpleInfoJobIndirect.Poses = poses;
			initialValidateSimpleInfoJobIndirect.WaterHeights = WaterHeights.AsReadOnly();
			initialValidateSimpleInfoJobIndirect.TerrainHeights = TerrainHeights.AsReadOnly();
			initialValidateSimpleInfoJobIndirect.Indices = values.AsReadOnly();
			InitialValidateSimpleInfoJobIndirect jobData3 = initialValidateSimpleInfoJobIndirect;
			IJobExtensions.RunByRef(ref jobData3);
			if (volumes)
			{
				using (TimeWarning.New("WaterTestFromVolumes"))
				{
					GatherInvalidInfosJobIndirect gatherInvalidInfosJobIndirect = default(GatherInvalidInfosJobIndirect);
					gatherInvalidInfosJobIndirect.InvalidIndices = Indices;
					gatherInvalidInfosJobIndirect.InvalidIndexCount = CounterRef;
					gatherInvalidInfosJobIndirect.Infos = results.AsReadOnly();
					gatherInvalidInfosJobIndirect.Indices = values.AsReadOnly();
					GatherInvalidInfosJobIndirect jobData4 = gatherInvalidInfosJobIndirect;
					IJobExtensions.RunByRef(ref jobData4);
					int value = CounterRef.Value;
					if (value > 0)
					{
						NativeArray<int> source = Indices.GetSubArray(0, value);
						BaseEntity.WaterTestFromVolumesIndirect(entities, poses, source, results);
						UpdateWaterHeightsWithVolumesJobIndirect updateWaterHeightsWithVolumesJobIndirect = default(UpdateWaterHeightsWithVolumesJobIndirect);
						updateWaterHeightsWithVolumesJobIndirect.WaterHeights = WaterHeights;
						updateWaterHeightsWithVolumesJobIndirect.UseVolumeDepths = useVolumeDepths;
						updateWaterHeightsWithVolumesJobIndirect.Infos = results;
						updateWaterHeightsWithVolumesJobIndirect.Indices = source;
						UpdateWaterHeightsWithVolumesJobIndirect jobData5 = updateWaterHeightsWithVolumesJobIndirect;
						IJobExtensions.RunByRef(ref jobData5);
					}
				}
			}
			if ((bool)WaterSystem.Collision)
			{
				NativeArrayEx.Expand(ref Indices, poses.Length, NativeArrayOptions.UninitializedMemory, copyContents: false);
				GatherValidInfosJobIndirect gatherValidInfosJobIndirect = default(GatherValidInfosJobIndirect);
				gatherValidInfosJobIndirect.ValidIndices = Indices;
				gatherValidInfosJobIndirect.ValidIndexCount = CounterRef;
				gatherValidInfosJobIndirect.Infos = results.AsReadOnly();
				gatherValidInfosJobIndirect.Indices = values.AsReadOnly();
				GatherValidInfosJobIndirect jobData6 = gatherValidInfosJobIndirect;
				IJobExtensions.RunByRef(ref jobData6);
				int value2 = CounterRef.Value;
				if (value2 > 0)
				{
					using (TimeWarning.New("WaterSystem.Collision.Entity"))
					{
						using NativeArray<float> values2 = new NativeArray<float>(poses.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
						FillJob<float> fillJob2 = default(FillJob<float>);
						fillJob2.Values = values2;
						fillJob2.Value = 0.01f;
						FillJob<float> jobData7 = fillJob2;
						IJobExtensions.RunByRef(ref jobData7);
						NativeArray<int> subArray = Indices.GetSubArray(0, value2);
						NativeArrayEx.Expand(ref GetIgnoreResults, poses.Length, NativeArrayOptions.UninitializedMemory, copyContents: false);
						WaterSystem.Collision.GetIgnoreIndirect(poses, values2.AsReadOnly(), subArray.AsReadOnly(), GetIgnoreResults);
						ResolveIgnoreWaterInfosJob resolveIgnoreWaterInfosJob = default(ResolveIgnoreWaterInfosJob);
						resolveIgnoreWaterInfosJob.Infos = results;
						resolveIgnoreWaterInfosJob.WaterHeights = WaterHeights;
						resolveIgnoreWaterInfosJob.Indices = values.AsReadOnly();
						resolveIgnoreWaterInfosJob.Results = GetIgnoreResults.AsReadOnly();
						ResolveIgnoreWaterInfosJob jobData8 = resolveIgnoreWaterInfosJob;
						IJobExtensions.RunByRef(ref jobData8);
					}
				}
			}
			ResolveWaterInfosSimpleJobIndirect resolveWaterInfosSimpleJobIndirect = default(ResolveWaterInfosSimpleJobIndirect);
			resolveWaterInfosSimpleJobIndirect.Infos = results;
			resolveWaterInfosSimpleJobIndirect.Poses = poses;
			resolveWaterInfosSimpleJobIndirect.WaterHeights = WaterHeights.AsReadOnly();
			resolveWaterInfosSimpleJobIndirect.TerrainHeights = TerrainHeights.AsReadOnly();
			resolveWaterInfosSimpleJobIndirect.UseVolumeDepths = useVolumeDepths.AsReadOnly();
			resolveWaterInfosSimpleJobIndirect.Indices = values.AsReadOnly();
			ResolveWaterInfosSimpleJobIndirect jobData9 = resolveWaterInfosSimpleJobIndirect;
			IJobExtensions.RunByRef(ref jobData9);
		}
	}

	public static void GetWaterInfos(NativeArray<Vector3>.ReadOnly starts, NativeArray<Vector3>.ReadOnly ends, NativeArray<float>.ReadOnly radii, ReadOnlySpan<BaseEntity> entities, NativeArray<int>.ReadOnly indices, bool waves, bool volumes, NativeArray<WaterInfo> results)
	{
		using (TimeWarning.New("GetWaterInfos"))
		{
			NativeArrayEx.Expand(ref Centers, starts.Length, NativeArrayOptions.UninitializedMemory, copyContents: false);
			CalcCenterJobIndirect calcCenterJobIndirect = default(CalcCenterJobIndirect);
			calcCenterJobIndirect.Results = Centers;
			calcCenterJobIndirect.Starts = starts;
			calcCenterJobIndirect.Ends = ends;
			calcCenterJobIndirect.Indices = indices;
			CalcCenterJobIndirect jobData = calcCenterJobIndirect;
			IJobExtensions.RunByRef(ref jobData);
			NativeArrayEx.Expand(ref WaterHeights, starts.Length, NativeArrayOptions.UninitializedMemory, copyContents: false);
			GetWaterLevels(Centers.AsReadOnly(), indices, waves, WaterHeights);
			NativeArrayEx.Expand(ref TerrainHeights, starts.Length, NativeArrayOptions.UninitializedMemory, copyContents: false);
			TerrainMeta.HeightMap?.GetHeightsIndirect(Centers.AsReadOnly(), indices, TerrainHeights);
			NativeArrayEx.Expand(ref Indices, starts.Length, NativeArrayOptions.UninitializedMemory, copyContents: false);
			InitialValidateInfoJobIndirect initialValidateInfoJobIndirect = default(InitialValidateInfoJobIndirect);
			initialValidateInfoJobIndirect.Results = results;
			initialValidateInfoJobIndirect.Starts = starts;
			initialValidateInfoJobIndirect.Ends = ends;
			initialValidateInfoJobIndirect.Radii = radii;
			initialValidateInfoJobIndirect.WaterHeights = WaterHeights.AsReadOnly();
			initialValidateInfoJobIndirect.TerrainHeights = TerrainHeights.AsReadOnly();
			initialValidateInfoJobIndirect.Indices = indices;
			InitialValidateInfoJobIndirect jobData2 = initialValidateInfoJobIndirect;
			IJobExtensions.RunByRef(ref jobData2);
			if (volumes)
			{
				using (TimeWarning.New("WaterTestFromVolumes"))
				{
					GatherInvalidInfosJobIndirect gatherInvalidInfosJobIndirect = default(GatherInvalidInfosJobIndirect);
					gatherInvalidInfosJobIndirect.InvalidIndices = Indices;
					gatherInvalidInfosJobIndirect.InvalidIndexCount = CounterRef;
					gatherInvalidInfosJobIndirect.Infos = results.AsReadOnly();
					gatherInvalidInfosJobIndirect.Indices = indices;
					GatherInvalidInfosJobIndirect jobData3 = gatherInvalidInfosJobIndirect;
					IJobExtensions.RunByRef(ref jobData3);
					int value = CounterRef.Value;
					if (value > 0)
					{
						NativeArray<int> source = Indices.GetSubArray(0, value);
						BaseEntity.WaterTestFromVolumesIndirect(entities, starts, ends, radii, source, results);
						UpdateWaterHeightsJobIndirect updateWaterHeightsJobIndirect = default(UpdateWaterHeightsJobIndirect);
						updateWaterHeightsJobIndirect.WaterHeights = WaterHeights;
						updateWaterHeightsJobIndirect.Infos = results;
						updateWaterHeightsJobIndirect.Indices = source;
						UpdateWaterHeightsJobIndirect jobData4 = updateWaterHeightsJobIndirect;
						IJobExtensions.RunByRef(ref jobData4);
					}
				}
			}
			if ((bool)WaterSystem.Collision)
			{
				using (TimeWarning.New("WaterSystem.Collision"))
				{
					GatherValidInfosJobIndirect gatherValidInfosJobIndirect = default(GatherValidInfosJobIndirect);
					gatherValidInfosJobIndirect.ValidIndices = Indices;
					gatherValidInfosJobIndirect.ValidIndexCount = CounterRef;
					gatherValidInfosJobIndirect.Infos = results.AsReadOnly();
					gatherValidInfosJobIndirect.Indices = indices;
					GatherValidInfosJobIndirect jobData5 = gatherValidInfosJobIndirect;
					IJobExtensions.RunByRef(ref jobData5);
					int value2 = CounterRef.Value;
					if (value2 > 0)
					{
						using (TimeWarning.New("WaterSystem.Collision.Entity"))
						{
							NativeArray<int> subArray = Indices.GetSubArray(0, value2);
							NativeArrayEx.Expand(ref GetIgnoreResults, starts.Length, NativeArrayOptions.UninitializedMemory, copyContents: false);
							WaterSystem.Collision.GetIgnoreIndirect(starts, ends, radii, subArray.AsReadOnly(), GetIgnoreResults);
							NativeArrayEx.Expand(ref GetIgnoreHeadStarts, starts.Length, NativeArrayOptions.UninitializedMemory, copyContents: false);
							NativeArrayEx.Expand(ref GetIgnoreHeadRadii, starts.Length, NativeArrayOptions.UninitializedMemory, copyContents: false);
							SetupHeadQueryJobIndirect setupHeadQueryJobIndirect = default(SetupHeadQueryJobIndirect);
							setupHeadQueryJobIndirect.Indices = subArray;
							setupHeadQueryJobIndirect.QueryIndexCount = CounterRef;
							setupHeadQueryJobIndirect.QueryStarts = GetIgnoreHeadStarts;
							setupHeadQueryJobIndirect.QueryRadii = GetIgnoreHeadRadii;
							setupHeadQueryJobIndirect.ValidInfos = GetIgnoreResults.AsReadOnly();
							setupHeadQueryJobIndirect.Starts = starts;
							setupHeadQueryJobIndirect.Ends = ends;
							setupHeadQueryJobIndirect.Radii = radii;
							SetupHeadQueryJobIndirect jobData6 = setupHeadQueryJobIndirect;
							IJobExtensions.RunByRef(ref jobData6);
							int value3 = CounterRef.Value;
							if (value3 > 0)
							{
								using (TimeWarning.New("WaterSystem.Collision.Head"))
								{
									NativeArray<int> subArray2 = Indices.GetSubArray(0, value3);
									WaterSystem.Collision.GetIgnoreIndirect(GetIgnoreHeadStarts.AsReadOnly(), GetIgnoreHeadRadii.AsReadOnly(), subArray2.AsReadOnly(), GetIgnoreResults);
									ApplyHeadQueryResultsJobIndirect applyHeadQueryResultsJobIndirect = default(ApplyHeadQueryResultsJobIndirect);
									applyHeadQueryResultsJobIndirect.WaterHeights = WaterHeights;
									applyHeadQueryResultsJobIndirect.Infos = results;
									applyHeadQueryResultsJobIndirect.Indices = subArray2.AsReadOnly();
									applyHeadQueryResultsJobIndirect.ValidInfos = GetIgnoreResults.AsReadOnly();
									applyHeadQueryResultsJobIndirect.Starts = GetIgnoreHeadStarts.AsReadOnly();
									ApplyHeadQueryResultsJobIndirect jobData7 = applyHeadQueryResultsJobIndirect;
									IJobExtensions.RunByRef(ref jobData7);
								}
							}
						}
					}
				}
			}
			ResolveWaterInfosJobIndirect resolveWaterInfosJobIndirect = default(ResolveWaterInfosJobIndirect);
			resolveWaterInfosJobIndirect.Infos = results;
			resolveWaterInfosJobIndirect.Starts = starts;
			resolveWaterInfosJobIndirect.Ends = ends;
			resolveWaterInfosJobIndirect.Radii = radii;
			resolveWaterInfosJobIndirect.WaterHeights = WaterHeights.AsReadOnly();
			resolveWaterInfosJobIndirect.TerrainHeights = TerrainHeights.AsReadOnly();
			resolveWaterInfosJobIndirect.Indices = indices;
			ResolveWaterInfosJobIndirect jobData8 = resolveWaterInfosJobIndirect;
			IJobExtensions.RunByRef(ref jobData8);
		}
	}

	public static WaterInfo GetWaterInfo(Vector3 start, Vector3 end, float radius, bool waves, bool volumes, BaseEntity forEntity = null)
	{
		using (TimeWarning.New("WaterLevel.GetWaterInfo"))
		{
			Vector3 vector = (start + end) * 0.5f;
			float num = Mathf.Min(start.y, end.y) - radius;
			float num2 = Mathf.Max(start.y, end.y) + radius;
			float num3 = GetWaterLevel(vector, waves);
			float num4 = (TerrainMeta.HeightMap ? TerrainMeta.HeightMap.GetHeight(vector) : 0f);
			WaterInfo result = InitialValidate(num, num2, num3, num4);
			if (!result.isValid && volumes)
			{
				result = GetWaterInfoFromVolumes(start, end, radius, forEntity);
				if (result.isValid)
				{
					num3 = result.surfaceLevel;
				}
			}
			if (result.isValid && (bool)WaterSystem.Collision && WaterSystem.Collision.GetIgnore(start, end, radius))
			{
				Vector3 pos = vector.WithY(Mathf.Lerp(num, num2, 0.75f));
				if (!WaterSystem.Collision.GetIgnore(pos))
				{
					num3 = Mathf.Min(num3, pos.y);
				}
				else
				{
					result.isValid = false;
					num3 = -1000f;
				}
			}
			result.currentDepth = Mathf.Max(0f, num3 - num);
			result.overallDepth = Mathf.Max(0f, num3 - num4);
			result.surfaceLevel = num3;
			result.terrainHeight = num4;
			return result;
		}
	}

	public static WaterInfo GetWaterInfo(Camera cam, bool waves, bool volumes, BaseEntity forEntity = null)
	{
		using (TimeWarning.New("WaterLevel.GetWaterInfo"))
		{
			waves = waves && WaterSystem.Instance != null;
			float num = WaterSystem.OceanLevel;
			if (waves)
			{
				num += WaterSystem.Instance.GetOceanSimulation(cam.transform.position).MinLevel();
			}
			if (cam.transform.position.y < num - 1f)
			{
				return GetWaterInfo(cam.transform.position, waves, volumes, forEntity);
			}
			return GetWaterInfo(cam.transform.position - Vector3.up, waves, volumes, forEntity);
		}
	}

	public static float GetWaterLevel(Vector3 pos, bool waves)
	{
		waves = waves && WaterSystem.Instance != null;
		float normX = TerrainMeta.NormalizeX(pos.x);
		float normZ = TerrainMeta.NormalizeZ(pos.z);
		float num = (TerrainMeta.WaterMap ? TerrainMeta.WaterMap.GetHeight(pos) : TerrainMeta.Position.y);
		float num2 = WaterSystem.OceanLevel;
		OceanSimulation oceanSimulation = (waves ? WaterSystem.Instance.GetOceanSimulation(pos) : null);
		if (waves)
		{
			num2 += oceanSimulation.MaxLevel();
		}
		if (num < num2 && (!TerrainMeta.TopologyMap || TerrainMeta.TopologyMap.GetTopology(normX, normZ, 384)))
		{
			float num3 = WaterSystem.OceanLevel;
			if (waves)
			{
				num3 += oceanSimulation.GetHeight(pos);
			}
			return Mathf.Max(num, num3);
		}
		return num;
	}

	public static float RaycastWaterColliders(Vector3 pos)
	{
		if (!Physics.Raycast(pos.WithY(TerrainMeta.Max.y), Vector3.down, out var hitInfo, TerrainMeta.Size.y, 16, QueryTriggerInteraction.Collide))
		{
			return WaterSystem.OceanLevel;
		}
		return hitInfo.point.y;
	}

	public static void GetWaterLevels(NativeArray<Vector3>.ReadOnly positions, NativeArray<int>.ReadOnly indices, bool waves, NativeArray<float> heights)
	{
		using (TimeWarning.New("WaterLevels"))
		{
			waves = waves && WaterSystem.Instance != null;
			using NativeList<int> overworldIndices = new NativeList<int>(positions.Length, Allocator.TempJob);
			using NativeList<int> deepSeaIndices = new NativeList<int>(positions.Length, Allocator.TempJob);
			FilterPositionsJobIndirect filterPositionsJobIndirect = default(FilterPositionsJobIndirect);
			filterPositionsJobIndirect.DeepSeaIndices = deepSeaIndices;
			filterPositionsJobIndirect.OverworldIndices = overworldIndices;
			filterPositionsJobIndirect.Positions = positions;
			filterPositionsJobIndirect.Indices = indices;
			filterPositionsJobIndirect.DeepSeaBounds = DeepSeaManager.DeepSeaBounds;
			FilterPositionsJobIndirect jobData = filterPositionsJobIndirect;
			IJobExtensions.RunByRef(ref jobData);
			NativeArrayEx.Expand(ref WaterLevels, positions.Length, NativeArrayOptions.UninitializedMemory, copyContents: false);
			if (waves)
			{
				GatherMaxWaterLevelsJob gatherMaxWaterLevelsJob = default(GatherMaxWaterLevelsJob);
				gatherMaxWaterLevelsJob.WaterLevels = WaterLevels;
				gatherMaxWaterLevelsJob.Positions = positions;
				gatherMaxWaterLevelsJob.DeepSeaBounds = DeepSeaManager.DeepSeaBounds;
				gatherMaxWaterLevelsJob.waterLevelMain = WaterSystem.Instance.GetOceanSimulation(isDeep: false).MaxLevel() + WaterSystem.OceanLevel;
				gatherMaxWaterLevelsJob.waterLevelDeep = WaterSystem.Instance.GetOceanSimulation(isDeep: true).MaxLevel() + WaterSystem.OceanLevel;
				GatherMaxWaterLevelsJob jobData2 = gatherMaxWaterLevelsJob;
				IJobExtensions.RunByRef(ref jobData2);
			}
			else
			{
				FillJobIndirect<float> fillJobIndirect = default(FillJobIndirect<float>);
				fillJobIndirect.Value = WaterSystem.OceanLevel;
				fillJobIndirect.Values = WaterLevels;
				fillJobIndirect.Indices = indices;
				FillJobIndirect<float> jobData3 = fillJobIndirect;
				IJobExtensions.RunByRef(ref jobData3);
			}
			NativeArrayEx.Expand(ref UVs, positions.Length, NativeArrayOptions.UninitializedMemory, copyContents: false);
			NativeArray<Vector2> subArray = UVs.GetSubArray(0, positions.Length);
			ToUVJobIndirect toUVJobIndirect = default(ToUVJobIndirect);
			toUVJobIndirect.UV = subArray;
			toUVJobIndirect.Pos = positions;
			toUVJobIndirect.Indices = overworldIndices.AsReadOnly();
			toUVJobIndirect.TerrainPos = TerrainMeta.Position.XZ2D();
			toUVJobIndirect.TerrainOneOverSize = TerrainMeta.OneOverSize.XZ2D();
			ToUVJobIndirect jobData4 = toUVJobIndirect;
			IJobExtensions.RunByRef(ref jobData4);
			jobData4.Indices = deepSeaIndices.AsReadOnly();
			jobData4.TerrainPos = DeepSeaManager.DeepSeaBounds.min;
			jobData4.TerrainOneOverSize = new Vector2(1f / DeepSeaManager.DeepSeaBounds.size.x, 1f / DeepSeaManager.DeepSeaBounds.size.z);
			IJobExtensions.RunByRef(ref jobData4);
			if ((bool)TerrainMeta.WaterMap)
			{
				TerrainMeta.WaterMap.GetHeightsIndirect(subArray.AsReadOnly(), overworldIndices.AsReadOnly(), heights);
				FillJobIndirect<float> fillJobIndirect = default(FillJobIndirect<float>);
				fillJobIndirect.Values = heights;
				fillJobIndirect.Value = TerrainMeta.WaterMap.DeepSeaDepth();
				fillJobIndirect.Indices = deepSeaIndices.AsReadOnly();
				FillJobIndirect<float> jobData5 = fillJobIndirect;
				IJobExtensions.RunByRef(ref jobData5);
			}
			else
			{
				FillJob<float> fillJob = default(FillJob<float>);
				fillJob.Values = heights;
				fillJob.Value = TerrainMeta.Position.y;
				FillJob<float> jobData6 = fillJob;
				IJobExtensions.RunByRef(ref jobData6);
			}
			NativeArrayEx.Expand(ref Topologies, positions.Length, NativeArrayOptions.UninitializedMemory, copyContents: false);
			NativeArray<int> subArray2 = Topologies.GetSubArray(0, positions.Length);
			if ((bool)TerrainMeta.TopologyMap)
			{
				TerrainMeta.TopologyMap.GetTopologiesIndirect(subArray.AsReadOnly(), overworldIndices.AsReadOnly(), subArray2);
				FillJobIndirect<int> fillJobIndirect2 = default(FillJobIndirect<int>);
				fillJobIndirect2.Values = subArray2;
				fillJobIndirect2.Value = 128;
				fillJobIndirect2.Indices = deepSeaIndices.AsReadOnly();
				FillJobIndirect<int> jobData7 = fillJobIndirect2;
				IJobExtensions.RunByRef(ref jobData7);
			}
			else
			{
				FillJob<int> fillJob2 = default(FillJob<int>);
				fillJob2.Values = subArray2;
				fillJob2.Value = 384;
				FillJob<int> jobData8 = fillJob2;
				IJobExtensions.RunByRef(ref jobData8);
			}
			if (!waves)
			{
				ApplyMaxHeightsJobIndirect applyMaxHeightsJobIndirect = default(ApplyMaxHeightsJobIndirect);
				applyMaxHeightsJobIndirect.Heights = heights;
				applyMaxHeightsJobIndirect.Topologies = subArray2.AsReadOnly();
				applyMaxHeightsJobIndirect.Indices = indices;
				applyMaxHeightsJobIndirect.WaterLevels = WaterLevels.AsReadOnly();
				applyMaxHeightsJobIndirect.OceanLevel = WaterSystem.OceanLevel;
				ApplyMaxHeightsJobIndirect jobData9 = applyMaxHeightsJobIndirect;
				IJobExtensions.RunByRef(ref jobData9);
				return;
			}
			NativeArrayEx.Expand(ref Indices, positions.Length, NativeArrayOptions.UninitializedMemory, copyContents: false);
			NativeArrayEx.Expand(ref DeepIndices, positions.Length, NativeArrayOptions.UninitializedMemory, copyContents: false);
			GatherWavesIndicesJobIndirect gatherWavesIndicesJobIndirect = default(GatherWavesIndicesJobIndirect);
			gatherWavesIndicesJobIndirect.WaveIndices = Indices;
			gatherWavesIndicesJobIndirect.WaveIndexCount = CounterRef;
			gatherWavesIndicesJobIndirect.Positions = positions;
			gatherWavesIndicesJobIndirect.Topologies = subArray2.AsReadOnly();
			gatherWavesIndicesJobIndirect.Heights = heights.AsReadOnly();
			gatherWavesIndicesJobIndirect.Indices = overworldIndices.AsReadOnly();
			gatherWavesIndicesJobIndirect.WaterLevels = WaterLevels.AsReadOnly();
			GatherWavesIndicesJobIndirect jobData10 = gatherWavesIndicesJobIndirect;
			IJobExtensions.RunByRef(ref jobData10);
			jobData10.WaveIndices = DeepIndices;
			jobData10.WaveIndexCount = DeepCounterRef;
			jobData10.Indices = deepSeaIndices.AsReadOnly();
			IJobExtensions.RunByRef(ref jobData10);
			int value = CounterRef.Value;
			int value2 = DeepCounterRef.Value;
			if (value == 0 && value2 == 0)
			{
				return;
			}
			using (TimeWarning.New("Waves"))
			{
				NativeArrayEx.Expand(ref TerrainHeights, positions.Length, NativeArrayOptions.UninitializedMemory, copyContents: false);
				NativeArrayEx.Expand(ref ShoreDists, positions.Length, NativeArrayOptions.UninitializedMemory, copyContents: false);
				NativeArrayEx.Expand(ref WaveHeights, positions.Length, NativeArrayOptions.UninitializedMemory, copyContents: false);
				NativeArray<int>.ReadOnly indices2 = Indices.GetSubArray(0, value).AsReadOnly();
				NativeArray<int>.ReadOnly indices3 = DeepIndices.GetSubArray(0, value2).AsReadOnly();
				if ((bool)TerrainMeta.HeightMap)
				{
					TerrainHeightMap heightMap = TerrainMeta.HeightMap;
					if (value > 0)
					{
						heightMap.GetHeightsIndirect(subArray.AsReadOnly(), heightMap.Data, indices2, TerrainHeights);
					}
					if (value2 > 0)
					{
						heightMap.GetHeightsIndirect(subArray.AsReadOnly(), heightMap.DeepSeaData, indices3, TerrainHeights);
					}
				}
				else
				{
					FillJob<float> fillJob = default(FillJob<float>);
					fillJob.Values = TerrainHeights;
					fillJob.Value = 0f;
					FillJob<float> jobData11 = fillJob;
					IJobExtensions.RunByRef(ref jobData11);
				}
				if ((bool)TerrainTexturing.Instance)
				{
					if (value > 0)
					{
						TerrainTexturing.Instance.GetCoarseDistancesToShoreIndirect(positions, indices2, ShoreDists);
					}
					if (value2 > 0)
					{
						TerrainTexturing.Instance.GetCoarseDistancesToShoreIndirect(positions, indices3, ShoreDists);
					}
				}
				else
				{
					FillJob<float> fillJob = default(FillJob<float>);
					fillJob.Values = ShoreDists;
					fillJob.Value = 0f;
					FillJob<float> jobData12 = fillJob;
					IJobExtensions.RunByRef(ref jobData12);
				}
				if (value > 0)
				{
					WaterSystem.Instance.GetOceanSimulation(isDeep: false).GetHeightsIndirect(positions, ShoreDists.AsReadOnly(), TerrainHeights.AsReadOnly(), indices2, WaveHeights);
					SelectMaxWaterLevelJobIndirect selectMaxWaterLevelJobIndirect = default(SelectMaxWaterLevelJobIndirect);
					selectMaxWaterLevelJobIndirect.Heights = heights;
					selectMaxWaterLevelJobIndirect.DynamicHeights = WaveHeights.AsReadOnly();
					selectMaxWaterLevelJobIndirect.Indices = indices2;
					selectMaxWaterLevelJobIndirect.OceanLevel = WaterSystem.OceanLevel;
					SelectMaxWaterLevelJobIndirect jobData13 = selectMaxWaterLevelJobIndirect;
					IJobExtensions.RunByRef(ref jobData13);
				}
				if (value2 > 0)
				{
					WaterSystem.Instance.GetOceanSimulation(isDeep: true).GetHeightsIndirect(positions, ShoreDists.AsReadOnly(), TerrainHeights.AsReadOnly(), indices3, WaveHeights);
					SelectMaxWaterLevelJobIndirect selectMaxWaterLevelJobIndirect = default(SelectMaxWaterLevelJobIndirect);
					selectMaxWaterLevelJobIndirect.Heights = heights;
					selectMaxWaterLevelJobIndirect.DynamicHeights = WaveHeights.AsReadOnly();
					selectMaxWaterLevelJobIndirect.Indices = indices3;
					selectMaxWaterLevelJobIndirect.OceanLevel = WaterSystem.OceanLevel;
					SelectMaxWaterLevelJobIndirect jobData14 = selectMaxWaterLevelJobIndirect;
					IJobExtensions.RunByRef(ref jobData14);
				}
			}
		}
	}

	private static WaterInfo GetWaterInfoFromVolumes(Bounds bounds, BaseEntity forEntity)
	{
		WaterInfo info = default(WaterInfo);
		if (forEntity == null)
		{
			List<WaterVolume> obj = Pool.Get<List<WaterVolume>>();
			Vis.Components(new OBB(bounds), obj, 262144);
			using (List<WaterVolume>.Enumerator enumerator = obj.GetEnumerator())
			{
				while (enumerator.MoveNext() && !enumerator.Current.Test(bounds, out info))
				{
				}
			}
			Pool.FreeUnmanaged(ref obj);
			return info;
		}
		forEntity.WaterTestFromVolumes(bounds, out info);
		return info;
	}

	private static WaterInfo GetWaterInfoFromVolumes(Vector3 pos, BaseEntity forEntity)
	{
		WaterInfo info = default(WaterInfo);
		if (forEntity == null)
		{
			List<WaterVolume> obj = Pool.Get<List<WaterVolume>>();
			Vis.Components(pos, 0.1f, obj, 262144);
			foreach (WaterVolume item in obj)
			{
				if (item.Test(pos, out info))
				{
					info.artificalWater = !item.naturalSource;
					break;
				}
			}
			Pool.FreeUnmanaged(ref obj);
			return info;
		}
		forEntity.WaterTestFromVolumes(pos, out info);
		return info;
	}

	private static WaterInfo GetWaterInfoFromVolumes(Vector3 start, Vector3 end, float radius, BaseEntity forEntity)
	{
		WaterInfo info = default(WaterInfo);
		if (forEntity == null)
		{
			List<WaterVolume> obj = Pool.Get<List<WaterVolume>>();
			Vis.Components(start, end, radius, obj, 262144);
			using (List<WaterVolume>.Enumerator enumerator = obj.GetEnumerator())
			{
				while (enumerator.MoveNext() && !enumerator.Current.Test(start, end, radius, out info))
				{
				}
			}
			Pool.FreeUnmanaged(ref obj);
			return info;
		}
		forEntity.WaterTestFromVolumes(start, end, radius, out info);
		return info;
	}

	public static WaterInfo InitialValidate(float minY, float maxY, float waterHeight, float terrainHeight)
	{
		WaterInfo result = default(WaterInfo);
		result.isValid = true;
		if (minY > waterHeight)
		{
			result.isValid = false;
		}
		else if (maxY < terrainHeight - 1f)
		{
			result.isValid = false;
		}
		if (result.isValid && terrainHeight >= waterHeight + 0.015f)
		{
			result.isValid = false;
		}
		return result;
	}
}
