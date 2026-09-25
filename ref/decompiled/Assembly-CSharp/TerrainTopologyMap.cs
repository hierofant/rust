using System;
using System.Threading;
using System.Threading.Tasks;
using TerrainTopologyMapJobs;
using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

public class TerrainTopologyMap : TerrainMap<int>
{
	public struct TopologyQueryStructure
	{
		public NativeArray<int>.ReadOnly source;

		public int res;

		[BurstCompile]
		public readonly int GetTopologyFast(Vector2 uv)
		{
			if (Hint.Unlikely(!source.IsCreated))
			{
				return 0;
			}
			int num = res - 1;
			int num2 = (int)(uv.x * (float)res);
			int num3 = (int)(uv.y * (float)res);
			num2 = ((num2 >= 0) ? num2 : 0);
			num3 = ((num3 >= 0) ? num3 : 0);
			num2 = ((num2 <= num) ? num2 : num);
			num3 = ((num3 <= num) ? num3 : num);
			return source[num3 * res + num2];
		}

		public readonly bool GetTopology(float normX, float normZ, int mask)
		{
			int x = Index(normX);
			int z = Index(normZ);
			return GetTopology(x, z, mask);
		}

		public readonly int Index(float normalized)
		{
			int num = (int)(normalized * (float)res);
			if (num >= 0)
			{
				if (num <= res - 1)
				{
					return num;
				}
				return res - 1;
			}
			return 0;
		}

		public readonly bool GetTopology(int x, int z, int mask)
		{
			return (source[z * res + x] & mask) != 0;
		}
	}

	public Texture2D TopologyTexture;

	private bool _generatedTopologyTexture;

	private ThreadLocal<NativeReference<int>> topoNative = new ThreadLocal<NativeReference<int>>(() => new NativeReference<int>(0, Allocator.Persistent), trackAllValues: true);

	public override void Setup()
	{
		res = terrainData.alphamapResolution;
		InitArrays(res * res);
		if (!(TopologyTexture != null))
		{
			return;
		}
		if (TopologyTexture.width == TopologyTexture.height && TopologyTexture.width == res)
		{
			Color32[] pixels = TopologyTexture.GetPixels32();
			int i = 0;
			int num = 0;
			for (; i < res; i++)
			{
				int num2 = 0;
				while (num2 < res)
				{
					dst[i * res + num2] = BitUtility.DecodeInt(pixels[num]);
					num2++;
					num++;
				}
			}
		}
		else
		{
			Debug.LogError("Invalid topology texture: " + TopologyTexture.name);
		}
	}

	public void SetupEmpty(int newRes)
	{
		res = newRes;
		InitArrays(res * res);
	}

	public void GenerateTextures()
	{
		TopologyTexture = new Texture2D(res, res, TextureFormat.RGBA32, mipChain: false, linear: true);
		TopologyTexture.name = "TopologyTexture";
		TopologyTexture.wrapMode = TextureWrapMode.Clamp;
		NativeArray<Color32> col = TopologyTexture.GetPixelData<Color32>(0);
		Parallel.For(0, res, delegate(int z)
		{
			for (int i = 0; i < res; i++)
			{
				col[z * res + i] = BitUtility.EncodeInt(src[z * res + i]);
			}
		});
		_generatedTopologyTexture = Application.isPlaying;
	}

	public void ApplyTextures()
	{
		TopologyTexture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
	}

	public bool GetTopology(Vector3 worldPos, int mask)
	{
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		return GetTopology(normX, normZ, mask);
	}

	public bool GetTopology(float normX, float normZ, int mask)
	{
		int x = Index(normX);
		int z = Index(normZ);
		return GetTopology(x, z, mask);
	}

	public bool GetTopology(int x, int z, int mask)
	{
		return (src[z * res + x] & mask) != 0;
	}

	public int GetTopology(Vector3 worldPos)
	{
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		return GetTopology(normX, normZ);
	}

	public int GetTopology(float normX, float normZ)
	{
		int x = Index(normX);
		int z = Index(normZ);
		return GetTopology(x, z);
	}

	public int GetTopologyFast(Vector2 uv)
	{
		int num = res - 1;
		int num2 = (int)(uv.x * (float)res);
		int num3 = (int)(uv.y * (float)res);
		num2 = ((num2 >= 0) ? num2 : 0);
		num3 = ((num3 >= 0) ? num3 : 0);
		num2 = ((num2 <= num) ? num2 : num);
		num3 = ((num3 <= num) ? num3 : num);
		return src[num3 * res + num2];
	}

	public TopologyQueryStructure GetQueryStructure()
	{
		TopologyQueryStructure result = default(TopologyQueryStructure);
		result.source = src.AsReadOnly();
		result.res = res;
		return result;
	}

	public int GetTopology(int x, int z)
	{
		return src[z * res + x];
	}

