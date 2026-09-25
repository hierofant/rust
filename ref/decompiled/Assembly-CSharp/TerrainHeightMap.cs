#define UNITY_ASSERTIONS
using System;
using System.Threading.Tasks;
using TerrainHeightMapJobs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Jobs;

public class TerrainHeightMap : TerrainMap<short>
{
	public struct RG16
	{
		public ushort r;

		public ushort g;

		public RG16(ushort r, ushort g)
		{
			this.r = r;
			this.g = g;
		}
	}

	public struct HeightMapQueryStructure
	{
		public NativeArray<short>.ReadOnly Data;

		public Vector3 TerrainPosition;

		public Vector3 TerrainOneOverSize;

		public int Res;

		public float NormY;

		public float HeightOffset;

		public float HeightScale;

		public readonly float GetHeightFromUV(Vector2 uv)
		{
			float height = HeightMapData.GetHeight01(uv, Data, Res);
			return HeightOffset + height * HeightScale;
		}

		public readonly Vector3 GetNormalFromUV(Vector2 uv)
		{
			return HeightMapData.GetNormal(uv, NormY, Data, Res);
		}
	}

	public struct HeightSampler
	{
		private unsafe short* data;

		private int res;

		private int len;

		private float scaleX;

		private float baseX;

		private float scaleZ;

		private float baseZ;

		private float posY;

		private float sizeY;

		private unsafe short* rowT;

		private unsafe short* rowB;

		private float wz;

		public unsafe static HeightSampler Create(NativeArray<short> heights, int res, Vector3 mapOrigin, Vector3 mapSize, float posY, float sizeY)
		{
			int num = res - 1;
			HeightSampler result = default(HeightSampler);
			result.data = (short*)heights.GetUnsafeReadOnlyPtr();
			result.res = res;
			result.len = num;
			result.scaleX = (float)num / mapSize.x;
			result.baseX = (0f - mapOrigin.x) * ((float)num / mapSize.x);
			result.scaleZ = (float)num / mapSize.z;
			result.baseZ = (0f - mapOrigin.z) * ((float)num / mapSize.z);
			result.posY = posY;
			result.sizeY = sizeY;
			return result;
		}

		public unsafe void BeginRow(float worldZ)
		{
			float num = worldZ * scaleZ + baseZ;
			int num2 = (int)num;
			wz = Mathf.Clamp01(num - (float)num2);
			num2 = ((num2 >= 0) ? num2 : 0);
			num2 = ((num2 <= len) ? num2 : len);
			int num3 = ((num < (float)len) ? res : 0);
			rowT = data + num2 * res;
			rowB = rowT + num3;
		}

		public unsafe float SampleRow(float worldX)
		{
			float num = worldX * scaleX + baseX;
			int num2 = (int)num;
			float num3 = Mathf.Clamp01(num - (float)num2);
			num2 = ((num2 >= 0) ? num2 : 0);
			num2 = ((num2 <= len) ? num2 : len);
			int num4 = ((num < (float)len) ? 1 : 0);
			float num5 = BitUtility.Short2Float(rowT[num2]);
			float num6 = BitUtility.Short2Float(rowT[num2 + num4]);
			float num7 = BitUtility.Short2Float(rowB[num2]);
			float num8 = BitUtility.Short2Float(rowB[num2 + num4]);
			float num9 = (num6 - num5) * num3 + num5;
			float num10 = (num8 - num7) * num3 + num7;
			return posY + ((num10 - num9) * wz + num9) * sizeY;
		}
	}

	public Texture2D HeightTexture;

	public Texture2D NormalTexture;

	[Header("Collider Sampling")]
	[Min(1f)]
	public int ColliderSamplesPerAxis = 1;

	[Range(0f, 1f)]
	public float ColliderSampleSpread = 1f;

	public float normY;

	private bool _generatedHeightTexture;

	private bool _generatedNormalTexture;

	public NativeArray<short> deepSeaHeights;

	public NativeArray<short>.ReadOnly DeepSeaData => deepSeaHeights.AsReadOnly();

