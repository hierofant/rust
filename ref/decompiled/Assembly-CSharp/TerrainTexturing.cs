using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rust;
using TerrainTexturingJobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using UtilityJobs;

[ExecuteInEditMode]
public class TerrainTexturing : TerrainExtension
{
	public struct ShoreData : IDisposable
	{
		public int ShoreMapSize;

		public float ShoreDistanceScale;

		public Vector3 Position;

		public Vector3 Size;

		public Vector3 OneOverSize;

		[Unity.Collections.ReadOnly]
		public NativeArray<float> ShoreDistances;

		[Unity.Collections.ReadOnly]
		public NativeArray<Vector4> ShoreVectors;

		public Vector4 DefaultVector;

		public float DefaultDistance;

		public int Len => ShoreMapSize * ShoreMapSize;

		public void FillWithDefault()
		{
			FillJob<float> jobData = default(FillJob<float>);
			jobData.Value = DefaultDistance;
			jobData.Values = ShoreDistances;
			jobData.Run();
			FillJob<Vector4> jobData2 = default(FillJob<Vector4>);
			jobData2.Value = DefaultVector;
			jobData2.Values = ShoreVectors;
			jobData2.Run();
		}

		[BurstDiscard]
		public Texture2D CreateTexture(string name)
		{
			Texture2D obj = new Texture2D(ShoreMapSize, ShoreMapSize, TextureFormat.RGBAHalf, mipChain: false, linear: true, createUninitialized: true)
			{
				name = name,
				filterMode = FilterMode.Bilinear,
				wrapMode = TextureWrapMode.Clamp
			};
			NativeArray<half4> nativeArray = new NativeArray<half4>(Len, Allocator.TempJob);
			TerrainTexturingJobs.PopulateTextureDataJob jobData = new TerrainTexturingJobs.PopulateTextureDataJob
			{
				colors = nativeArray,
				vectors = ShoreVectors.AsReadOnly(),
				distances = ShoreDistances.AsReadOnly()
			};
			ParallelJobEx.ScheduleParallel(ref jobData, nativeArray.Length, default(JobHandle)).Complete();
			obj.SetPixelData(nativeArray, 0);
			obj.Apply(updateMipmaps: false, makeNoLongerReadable: true);
			nativeArray.Dispose(default(JobHandle));
			return obj;
		}

		public float GetCoarseDistanceToShore(Vector3 pos)
		{
			Vector2 uv = default(Vector2);
			uv.x = (pos.x - Position.x) * OneOverSize.x;
			uv.y = (pos.z - Position.z) * OneOverSize.z;
			return GetCoarseDistanceToShore(uv);
		}

		public (Vector3 shoreDir, float shoreDist) GetCoarseVectorToShore(Vector3 pos)
		{
			Vector2 uv = default(Vector2);
			uv.x = (pos.x - Position.x) * OneOverSize.x;
			uv.y = (pos.z - Position.z) * OneOverSize.z;
			return GetCoarseVectorToShore(uv);
		}

		public (Vector3 shoreDir, float shoreDist) GetCoarseVectorToShore(Vector2 uv)
		{
			int shoreMapSize = ShoreMapSize;
			int num = shoreMapSize - 1;
			float num2 = uv.x * (float)num;
			float num3 = uv.y * (float)num;
			int num4 = (int)num2;
			int num5 = (int)num3;
			float num6 = num2 - (float)num4;
			float num7 = num3 - (float)num5;
			num4 = ((num4 >= 0) ? num4 : 0);
			num5 = ((num5 >= 0) ? num5 : 0);
			num4 = ((num4 <= num) ? num4 : num);
			num5 = ((num5 <= num) ? num5 : num);
			int num8 = ((num2 < (float)num) ? 1 : 0);
			int num9 = ((num3 < (float)num) ? shoreMapSize : 0);
			int num10 = num5 * shoreMapSize + num4;
			int index = num10 + num8;
			int num11 = num10 + num9;
			int index2 = num11 + num8;
			Vector3 vector = ShoreVectors[num10];
			Vector3 vector2 = ShoreVectors[index];
			Vector3 vector3 = ShoreVectors[num11];
			Vector3 vector4 = ShoreVectors[index2];
			Vector3 vector5 = default(Vector3);
			vector5.x = (vector2.x - vector.x) * num6 + vector.x;
			vector5.y = (vector2.y - vector.y) * num6 + vector.y;
			vector5.z = (vector2.z - vector.z) * num6 + vector.z;
			Vector3 vector6 = default(Vector3);
			vector6.x = (vector4.x - vector3.x) * num6 + vector3.x;
			vector6.y = (vector4.y - vector3.y) * num6 + vector3.y;
			vector6.z = (vector4.z - vector3.z) * num6 + vector3.z;
			float x = (vector6.x - vector5.x) * num7 + vector5.x;
			float z = (vector6.y - vector5.y) * num7 + vector5.y;
			return new ValueTuple<Vector3, float>(item2: ((vector6.z - vector5.z) * num7 + vector5.z) * ShoreDistanceScale, item1: new Vector3(x, 0f, z));
		}

