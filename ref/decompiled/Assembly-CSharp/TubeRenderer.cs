using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class TubeRenderer : FacepunchBehaviour
{
	[Range(3f, 64f)]
	[Header("Settings")]
	public int Segments = 12;

	public float Radius = 0.1f;

	public bool useLocalPositions;

	[Header("Caps")]
	public bool EnableCaps = true;

	[Range(1f, 8f)]
	public int HemisphereRings = 4;

	[NonSerialized]
	public List<Vector3> points = new List<Vector3>();

	private Mesh mesh;

	public void ClearPositions()
	{
		points.Clear();
	}

	public void SetPosition(int index, Vector3 position)
	{
		if (index >= 0 && index < points.Count)
		{
			points[index] = position;
		}
	}

	public void SetPositions(List<Vector3> positions)
	{
		points.Clear();
		points.AddRange(positions);
	}

	public void UpdateRenderer()
	{
		GenerateTube(points, Radius, Segments, HemisphereRings);
	}

	private void SetupMesh()
	{
		mesh = new Mesh
		{
			name = "Tube Mesh"
		};
		mesh.MarkDynamic();
		MeshFilter meshFilter = GetComponent<MeshFilter>();
		if (meshFilter == null)
		{
			meshFilter = base.gameObject.AddComponent<MeshFilter>();
		}
		meshFilter.mesh = mesh;
		if (GetComponent<MeshRenderer>() == null)
		{
			base.gameObject.AddComponent<MeshRenderer>();
		}
	}

	private void GenerateTube(List<Vector3> points, float radius, int segments, int rings)
	{
		if (points == null || points.Count < 2)
		{
			return;
		}
		using (TimeWarning.New("TubeRenderer.GenerateTube"))
		{
			if (mesh == null)
			{
				SetupMesh();
			}
			List<Vector3> obj = Pool.Get<List<Vector3>>();
			List<int> obj2 = Pool.Get<List<int>>();
			List<Vector2> obj3 = Pool.Get<List<Vector2>>();
			List<Quaternion> obj4 = Pool.Get<List<Quaternion>>();
			List<float> obj5 = Pool.Get<List<float>>();
			int vertOffset = 0;
			ComputeParallelTransportFrames(points, obj4);
			obj5.Add(0f);
			float num = 0f;
			for (int i = 1; i < points.Count; i++)
			{
				float num2 = Vector3.Distance(points[i - 1], points[i]);
				num += num2;
				obj5.Add(num);
			}
			for (int j = 0; j < points.Count; j++)
			{
				Vector3 center = (useLocalPositions ? points[j] : base.transform.InverseTransformPoint(points[j]));
				float v = ((num > 0f) ? (obj5[j] / num) : 0f);
				AppendRing(obj, obj3, obj4[j], center, radius, segments, v);
				if (j > 0)
				{
					BridgeLastTwoRings(obj2, vertOffset, segments);
				}
				vertOffset += segments + 1;
			}
			if (EnableCaps)
			{
				AppendHemisphereCap(obj, obj2, obj3, ref vertOffset, points[0], obj4[0], radius, segments, rings, -1);
				List<Vector3> verts = obj;
				List<int> tris = obj2;
				List<Vector2> uvs = obj3;
				Vector3 position = points[points.Count - 1];
				List<Quaternion> list = obj4;
				AppendHemisphereCap(verts, tris, uvs, ref vertOffset, position, list[list.Count - 1], radius, segments, rings, 1);
			}
			else
			{
				AppendFlatCap(obj, obj2, obj3, ref vertOffset, points[0], obj4[0], radius, segments, -1);
				List<Vector3> verts2 = obj;
				List<int> tris2 = obj2;
				List<Vector2> uvs2 = obj3;
				Vector3 position2 = points[points.Count - 1];
				List<Quaternion> list2 = obj4;
				AppendFlatCap(verts2, tris2, uvs2, ref vertOffset, position2, list2[list2.Count - 1], radius, segments, 1);
			}
			mesh.Clear();
			mesh.SetVertices(obj);
			mesh.SetUVs(0, obj3);
			mesh.SetTriangles(obj2, 0);
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			Pool.FreeUnmanaged(ref obj);
			Pool.FreeUnmanaged(ref obj2);
			Pool.FreeUnmanaged(ref obj4);
			Pool.FreeUnmanaged(ref obj3);
			Pool.FreeUnmanaged(ref obj5);
		}
	}

	private void ComputeParallelTransportFrames(List<Vector3> points, List<Quaternion> rotations)
	{
		rotations.Clear();
		if (points.Count >= 2)
		{
			Vector3 vector = (points[1] - points[0]).normalized;
			Vector3 rhs = ((Mathf.Abs(Vector3.Dot(vector, Vector3.up)) < 0.99f) ? Vector3.up : Vector3.right);
			Vector3 vector2 = Vector3.Cross(vector, rhs).normalized;
			Vector3 normalized = Vector3.Cross(vector, vector2).normalized;
			rotations.Add(Quaternion.LookRotation(vector, normalized));
			for (int i = 1; i < points.Count; i++)
			{
				Vector3 normalized2 = (points[i] - points[i - 1]).normalized;
				Vector3 vector3 = Quaternion.FromToRotation(vector, normalized2) * vector2;
				Vector3 normalized3 = Vector3.Cross(normalized2, vector3).normalized;
				rotations.Add(Quaternion.LookRotation(normalized2, normalized3));
				vector = normalized2;
				vector2 = vector3;
			}
		}
	}

	private void AppendRing(List<Vector3> verts, List<Vector2> uvs, Quaternion rotation, Vector3 center, float radius, int segments, float v)
	{
		Vector3 vector = rotation * Vector3.right;
		Vector3 vector2 = rotation * Vector3.up;
		for (int i = 0; i <= segments; i++)
		{
			float f = (float)i / (float)segments * MathF.PI * 2f;
			Vector3 vector3 = Mathf.Cos(f) * vector + Mathf.Sin(f) * vector2;
			verts.Add(center + vector3 * radius);
			uvs.Add(new Vector2((float)i / (float)segments, v));
		}
	}

	private void BridgeLastTwoRings(List<int> tris, int vertOffset, int segments)
	{
		int num = vertOffset - (segments + 1);
		for (int i = 0; i < segments; i++)
		{
			int item = num + i;
			int item2 = num + i + 1;
			int item3 = vertOffset + i;
			int item4 = vertOffset + i + 1;
			tris.Add(item);
			tris.Add(item2);
			tris.Add(item3);
			tris.Add(item2);
			tris.Add(item4);
			tris.Add(item3);
		}
	}

	private void AppendFlatCap(List<Vector3> verts, List<int> tris, List<Vector2> uvs, ref int vertOffset, Vector3 position, Quaternion rotation, float radius, int segments, int direction)
	{
		position = (useLocalPositions ? position : base.transform.InverseTransformPoint(position));
		Vector3 vector = rotation * Vector3.right;
		Vector3 vector2 = rotation * Vector3.up;
		int count = verts.Count;
		verts.Add(position);
		uvs.Add(new Vector2(0.5f, 0.5f));
		for (int i = 0; i <= segments; i++)
		{
			float f = (float)i / (float)segments * MathF.PI * 2f;
			Vector3 vector3 = Mathf.Cos(f) * vector + Mathf.Sin(f) * vector2;
			verts.Add(position + vector3 * radius);
			float x = Mathf.Cos(f) * 0.5f + 0.5f;
			float y = Mathf.Sin(f) * 0.5f + 0.5f;
			uvs.Add(new Vector2(x, y));
		}
		for (int j = 0; j < segments; j++)
		{
			int item = count + j + 1;
			int item2 = count + j + 2;
			if (direction == 1)
			{
				tris.Add(count);
				tris.Add(item);
				tris.Add(item2);
			}
			else
			{
				tris.Add(count);
				tris.Add(item2);
				tris.Add(item);
			}
		}
		vertOffset += segments + 2;
	}

	private void AppendHemisphereCap(List<Vector3> verts, List<int> tris, List<Vector2> uvs, ref int vertOffset, Vector3 position, Quaternion rotation, float radius, int segments, int rings, int direction)
	{
		position = (useLocalPositions ? position : base.transform.InverseTransformPoint(position));
		List<Vector3> obj = Pool.Get<List<Vector3>>();
		List<Vector2> obj2 = Pool.Get<List<Vector2>>();
		for (int i = 0; i <= rings; i++)
		{
			float f = (float)i / (float)rings * MathF.PI / 2f;
			for (int j = 0; j <= segments; j++)
			{
				float f2 = (float)j / (float)segments * MathF.PI * 2f;
				float num = Mathf.Sin(f);
				Vector3 vector = new Vector3(Mathf.Cos(f2) * num, Mathf.Sin(f2) * num, Mathf.Cos(f) * (float)direction);
				Vector3 item = rotation * (vector * radius) + position;
				obj.Add(item);
				float x = Mathf.Cos(f2) * num * 0.5f + 0.5f;
				float y = Mathf.Sin(f2) * num * 0.5f + 0.5f;
				obj2.Add(new Vector2(x, y));
			}
		}
		int count = verts.Count;
		verts.AddRange(obj);
		uvs.AddRange(obj2);
		for (int k = 0; k < rings; k++)
		{
			for (int l = 0; l < segments; l++)
			{
				int num2 = count + k * (segments + 1) + l;
				int item2 = num2 + 1;
				int num3 = num2 + (segments + 1);
				int item3 = num3 + 1;
				if (direction == 0)
				{
					tris.Add(num2);
					tris.Add(item2);
					tris.Add(num3);
					tris.Add(item2);
					tris.Add(item3);
					tris.Add(num3);
				}
				else
				{
					tris.Add(num2);
					tris.Add(num3);
					tris.Add(item2);
					tris.Add(item2);
					tris.Add(num3);
					tris.Add(item3);
				}
			}
		}
		vertOffset += obj.Count;
		Pool.FreeUnmanaged(ref obj);
		Pool.FreeUnmanaged(ref obj2);
	}
}