	public override void Setup()
	{
		res = terrainData.heightmapResolution;
		InitArrays(res * res);
		deepSeaHeights = new NativeArray<short>(res * res, Allocator.Persistent);
		ResetDeepSeaToFloor();
		normY = TerrainMeta.Size.x / TerrainMeta.Size.y / (float)res;
		if (!(HeightTexture != null))
		{
			return;
		}
		if (HeightTexture.width == HeightTexture.height && HeightTexture.width == res)
		{
			if (HeightTexture.graphicsFormat != GraphicsFormat.R16G16_UNorm)
			{
				Color32[] pixels = HeightTexture.GetPixels32();
				int i = 0;
				int num = 0;
				for (; i < res; i++)
				{
					int num2 = 0;
					while (num2 < res)
					{
						Color32 c = pixels[num];
						dst[i * res + num2] = BitUtility.DecodeShort(c);
						num2++;
						num++;
					}
				}
			}
			else
			{
				NativeArray<RG16> pixelData = HeightTexture.GetPixelData<RG16>(0);
				int j = 0;
				int num3 = 0;
				for (; j < res; j++)
				{
					int num4 = 0;
					while (num4 < res)
					{
						RG16 rG = pixelData[num3];
						dst[j * res + num4] = BitUtility.Float2Short(BitUtility.UShort2Float(rG.r));
						num4++;
						num3++;
					}
				}
			}
			if (HeightTexture.graphicsFormat != GraphicsFormat.R16G16_UNorm)
			{
				ConvertHeightMapTexture();
			}
		}
		else
		{
			Debug.LogError("Invalid height texture: " + HeightTexture.name);
		}
	}

	public void SetupEmpty(int newRes)
	{
		res = newRes;
		InitArrays(res * res);
		deepSeaHeights = new NativeArray<short>(res * res, Allocator.Persistent);
		ResetDeepSeaToFloor();
		normY = TerrainMeta.Size.x / TerrainMeta.Size.y / (float)res;
	}

	public void SetupFrom(int newRes, NativeArray<float>.ReadOnly heights, NativeArray<float>.ReadOnly dsHeights)
	{
		res = newRes;
		InitArrays(res * res);
		for (int i = 0; i < heights.Length; i++)
		{
			src[i] = BitUtility.Float2Short(heights[i]);
		}
		deepSeaHeights = new NativeArray<short>(res * res, Allocator.Persistent);
		for (int j = 0; j < dsHeights.Length; j++)
		{
			deepSeaHeights[j] = BitUtility.Float2Short(dsHeights[j]);
		}
		normY = TerrainMeta.Size.x / TerrainMeta.Size.y / (float)res;
	}

	public override void Dispose()
	{
		base.Dispose();
		NativeArrayEx.SafeDispose(ref deepSeaHeights);
		if (_generatedHeightTexture && HeightTexture != null)
		{
			UnityEngine.Object.Destroy(HeightTexture);
			HeightTexture = null;
		}
		if (_generatedNormalTexture && NormalTexture != null)
		{
			UnityEngine.Object.Destroy(NormalTexture);
			NormalTexture = null;
		}
	}

	public void ResetDeepSeaToFloor()
	{
		short value = BitUtility.Float2Short(TerrainMeta.NormalizeY(DeepSeaManager.SeaFloorDepth));
		for (int i = 0; i < res * res; i++)
		{
			deepSeaHeights[i] = value;
		}
	}

	public void ApplyToTerrain()
	{
		float[,] heights = terrainData.GetHeights(0, 0, res, res);
		Parallel.For(0, res, delegate(int z)
		{
			for (int i = 0; i < res; i++)
			{
				heights[z, i] = GetHeight01(i, z);
			}
		});
		terrainData.SetHeights(0, 0, heights);
		TerrainCollider component = terrainRenderer.gameObject.GetComponent<TerrainCollider>();
		if ((bool)component)
		{
			component.enabled = false;
			component.enabled = true;
		}
	}

	public void ApplyToTerrainDelay()
	{
		float[,] heights = terrainData.GetHeights(0, 0, res, res);
		Parallel.For(0, res, delegate(int z)
		{
			for (int i = 0; i < res; i++)
			{
				heights[z, i] = GetHeight01(i, z);
			}
		});
		terrainData.SetHeightsDelayLOD(0, 0, heights);
		TerrainCollider component = terrainRenderer.gameObject.GetComponent<TerrainCollider>();
		terrainData.SyncHeightmap();
		if ((bool)component)
		{
			component.enabled = false;
			component.enabled = true;
		}
	}