		public (Vector3 shoreDir, float shoreDist) GetCoarseVectorToShore(float normX, float normY)
		{
			return GetCoarseVectorToShore(new Vector2(normX, normY));
		}

		public Vector4 GetRawShoreVector(Vector3 pos)
		{
			Vector2 uv = default(Vector2);
			uv.x = (pos.x - Position.x) * OneOverSize.x;
			uv.y = (pos.z - Position.z) * OneOverSize.z;
			return GetRawShoreVector(uv);
		}

		public Vector4 GetRawShoreVector(Vector2 uv)
		{
			int shoreMapSize = ShoreMapSize;
			int num = shoreMapSize - 1;
			float num2 = uv.x * (float)num;
			float num3 = uv.y * (float)num;
			int num4 = (int)num2;
			int num5 = (int)num3;
			num4 = ((num4 >= 0) ? num4 : 0);
			num5 = ((num5 >= 0) ? num5 : 0);
			num4 = ((num4 <= num) ? num4 : num);
			num5 = ((num5 <= num) ? num5 : num);
			return ShoreVectors[num5 * shoreMapSize + num4];
		}

		public readonly float GetCoarseDistanceToShore(Vector2 uv)
		{
			int shoreMapSize = ShoreMapSize;
			int num = shoreMapSize - 1;
			float num2 = uv.x * (float)num;
			float num3 = uv.y * (float)num;
			int num4 = (int)num2;
			int num5 = (int)num3;
			float num6 = num2 - (float)num4;
			float num7 = num3 - (float)num5;
			num4 = ((num4 >= 0) ? num4 : 0);
			num5 = ((num5 >= 0) ? num5 : 0);
			num4 = ((num4 <= num) ? num4 : num);
			num5 = ((num5 <= num) ? num5 : num);
			int num8 = ((num2 < (float)num) ? 1 : 0);
			int num9 = ((num3 < (float)num) ? shoreMapSize : 0);
			int num10 = num5 * shoreMapSize + num4;
			int index = num10 + num8;
			int num11 = num10 + num9;
			int index2 = num11 + num8;
			float num12 = ShoreDistances[num10];
			float num13 = ShoreDistances[index];
			float num14 = ShoreDistances[num11];
			float num15 = ShoreDistances[index2];
			float num16 = (num13 - num12) * num6 + num12;
			return (((num15 - num14) * num6 + num14 - num16) * num7 + num16) * ShoreDistanceScale;
		}

		public void Dispose()
		{
			NativeArrayEx.SafeDispose(ref ShoreDistances);
			NativeArrayEx.SafeDispose(ref ShoreVectors);
		}
	}

	public struct ShoreVectorQueryStructure
	{
		private ShoreData mainlandData;

		private ShoreData deepSeaData;

		private Bounds deepSeaBounds;

		internal ShoreVectorQueryStructure(ShoreData mainlandData, ShoreData deepSeaData, Bounds deepSeaBounds)
		{
			this.mainlandData = mainlandData;
			this.deepSeaData = deepSeaData;
			this.deepSeaBounds = deepSeaBounds;
		}

		public float GetCoarseDistanceToShore(Vector3 pos)
		{
			return (deepSeaBounds.Contains(pos) ? deepSeaData : mainlandData).GetCoarseDistanceToShore(pos);
		}
	}

	public const int ShoreVectorDownscale = 1;

	private const int ShoreVectorBlurPasses = 1;

	private float terrainSize;

	private float deepSeaSize;

	private ShoreData MainlandShoreData;

	private ShoreData DeepSeaShoreData;

	private bool deepSeaShoreDataDirty;

	private List<(BakedShoreVectors data, Transform t)> deepSeaPostGenApplication;

