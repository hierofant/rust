using System.Collections.Generic;
using ConVar;
using Facepunch;
using UnityEngine;
using UnityEngine.Rendering;

public class MeshRendererData
{
	public List<List<int>> triangles;

	public List<Vector3> vertices;

	public List<Vector3> normals;

	public List<Vector4> tangents;

	public List<Color32> colors32;

	public List<Vector2> uv;

	public List<Vector2> uv2;

	public List<Vector4> positions;

	public void Alloc()
	{
		if (triangles == null)
		{
			triangles = Facepunch.Pool.Get<List<List<int>>>();
		}
		if (vertices == null)
		{
			vertices = Facepunch.Pool.Get<List<Vector3>>();
		}
		if (normals == null)
		{
			normals = Facepunch.Pool.Get<List<Vector3>>();
		}
		if (tangents == null)
		{
			tangents = Facepunch.Pool.Get<List<Vector4>>();
		}
		if (colors32 == null)
		{
			colors32 = Facepunch.Pool.Get<List<Color32>>();
		}
		if (uv == null)
		{
			uv = Facepunch.Pool.Get<List<Vector2>>();
		}
		if (uv2 == null)
		{
			uv2 = Facepunch.Pool.Get<List<Vector2>>();
		}
		if (positions == null)
		{
			positions = Facepunch.Pool.Get<List<Vector4>>();
		}
	}

	public void Free()
	{
		if (triangles != null)
		{
			foreach (List<int> triangle in triangles)
			{
				List<int> obj = triangle;
				Facepunch.Pool.FreeUnmanaged(ref obj);
			}
			Facepunch.Pool.FreeUnmanaged(ref triangles);
		}
		if (vertices != null)
		{
			Facepunch.Pool.FreeUnmanaged(ref vertices);
		}
		if (normals != null)
		{
			Facepunch.Pool.FreeUnmanaged(ref normals);
		}
		if (tangents != null)
		{
			Facepunch.Pool.FreeUnmanaged(ref tangents);
		}
		if (colors32 != null)
		{
			Facepunch.Pool.FreeUnmanaged(ref colors32);
		}
		if (uv != null)
		{
			Facepunch.Pool.FreeUnmanaged(ref uv);
		}
		if (uv2 != null)
		{
			Facepunch.Pool.FreeUnmanaged(ref uv2);
		}
		if (positions != null)
		{
			Facepunch.Pool.FreeUnmanaged(ref positions);
		}
	}

	public void Clear()
	{
		if (triangles != null)
		{
			foreach (List<int> triangle in triangles)
			{
				List<int> obj = triangle;
				Facepunch.Pool.FreeUnmanaged(ref obj);
			}
			triangles.Clear();
		}
		if (vertices != null)
		{
			vertices.Clear();
		}
		if (normals != null)
		{
			normals.Clear();
		}
		if (tangents != null)
		{
			tangents.Clear();
		}
		if (colors32 != null)
		{
			colors32.Clear();
		}
		if (uv != null)
		{
			uv.Clear();
		}
		if (uv2 != null)
		{
			uv2.Clear();
		}
		if (positions != null)
		{
			positions.Clear();
		}
	}

	public void Apply(UnityEngine.Mesh mesh, MeshRendererBatch batch)
	{
		mesh.Clear();
		mesh.subMeshCount = ((triangles == null) ? 1 : triangles.Count);
		if (vertices != null)
		{
			mesh.SetVertices(vertices);
		}
		if (triangles != null)
		{
			for (int i = 0; i < triangles.Count; i++)
			{
				mesh.SetTriangles(triangles[i], i);
			}
		}
		if (normals != null)
		{
			if (normals.Count == vertices.Count)
			{
				mesh.SetNormals(normals);
			}
			else if (normals.Count > 0 && Batching.verbose > 0)
			{
				Debug.LogWarning("Skipping renderer normals because some meshes were missing them.");
			}
		}
		if (tangents != null)
		{
			if (tangents.Count == vertices.Count)
			{
				mesh.SetTangents(tangents);
			}
			else if (tangents.Count > 0 && Batching.verbose > 0)
			{
				Debug.LogWarning("Skipping renderer tangents because some meshes were missing them.");
			}
		}
		if (colors32 != null)
		{
			if (colors32.Count == vertices.Count)
			{
				mesh.SetColors(colors32);
			}
			else if (colors32.Count > 0 && Batching.verbose > 0)
			{
				Debug.LogWarning("Skipping renderer colors because some meshes were missing them.", batch);
			}
		}
		if (uv != null)
		{
			if (uv.Count == vertices.Count)
			{
				mesh.SetUVs(0, uv);
			}
			else if (uv.Count > 0 && Batching.verbose > 0)
			{
				Debug.LogWarning("Skipping renderer uvs because some meshes were missing them.");
			}
		}
		if (uv2 != null)
		{
			if (uv2.Count == vertices.Count)
			{
				mesh.SetUVs(1, uv2);
			}
			else if (uv2.Count > 0 && Batching.verbose > 0)
			{
				Debug.LogWarning("Skipping renderer uv2s because some meshes were missing them.");
			}
		}
		if (positions != null)
		{
			mesh.SetUVs(2, positions);
		}
	}