	public void GetTopologies(NativeArray<Vector3> worldPos, NativeArray<int> results)
	{
		GetTopologyByPosJob getTopologyByPosJob = default(GetTopologyByPosJob);
		getTopologyByPosJob.Topologies = results;
		getTopologyByPosJob.Pos = worldPos;
		getTopologyByPosJob.Data = src;
		getTopologyByPosJob.Res = res;
		getTopologyByPosJob.DataOrigin = new Vector2(TerrainMeta.Position.x, TerrainMeta.Position.z);
		getTopologyByPosJob.DataScale = new Vector2(TerrainMeta.OneOverSize.x, TerrainMeta.OneOverSize.z);
		GetTopologyByPosJob jobData = getTopologyByPosJob;
		IJobExtensions.RunByRef(ref jobData);
	}

	public void GetTopologies(NativeArray<Vector2> uvs, NativeArray<int> results)
	{
		GetTopologyByUVJob getTopologyByUVJob = default(GetTopologyByUVJob);
		getTopologyByUVJob.Topologies = results;
		getTopologyByUVJob.UV = uvs;
		getTopologyByUVJob.Data = src;
		getTopologyByUVJob.Res = res;
		GetTopologyByUVJob jobData = getTopologyByUVJob;
		IJobExtensions.RunByRef(ref jobData);
	}

	public void GetTopologies(NativeArray<Vector2i> indices, NativeArray<int> results)
	{
		GetTopologyByIndexJob getTopologyByIndexJob = default(GetTopologyByIndexJob);
		getTopologyByIndexJob.Topologies = results;
		getTopologyByIndexJob.Indices = indices;
		getTopologyByIndexJob.Data = src;
		getTopologyByIndexJob.Res = res;
		GetTopologyByIndexJob jobData = getTopologyByIndexJob;
		IJobExtensions.RunByRef(ref jobData);
	}

	public void GetTopologiesIndirect(NativeArray<Vector2>.ReadOnly uvs, NativeArray<int>.ReadOnly indices, NativeArray<int> results)
	{
		GetTopologyByUVJobIndirect getTopologyByUVJobIndirect = default(GetTopologyByUVJobIndirect);
		getTopologyByUVJobIndirect.Topologies = results;
		getTopologyByUVJobIndirect.UV = uvs;
		getTopologyByUVJobIndirect.Indices = indices;
		getTopologyByUVJobIndirect.Data = src.AsReadOnly();
		getTopologyByUVJobIndirect.Res = res;
		GetTopologyByUVJobIndirect jobData = getTopologyByUVJobIndirect;
		IJobExtensions.RunByRef(ref jobData);
	}

	public JobHandle GetTopologiesIndirect(NativeArray<Vector3>.ReadOnly worldPositions, NativeArray<float>.ReadOnly radii, NativeArray<int> results, JobHandle inputDeps = default(JobHandle))
	{
		TerrainTopologyMapJobs.GetTopologyRadiusJobIndirect getTopologyRadiusJobIndirect = default(TerrainTopologyMapJobs.GetTopologyRadiusJobIndirect);
		getTopologyRadiusJobIndirect.WorldX = TerrainMeta.Position.x;
		getTopologyRadiusJobIndirect.WorldZ = TerrainMeta.Position.z;
		getTopologyRadiusJobIndirect.OneOverSizeX = TerrainMeta.OneOverSize.x;
		getTopologyRadiusJobIndirect.OneOverSizeZ = TerrainMeta.OneOverSize.z;
		getTopologyRadiusJobIndirect.Src = src.AsReadOnly();
		getTopologyRadiusJobIndirect.Res = res;
		getTopologyRadiusJobIndirect.WorldPositions = worldPositions;
		getTopologyRadiusJobIndirect.Radii = radii;
		getTopologyRadiusJobIndirect.Topologies = results;
		TerrainTopologyMapJobs.GetTopologyRadiusJobIndirect jobData = getTopologyRadiusJobIndirect;
		return ParallelJobEx.ScheduleParallelByRef(ref jobData, worldPositions.Length, inputDeps);
	}

	public void GetTopologiesIndirect(NativeArray<Vector2>.ReadOnly normalizedCoords, NativeArray<float>.ReadOnly radii, NativeArray<int> results)
	{
		TerrainTopologyMapJobs.GetTopologyRadiusNormalizedJobIndirect getTopologyRadiusNormalizedJobIndirect = default(TerrainTopologyMapJobs.GetTopologyRadiusNormalizedJobIndirect);
		getTopologyRadiusNormalizedJobIndirect.OneOverSizeX = TerrainMeta.OneOverSize.x;
		getTopologyRadiusNormalizedJobIndirect.Src = src.AsReadOnly();
		getTopologyRadiusNormalizedJobIndirect.Res = res;
		getTopologyRadiusNormalizedJobIndirect.WorldNXZ = normalizedCoords;
		getTopologyRadiusNormalizedJobIndirect.Radii = radii;
		getTopologyRadiusNormalizedJobIndirect.Topologies = results;
		TerrainTopologyMapJobs.GetTopologyRadiusNormalizedJobIndirect jobData = getTopologyRadiusNormalizedJobIndirect;
		ParallelJobEx.ScheduleParallelByRef(ref jobData, normalizedCoords.Length, default(JobHandle)).Complete();
	}