	public void ConvertHeightMapTexture()
	{
		res = terrainData.heightmapResolution;
		GenerateTextures(heightTexture: true, normalTexture: false);
		HeightTexture.Apply(updateMipmaps: false, makeNoLongerReadable: false);
		HeightTexture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
	}

	public bool TrySampleColliderHeight01(int layerMask, Vector3 terrainPos, Vector3 terrainSize, float normX, float normZ, out float height01, Collider filterCollider = null)
	{
		int num = Mathf.Max(1, ColliderSamplesPerAxis);
		float num2 = ((num > 1) ? (ColliderSampleSpread / (float)(res - 1) / (float)num) : 0f);
		float num3 = 0.5f * (float)(num - 1) * num2;
		bool flag = false;
		float num4 = 1f;
		for (int i = 0; i < num; i++)
		{
			float num5 = (float)i * num2 - num3;
			for (int j = 0; j < num; j++)
			{
				float num6 = (float)j * num2 - num3;
				float num7 = Mathf.Clamp01(normX + num6);
				float num8 = Mathf.Clamp01(normZ + num5);
				if (Physics.Raycast(new Vector3(terrainPos.x + terrainSize.x * num7, terrainPos.y + terrainSize.y + 1f, terrainPos.z + terrainSize.z * num8), Vector3.down, out var hitInfo, terrainSize.y + 2f, layerMask) && (!(filterCollider != null) || !(hitInfo.collider != filterCollider)))
				{
					float num9 = Mathf.Clamp01((hitInfo.point.y - terrainPos.y) / terrainSize.y);
					if (!flag || num9 < num4)
					{
						num4 = num9;
					}
					flag = true;
				}
			}
		}
		height01 = num4;
		return flag;
	}

	public void GenerateTextures(bool heightTexture = true, bool normalTexture = true, bool useRGBA32 = false)
	{
		if (heightTexture)
		{
			if (useRGBA32)
			{
				HeightTexture = new Texture2D(res, res, TextureFormat.RGBA32, mipChain: false, linear: true);
				HeightTexture.name = "HeightTexture";
				HeightTexture.wrapMode = TextureWrapMode.Clamp;
				NativeArray<Color32> heights = HeightTexture.GetPixelData<Color32>(0);
				Parallel.For(0, res, delegate(int z)
				{
					for (int k = 0; k < res; k++)
					{
						heights[z * res + k] = BitUtility.EncodeShort(src[z * res + k]);
					}
				});
				HeightTexture.ignoreMipmapLimit = true;
				_generatedHeightTexture = Application.isPlaying;
			}
			else
			{
				HeightTexture = new Texture2D(res, res, GraphicsFormat.R16G16_UNorm, 0, TextureCreationFlags.None);
				HeightTexture.name = "HeightTexture";
				HeightTexture.wrapMode = TextureWrapMode.Clamp;
				NativeArray<RG16> heights2 = HeightTexture.GetPixelData<RG16>(0);
				float[,] terrainHeights = terrainData.GetHeights(0, 0, res, res);
				Parallel.For(0, res, delegate(int z)
				{
					for (int j = 0; j < res; j++)
					{
						heights2[z * res + j] = new RG16(BitUtility.Float2UShort(BitUtility.Short2Float(src[z * res + j])), (ushort)(terrainHeights[z, j] * 65535f + 0.5f));
					}
				});
				HeightTexture.ignoreMipmapLimit = true;
				_generatedHeightTexture = Application.isPlaying;
			}
		}
		if (!normalTexture)
		{
			return;
		}
		int normalres = (res - 1) / 2;
		NormalTexture = new Texture2D(normalres, normalres, TextureFormat.RGBA32, mipChain: false, linear: true);
		NormalTexture.name = "NormalTexture";
		NormalTexture.wrapMode = TextureWrapMode.Clamp;
		NativeArray<Color32> normals = NormalTexture.GetPixelData<Color32>(0);
		Parallel.For(0, normalres, delegate(int z)
		{
			float normZ = ((float)z + 0.5f) / (float)normalres;
			for (int i = 0; i < normalres; i++)
			{
				float normX = ((float)i + 0.5f) / (float)normalres;
				Vector3 normal = GetNormal(normX, normZ);
				float value = Vector3.Angle(Vector3.up, normal);
				float t = Mathf.InverseLerp(50f, 70f, value);
				normal = Vector3.Slerp(normal, Vector3.up, t);
				normals[z * normalres + i] = BitUtility.EncodeNormal(normal);
			}
		});
		_generatedNormalTexture = Application.isPlaying;
	}

