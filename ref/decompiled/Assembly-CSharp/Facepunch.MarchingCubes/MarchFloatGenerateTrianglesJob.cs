using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Facepunch.MarchingCubes;

[BurstCompile(FloatMode = FloatMode.Fast)]
internal struct MarchFloatGenerateTrianglesJob : IJobParallelForBatch
{
	[NativeDisableContainerSafetyRestriction]
	public QuantizedFloatData3DArray sampler;

	public NativeStream.Writer edgeStream;

	public float iso;

	public float3 vertexOffset;

	public float scale;

	public int batchSize;

	[SkipLocalsInit]
	public void Execute(int startIndex, int count)
	{
		int num = startIndex / batchSize;
		edgeStream.PatchMinMaxRange(num);
		edgeStream.BeginForEachIndex(num);
		Span<int3> corners = stackalloc int3[8];
		Span<int> cornerIndices = stackalloc int[8];
		Span<float> cornerSamples = stackalloc float[8];
		for (int i = startIndex; i < startIndex + count; i++)
		{
			int3 @int = new int3(i % sampler.Width, i % sampler.WidthHeight / sampler.Width, i / sampler.WidthHeight);
			if (math.any(@int > sampler.Bounds - new int3(2)))
			{
				continue;
			}
			corners[0] = @int + new int3(0, 0, 0);
			corners[1] = @int + new int3(1, 0, 0);
			corners[2] = @int + new int3(1, 0, 1);
			corners[3] = @int + new int3(0, 0, 1);
			corners[4] = @int + new int3(0, 1, 0);
			corners[5] = @int + new int3(1, 1, 0);
			corners[6] = @int + new int3(1, 1, 1);
			corners[7] = @int + new int3(0, 1, 1);
			cornerIndices[0] = i;
			cornerIndices[1] = i + 1;
			cornerIndices[2] = i + 1 + sampler.WidthHeight;
			cornerIndices[3] = i + sampler.WidthHeight;
			cornerIndices[4] = i + sampler.Width;
			cornerIndices[5] = i + 1 + sampler.Width;
			cornerIndices[6] = i + 1 + sampler.Width + sampler.WidthHeight;
			cornerIndices[7] = i + sampler.Width + sampler.WidthHeight;
			int num2 = 0;
			for (int j = 0; j < cornerIndices.Length; j++)
			{
				float num3 = sampler.Sample(cornerIndices[j]);
				cornerSamples[j] = num3;
				num2 |= math.select(0, 1 << j, num3 < iso);
			}
			int num4 = num2 * 16;
			for (int k = 0; k < 16; k += 3)
			{
				int num5 = MarchingCubeLookup.triTableFlat[num4 + k];
				if (num5 == -1)
				{
					break;
				}
				int edge = MarchingCubeLookup.triTableFlat[num4 + k + 1];
				int edge2 = MarchingCubeLookup.triTableFlat[num4 + k + 2];
				edgeStream.Write(MakeEdge(num5, corners, cornerSamples, cornerIndices));
				edgeStream.Write(MakeEdge(edge, corners, cornerSamples, cornerIndices));
				edgeStream.Write(MakeEdge(edge2, corners, cornerSamples, cornerIndices));
			}
		}
		edgeStream.EndForEachIndex();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private Facepunch.MarchingCubes.EdgeKey MakeEdge(int edge, Span<int3> corners, Span<float> cornerSamples, Span<int> cornerIndices)
	{
		int num = MarchingCubeLookup.cornerIndexAFromEdge[edge];
		int num2 = MarchingCubeLookup.cornerIndexBFromEdge[edge];
		int num3 = cornerIndices[num];
		int num4 = cornerIndices[num2];
		bool num5 = num3 < num4;
		int index = (num5 ? num : num2);
		int index2 = (num5 ? num2 : num);
		int num6 = (num5 ? num3 : num4);
		int num7 = MarchingCubeLookup.axisFromEdge[edge];
		int edgeId = 3 * num6 + num7;
		return new Facepunch.MarchingCubes.EdgeKey(corners[index], corners[index2], cornerSamples[index], cornerSamples[index2], iso, vertexOffset, scale, edgeId);
	}
}
