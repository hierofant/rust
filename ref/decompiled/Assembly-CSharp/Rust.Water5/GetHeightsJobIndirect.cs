using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Rust.Water5;

[BurstCompile]
internal struct GetHeightsJobIndirect : IJob
{
	[WriteOnly]
	public NativeArray<float> Heights;

	[Unity.Collections.ReadOnly]
	public NativeArray<Vector3>.ReadOnly Pos;

	[Unity.Collections.ReadOnly]
	public NativeArray<float>.ReadOnly ShoreDists;

	[Unity.Collections.ReadOnly]
	public NativeArray<float>.ReadOnly TerrainHeights;

	[Unity.Collections.ReadOnly]
	public NativeArray<int>.ReadOnly Indices;

	[Unity.Collections.ReadOnly]
	public float OneOverOctave0Scale;

	[Unity.Collections.ReadOnly]
	public Rust.Water5.NativeOceanDisplacementShort3 SimData;

	[Unity.Collections.ReadOnly]
	public int Spectrum0;

	[Unity.Collections.ReadOnly]
	public int Spectrum1;

	[Unity.Collections.ReadOnly]
	public int Frame0;

	[Unity.Collections.ReadOnly]
	public int Frame1;

	[Unity.Collections.ReadOnly]
	public float SpectrumBlend;

	[Unity.Collections.ReadOnly]
	public float FrameBlend;

	[Unity.Collections.ReadOnly]
	public float DistanceAttenuationFactor;

	[Unity.Collections.ReadOnly]
	public float DepthAttenuationFactor;

	public void Execute()
	{
		for (int i = 0; i < Indices.Length; i++)
		{
			int index = Indices[i];
			float heightRaw = GetHeightRaw(Pos[index]);
			Heights[index] = heightRaw * GetHeightAttenuation(ShoreDists[index], TerrainHeights[index]);
		}
	}

	private float GetHeightRaw(float3 position)
	{
		float3 zero = float3.zero;
		zero = GetDisplacement(position);
		zero = GetDisplacement(position - zero);
		zero = GetDisplacement(position - zero);
		return GetDisplacement(position - zero).y;
	}

	private Vector3 GetDisplacement(float3 position)
	{
		float normX = position.x * OneOverOctave0Scale;
		float normZ = position.z * OneOverOctave0Scale;
		return GetDisplacement(normX, normZ);
	}

	private float3 GetDisplacement(float normX, float normZ)
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
		float3 displacementFromSimData = GetDisplacementFromSimData(x, z);
		float3 displacementFromSimData2 = GetDisplacementFromSimData(x2, z);
		float3 displacementFromSimData3 = GetDisplacementFromSimData(x, z2);
		float3 displacementFromSimData4 = GetDisplacementFromSimData(x2, z2);
		float3 start = math.lerp(displacementFromSimData, displacementFromSimData2, t);
		float3 end = math.lerp(displacementFromSimData3, displacementFromSimData4, t);
		return math.lerp(start, end, t2);
	}

	private float3 GetDisplacementFromSimData(int x, int z)
	{
		int z2 = x * 256 + z;
		float3 start = math.lerp(SimData[Spectrum0, Frame0, z2], SimData[Spectrum1, Frame0, z2], SpectrumBlend);
		float3 end = math.lerp(SimData[Spectrum0, Frame1, z2], SimData[Spectrum1, Frame1, z2], SpectrumBlend);
		return math.lerp(start, end, FrameBlend);
	}

	public float GetHeightAttenuation(float shore, float terrainHeight)
	{
		float num = Mathf.Clamp01(shore / DistanceAttenuationFactor);
		float num2 = Mathf.Clamp01(Mathf.Abs(terrainHeight) / DepthAttenuationFactor);
		return num * num2;
	}
}