	public void ApplyTextures()
	{
		HeightTexture.Apply(updateMipmaps: false, makeNoLongerReadable: false);
		NormalTexture.Apply(updateMipmaps: true, makeNoLongerReadable: false);
		NormalTexture.Compress(highQuality: false);
		HeightTexture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
		NormalTexture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
	}

	public float GetHeight(Vector3 worldPos)
	{
		if (DeepSeaManager.IsInsideDeepSea(worldPos))
		{
			float normX = DeepSeaManager.NormalizeX(worldPos.x);
			float normZ = DeepSeaManager.NormalizeZ(worldPos.z);
			return GetHeight(normX, normZ, deepSeaHeights.AsReadOnly());
		}
		float normX2 = TerrainMeta.NormalizeX(worldPos.x);
		float normZ2 = TerrainMeta.NormalizeZ(worldPos.z);
		return GetHeight(normX2, normZ2, src.AsReadOnly());
	}

	public JobHandle GetHeights(NativeArray<Vector3>.ReadOnly worldPos, NativeArray<float> results, JobHandle inputDeps = default(JobHandle))
	{
		GetHeightsJob getHeightsJob = default(GetHeightsJob);
		getHeightsJob.Heights = results;
		getHeightsJob.Pos = worldPos;
		getHeightsJob.HeightMapData = new HeightMapData
		{
			Data = src.AsReadOnly(),
			DeepSeaData = deepSeaHeights.AsReadOnly(),
			DeepSeaBounds = DeepSeaManager.DeepSeaBounds,
			Res = res,
			TerrainPos = TerrainMeta.Position,
			TerrainScale = TerrainMeta.Size.y,
			TerrainOneOverSize = TerrainMeta.OneOverSize.XZ2D(),
			NormY = normY
		};
		GetHeightsJob jobData = getHeightsJob;
		return ParallelJobEx.ScheduleParallel(ref jobData, worldPos.Length, inputDeps);
	}

	public void GetHeightsIndirect(NativeArray<Vector3>.ReadOnly worldPos, NativeArray<int>.ReadOnly indices, NativeArray<float> results)
	{
		GetHeightsJobIndirect getHeightsJobIndirect = default(GetHeightsJobIndirect);
		getHeightsJobIndirect.Heights = results;
		getHeightsJobIndirect.Pos = worldPos;
		getHeightsJobIndirect.Indices = indices;
		getHeightsJobIndirect.HeightMapData = new HeightMapData
		{
			Data = src.AsReadOnly(),
			DeepSeaData = deepSeaHeights.AsReadOnly(),
			DeepSeaBounds = DeepSeaManager.DeepSeaBounds,
			Res = res,
			TerrainPos = TerrainMeta.Position,
			TerrainScale = TerrainMeta.Size.y,
			TerrainOneOverSize = TerrainMeta.OneOverSize.XZ2D(),
			NormY = normY
		};
		GetHeightsJobIndirect jobData = getHeightsJobIndirect;
		IJobExtensions.RunByRef(ref jobData);
	}

	public float GetHeight(float normX, float normZ)
	{
		return GetHeight(normX, normZ, src.AsReadOnly());
	}

	public float GetHeight(float normX, float normZ, NativeArray<short>.ReadOnly data)
	{
		return TerrainMeta.Position.y + GetHeight01(normX, normZ, data) * TerrainMeta.Size.y;
	}

	public float GetHeight(Vector2 uv)
	{
		return TerrainMeta.Position.y + GetHeight01(uv, src.AsReadOnly()) * TerrainMeta.Size.y;
	}

	public float GetHeight(Vector2 uv, NativeArray<short>.ReadOnly data)
	{
		return TerrainMeta.Position.y + GetHeight01(uv, data) * TerrainMeta.Size.y;
	}