	public bool debugFoliageDisplacement;

	private bool initialized;

	private static TerrainTexturing instance;

	private int afCached;

	private int globalTextureMipmapLimitCached;

	private int anisotropicFilteringCached;

	private bool streamingMipmapsActiveCached;

	private bool billboardsFaceCameraPositionCached;

	public static TerrainTexturing Instance => instance;

	public bool TexturesInitialized => initialized;

	private void ReleaseBasePyramid()
	{
	}

	private void UpdateBasePyramid()
	{
	}

	private void InitializeCoarseHeightSlope()
	{
	}

	private void ReleaseCoarseHeightSlope()
	{
	}

	private void UpdateCoarseHeightSlope()
	{
	}

	internal ShoreData GetMap(Vector3 position)
	{
		if (!DeepSeaManager.IsInsideDeepSea(position))
		{
			return MainlandShoreData;
		}
		return DeepSeaShoreData;
	}

	internal ShoreData GetMap(bool isDeepSea)
	{
		if (!isDeepSea)
		{
			return MainlandShoreData;
		}
		return DeepSeaShoreData;
	}

	internal ref ShoreData GetMapByRef(bool isDeepSea)
	{
		if (!isDeepSea)
		{
			return ref MainlandShoreData;
		}
		return ref DeepSeaShoreData;
	}

	private void InitializeShoreVector()
	{
		int num = Mathf.ClosestPowerOfTwo(terrainData.heightmapResolution) >> 1;
		terrainSize = Mathf.Max(terrainData.size.x, terrainData.size.z);
		deepSeaSize = DeepSeaManager.DeepSeaBounds.size.XZ2D().Max();
		MainlandShoreData = new ShoreData
		{
			ShoreMapSize = num,
			ShoreDistanceScale = terrainSize / (float)num,
			ShoreDistances = new NativeArray<float>(num * num, Allocator.Persistent, NativeArrayOptions.UninitializedMemory),
			ShoreVectors = new NativeArray<Vector4>(num * num, Allocator.Persistent, NativeArrayOptions.UninitializedMemory),
			Position = TerrainMeta.Position,
			Size = TerrainMeta.Size,
			OneOverSize = TerrainMeta.OneOverSize,
			DefaultDistance = 10000f,
			DefaultVector = new Vector4(1f, 1f, 1f, 0f)
		};
		MainlandShoreData.FillWithDefault();
		int num2 = MainlandShoreData.ShoreMapSize >> 1;
		DeepSeaShoreData = new ShoreData
		{
			ShoreMapSize = num2,
			ShoreDistanceScale = deepSeaSize / (float)num2,
			ShoreDistances = new NativeArray<float>(num2 * num2, Allocator.Persistent, NativeArrayOptions.UninitializedMemory),
			ShoreVectors = new NativeArray<Vector4>(num2 * num2, Allocator.Persistent, NativeArrayOptions.UninitializedMemory),
			Position = DeepSeaManager.DeepSeaBounds.min,
			Size = DeepSeaManager.DeepSeaBounds.size,
			OneOverSize = DeepSeaManager.DeepSeaBounds.size.Inverse(),
			DefaultDistance = 10000f,
			DefaultVector = new Vector4(1f, 1f, 1f, 1f)
		};
		DeepSeaShoreData.FillWithDefault();
		deepSeaShoreDataDirty = true;
		deepSeaPostGenApplication = new List<(BakedShoreVectors, Transform)>();
	}

	private void GenerateShoreVector()
	{
		using (TimeWarning.New("GenerateShoreVector", 500))
		{
			GenerateShoreVector(out var distances, out var vectors);
			MainlandShoreData.ShoreDistances = distances;
			MainlandShoreData.ShoreVectors = vectors;
			if (!DeepSeaShoreData.ShoreDistances.IsCreated)
			{
				DeepSeaShoreData.ShoreDistances = new NativeArray<float>(DeepSeaShoreData.Len, Allocator.Persistent);
			}
			if (!DeepSeaShoreData.ShoreVectors.IsCreated)
			{
				DeepSeaShoreData.ShoreVectors = new NativeArray<Vector4>(DeepSeaShoreData.Len, Allocator.Persistent);
			}
			DeepSeaShoreData.FillWithDefault();
		}
	}

