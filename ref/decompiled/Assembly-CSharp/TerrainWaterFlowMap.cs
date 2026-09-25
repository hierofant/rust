using System;
using System.Threading.Tasks;
using Facepunch;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class TerrainWaterFlowMap : TerrainMap<byte>
{
	private const float TwoPi = MathF.PI * 2f;

	public override void Setup()
	{
		res = terrainData.heightmapResolution;
		InitArrays(res * res);
	}

	public override void PostSetup()
	{
		using (TimeWarning.New("TerrainWaterFlowMap.PostSetup"))
		{
			WriteWaterFlowFromShoreVectors();
			WriteWaterFlowFromRivers();
		}
	}

	private void WriteWaterFlowFromShoreVectors()
	{
		NativeArray<Vector2> normalizedCoords = new NativeArray<Vector2>(res * res, Allocator.TempJob);
		NativeArray<float> radii = new NativeArray<float>(res * res, Allocator.TempJob);
		NativeArray<int> topologies = new NativeArray<int>(res * res, Allocator.TempJob);
		Parallel.For(0, res, delegate(int z)
		{
			float y = Coordinate(z);
			for (int j = 0; j < res; j++)
			{
				float x = Coordinate(j);
				normalizedCoords[z * res + j] = new Vector2(x, y);
				radii[z * res + j] = 16f;
			}
		});
		TerrainMeta.TopologyMap.GetTopologiesIndirect(normalizedCoords.AsReadOnly(), radii.AsReadOnly(), topologies);
		TerrainTexturing.ShoreData shoreMap = TerrainTexturing.Instance.GetMap(isDeepSea: false);
		Parallel.For(0, res, delegate(int z)
		{
			float num = Coordinate(z);
			for (int i = 0; i < res; i++)
			{
				float num2 = Coordinate(i);
				int num3 = topologies[z * res + i];
				Vector4 rawShoreVector = shoreMap.GetRawShoreVector(new Vector2(num2, num));
				Vector3 flow = new Vector3(rawShoreVector.x, 0f, rawShoreVector.y);
				if (((uint)num3 & 0x14080u) != 0)
				{
					SetFlowDirection(num2, num, flow);
				}
			}
		});
		normalizedCoords.Dispose(default(JobHandle));
		radii.Dispose(default(JobHandle));
		topologies.Dispose(default(JobHandle));
	}

	private void WriteWaterFlowFromRivers()
	{
		foreach (PathList river in TerrainMeta.Path.Rivers)
		{
			river.AdjustTerrainWaterFlow(scaleWidthWithLength: true);
		}
	}

	public Vector3 GetFlowDirection(Vector3 worldPos)
	{
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		return GetFlowDirection(normX, normZ);
	}

	public Vector3 GetFlowDirection(Vector2 worldPos2D)
	{
		float normX = TerrainMeta.NormalizeX(worldPos2D.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos2D.y);
		return GetFlowDirection(normX, normZ);
	}

	public Vector3 GetFlowDirection(float normX, float normZ)
	{
		int num = Index(normX);
		int num2 = Index(normZ);
		float f = ByteToAngle(src[num2 * res + num]);
		return new Vector3(Mathf.Sin(f), 0f, Mathf.Cos(f));
	}

	public void SetFlowDirection(Vector3 worldPos, Vector3 flow)
	{
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		SetFlowDirection(normX, normZ, flow);
	}

	public void SetFlowDirection(float normX, float normZ, Vector3 flow)
	{
		int num = Index(normX);
		int num2 = Index(normZ);
		Vector3 normalized = flow.XZ().normalized;
		byte value = AngleToByte(Mathf.Atan2(normalized.x, normalized.z));
		src[num2 * res + num] = value;
	}

	public static float ByteToAngle(byte b)
	{
		return (float)(int)b / 255f * (MathF.PI * 2f) - MathF.PI;
	}

	private static byte AngleToByte(float a)
	{
		a = Mathf.Clamp(a, -MathF.PI, MathF.PI);
		return (byte)Mathf.RoundToInt((a + MathF.PI) / (MathF.PI * 2f) * 255f);
	}

	public NativeArray<float3> GetFlowDirections(NativeArray<Vector3> positions3D, Allocator allocator)
	{
		NativeArray<float3> results = new NativeArray<float3>(positions3D.Length, allocator);
		TerrainWaterFlowMapBurst.GetFlowDirections(in positions3D, ref results, in src, in res);
		return results;
	}
}