	public HeightMapQueryStructure GetQueryStructure(bool isForDeepSea)
	{
		NativeArray<short>.ReadOnly data = (isForDeepSea ? deepSeaHeights.AsReadOnly() : src.AsReadOnly());
		Bounds deepSeaBounds = DeepSeaManager.DeepSeaBounds;
		Vector3 min = deepSeaBounds.min;
		Vector2 vector = new Vector2(1f / deepSeaBounds.size.x, 1f / deepSeaBounds.size.z);
		Vector3 terrainPosition = (isForDeepSea ? min : TerrainMeta.Position);
		Vector2 vector2 = (isForDeepSea ? vector : TerrainMeta.OneOverSize.XZ2D());
		HeightMapQueryStructure result = default(HeightMapQueryStructure);
		result.Data = data;
		result.TerrainPosition = terrainPosition;
		result.TerrainOneOverSize = vector2;
		result.Res = res;
		result.NormY = normY;
		result.HeightOffset = TerrainMeta.Position.y;
		result.HeightScale = TerrainMeta.Size.y;
		return result;
	}

	public float GetHeight(int x, int z)
	{
		return TerrainMeta.Position.y + GetHeight01(x, z) * TerrainMeta.Size.y;
	}

	public float GetHeight(int x, int z, NativeArray<short>.ReadOnly data)
	{
		return TerrainMeta.Position.y + GetHeight01(x, z, data) * TerrainMeta.Size.y;
	}

	public void GetHeightsIndirect(NativeArray<Vector2>.ReadOnly uvs, NativeArray<int>.ReadOnly indices, NativeArray<float> results)
	{
		GetHeightsIndirect(uvs, src.AsReadOnly(), indices, results);
	}

	public void GetHeightsIndirect(NativeArray<Vector2>.ReadOnly uvs, NativeArray<short>.ReadOnly data, NativeArray<int>.ReadOnly indices, NativeArray<float> results)
	{
		GetHeightsByUVJobIndirect getHeightsByUVJobIndirect = default(GetHeightsByUVJobIndirect);
		getHeightsByUVJobIndirect.Heights = results;
		getHeightsByUVJobIndirect.UVs = uvs;
		getHeightsByUVJobIndirect.Indices = indices;
		getHeightsByUVJobIndirect.HeightMapData = new HeightMapData
		{
			Data = src.AsReadOnly(),
			DeepSeaData = deepSeaHeights.AsReadOnly(),
			DeepSeaBounds = DeepSeaManager.DeepSeaBounds,
			Res = res,
			TerrainPos = TerrainMeta.Position,
			TerrainScale = TerrainMeta.Size.y,
			TerrainOneOverSize = TerrainMeta.OneOverSize.XZ2D(),
			NormY = normY
		};
		getHeightsByUVJobIndirect.Data = data;
		GetHeightsByUVJobIndirect jobData = getHeightsByUVJobIndirect;
		IJobExtensions.RunByRef(ref jobData);
	}

	public float GetHeight01(float normX, float normZ)
	{
		return GetHeight01(normX, normZ, src.AsReadOnly());
	}

	public float GetHeight01(float normX, float normZ, NativeArray<short>.ReadOnly data)
	{
		return HeightMapData.GetHeight01(new Vector2(normX, normZ), data, res);
	}

	public float GetHeight01(Vector2 uv, NativeArray<short>.ReadOnly data)
	{
		return HeightMapData.GetHeight01(uv, data, res);
	}

	public float GetHeight01(int x, int z)
	{
		return BitUtility.Short2Float(src[z * res + x]);
	}

	public float GetHeight01(int x, int z, NativeArray<short>.ReadOnly data)
	{
		return BitUtility.Short2Float(data[z * res + x]);
	}

	private float GetSrcHeight01(int x, int z)
	{
		return BitUtility.Short2Float(src[z * res + x]);
	}

	private float GetDstHeight01(int x, int z)
	{
		return BitUtility.Short2Float(dst[z * res + x]);
	}