	private void UpdateDeepSeaShoreVectorTexture()
	{
		using (TimeWarning.New("UpdateDeepSeaShoreVectorTexture"))
		{
			if (!deepSeaShoreDataDirty)
			{
				return;
			}
			deepSeaShoreDataDirty = false;
			GenerateShoreVector(out var distances, out var vectors, genDeepSea: true);
			NativeArrayEx.SafeDispose(ref DeepSeaShoreData.ShoreDistances);
			DeepSeaShoreData.ShoreDistances = distances;
			NativeArrayEx.SafeDispose(ref DeepSeaShoreData.ShoreVectors);
			DeepSeaShoreData.ShoreVectors = vectors;
			Bounds deepSeaBounds = DeepSeaManager.DeepSeaBounds;
			Vector3 min = deepSeaBounds.min;
			Vector3 vector = deepSeaBounds.size.Inverse();
			NativeArray<float> deepSeaShoreDistances = DeepSeaShoreData.ShoreDistances;
			NativeArray<Vector4> deepSeaShoreVectors = DeepSeaShoreData.ShoreVectors;
			foreach (var item3 in deepSeaPostGenApplication)
			{
				BakedShoreVectors item = item3.data;
				Transform item2 = item3.t;
				if (!item || !item2)
				{
					continue;
				}
				ShoreVectorData shoreVectorData = item.ShoreVectorData;
				item2.GetPositionAndRotation(out var position, out var rotation);
				float y = rotation.eulerAngles.y;
				float normX = (position.x - min.x) * vector.x;
				float normZ = (position.z - min.z) * vector.z;
				float worldSize = shoreVectorData.WorldSize;
				int shoreMapSize = DeepSeaShoreData.ShoreMapSize;
				float[] srcDistances = shoreVectorData.Distances;
				Vector4[] srcVectors = shoreVectorData.Vectors;
				Quaternion quat = Quaternion.Euler(0f, y, 0f);
				BlitBakedData(worldSize, shoreVectorData.ShoreVectorDimension, deepSeaBounds, shoreMapSize, normX, normZ, y, delegate(int si, int di)
				{
					float num = srcDistances[si];
					float num2 = deepSeaShoreDistances[di];
					if (num < num2)
					{
						deepSeaShoreDistances[di] = srcDistances[si];
						Vector4 vector2 = srcVectors[si];
						Vector3 vector3 = new Vector3(vector2.x, 0f, vector2.y);
						vector3 = quat * vector3;
						vector2 = new Vector4(vector3.x, vector3.z, vector2.z, vector2.w);
						deepSeaShoreVectors[di] = vector2;
					}
				});
			}
		}
	}

	private void OnDestroy()
	{
		ReleaseShoreVector();
	}

	private void ReleaseShoreVector()
	{
		MainlandShoreData.Dispose();
		DeepSeaShoreData.Dispose();
	}

