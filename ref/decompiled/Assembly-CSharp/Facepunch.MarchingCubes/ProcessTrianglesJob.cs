using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Facepunch.MarchingCubes;

[BurstCompile(FloatMode = FloatMode.Fast)]
internal struct ProcessTrianglesJob : IJob
{
	public NativeStream.Reader edgeStream;

	public NativeList<float3> vertices;

	public NativeList<int> indices;

	public int edgeArraySize;

	public unsafe void Execute()
	{
		vertices.Clear();
		indices.Clear();
		int num = edgeStream.Count();
		if (indices.Capacity < num)
		{
			indices.SetCapacity(num);
		}
		if (vertices.Capacity < num)
		{
			vertices.SetCapacity(num);
		}
		NativeArray<int> nativeArray = new NativeArray<int>(edgeArraySize, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
		UnsafeUtility.MemSet(nativeArray.GetUnsafePtr(), byte.MaxValue, (long)nativeArray.Length * 4L);
		int generatedVertices = 0;
		for (int i = 0; i < edgeStream.ForEachCount; i++)
		{
			edgeStream.BeginForEachIndex(i);
			while (edgeStream.RemainingItemCount > 0)
			{
				ProcessEdge(in edgeStream.Read<Facepunch.MarchingCubes.EdgeKey>(), nativeArray, ref generatedVertices);
			}
			edgeStream.EndForEachIndex();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void ProcessEdge(in Facepunch.MarchingCubes.EdgeKey edge, NativeArray<int> vertexByEdge, ref int generatedVertices)
	{
		int num = vertexByEdge[edge.edgeId];
		if (num != -1)
		{
			indices.AddNoResize(num);
			return;
		}
		int value = generatedVertices++;
		vertices.AddNoResize(edge.vertex);
		indices.AddNoResize(value);
		vertexByEdge[edge.edgeId] = value;
	}
}