	public Vector3 GetNormal(Vector3 worldPos)
	{
		if (DeepSeaManager.IsInsideDeepSea(worldPos))
		{
			float normX = DeepSeaManager.NormalizeX(worldPos.x);
			float normZ = DeepSeaManager.NormalizeZ(worldPos.z);
			return GetNormal(normX, normZ, deepSeaHeights.AsReadOnly());
		}
		float normX2 = TerrainMeta.NormalizeX(worldPos.x);
		float normZ2 = TerrainMeta.NormalizeZ(worldPos.z);
		return GetNormal(normX2, normZ2, src.AsReadOnly());
	}

	public JobHandle GetNormalsIndirect(NativeArray<Vector3>.ReadOnly worldPos, NativeArray<Vector3> results, NativeArray<int>.ReadOnly indices, JobHandle dependsOn = default(JobHandle))
	{
		GetNormalsJobIndirect getNormalsJobIndirect = default(GetNormalsJobIndirect);
		getNormalsJobIndirect.Normals = results;
		getNormalsJobIndirect.Pos = worldPos;
		getNormalsJobIndirect.Indices = indices;
		getNormalsJobIndirect.HeightMapData = new HeightMapData
		{
			Data = src.AsReadOnly(),
			DeepSeaData = deepSeaHeights.AsReadOnly(),
			DeepSeaBounds = DeepSeaManager.DeepSeaBounds,
			Res = res,
			TerrainPos = TerrainMeta.Position,
			TerrainScale = TerrainMeta.Size.y,
			TerrainOneOverSize = TerrainMeta.OneOverSize.XZ2D(),
			NormY = normY
		};
		GetNormalsJobIndirect jobData = getNormalsJobIndirect;
		return IJobExtensions.ScheduleByRef(ref jobData, dependsOn);
	}

	public JobHandle GetNormalsIndirect(NativeArray<Vector3>.ReadOnly worldPos, NativeArray<Vector3> results, NativeArray<int> deferredIndices, JobHandle dependsOn = default(JobHandle))
	{
		GetNormalsJobIndirectDeferred getNormalsJobIndirectDeferred = default(GetNormalsJobIndirectDeferred);
		getNormalsJobIndirectDeferred.Normals = results;
		getNormalsJobIndirectDeferred.Pos = worldPos;
		getNormalsJobIndirectDeferred.Indices = deferredIndices;
		getNormalsJobIndirectDeferred.HeightMapData = new HeightMapData
		{
			Data = src.AsReadOnly(),
			DeepSeaData = deepSeaHeights.AsReadOnly(),
			DeepSeaBounds = DeepSeaManager.DeepSeaBounds,
			Res = res,
			TerrainPos = TerrainMeta.Position,
			TerrainScale = TerrainMeta.Size.y,
			TerrainOneOverSize = TerrainMeta.OneOverSize.XZ2D(),
			NormY = normY
		};
		GetNormalsJobIndirectDeferred jobData = getNormalsJobIndirectDeferred;
		return IJobExtensions.ScheduleByRef(ref jobData, dependsOn);
	}

	public Vector3 GetNormal(float normX, float normZ)
	{
		return GetNormal(normX, normZ, src.AsReadOnly());
	}

	public Vector3 GetNormal(float normX, float normZ, NativeArray<short>.ReadOnly data)
	{
		return HeightMapData.GetNormal(new Vector2(normX, normZ), normY, data, res);
	}

	public Vector3 GetNormal(int x, int z)
	{
		return GetNormal(x, z, src.AsReadOnly());
	}

	public Vector3 GetNormal(int x, int z, NativeArray<short>.ReadOnly data)
	{
		return HeightMapData.GetNormal(x, z, normY, data, res);
	}

	public float GetSlope(Vector3 worldPos)
	{
		return Vector3.Angle(Vector3.up, GetNormal(worldPos));
	}

	public float GetSlope(float normX, float normZ)
	{
		return Vector3.Angle(Vector3.up, GetNormal(normX, normZ));
	}

	public float GetSlope(int x, int z)
	{
		return Vector3.Angle(Vector3.up, GetNormal(x, z));
	}

	public float GetSlope01(Vector3 worldPos)
	{
		return GetSlope(worldPos) * (1f / 90f);
	}

	public float GetSlope01(float normX, float normZ)
	{
		return GetSlope(normX, normZ) * (1f / 90f);
	}