	public void GenerateShoreVector(out NativeArray<float> distances, out NativeArray<Vector4> vectors, bool genDeepSea = false)
	{
		using (TimeWarning.New("GenerateShoreVector"))
		{
			int size;
			float shoreDistanceScale;
			Vector3 position;
			if (genDeepSea)
			{
				size = DeepSeaShoreData.ShoreMapSize;
				shoreDistanceScale = DeepSeaShoreData.ShoreDistanceScale;
				position = DeepSeaShoreData.Position;
			}
			else
			{
				size = MainlandShoreData.ShoreMapSize;
				shoreDistanceScale = MainlandShoreData.ShoreDistanceScale;
				position = MainlandShoreData.Position;
			}
			NativeArray<Vector3> positions = new NativeArray<Vector3>(size * size, Allocator.TempJob);
			NativeArray<float> nativeArray = new NativeArray<float>(size * size, Allocator.TempJob);
			NativeArray<byte> bitmap = new NativeArray<byte>(size * size, Allocator.TempJob);
			distances = new NativeArray<float>(size * size, Allocator.Persistent);
			vectors = new NativeArray<Vector4>(size * size, Allocator.Persistent);
			JobHandle jobHandle;
			using (TimeWarning.New("WaterDepth"))
			{
				NativeArray<int> indices = new NativeArray<int>(size * size, Allocator.TempJob);
				NativeArray<float> heights = new NativeArray<float>(size * size, Allocator.TempJob);
				TerrainTexturingJobs.GenSamplePoints genSamplePoints = default(TerrainTexturingJobs.GenSamplePoints);
				genSamplePoints.indices = indices;
				genSamplePoints.positions = positions;
				genSamplePoints.shoreMapSize = size;
				genSamplePoints.terrainPosition = position;
				genSamplePoints.upscaleCoords = shoreDistanceScale;
				TerrainTexturingJobs.GenSamplePoints jobData = genSamplePoints;
				IJobExtensions.RunByRef(ref jobData);
				JobHandle dependsOn = default(JobHandle);
				if ((bool)TerrainMeta.HeightMap && TerrainMeta.HeightMap.isInitialized)
				{
					dependsOn = TerrainMeta.HeightMap.GetHeights(positions.AsReadOnly(), nativeArray);
				}
				else
				{
					FillJob<float> jobData2 = default(FillJob<float>);
					jobData2.Value = 0f;
					jobData2.Values = nativeArray;
					dependsOn = jobData2.Schedule(dependsOn);
				}
				WaterLevel.GetWaterLevels(positions.AsReadOnly(), indices.AsReadOnly(), waves: false, heights);
				TerrainTexturingJobs.GenShoreVecBitMapJob genShoreVecBitMapJob = default(TerrainTexturingJobs.GenShoreVecBitMapJob);
				genShoreVecBitMapJob.bitmap = bitmap;
				genShoreVecBitMapJob.terrainHeights = nativeArray.AsReadOnly();
				genShoreVecBitMapJob.waterHeights = heights.AsReadOnly();
				TerrainTexturingJobs.GenShoreVecBitMapJob jobData3 = genShoreVecBitMapJob;
				jobHandle = ParallelJobEx.ScheduleParallel(ref jobData3, bitmap.Length, dependsOn);
				indices.Dispose(jobHandle);
				heights.Dispose(jobHandle);
			}
			using (TimeWarning.New("DistanceField.XXX"))
			{
				JobHandle inputDeps = jobHandle;
				byte threshold = 127;
				NativeArray<byte>.ReadOnly image = bitmap.AsReadOnly();
				inputDeps = DistanceField.GenerateNative(in size, in threshold, in image, in distances, inputDeps);
				inputDeps = DistanceField.ApplyGaussianBlurNative(size, distances, 1, inputDeps);
				inputDeps = DistanceField.GenerateVectorsNative(in size, distances.AsReadOnly(), vectors, inputDeps);
				bitmap.Dispose(inputDeps);
				inputDeps.Complete();
			}
			using (TimeWarning.New("Topology Mask"))
			{
				if (!(TerrainMeta.TopologyMap != null) || !TerrainMeta.TopologyMap.isInitialized || !(TerrainMeta.HeightMap != null) || !TerrainMeta.HeightMap.isInitialized)
				{
					for (int i = 0; i < vectors.Length; i++)
					{
						Vector4 value = vectors[i];
						value.w = -1f;
						vectors[i] = value;
					}
					positions.Dispose(default(JobHandle));
					nativeArray.Dispose(default(JobHandle));
					return;
				}
				JobHandle dependsOn2 = default(JobHandle);
				if (genDeepSea)
				{
					TerrainTexturingJobs.FillAsOceanTopologyJob fillAsOceanTopologyJob = default(TerrainTexturingJobs.FillAsOceanTopologyJob);
					fillAsOceanTopologyJob.vectors = vectors;
					TerrainTexturingJobs.FillAsOceanTopologyJob jobData4 = fillAsOceanTopologyJob;
					dependsOn2 = ParallelJobEx.ScheduleParallel(ref jobData4, vectors.Length, dependsOn2);
				}
				else
				{
					NativeArray<float> radii = new NativeArray<float>(size * size, Allocator.TempJob);
					NativeArray<int> results = new NativeArray<int>(size * size, Allocator.TempJob);
					TerrainTexturingJobs.GenTopologyRadiiJob genTopologyRadiiJob = default(TerrainTexturingJobs.GenTopologyRadiiJob);
					genTopologyRadiiJob.heights = nativeArray.AsReadOnly();
					genTopologyRadiiJob.radii = radii;
					TerrainTexturingJobs.GenTopologyRadiiJob jobData5 = genTopologyRadiiJob;
					dependsOn2 = ParallelJobEx.ScheduleParallel(ref jobData5, radii.Length, dependsOn2);
					dependsOn2 = TerrainMeta.TopologyMap.GetTopologiesIndirect(positions.AsReadOnly(), radii.AsReadOnly(), results, dependsOn2);
					TerrainTexturingJobs.ProcessTopologyJob processTopologyJob = default(TerrainTexturingJobs.ProcessTopologyJob);
					processTopologyJob.topologies = results.AsReadOnly();
					processTopologyJob.vectors = vectors;
					TerrainTexturingJobs.ProcessTopologyJob jobData6 = processTopologyJob;
					dependsOn2 = ParallelJobEx.ScheduleParallel(ref jobData6, vectors.Length, dependsOn2);
					radii.Dispose(dependsOn2);
					results.Dispose(dependsOn2);
				}
				positions.Dispose(dependsOn2);
				nativeArray.Dispose(dependsOn2);
				dependsOn2.Complete();
			}
		}
	}

