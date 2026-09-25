using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class PathMeshTemplate
{
	private static Quaternion rot90 = Quaternion.Euler(0f, 90f, 0f);

	public MeshCache.Data[] srcData;

	public float normalSmoothing;

	public bool snapToTerrain;

	public bool snapStartToTerrain;

	public bool snapEndToTerrain;

	public bool scaleWidthWithLength;

	public bool topAligned;

	public int roundVertices;

	public PathList PathList;

	public TerrainHeightMap heightmap;

	public Vector3 Position;

	public Vector3 origin;

	public int stepIndex;

	public int segmentCount;

	public float stepSize;

	public Mesh.MeshDataArray[] dstData;

	public Mesh[] outputMeshes;

	public void DestroyMeshes()
	{
		for (int i = 0; i < outputMeshes.Length; i++)
		{
			Mesh mesh = outputMeshes[i];
			if (!(mesh == null))
			{
				UnityEngine.Object.Destroy(mesh);
				outputMeshes[i] = null;
			}
		}
	}

	public void IntegrateMainThread()
	{
		for (int i = 0; i < outputMeshes.Length; i++)
		{
			Mesh mesh = outputMeshes[i];
			if (!(mesh == null))
			{
				Mesh.ApplyAndDisposeWritableMeshData(dstData[i], mesh);
				mesh.RecalculateBounds();
				mesh.RecalculateUVDistributionMetrics();
			}
		}
	}

	private void PreJob(int[] filter)
	{
		for (int i = 0; i < outputMeshes.Length; i++)
		{
			if (filter == null || filter.Length == 0 || Array.IndexOf(filter, i) != -1)
			{
				Mesh mesh = new Mesh();
				outputMeshes[i] = mesh;
				dstData[i] = Mesh.AllocateWritableMeshData(srcData[i].submeshes.Length);
			}
		}
	}

	public void Generate()
	{
		GenerateCertainLODs();
	}

	public void GenerateCertainLODs(params int[] filter)
	{
		PreJob(filter);
		GenerateImpl();
		IntegrateMainThread();
	}

	private void GenerateImpl()
	{
		Bounds bounds = srcData[srcData.Length - 1].bounds;
		Vector3 min = bounds.min;
		Vector3 size = bounds.size;
		_ = PathList.Width / bounds.size.x;
		float randomScale = PathList.RandomScale;
		float meshOffset = PathList.MeshOffset;
		float baseRadius = PathList.Width * 0.5f;
		for (int i = 0; i < srcData.Length; i++)
		{
			if (outputMeshes[i] == null)
			{
				continue;
			}
			MeshCache.Data data = srcData[i];
			Mesh.MeshData meshData = dstData[i][0];
			int num = data.vertices.Length;
			int num2 = data.triangles.Length;
			int num3 = segmentCount * num;
			int indexCount = segmentCount * num2;
			IndexFormat indexFormat = ((num3 > 65535) ? IndexFormat.UInt32 : IndexFormat.UInt16);
			meshData.SetVertexBufferParams(num3, new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0), new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3, 1), new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float32, 4, 2), new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2, 3));
			meshData.SetIndexBufferParams(indexCount, indexFormat);
			NativeArray<Vector3> vertexData = meshData.GetVertexData<Vector3>();
			NativeArray<Vector3> vertexData2 = meshData.GetVertexData<Vector3>(1);
			NativeArray<Vector4> vertexData3 = meshData.GetVertexData<Vector4>(2);
			NativeArray<Vector2> vertexData4 = meshData.GetVertexData<Vector2>(3);
			NativeArray<ushort> nativeArray = default(NativeArray<ushort>);
			NativeArray<uint> nativeArray2 = default(NativeArray<uint>);
			if (indexFormat == IndexFormat.UInt16)
			{
				nativeArray = meshData.GetIndexData<ushort>();
			}
			else
			{
				nativeArray2 = meshData.GetIndexData<uint>();
			}
			for (int j = 0; j < segmentCount; j++)
			{
				float num4 = (float)(stepIndex + j) * stepSize;
				int num5 = j * num;
				int num6 = j * num2;
				for (int k = 0; k < num; k++)
				{
					Vector2 value = data.uv[k];
					Vector3 vector = data.vertices[k];
					Vector3 vector2 = data.normals[k];
					Vector4 value2 = data.tangents[k];
					float t = (vector.x - min.x) / size.x;
					float num7 = vector.y - min.y;
					if (topAligned)
					{
						num7 -= size.y;
					}
					float num8 = (vector.z - min.z) / size.z;
					float num9 = num4 + num8 * stepSize;
					Vector3 obj = (PathList.Spline ? PathList.Path.GetPointCubicHermite(num9) : PathList.Path.GetPoint(num9));
					Vector3 tangent = PathList.Path.GetTangent(num9);
					Vector3 normalized = tangent.XZ3D().normalized;
					Vector3 vector3 = rot90 * normalized;
					Vector3 vector4 = Vector3.Cross(tangent, vector3);
					Quaternion quaternion = Quaternion.LookRotation(normalized, vector4);
					float radius = PathList.GetRadius(num9, PathList.Path.Length, baseRadius, randomScale, scaleWidthWithLength);
					Vector3 vector5 = obj - vector3 * radius;
					Vector3 vector6 = obj + vector3 * radius;
					if (snapToTerrain)
					{
						vector5.y = heightmap.GetHeight(vector5);
						vector6.y = heightmap.GetHeight(vector6);
					}
					vector5 += vector4 * meshOffset;
					vector6 += vector4 * meshOffset;
					vector = Vector3.Lerp(vector5, vector6, t);
					if ((snapStartToTerrain && num9 < 0.1f) || (snapEndToTerrain && num9 > PathList.Path.Length - 0.1f))
					{
						vector.y = heightmap.GetHeight(vector);
					}
					else
					{
						vector.y += num7;
					}
					vector -= origin;
					vector2 = quaternion * vector2;
					Vector3 vector7 = new Vector3(value2.x, value2.y, value2.z);
					vector7 = quaternion * vector7;
					value2.Set(vector7.x, vector7.y, vector7.z, value2.w);
					if (normalSmoothing > 0f)
					{
						vector2 = Vector3.Slerp(vector2, Vector3.up, normalSmoothing);
					}
					if (roundVertices > 0)
					{
						vector.x = (float)Math.Round(vector.x, roundVertices);
						vector.y = (float)Math.Round(vector.y, roundVertices);
						vector.z = (float)Math.Round(vector.z, roundVertices);
					}
					vertexData[num5 + k] = vector;
					vertexData2[num5 + k] = vector2;
					vertexData3[num5 + k] = value2;
					vertexData4[num5 + k] = value;
				}
				if (indexFormat == IndexFormat.UInt16)
				{
					for (int l = 0; l < num2; l++)
					{
						nativeArray[num6 + l] = (ushort)(num5 + data.triangles[l]);
					}
				}
				else
				{
					for (int m = 0; m < num2; m++)
					{
						nativeArray2[num6 + m] = (uint)(num5 + data.triangles[m]);
					}
				}
			}
			meshData.subMeshCount = 1;
			meshData.SetSubMesh(0, new SubMeshDescriptor(0, indexCount));
		}
	}
}