	public float GetSlope01(int x, int z)
	{
		return GetSlope(x, z) * (1f / 90f);
	}

	public void SetHeight(Vector3 worldPos, float height)
	{
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		SetHeight(normX, normZ, height);
	}

	public void SetHeight(float normX, float normZ, float height)
	{
		int x = Index(normX);
		int z = Index(normZ);
		SetHeight(x, z, height);
	}

	public void SetHeight(int x, int z, float height)
	{
		dst[z * res + x] = BitUtility.Float2Short(height);
	}

	public void SetHeight(Vector3 worldPos, float height, float opacity)
	{
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		SetHeight(normX, normZ, height, opacity);
	}

	public void SetHeight(float normX, float normZ, float height, float opacity)
	{
		int x = Index(normX);
		int z = Index(normZ);
		SetHeight(x, z, height, opacity);
	}

	public void SetHeight(int x, int z, float height, float opacity)
	{
		float height2 = Mathf.SmoothStep(GetSrcHeight01(x, z), height, opacity);
		SetHeight(x, z, height2);
	}

	public void AddHeight(Vector3 worldPos, float delta)
	{
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		AddHeight(normX, normZ, delta);
	}

	public void AddHeight(float normX, float normZ, float delta)
	{
		int x = Index(normX);
		int z = Index(normZ);
		AddHeight(x, z, delta);
	}

	public void AddHeight(int x, int z, float delta)
	{
		float height = Mathf.Clamp01(GetDstHeight01(x, z) + delta);
		SetHeight(x, z, height);
	}

	public float GetAddHeight(int x, int z, float delta)
	{
		float num = Mathf.Clamp01(GetDstHeight01(x, z) + delta);
		SetHeight(x, z, num);
		return num;
	}

	public void LowerHeight(Vector3 worldPos, float height, float opacity)
	{
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		LowerHeight(normX, normZ, height, opacity);
	}

	public void LowerHeight(float normX, float normZ, float height, float opacity)
	{
		int x = Index(normX);
		int z = Index(normZ);
		LowerHeight(x, z, height, opacity);
	}

	public void LowerHeight(int x, int z, float height, float opacity)
	{
		float height2 = Mathf.Min(GetDstHeight01(x, z), Mathf.SmoothStep(GetSrcHeight01(x, z), height, opacity));
		SetHeight(x, z, height2);
	}

	public void RaiseHeight(Vector3 worldPos, float height, float opacity)
	{
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		RaiseHeight(normX, normZ, height, opacity);
	}

	public void RaiseHeight(float normX, float normZ, float height, float opacity)
	{
		int x = Index(normX);
		int z = Index(normZ);
		RaiseHeight(x, z, height, opacity);
	}

	public void RaiseHeight(int x, int z, float height, float opacity)
	{
		float height2 = Mathf.Max(GetDstHeight01(x, z), Mathf.SmoothStep(GetSrcHeight01(x, z), height, opacity));
		SetHeight(x, z, height2);
	}

	public void SetHeight(Vector3 worldPos, float opacity, float radius, float fade = 0f)
	{
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		float height = TerrainMeta.NormalizeY(worldPos.y);
		SetHeight(normX, normZ, height, opacity, radius, fade);
	}

	public void SetHeight(float normX, float normZ, float height, float opacity, float radius, float fade = 0f)
	{
		Action<int, int, float> action = delegate(int x, int z, float lerp)
		{
			if (lerp > 0f)
			{
				SetHeight(x, z, height, lerp * opacity);
			}
		};
		ApplyFilter(normX, normZ, radius, fade, action);
	}

	public void LowerHeight(Vector3 worldPos, float opacity, float radius, float fade = 0f)
	{
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		float height = TerrainMeta.NormalizeY(worldPos.y);
		LowerHeight(normX, normZ, height, opacity, radius, fade);
	}

	public void LowerHeight(float normX, float normZ, float height, float opacity, float radius, float fade = 0f)
	{
		Action<int, int, float> action = delegate(int x, int z, float lerp)
		{
			if (lerp > 0f)
			{
				LowerHeight(x, z, height, lerp * opacity);
			}
		};
		ApplyFilter(normX, normZ, radius, fade, action);
	}