	public float GetCoarseDistanceToShore(Vector3 pos)
	{
		return GetMap(pos).GetCoarseDistanceToShore(pos);
	}

	public (Vector3 shoreDir, float shoreDist) GetCoarseVectorToShore(Vector3 pos)
	{
		return GetMap(pos).GetCoarseVectorToShore(pos);
	}

	public (Vector3 shoreDir, float shoreDist) GetMainlandCoarseVectorToShore(float normX, float normY)
	{
		return MainlandShoreData.GetCoarseVectorToShore(new Vector2(normX, normY));
	}

	public Vector4 GetRawShoreVector(Vector3 pos)
	{
		return GetMap(pos).GetRawShoreVector(pos);
	}

	public ShoreVectorQueryStructure GetShoreVectorQueryStructure()
	{
		return new ShoreVectorQueryStructure(MainlandShoreData, DeepSeaShoreData, DeepSeaManager.DeepSeaBounds);
	}

	public void GetCoarseDistancesToShoreIndirect(NativeArray<Vector3>.ReadOnly positions, NativeArray<int>.ReadOnly indices, NativeArray<float> results)
	{
		GetCoarseDistsToShoreJobIndirect getCoarseDistsToShoreJobIndirect = default(GetCoarseDistsToShoreJobIndirect);
		getCoarseDistsToShoreJobIndirect.Dists = results;
		getCoarseDistsToShoreJobIndirect.Positions = positions;
		getCoarseDistsToShoreJobIndirect.Indices = indices;
		getCoarseDistsToShoreJobIndirect.QueryStructure = GetShoreVectorQueryStructure();
		GetCoarseDistsToShoreJobIndirect jobData = getCoarseDistsToShoreJobIndirect;
		IJobExtensions.RunByRef(ref jobData);
	}

	public void ApplyBakedDeepSeaVectors(BakedShoreVectors bakedShoreVectors, Transform t)
	{
		using (TimeWarning.New("ApplyBakedDeepSeaVectors"))
		{
			if (bakedShoreVectors.ShoreVectorData == null || bakedShoreVectors.ShoreVectorData.Distances == null)
			{
				return;
			}
			ShoreVectorData shoreVectorData = bakedShoreVectors.ShoreVectorData;
			t.GetPositionAndRotation(out var position, out var rotation);
			if (bakedShoreVectors.OnlyBakeShoreVectors)
			{
				deepSeaPostGenApplication.Add((bakedShoreVectors, t));
			}
			float y = rotation.eulerAngles.y;
			Bounds deepSeaBounds = DeepSeaManager.DeepSeaBounds;
			Vector3 min = deepSeaBounds.min;
			Vector3 vector = deepSeaBounds.size.Inverse();
			float normX = (position.x - min.x) * vector.x;
			float normZ = (position.z - min.z) * vector.z;
			float worldSize = shoreVectorData.WorldSize;
			short[] srcHeightData = shoreVectorData.HeightData;
			short[] array = srcHeightData;
			if (array != null && array.Length != 0)
			{
				float srcPositionY = shoreVectorData.HeightInfo.x;
				float srcSizeY = shoreVectorData.HeightInfo.y;
				BlitBakedData(worldSize, shoreVectorData.HeightDimension, deepSeaBounds, TerrainMeta.HeightMap.res, normX, normZ, y, delegate(int si, int di)
				{
					float num = BitUtility.Short2Float(srcHeightData[si]);
					short num2 = BitUtility.Float2Short(TerrainMeta.NormalizeY(srcPositionY + num * srcSizeY));
					short num3 = TerrainMeta.HeightMap.deepSeaHeights[di];
					if (num2 > num3)
					{
						TerrainMeta.HeightMap.deepSeaHeights[di] = num2;
					}
				});
			}
			deepSeaShoreDataDirty = true;
		}
	}

