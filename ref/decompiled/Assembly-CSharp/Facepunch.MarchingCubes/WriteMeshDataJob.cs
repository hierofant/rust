using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Facepunch.MarchingCubes;

[BurstCompile(FloatMode = FloatMode.Fast)]
internal struct WriteMeshDataJob : IJob
{
	[global::Unity.Collections.ReadOnly]
	public NativeArray<float3> vertices;

	[global::Unity.Collections.ReadOnly]
	public NativeArray<int> indices;

	public Mesh.MeshData meshData;

	public bool withNormals;

	public void Execute()
	{
		int length = vertices.Length;
		int length2 = indices.Length;
		NativeArray<VertexAttributeDescriptor> attributes = new NativeArray<VertexAttributeDescriptor>((!withNormals) ? 1 : 2, Allocator.Temp);
		attributes[0] = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0);
		if (withNormals)
		{
			attributes[1] = new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3, 1);
		}
		meshData.SetVertexBufferParams(length, attributes);
		meshData.GetVertexData<float3>().CopyFrom(vertices);
		bool flag = length <= 65535;
		meshData.SetIndexBufferParams(length2, (!flag) ? IndexFormat.UInt32 : IndexFormat.UInt16);
		if (flag)
		{
			NativeArray<ushort> indexData = meshData.GetIndexData<ushort>();
			for (int i = 0; i < length2; i++)
			{
				indexData[i] = (ushort)indices[i];
			}
		}
		else
		{
			meshData.GetIndexData<int>().CopyFrom(indices);
		}
		if (withNormals)
		{
			WriteNormals();
		}
		meshData.subMeshCount = 1;
		meshData.SetSubMesh(0, new SubMeshDescriptor(0, length2), MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontRecalculateBounds);
	}

	private void WriteNormals()
	{
		NativeArray<float3> array = meshData.GetVertexData<float3>(1);
		NativeArrayEx.MemClear(in array);
		for (int i = 0; i < indices.Length; i += 3)
		{
			int index = indices[i];
			int index2 = indices[i + 1];
			int index3 = indices[i + 2];
			float3 @float = vertices[index];
			float3 float2 = math.cross(vertices[index2] - @float, vertices[index3] - @float);
			array[index] += float2;
			array[index2] += float2;
			array[index3] += float2;
		}
		for (int j = 0; j < array.Length; j++)
		{
			array[j] = math.normalizesafe(array[j]);
		}
	}
}