	public void RaiseHeight(Vector3 worldPos, float opacity, float radius, float fade = 0f)
	{
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		float height = TerrainMeta.NormalizeY(worldPos.y);
		RaiseHeight(normX, normZ, height, opacity, radius, fade);
	}

	public void RaiseHeight(float normX, float normZ, float height, float opacity, float radius, float fade = 0f)
	{
		Action<int, int, float> action = delegate(int x, int z, float lerp)
		{
			if (lerp > 0f)
			{
				RaiseHeight(x, z, height, lerp * opacity);
			}
		};
		ApplyFilter(normX, normZ, radius, fade, action);
	}

	public void AddHeight(Vector3 worldPos, float delta, float radius, float fade = 0f)
	{
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		AddHeight(normX, normZ, delta, radius, fade);
	}

	public void AddHeight(float normX, float normZ, float delta, float radius, float fade = 0f)
	{
		Action<int, int, float> action = delegate(int x, int z, float lerp)
		{
			if (lerp > 0f)
			{
				AddHeight(x, z, lerp * delta);
			}
		};
		ApplyFilter(normX, normZ, radius, fade, action);
	}

	public void AddHeightArea(float[,] subHeights, int subDimensions, Vector3 worldPos, float delta, float radius, float fade = 0f)
	{
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		ApplyFilterSubHeights(subHeights, subDimensions, normX, normZ, radius, fade, delta);
	}

	public void ApplyFilterSubHeights(float[,] subHeights, int subDimensions, float normX, float normZ, float radius, float fade, float delta)
	{
		float num = TerrainMeta.OneOverSize.x * radius;
		float num2 = TerrainMeta.OneOverSize.x * fade;
		float num3 = (float)res * (num - num2);
		float num4 = (float)res * num;
		float num5 = normX * (float)res;
		float num6 = normZ * (float)res;
		int num7 = Index(normX - num);
		int num8 = Index(normX + num);
		int num9 = Index(normZ - num);
		int num10 = Index(normZ + num);
		Debug.Assert(num8 - num7 <= subDimensions);
		Debug.Assert(num10 - num9 <= subDimensions);
		int num11 = (num8 - num7) / 2 + num7;
		int num12 = (num10 - num9) / 2 + num9;
		int num13 = subDimensions / 2;
		for (int i = 0; i < subDimensions; i++)
		{
			for (int j = 0; j < subDimensions; j++)
			{
				subHeights[i, j] = GetHeight01(i + num7, j + num9);
				MonoBehaviour.print($"Sub Height {i},{j} = {subHeights[i, j]}");
			}
		}
		if (num3 != num4)
		{
			for (int k = num9; k <= num10; k++)
			{
				for (int l = num7; l <= num8; l++)
				{
					float magnitude = new Vector2((float)l + 0.5f - num5, (float)k + 0.5f - num6).magnitude;
					float num14 = Mathf.InverseLerp(num4, num3, magnitude);
					if (num14 > 0f)
					{
						subHeights[l - num11 + num13, k - num12 + num13] = GetAddHeight(l, k, num14 * delta);
					}
					else
					{
						subHeights[l - num11 + num13, k - num12 + num13] = GetHeight01(l, k);
					}
				}
			}
			return;
		}
		for (int m = num9; m <= num10; m++)
		{
			for (int n = num7; n <= num8; n++)
			{
				float num15 = ((new Vector2((float)n + 0.5f - num5, (float)m + 0.5f - num6).magnitude < num4) ? 1 : 0);
				if (num15 > 0f)
				{
					subHeights[n - num11 + num13, m - num12 + num13] = GetAddHeight(n, m, num15 * delta);
				}
				else
				{
					subHeights[n - num11 + num13, m - num12 + num13] = GetHeight(n, m);
				}
			}
		}
	}

	public HeightSampler CreateSampler(bool deepSea)
	{
		if (!deepSea)
		{
			return HeightSampler.Create(src, res, TerrainMeta.Position, TerrainMeta.Size, TerrainMeta.Position.y, TerrainMeta.Size.y);
		}
		return HeightSampler.Create(deepSeaHeights, res, DeepSeaManager.DeepSeaBounds.min, DeepSeaManager.DeepSeaBounds.size, TerrainMeta.Position.y, TerrainMeta.Size.y);
	}
}
