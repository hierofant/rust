using Rust.Water5;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace OceanSimulationJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
internal struct OceanTraceJob : IJobParallelForDefer
{
	public const int maxSteps = 16;

	public const int maxBinarySteps = 16;

	public const float intersectionThreshold = 0.1f;

	public float SeaLevel;

	public float MaxDisplacement;

	[Unity.Collections.ReadOnly]
	public NativeList<int> Indices;

	[Unity.Collections.ReadOnly]
	public NativeArray<Ray> Rays;

	public NativeArray<float>.ReadOnly MaxDists;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<bool> HitResults;

	[NativeDisableParallelForRestriction]
	[WriteOnly]
	public NativeArray<Vector3> HitPositions;

	public float OneOverOctave0Scale;

	[NativeDisableParallelForRestriction]
	[Unity.Collections.ReadOnly]
	public Rust.Water5.NativeOceanDisplacementShort3 SimData;

	public int Spectrum0;

	public int Spectrum1;

	public int Frame0;

	public int Frame1;

	public float spectrumBlend;

	public float frameBlend;

	public TerrainHeightMap.HeightMapQueryStructure HeightMapQueryStructure;

	public TerrainTexturing.ShoreVectorQueryStructure ShoreVectorQueryStructure;

	public float distanceAttenuationFactor;

	public float depthAttenuationFactor;

	public void Execute(int indicesIndex)
	{
		int index = Indices[indicesIndex];
		Ray ray = Rays[index];
		float maxDist = MaxDists[index];
		Vector3 result;
		bool value = Trace(in ray, maxDist, out result);
		HitResults[index] = value;
		HitPositions[index] = result;
	}

	private bool Trace(in Ray ray, float maxDist, out Vector3 result)
	{
		float num = 0f - MaxDisplacement;
		Vector3 point = ray.GetPoint(maxDist);
		if (ray.origin.y > MaxDisplacement + SeaLevel && point.y > MaxDisplacement + SeaLevel)
		{
			result = Vector3.zero;
			return false;
		}
		if (ray.origin.y < num + SeaLevel && point.y < num + SeaLevel)
		{
			result = Vector3.zero;
			return false;
		}
		Vector3 vector = ray.origin;
		Vector3 direction = ray.direction;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 2f / (math.abs(direction.y) + 1f);
		result = vector;
		if (direction.y <= -0.99f)
		{
			result.y = GetHeight(vector);
			return math.lengthsq(result - vector) < maxDist * maxDist;
		}
		if (vector.y >= MaxDisplacement + SeaLevel)
		{
			num3 = (num2 = (0f - (vector.y - MaxDisplacement - SeaLevel)) / direction.y);
			vector += num2 * direction;
			if (num3 >= maxDist)
			{
				result = Vector3.zero;
				return false;
			}
		}
		int num5 = 0;
		while (true)
		{
			float height = GetHeight(vector);
			num2 = num4 * Mathf.Abs(vector.y - height - SeaLevel);
			vector += num2 * direction;
			num3 += num2;
			if (num5 >= 16 || num2 < 0.1f)
			{
				break;
			}
			if (num3 >= maxDist)
			{
				return false;
			}
			num5++;
		}
		if (num2 < 0.1f && num3 >= 0f)
		{
			result = vector;
			return true;
		}
		if (direction.y < 0f)
		{
			num2 = (0f - (vector.y + MaxDisplacement - SeaLevel)) / direction.y;
			Vector3 vector2 = vector;
			Vector3 vector3 = vector + num2 * ray.direction;
			for (int i = 0; i < 16; i++)
			{
				vector = (vector2 + vector3) * 0.5f;
				float height2 = GetHeight(vector);
				if (vector.y - height2 - SeaLevel > 0f)
				{
					vector2 = vector;
				}
				else
				{
					vector3 = vector;
				}
				if (math.abs(vector.y - height2) < 0.1f)
				{
					vector.y = height2;
					break;
				}
			}
			result = vector;
			return true;
		}
		return false;
	}

	private float GetHeight(Vector3 point)
	{
		return GetHeightRaw(point) * GetHeightAttenuation(point);
	}

	public float GetHeightAttenuation(Vector3 position)
	{
		float x = HeightMapQueryStructure.TerrainPosition.x;
		float z = HeightMapQueryStructure.TerrainPosition.z;
		float x2 = HeightMapQueryStructure.TerrainOneOverSize.x;
		float z2 = HeightMapQueryStructure.TerrainOneOverSize.z;
		float x3 = (position.x - x) * x2;
		float y = (position.z - z) * z2;
		Vector2 uv = new Vector2(x3, y);
		float coarseDistanceToShore = ShoreVectorQueryStructure.GetCoarseDistanceToShore(position);
		float heightFromUV = HeightMapQueryStructure.GetHeightFromUV(uv);
		float num = Mathf.Clamp01(coarseDistanceToShore / distanceAttenuationFactor);
		float num2 = Mathf.Clamp01(Mathf.Abs(heightFromUV) / depthAttenuationFactor);
		return num * num2;
	}

	private float GetHeightRaw(Vector3 position)
	{
		Vector3 zero = Vector3.zero;
		zero = GetDisplacement(position);
		zero = GetDisplacement(position - zero);
		zero = GetDisplacement(position - zero);
		return GetDisplacement(position - zero).y;
	}

	private Vector3 GetDisplacement(Vector3 position)
	{
		float normX = position.x * OneOverOctave0Scale;
		float normZ = position.z * OneOverOctave0Scale;
		return GetDisplacement(normX, normZ);
	}

	private Vector3 GetDisplacement(float normX, float normZ)
	{
		normX -= math.floor(normX);
		normZ -= math.floor(normZ);
		float num = normX * 256f - 0.5f;
		float num2 = normZ * 256f - 0.5f;
		int num3 = (int)math.floor(num);
		int num4 = (int)math.floor(num2);
		float t = num - (float)num3;
		float t2 = num2 - (float)num4;
		int num5 = num3 % 256;
		int num6 = num4 % 256;
		int x = (num5 + 256) % 256;
		int z = (num6 + 256) % 256;
		int x2 = (num5 + 1 + 256) % 256;
		int z2 = (num6 + 1 + 256) % 256;
		Vector3 displacementFromSimData = GetDisplacementFromSimData(x, z);
		Vector3 displacementFromSimData2 = GetDisplacementFromSimData(x2, z);
		Vector3 displacementFromSimData3 = GetDisplacementFromSimData(x, z2);
		Vector3 displacementFromSimData4 = GetDisplacementFromSimData(x2, z2);
		float3 start = math.lerp(displacementFromSimData, displacementFromSimData2, t);
		float3 end = math.lerp(displacementFromSimData3, displacementFromSimData4, t);
		return math.lerp(start, end, t2);
	}

	private Vector3 GetDisplacementFromSimData(int x, int z)
	{
		int z2 = x * 256 + z;
		float3 start = math.lerp(SimData[Spectrum0, Frame0, z2], SimData[Spectrum1, Frame0, z2], spectrumBlend);
		float3 end = math.lerp(SimData[Spectrum0, Frame1, z2], SimData[Spectrum1, Frame1, z2], spectrumBlend);
		return math.lerp(start, end, frameBlend);
	}
}
