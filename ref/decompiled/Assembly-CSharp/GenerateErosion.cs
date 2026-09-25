#define UNITY_ASSERTIONS
using System;
using System.Threading.Tasks;
using GenerateErosionJobs;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

public class GenerateErosion : ProceduralComponent
{
	public struct SplatPaintingData : IDisposable
	{
		public bool IsValid;

		public readonly NativeArray<float> HeightMapDelta;

		public readonly NativeArray<float> AngleMap;

		public SplatPaintingData(NativeArray<float> heightMapDelta, NativeArray<float> angleMap)
		{
			HeightMapDelta = heightMapDelta;
			AngleMap = angleMap;
			IsValid = true;
		}

		public void Dispose()
		{
			IsValid = false;
			if (HeightMapDelta.IsCreated)
			{
				HeightMapDelta.Dispose();
			}
			if (AngleMap.IsCreated)
			{
				AngleMap.Dispose();
			}
		}

		public void Dispose(JobHandle inputDeps)
		{
			IsValid = false;
			if (HeightMapDelta.IsCreated)
			{
				HeightMapDelta.Dispose(inputDeps);
			}
			if (AngleMap.IsCreated)
			{
				AngleMap.Dispose(inputDeps);
			}
		}
	}

	public static SplatPaintingData splatPaintingData;

	public override void Process(uint seed)
	{
		if (!World.Networked)
		{
			GridErosion(seed);
		}
	}

	private static int GetBatchSize(int length)
	{
		int num = length / JobsUtility.JobWorkerCount;
		if (num < 64)
		{
			return 64;
		}
		return num;
	}