	public void SetTopology(Vector3 worldPos, int mask)
	{
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		SetTopology(normX, normZ, mask);
	}

	public void SetTopology(float normX, float normZ, int mask)
	{
		int x = Index(normX);
		int z = Index(normZ);
		SetTopology(x, z, mask);
	}

	public void SetTopology(int x, int z, int mask)
	{
		dst[z * res + x] = mask;
	}

	public void AddTopology(Vector3 worldPos, int mask)
	{
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		AddTopology(normX, normZ, mask);
	}

	public void AddTopology(float normX, float normZ, int mask)
	{
		int x = Index(normX);
		int z = Index(normZ);
		AddTopology(x, z, mask);
	}

	public void AddTopology(int x, int z, int mask)
	{
		dst[z * res + x] |= mask;
	}

	public void RemoveTopology(Vector3 worldPos, int mask)
	{
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		RemoveTopology(normX, normZ, mask);
	}

	public void RemoveTopology(float normX, float normZ, int mask)
	{
		int x = Index(normX);
		int z = Index(normZ);
		RemoveTopology(x, z, mask);
	}

	public void RemoveTopology(int x, int z, int mask)
	{
		dst[z * res + x] &= ~mask;
	}

	public int GetTopology(Vector3 worldPos, float radius)
	{
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		return GetTopology(normX, normZ, radius);
	}

	public int GetTopologyJob(Vector3 worldPos, float radius)
	{
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		return GetTopologyJob(normX, normZ, radius);
	}

	public int GetTopologyJob(float normX, float normZ, float radius)
	{
		float num = TerrainMeta.OneOverSize.x * radius;
		int x_mid = Index(normX);
		int z_mid = Index(normZ);
		int x_min = Index(normX - num);
		int x_max = Index(normX + num);
		int z_min = Index(normZ - num);
		int z_max = Index(normZ + num);
		NativeReference<int> value = topoNative.Value;
		TerrainTopologyMapJobs.GetTopologyRadiusJob getTopologyRadiusJob = default(TerrainTopologyMapJobs.GetTopologyRadiusJob);
		getTopologyRadiusJob.Res = res;
		getTopologyRadiusJob.Src = src.AsReadOnly();
		getTopologyRadiusJob.Topo = value;
		getTopologyRadiusJob.radius = radius;
		getTopologyRadiusJob.x_mid = x_mid;
		getTopologyRadiusJob.z_mid = z_mid;
		getTopologyRadiusJob.x_min = x_min;
		getTopologyRadiusJob.x_max = x_max;
		getTopologyRadiusJob.z_min = z_min;
		getTopologyRadiusJob.z_max = z_max;
		TerrainTopologyMapJobs.GetTopologyRadiusJob jobData = getTopologyRadiusJob;
		IJobExtensions.RunByRef(ref jobData);
		return value.Value;
	}

	public int GetTopology(float normX, float normZ, float radius)
	{
		return GetTopologyJob(normX, normZ, radius);
	}

	public void SetTopology(Vector3 worldPos, int mask, float radius, float fade = 0f)
	{
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		SetTopology(normX, normZ, mask, radius, fade);
	}

	public void SetTopology(float normX, float normZ, int mask, float radius, float fade = 0f)
	{
		Action<int, int, float> action = delegate(int x, int z, float lerp)
		{
			if ((double)lerp > 0.5)
			{
				dst[z * res + x] = mask;
			}
		};
		ApplyFilter(normX, normZ, radius, fade, action);
	}

	public void AddTopology(Vector3 worldPos, int mask, float radius, float fade = 0f)
	{
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		AddTopology(normX, normZ, mask, radius, fade);
	}

	public void AddTopology(float normX, float normZ, int mask, float radius, float fade = 0f)
	{
		Action<int, int, float> action = delegate(int x, int z, float lerp)
		{
			if ((double)lerp > 0.5)
			{
				dst[z * res + x] |= mask;
			}
		};
		ApplyFilter(normX, normZ, radius, fade, action);
	}

	public override void Dispose()
	{
		base.Dispose();
		if (_generatedTopologyTexture && TopologyTexture != null)
		{
			UnityEngine.Object.Destroy(TopologyTexture);
			TopologyTexture = null;
		}
		foreach (NativeReference<int> value in topoNative.Values)
		{
			if (value.IsCreated)
			{
				value.Dispose();
			}
		}
	}
}