	private static void BlitBakedData(float worldSize, int dimension, Bounds deepSeaBounds, int dstMapSize, float normX, float normZ, float yaw, Action<int, int> action)
	{
		using (TimeWarning.New("BlitBakedData"))
		{
			float scaleMod = worldSize / (float)dimension / (deepSeaBounds.size.XZ2D().Max() / (float)dstMapSize);
			Vector2 destCenterPx = new Vector2(normX * (float)dstMapSize, normZ * (float)dstMapSize);
			Vector2 vector = new Vector2((float)dimension * scaleMod, (float)dimension * scaleMod);
			float f = yaw * (MathF.PI / 180f);
			float cosA = Mathf.Cos(f);
			float sinA = Mathf.Sin(f);
			Vector2 vector2 = vector * 0.5f;
			float num = Mathf.Abs(cosA);
			float num2 = Mathf.Abs(sinA);
			float num3 = num * vector2.x + num2 * vector2.y;
			float num4 = num2 * vector2.x + num * vector2.y;
			int left = Mathf.FloorToInt(destCenterPx.x - num3);
			int right = Mathf.CeilToInt(destCenterPx.x + num3);
			int num5 = Mathf.FloorToInt(destCenterPx.y - num4);
			int num6 = Mathf.CeilToInt(destCenterPx.y + num4);
			if (right < 0 || left >= dstMapSize || num6 < 0 || num5 >= dstMapSize)
			{
				return;
			}
			left = Mathf.Clamp(left, 0, dstMapSize - 1);
			right = Mathf.Clamp(right, 0, dstMapSize - 1);
			num5 = Mathf.Clamp(num5, 0, dstMapSize - 1);
			num6 = Mathf.Clamp(num6, 0, dstMapSize - 1);
			Vector2 srcPivotPx = new Vector2((float)dimension * 0.5f, (float)dimension * 0.5f);
			Parallel.For(num5, num6 + 1, delegate(int z)
			{
				for (int i = left; i <= right; i++)
				{
					float num7 = (float)i + 0.5f - destCenterPx.x;
					float num8 = (float)z + 0.5f - destCenterPx.y;
					float num9 = (cosA * num7 - sinA * num8) / scaleMod;
					float num10 = (sinA * num7 + cosA * num8) / scaleMod;
					int num11 = (int)(srcPivotPx.x + num9);
					int num12 = (int)(srcPivotPx.y + num10);
					if (num11 >= 0 && num11 <= dimension - 1 && num12 >= 0 && num12 <= dimension - 1)
					{
						int arg = z * dstMapSize + i;
						int arg2 = num12 * dimension + num11;
						action(arg2, arg);
					}
				}
			});
		}
	}

	public void ClearDeepSeaData()
	{
		DeepSeaShoreData.FillWithDefault();
		deepSeaShoreDataDirty = true;
	}

	private void InitializeWaterHeight()
	{
	}

	private void ReleaseWaterHeight()
	{
	}

	private void UpdateWaterHeight()
	{
	}

	private void CheckInstance()
	{
		instance = ((instance != null) ? instance : this);
	}

	private void Awake()
	{
		CheckInstance();
	}

	public override void Setup()
	{
		CheckInstance();
		InitializeShoreVector();
	}

	public override void PostSetup()
	{
		TerrainMeta component = GetComponent<TerrainMeta>();
		if (component == null || component.config == null)
		{
			Debug.LogError("[TerrainTexturing] Missing TerrainMeta or TerrainConfig not assigned.");
			return;
		}
		Shutdown();
		InitializeCoarseHeightSlope();
		GenerateShoreVector();
		InitializeWaterHeight();
		initialized = true;
	}

	private void Shutdown()
	{
		ReleaseBasePyramid();
		ReleaseCoarseHeightSlope();
		ReleaseShoreVector();
		ReleaseWaterHeight();
		initialized = false;
	}

	public void OnEnable()
	{
		CheckInstance();
	}

	private void OnDisable()
	{
		if (!Rust.Application.isQuitting)
		{
			Shutdown();
		}
	}

	private void Update()
	{
		if (initialized)
		{
			UpdateBasePyramid();
			UpdateCoarseHeightSlope();
			UpdateWaterHeight();
			UpdateDeepSeaShoreVectorTexture();
		}
	}
}