	private void GridErosion(uint seed)
	{
		using (TimeWarning.New("GridErosion"))
		{
			TerrainHeightMap heightMap = TerrainMeta.HeightMap;
			heightMap.Push();
			NativeArray<short> src = heightMap.src;
			NativeArray<short> dst = heightMap.dst;
			NativeArray<float> minTerrainHeightMap = new NativeArray<float>(heightMap.src.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			NativeArray<float> waterMap = new NativeArray<float>(heightMap.src.Length, Allocator.Persistent);
			NativeList<int> nativeList = new NativeList<int>(heightMap.src.Length, Allocator.Persistent);
			NativeArray<float4> fluxMap = new NativeArray<float4>(heightMap.src.Length, Allocator.Persistent);
			NativeArray<float2> velocityMap = new NativeArray<float2>(heightMap.src.Length, Allocator.Persistent);
			NativeArray<float> nativeArray = new NativeArray<float>(heightMap.src.Length, Allocator.Persistent);
			NativeArray<float> copyTarget = new NativeArray<float>(heightMap.src.Length, Allocator.Persistent);
			NativeArray<float> angleMap = new NativeArray<float>(heightMap.src.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			Debug.Assert(waterMap.Length == heightMap.src.Length);
			Debug.Assert(fluxMap.Length == heightMap.src.Length);
			Debug.Assert(velocityMap.Length == heightMap.src.Length);
			Debug.Assert(nativeArray.Length == heightMap.src.Length);
			float num = TerrainMeta.Size.x / (float)heightMap.res * TerrainMeta.Size.z / (float)heightMap.res;
			float invGridCellSquareSize = 1f / num;
			float pipeLength = 1f;
			float pipeArea = 1f;
			JobHandle dependsOn = default(JobHandle);
			NativeArray<float> nativeArray2 = new NativeArray<float>(src.Length, Allocator.Persistent);
			NativeArray<float> nativeArray3 = new NativeArray<float>(nativeArray2, Allocator.Persistent);
			GenerateErosionJobs.PrepareMapJob jobData = default(GenerateErosionJobs.PrepareMapJob);
			jobData.HeightMapAsShort = src.AsReadOnly();
			jobData.HeightMapAsFloat = nativeArray2;
			jobData.OceanIndicesWriter = nativeList.AsParallelWriter();
			jobData.OceanLevel = WaterSystem.OceanLevel;
			jobData.TerrainPositionY = TerrainMeta.Position.y;
			jobData.TerrainSizeY = TerrainMeta.Size.y;
			dependsOn = IJobParallelForBatchExtensions.Schedule(jobData, src.Length, GetBatchSize(src.Length), dependsOn);
			GenerateErosionJobs.CalcMinHeightMapJob jobData2 = default(GenerateErosionJobs.CalcMinHeightMapJob);
			jobData2.TerrainHeightMap = nativeArray2.AsReadOnly();
			jobData2.MinTerrainHeightMap = minTerrainHeightMap;
			jobData2.HeightMapRes = TerrainMeta.HeightMap.res;
			jobData2.TopologyMap = TerrainMeta.TopologyMap.src.AsReadOnly();
			jobData2.TopologyMapRes = TerrainMeta.TopologyMap.res;
			jobData2.OceanHeight = WaterSystem.OceanLevel;
			jobData2.TerrainOneOverSizeX = TerrainMeta.OneOverSize.x;
			IJobParallelForExtensions.Schedule(jobData2, nativeArray2.Length, GetBatchSize(nativeArray2.Length), dependsOn).Complete();
			dependsOn = default(JobHandle);
			NativeArray<float> copyTarget2 = new NativeArray<float>(src.Length, Allocator.Persistent);
			GenerateErosionJobs.CopyArrayJob<float> copyArrayJob = default(GenerateErosionJobs.CopyArrayJob<float>);
			copyArrayJob.CopyTarget = copyTarget2;
			copyArrayJob.CopySource = nativeArray2;
			GenerateErosionJobs.CopyArrayJob<float> jobData3 = copyArrayJob;
			dependsOn = JobHandle.CombineDependencies(job1: new GenerateErosionJobs.CopyArrayJob<float>
			{
				CopyTarget = nativeArray3,
				CopySource = nativeArray2
			}.Schedule(dependsOn), job0: jobData3.Schedule(dependsOn));
			int num2 = 32;
			int num3 = 32;
			int num4 = (heightMap.res + num2 - 1) / num2;
			int num5 = (heightMap.res + num3 - 1) / num3;
			int num6 = num4 * num5;
			for (int i = 0; i < 512; i++)
			{
				GenerateErosionJobs.RefillOceanJob refillOceanJob = default(GenerateErosionJobs.RefillOceanJob);
				refillOceanJob.OceanIndices = nativeList.AsReadOnly();
				refillOceanJob.HeightMap = nativeArray2.AsReadOnly();
				refillOceanJob.OceanLevel = WaterSystem.OceanLevel;
				refillOceanJob.WaterMap = waterMap;
				GenerateErosionJobs.RefillOceanJob jobData4 = refillOceanJob;
				dependsOn = IJobParallelForExtensions.ScheduleByRef(ref jobData4, nativeList.Length, GetBatchSize(nativeList.Length), dependsOn);
				GenerateErosionJobs.WaterIncrementationJob waterIncrementationJob = default(GenerateErosionJobs.WaterIncrementationJob);
				waterIncrementationJob.WaterMap = waterMap;
				waterIncrementationJob.WaterFillRate = 0.04f;
				waterIncrementationJob.DT = 0.06f;
				GenerateErosionJobs.WaterIncrementationJob jobData5 = waterIncrementationJob;
				dependsOn = IJobParallelForExtensions.ScheduleByRef(ref jobData5, waterMap.Length, GetBatchSize(waterMap.Length), dependsOn);
				GenerateErosionJobs.CalculateOutputFluxJob calculateOutputFluxJob = default(GenerateErosionJobs.CalculateOutputFluxJob);
				calculateOutputFluxJob.TerrainHeightMapFloatVal = nativeArray2.AsReadOnly();
				calculateOutputFluxJob.WaterMap = waterMap.AsReadOnly();
				calculateOutputFluxJob.FluxMap = fluxMap;
				calculateOutputFluxJob.Res = heightMap.res;
				calculateOutputFluxJob.DT = 0.06f;
				calculateOutputFluxJob.GridCellSquareSize = num;
				calculateOutputFluxJob.PipeLength = pipeLength;
				calculateOutputFluxJob.PipeArea = pipeArea;
				GenerateErosionJobs.CalculateOutputFluxJob jobData6 = calculateOutputFluxJob;
				dependsOn = IJobParallelForExtensions.ScheduleByRef(ref jobData6, fluxMap.Length, GetBatchSize(fluxMap.Length), dependsOn);
				GenerateErosionJobs.AdjustWaterHeightByFluxJob adjustWaterHeightByFluxJob = default(GenerateErosionJobs.AdjustWaterHeightByFluxJob);
				adjustWaterHeightByFluxJob.WaterMap = waterMap;
				adjustWaterHeightByFluxJob.VelocityMap = velocityMap;
				adjustWaterHeightByFluxJob.FluxMap = fluxMap.AsReadOnly();
				adjustWaterHeightByFluxJob.Res = heightMap.res;
				adjustWaterHeightByFluxJob.DT = 0.06f;
				adjustWaterHeightByFluxJob.InvGridCellSquareSize = invGridCellSquareSize;
				GenerateErosionJobs.AdjustWaterHeightByFluxJob jobData7 = adjustWaterHeightByFluxJob;
				dependsOn = IJobParallelForExtensions.ScheduleByRef(ref jobData7, waterMap.Length, GetBatchSize(waterMap.Length), dependsOn);
				GenerateErosionJobs.TileCalculateAngleMap tileCalculateAngleMap = default(GenerateErosionJobs.TileCalculateAngleMap);
				tileCalculateAngleMap.AngleMap = angleMap;
				tileCalculateAngleMap.TerrainHeightMapSrcFloat = nativeArray2.AsReadOnly();
				tileCalculateAngleMap.NormY = heightMap.normY;
				tileCalculateAngleMap.Res = heightMap.res;
				tileCalculateAngleMap.TileSizeX = num2;
				tileCalculateAngleMap.TileSizeZ = num3;
				tileCalculateAngleMap.NumXTiles = num4;
				GenerateErosionJobs.TileCalculateAngleMap jobData8 = tileCalculateAngleMap;
				dependsOn = IJobParallelForExtensions.ScheduleByRef(ref jobData8, num6, num6 / JobsUtility.JobWorkerCount, dependsOn);
				GenerateErosionJobs.ErosionAndDepositionJob erosionAndDepositionJob = default(GenerateErosionJobs.ErosionAndDepositionJob);
				erosionAndDepositionJob.SedimentMap = nativeArray;
				erosionAndDepositionJob.MinTerrainHeightMap = minTerrainHeightMap.AsReadOnly();
				erosionAndDepositionJob.TerrainHeightMapSrcFloat = nativeArray2.AsReadOnly();
				erosionAndDepositionJob.TerrainHeightMapDstFloat = nativeArray3;
				erosionAndDepositionJob.WaterMap = waterMap;
				erosionAndDepositionJob.VelocityMap = velocityMap.AsReadOnly();
				erosionAndDepositionJob.AngleMap = angleMap.AsReadOnly();
				erosionAndDepositionJob.DT = 0.06f;
				GenerateErosionJobs.ErosionAndDepositionJob jobData9 = erosionAndDepositionJob;
				dependsOn = IJobParallelForExtensions.ScheduleByRef(ref jobData9, nativeArray.Length, GetBatchSize(nativeArray.Length), dependsOn);
				copyArrayJob = default(GenerateErosionJobs.CopyArrayJob<float>);
				copyArrayJob.CopyTarget = copyTarget;
				copyArrayJob.CopySource = nativeArray;
				GenerateErosionJobs.CopyArrayJob<float> jobData10 = copyArrayJob;
				dependsOn = IJobExtensions.ScheduleByRef(ref jobData10, dependsOn);
				copyArrayJob = default(GenerateErosionJobs.CopyArrayJob<float>);
				copyArrayJob.CopyTarget = nativeArray2;
				copyArrayJob.CopySource = nativeArray3;
				GenerateErosionJobs.CopyArrayJob<float> jobData11 = copyArrayJob;
				GenerateErosionJobs.TransportSedimentJob transportSedimentJob = default(GenerateErosionJobs.TransportSedimentJob);
				transportSedimentJob.SedimentMap = nativeArray;
				transportSedimentJob.SedimentReadOnlyMap = copyTarget.AsReadOnly();
				transportSedimentJob.VelocityMap = velocityMap.AsReadOnly();
				transportSedimentJob.Res = heightMap.res;
				transportSedimentJob.DT = 0.06f;
				GenerateErosionJobs.TransportSedimentJob jobData12 = transportSedimentJob;
				dependsOn = JobHandle.CombineDependencies(IJobExtensions.ScheduleByRef(ref jobData11, dependsOn), IJobParallelForExtensions.ScheduleByRef(ref jobData12, nativeArray.Length, GetBatchSize(nativeArray.Length), dependsOn));
				GenerateErosionJobs.EvaporationJob evaporationJob = default(GenerateErosionJobs.EvaporationJob);
				evaporationJob.WaterMap = waterMap;
				evaporationJob.DT = 0.06f;
				evaporationJob.EvaporationRate = 0.015f;
				GenerateErosionJobs.EvaporationJob jobData13 = evaporationJob;
				dependsOn = IJobParallelForExtensions.ScheduleByRef(ref jobData13, waterMap.Length, GetBatchSize(waterMap.Length), dependsOn);
			}
			GenerateErosionJobs.CopyBackFloatHeightToShortHeightJob copyBackFloatHeightToShortHeightJob = default(GenerateErosionJobs.CopyBackFloatHeightToShortHeightJob);
			copyBackFloatHeightToShortHeightJob.HeightMapAsFloat = nativeArray2.AsReadOnly();
			copyBackFloatHeightToShortHeightJob.HeightMapAsShort = dst;
			copyBackFloatHeightToShortHeightJob.TerrainOneOverSizeY = TerrainMeta.OneOverSize.y;
			copyBackFloatHeightToShortHeightJob.TerrainPositionY = TerrainMeta.Position.y;
			GenerateErosionJobs.CopyBackFloatHeightToShortHeightJob jobData14 = copyBackFloatHeightToShortHeightJob;
			dependsOn = IJobParallelForExtensions.ScheduleByRef(ref jobData14, nativeArray2.Length, GetBatchSize(nativeArray2.Length), dependsOn);
			NativeArray<float> nativeArray4 = new NativeArray<float>(nativeArray2.Length, Allocator.Persistent);
			GenerateErosionJobs.PopulateDeltaHeightJob populateDeltaHeightJob = default(GenerateErosionJobs.PopulateDeltaHeightJob);
			populateDeltaHeightJob.HeightMapOriginal = copyTarget2.AsReadOnly();
			populateDeltaHeightJob.HeightMap = nativeArray2.AsReadOnly();
			populateDeltaHeightJob.DeltaHeightMap = nativeArray4;
			GenerateErosionJobs.PopulateDeltaHeightJob jobData15 = populateDeltaHeightJob;
			dependsOn = IJobParallelForExtensions.ScheduleByRef(ref jobData15, nativeArray4.Length, GetBatchSize(nativeArray4.Length), dependsOn);
			minTerrainHeightMap.Dispose(dependsOn);
			waterMap.Dispose(dependsOn);
			fluxMap.Dispose(dependsOn);
			velocityMap.Dispose(dependsOn);
			nativeArray.Dispose(dependsOn);
			copyTarget.Dispose(dependsOn);
			nativeArray2.Dispose(dependsOn);
			nativeArray3.Dispose(dependsOn);
			nativeList.Dispose(dependsOn);
			copyTarget2.Dispose(dependsOn);
			dependsOn.Complete();
			heightMap.Pop();
			splatPaintingData = new SplatPaintingData(nativeArray4, angleMap);
		}
	}

	private void OnDestroy()
	{
		if (splatPaintingData.IsValid)
		{
			splatPaintingData.Dispose();
		}
	}

	private void OldErosion(uint seed)
	{
		TerrainTopologyMap topologyMap = TerrainMeta.TopologyMap;
		TerrainHeightMap heightmap = TerrainMeta.HeightMap;
		TerrainSplatMap splatmap = TerrainMeta.SplatMap;
		int erosion_res = heightmap.res;
		float[] erosion = new float[erosion_res * erosion_res];
		int deposit_res = splatmap.res;
		float[] deposit = new float[deposit_res * deposit_res];
		for (float num = TerrainMeta.Position.z; num < TerrainMeta.Position.z + TerrainMeta.Size.z; num += 10f)
		{
			for (float num2 = TerrainMeta.Position.x; num2 < TerrainMeta.Position.x + TerrainMeta.Size.x; num2 += 10f)
			{
				Vector3 worldPos = new Vector3(num2, 0f, num);
				float num3 = (worldPos.y = heightmap.GetHeight(worldPos));
				if (worldPos.y <= 15f)
				{
					continue;
				}
				Vector3 normal = heightmap.GetNormal(worldPos);
				if (normal.y <= 0.01f || normal.y >= 0.99f)
				{
					continue;
				}
				Vector2 normalized = normal.XZ2D().normalized;
				Vector2 vector = normalized;
				float num4 = 0f;
				float num5 = 0f;
				for (int i = 0; i < 300; i++)
				{
					worldPos.x += normalized.x;
					worldPos.z += normalized.y;
					if (Vector3.Angle((Vector3)normalized, (Vector3)vector) > 90f)
					{
						break;
					}
					float num6 = TerrainMeta.NormalizeX(worldPos.x);
					float num7 = TerrainMeta.NormalizeZ(worldPos.z);
					int topology = topologyMap.GetTopology(num6, num7);
					if (((uint)topology & 0xB4990u) != 0)
					{
						break;
					}
					float height = heightmap.GetHeight(num6, num7);
					if (height > num3 + 8f)
					{
						break;
					}
					float num8 = Mathf.Min(height, num3);
					worldPos.y = Mathf.Lerp(worldPos.y, num8, 0.5f);
					normal = heightmap.GetNormal(worldPos);
					normalized = Vector2.Lerp(normalized, normal.XZ2D().normalized, 0.5f).normalized;
					num3 = num8;
					float num9 = 0f;
					float target = 0f;
					if ((topology & 0x800400) == 0)
					{
						float value = Vector3.Angle(Vector3.up, normal);
						num9 = Mathf.InverseLerp(5f, 15f, value);
						target = 1f;
						if ((topology & 0x8000) == 0)
						{
							target = num9;
						}
					}
					num4 = Mathf.MoveTowards(num4, num9, 0.05f);
					num5 = Mathf.MoveTowards(num5, target, 0.05f);
					if ((topologyMap.GetTopology(num6, num7, 10f) & 2) == 0)
					{
						int num10 = Mathf.Clamp((int)(num6 * (float)erosion_res), 0, erosion_res - 1);
						int num11 = Mathf.Clamp((int)(num7 * (float)erosion_res), 0, erosion_res - 1);
						int num12 = Mathf.Clamp((int)(num6 * (float)deposit_res), 0, deposit_res - 1);
						int num13 = Mathf.Clamp((int)(num7 * (float)deposit_res), 0, deposit_res - 1);
						erosion[num11 * erosion_res + num10] += num4;
						deposit[num13 * deposit_res + num12] += num5;
					}
				}
			}
		}
		Parallel.For(1, erosion_res - 1, delegate(int z)
		{
			for (int k = 1; k < erosion_res - 1; k++)
			{
				float t = CalculateDelta(erosion, erosion_res, k, z, 1f, 0.8f, 0.6f);
				float delta = (0f - Mathf.Lerp(0f, 0.25f, t)) * TerrainMeta.OneOverSize.y;
				heightmap.AddHeight(k, z, delta);
			}
		});
		Parallel.For(1, deposit_res - 1, delegate(int z)
		{
			for (int j = 1; j < deposit_res - 1; j++)
			{
				float splat = splatmap.GetSplat(j, z, 2);
				float splat2 = splatmap.GetSplat(j, z, 4);
				if (splat > 0.1f || splat2 > 0.1f)
				{
					float value2 = CalculateDelta(deposit, deposit_res, j, z, 1f, 0.4f, 0.2f);
					value2 = Mathf.InverseLerp(1f, 3f, value2);
					value2 = Mathf.Lerp(0f, 0.5f, value2);
					splatmap.AddSplat(j, z, 128, value2);
				}
				else
				{
					float value3 = CalculateDelta(deposit, deposit_res, j, z, 1f, 0.2f, 0.1f);
					float value4 = CalculateDelta(deposit, deposit_res, j, z, 1f, 0.8f, 0.4f);
					value3 = Mathf.InverseLerp(1f, 3f, value3);
					value4 = Mathf.InverseLerp(1f, 3f, value4);
					value3 = Mathf.Lerp(0f, 1f, value3);
					value4 = Mathf.Lerp(0f, 1f, value4);
					splatmap.AddSplat(j, z, 1, value4 * 0.5f);
					splatmap.AddSplat(j, z, 64, value3 * 0.7f);
					splatmap.AddSplat(j, z, 128, value3 * 0.5f);
				}
			}
		});
		static float CalculateDelta(float[] data, int res, int x, int z, float cntr, float side, float diag)
		{
			int num14 = x - 1;
			int num15 = x + 1;
			int num16 = z - 1;
			int num17 = z + 1;
			side /= 4f;
			diag /= 4f;
			float num18 = data[z * res + x];
			float num19 = data[z * res + num14] + data[z * res + num15] + data[num17 * res + x] + data[num17 * res + x];
			float num20 = data[num16 * res + num14] + data[num16 * res + num15] + data[num17 * res + num14] + data[num17 * res + num15];
			return cntr * num18 + side * num19 + diag * num20;
		}
	}
}