	public void Combine(MeshRendererGroup meshGroup, MeshRendererLookup rendererLookup)
	{
		for (int i = 0; i < meshGroup.Count; i++)
		{
			MeshRendererInstance instance = meshGroup[i];
			Matrix4x4 matrix4x = Matrix4x4.TRS(instance.position, instance.rotation, instance.scale);
			MeshCache.Data data = instance.data;
			int num = data.submeshes.Length;
			for (int j = 0; j < num; j++)
			{
				if (triangles.Count <= j)
				{
					triangles.Add(Facepunch.Pool.Get<List<int>>());
				}
				SubMeshDescriptor subMeshDescriptor = data.submeshes[j];
				int num2 = vertices.Count - subMeshDescriptor.firstVertex;
				int indexCount = subMeshDescriptor.indexCount;
				int vertexCount = subMeshDescriptor.vertexCount;
				int num3 = ((data.normals.Length != 0) ? vertexCount : 0);
				int num4 = ((data.tangents.Length != 0) ? vertexCount : 0);
				int num5 = ((data.colors32.Length != 0) ? vertexCount : 0);
				int num6 = vertexCount;
				int num7 = vertexCount;
				List<int> list = triangles[j];
				for (int k = 0; k < indexCount; k++)
				{
					int num8 = data.triangles[k + subMeshDescriptor.indexStart];
					list.Add(num2 + num8);
				}
				for (int l = 0; l < vertexCount; l++)
				{
					vertices.Add(matrix4x.MultiplyPoint3x4(data.vertices[l + subMeshDescriptor.firstVertex]));
					positions.Add(instance.position);
				}
				for (int m = 0; m < num3; m++)
				{
					normals.Add(matrix4x.MultiplyVector(data.normals[m + subMeshDescriptor.firstVertex]));
				}
				for (int n = 0; n < num4; n++)
				{
					Vector4 vector = data.tangents[n + subMeshDescriptor.firstVertex];
					Vector3 vector2 = new Vector3(vector.x, vector.y, vector.z);
					Vector3 vector3 = matrix4x.MultiplyVector(vector2);
					tangents.Add(new Vector4(vector3.x, vector3.y, vector3.z, vector.w));
				}
				if (data.colors32.Length == 0)
				{
					for (int num9 = 0; num9 < vertexCount; num9++)
					{
						colors32.Add(Color.white);
					}
				}
				else
				{
					for (int num10 = 0; num10 < num5; num10++)
					{
						colors32.Add(data.colors32[num10 + subMeshDescriptor.firstVertex]);
					}
				}
				if (data.uv.Length == 0)
				{
					for (int num11 = 0; num11 < num6; num11++)
					{
						uv.Add(Vector2.zero);
					}
				}
				else
				{
					for (int num12 = 0; num12 < num6; num12++)
					{
						uv.Add(data.uv[num12 + subMeshDescriptor.firstVertex]);
					}
				}
				if (data.uv2.Length == 0)
				{
					for (int num13 = 0; num13 < num7; num13++)
					{
						uv2.Add(Vector2.zero);
					}
				}
				else
				{
					for (int num14 = 0; num14 < num7; num14++)
					{
						uv2.Add(data.uv2[num14 + subMeshDescriptor.firstVertex]);
					}
				}
			}
			rendererLookup.Add(instance);
		}
	}
}
