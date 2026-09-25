using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class PathList
{
	public enum Side
	{
		Both,
		Left,
		Right,
		Any
	}

	public enum Placement
	{
		Center,
		Side
	}

	public enum Alignment
	{
		None,
		Neighbor,
		Forward,
		Inward
	}

	[Serializable]
	public class BasicObject
	{
		public string Folder;

		public SpawnFilter Filter;

		public Placement Placement;

		public bool AlignToNormal = true;

		public bool HeightToTerrain = true;

		public float Offset;
	}

	[Serializable]
	public class SideObject
	{
		public string Folder;

		public SpawnFilter Filter;

		public Side Side;

		public Alignment Alignment;

		public float Density = 1f;

		public float Distance = 25f;

		public float Offset = 2f;
	}

	[Serializable]
	public class PathObject
	{
		public string Folder;

		public SpawnFilter Filter;

		public Alignment Alignment;

		public float Density = 1f;

		public float Distance = 5f;

		public float Dithering = 5f;
	}

	[Serializable]
	public class BridgeObject
	{
		public string Folder;

		public float Distance = 10f;
	}

	public class MeshObject
	{
		public Vector3 Position;

		public Mesh[] Meshes;

		public static MeshObject CreateAndRecalculateUVs(Vector3 meshPivot, MeshData[] meshData)
		{
			MeshObject meshObject = new MeshObject();
			meshObject.Position = meshPivot;
			meshObject.Meshes = new Mesh[meshData.Length];
			for (int i = 0; i < meshObject.Meshes.Length; i++)
			{
				MeshData obj = meshData[i];
				Mesh mesh = (meshObject.Meshes[i] = new Mesh());
				obj.Apply(mesh);
				mesh.RecalculateUVDistributionMetrics();
			}
			return meshObject;
		}
	}

	private struct WeldVertex : IEquatable<WeldVertex>
	{
		private const float EPSILON = 0.001f;

		private const float INV_EPSILON = 999.99994f;

		public float x;

		public float y;

		public float z;

		public float alwaysUnderwater;

		public float topSurface;

		public override bool Equals(object other)
		{
			if (other is WeldVertex)
			{
				return Equals((WeldVertex)other);
			}
			return false;
		}

		public bool Equals(WeldVertex other)
		{
			if (Vector3.Distance(new Vector3(x, y, z), new Vector3(other.x, other.y, other.z)) < 0.001f && alwaysUnderwater == other.alwaysUnderwater)
			{
				return topSurface == other.topSurface;
			}
			return false;
		}

		public override int GetHashCode()
		{
			int value = Mathf.RoundToInt(x * 999.99994f);
			int value2 = Mathf.RoundToInt(y * 999.99994f);
			int value3 = Mathf.RoundToInt(z * 999.99994f);
			return HashCode.Combine(value, value2, value3, alwaysUnderwater, topSurface);
		}
	}

	private static Quaternion rot90 = Quaternion.Euler(0f, 90f, 0f);

	private static Quaternion rot180 = Quaternion.Euler(0f, 180f, 0f);

	private static Quaternion rot270 = Quaternion.Euler(0f, 270f, 0f);

	public const float EndWidthScale = 3f;

	public const float EndScaleDistance = 100f;

	public const float LengthWidthScale = 8f;

	public const float LengthDepthScale = 3f;

	public const float LengthScaleDistance = 1000f;

	public string Name;

	public PathInterpolator Path;

	public bool Spline;

	public bool Start;

	public bool End;

	public float Width;

	public float InnerPadding;

	public float OuterPadding;

	public float InnerFade;

	public float OuterFade;

	public float RandomScale;

	public float MeshOffset;

	public float TerrainOffset;

	public int Topology;

	public int Splat;

	public int Hierarchy;

	public PathFinder.Node ProcgenStartNode;

	public PathFinder.Node ProcgenEndNode;

	public const float StepSize = 1f;

	private static float[] placements = new float[3] { 0f, -1f, 1f };

	public PathList(string name, Vector3[] points)
	{
		Name = name;
		Path = new PathInterpolator(points);
	}

	private void SpawnObjectsNeighborAligned(ref uint seed, Prefab[] prefabs, List<Vector3> positions, SpawnFilter filter = null)
	{
		if (positions.Count < 2)
		{
			return;
		}
		List<Prefab> obj = Pool.Get<List<Prefab>>();
		for (int i = 0; i < positions.Count; i++)
		{
			int index = Mathf.Max(i - 1, 0);
			int index2 = Mathf.Min(i + 1, positions.Count - 1);
			Vector3 position = positions[i];
			Quaternion rotation = Quaternion.LookRotation((positions[index2] - positions[index]).XZ3D());
			SpawnObject(ref seed, prefabs, position, rotation, obj, out var spawned, positions.Count, i, filter);
			if (spawned != null)
			{
				obj.Add(spawned);
			}
		}
		Pool.FreeUnmanaged(ref obj);
	}

	private bool SpawnObject(ref uint seed, Prefab[] prefabs, Vector3 position, Quaternion rotation, SpawnFilter filter = null)
	{
		Prefab random = ArrayEx.GetRandom(prefabs, ref seed);
		Vector3 pos = position;
		Quaternion rot = rotation;
		Vector3 scale = random.Object.transform.localScale;
		random.ApplyDecorComponents(ref pos, ref rot, ref scale);
		if (!random.ApplyTerrainAnchors(ref pos, rot, scale, filter))
		{
			return false;
		}
		World.AddPrefab(Name, random, pos, rot, scale);
		return true;
	}

	private bool SpawnObject(ref uint seed, Prefab[] prefabs, Vector3 position, Quaternion rotation, List<Prefab> previousSpawns, out Prefab spawned, int pathLength, int index, SpawnFilter filter = null)
	{
		spawned = null;
		Prefab replacement = ArrayEx.GetRandom(prefabs, ref seed);
		replacement.ApplySequenceReplacement(previousSpawns, ref replacement, prefabs, pathLength, index, position);
		Vector3 pos = position;
		Quaternion rot = rotation;
		Vector3 scale = replacement.Object.transform.localScale;
		replacement.ApplyDecorComponents(ref pos, ref rot, ref scale);
		if (!replacement.ApplyTerrainAnchors(ref pos, rot, scale, filter))
		{
			return false;
		}
		World.AddPrefab(Name, replacement, pos, rot, scale);
		spawned = replacement;
		return true;
	}

	private bool CheckObjects(Prefab[] prefabs, Vector3 position, Quaternion rotation, SpawnFilter filter = null)
	{
		foreach (Prefab obj in prefabs)
		{
			Vector3 pos = position;
			Vector3 localScale = obj.Object.transform.localScale;
			if (!obj.ApplyTerrainAnchors(ref pos, rotation, localScale, filter))
			{
				return false;
			}
		}
		return true;
	}

	private void SpawnObject(ref uint seed, Prefab[] prefabs, Vector3 pos, Vector3 dir, BasicObject obj)
	{
		if (!obj.AlignToNormal)
		{
			dir = dir.XZ3D().normalized;
		}
		SpawnFilter filter = obj.Filter;
		Vector3 vector = (Width * 0.5f + obj.Offset) * (rot90 * dir);
		for (int i = 0; i < placements.Length; i++)
		{
			if ((obj.Placement == Placement.Center && i != 0) || (obj.Placement == Placement.Side && i == 0))
			{
				continue;
			}
			Vector3 vector2 = pos + placements[i] * vector;
			if (obj.HeightToTerrain)
			{
				vector2.y = TerrainMeta.HeightMap.GetHeight(vector2);
			}
			if (filter.Test(vector2))
			{
				Quaternion rotation = ((i == 2) ? Quaternion.LookRotation(rot180 * dir) : Quaternion.LookRotation(dir));
				if (SpawnObject(ref seed, prefabs, vector2, rotation, filter))
				{
					break;
				}
			}
		}
	}

	private bool CheckObjects(Prefab[] prefabs, Vector3 pos, Vector3 dir, BasicObject obj)
	{
		if (!obj.AlignToNormal)
		{
			dir = dir.XZ3D().normalized;
		}
		SpawnFilter filter = obj.Filter;
		Vector3 vector = (Width * 0.5f + obj.Offset) * (rot90 * dir);
		for (int i = 0; i < placements.Length; i++)
		{
			if ((obj.Placement == Placement.Center && i != 0) || (obj.Placement == Placement.Side && i == 0))
			{
				continue;
			}
			Vector3 vector2 = pos + placements[i] * vector;
			if (obj.HeightToTerrain)
			{
				vector2.y = TerrainMeta.HeightMap.GetHeight(vector2);
			}
			if (filter.Test(vector2))
			{
				Quaternion rotation = ((i == 2) ? Quaternion.LookRotation(rot180 * dir) : Quaternion.LookRotation(dir));
				if (CheckObjects(prefabs, vector2, rotation, filter))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void SpawnSide(ref uint seed, SideObject obj)
	{
		if (string.IsNullOrEmpty(obj.Folder))
		{
			return;
		}
		Prefab[] array = Prefab.Load("assets/bundled/prefabs/autospawn/" + obj.Folder);
		if (array == null || array.Length == 0)
		{
			Debug.LogError("Empty decor folder: " + obj.Folder);
			return;
		}
		Side side = obj.Side;
		SpawnFilter filter = obj.Filter;
		float density = obj.Density;
		float distance = obj.Distance;
		float num = Width * 0.5f + obj.Offset;
		TerrainHeightMap heightMap = TerrainMeta.HeightMap;
		float[] array2 = new float[2]
		{
			0f - num,
			num
		};
		int num2 = 0;
		Vector3 vector = Path.GetStartPoint();
		List<Vector3> list = new List<Vector3>();
		float num3 = distance * 0.25f;
		float num4 = distance * 0.5f;
		float num5 = Path.StartOffset + num4;
		float num6 = Path.Length - Path.EndOffset - num4;
		for (float num7 = num5; num7 <= num6; num7 += num3)
		{
			Vector3 vector2 = (Spline ? Path.GetPointCubicHermite(num7) : Path.GetPoint(num7));
			if ((vector2 - vector).magnitude < distance)
			{
				continue;
			}
			Vector3 tangent = Path.GetTangent(num7);
			Vector3 vector3 = rot90 * tangent;
			for (int i = 0; i < array2.Length; i++)
			{
				int num8 = (num2 + i) % array2.Length;
				if ((side == Side.Left && num8 != 0) || (side == Side.Right && num8 != 1))
				{
					continue;
				}
				float num9 = array2[num8];
				Vector3 vector4 = vector2;
				vector4.x += vector3.x * num9;
				vector4.z += vector3.z * num9;
				float normX = TerrainMeta.NormalizeX(vector4.x);
				float normZ = TerrainMeta.NormalizeZ(vector4.z);
				if (filter.GetFactor(normX, normZ) < SeedRandom.Value(ref seed))
				{
					continue;
				}
				if (density >= SeedRandom.Value(ref seed))
				{
					vector4.y = heightMap.GetHeight(normX, normZ);
					if (obj.Alignment == Alignment.None)
					{
						if (!SpawnObject(ref seed, array, vector4, Quaternion.LookRotation(Vector3.zero), filter))
						{
							continue;
						}
					}
					else if (obj.Alignment == Alignment.Forward)
					{
						if (!SpawnObject(ref seed, array, vector4, Quaternion.LookRotation(tangent * num9), filter))
						{
							continue;
						}
					}
					else if (obj.Alignment == Alignment.Inward)
					{
						if (!SpawnObject(ref seed, array, vector4, Quaternion.LookRotation(tangent * num9) * rot270, filter))
						{
							continue;
						}
					}
					else
					{
						list.Add(vector4);
					}
				}
				num2 = num8;
				vector = vector2;
				if (side == Side.Any)
				{
					break;
				}
			}
		}
		if (list.Count > 0)
		{
			SpawnObjectsNeighborAligned(ref seed, array, list, filter);
		}
	}

	public void SpawnAlong(ref uint seed, PathObject obj)
	{
		if (string.IsNullOrEmpty(obj.Folder))
		{
			return;
		}
		Prefab[] array = Prefab.Load("assets/bundled/prefabs/autospawn/" + obj.Folder);
		if (array == null || array.Length == 0)
		{
			Debug.LogError("Empty decor folder: " + obj.Folder);
			return;
		}
		SpawnFilter filter = obj.Filter;
		float density = obj.Density;
		float distance = obj.Distance;
		float dithering = obj.Dithering;
		TerrainHeightMap heightMap = TerrainMeta.HeightMap;
		Vector3 vector = Path.GetStartPoint();
		List<Vector3> list = new List<Vector3>();
		float num = distance * 0.25f;
		float num2 = distance * 0.5f;
		float num3 = Path.StartOffset + num2;
		float num4 = Path.Length - Path.EndOffset - num2;
		for (float num5 = num3; num5 <= num4; num5 += num)
		{
			Vector3 vector2 = (Spline ? Path.GetPointCubicHermite(num5) : Path.GetPoint(num5));
			if ((vector2 - vector).magnitude < distance)
			{
				continue;
			}
			Vector3 tangent = Path.GetTangent(num5);
			Vector3 forward = rot90 * tangent;
			Vector3 vector3 = vector2;
			vector3.x += SeedRandom.Range(ref seed, 0f - dithering, dithering);
			vector3.z += SeedRandom.Range(ref seed, 0f - dithering, dithering);
			float normX = TerrainMeta.NormalizeX(vector3.x);
			float normZ = TerrainMeta.NormalizeZ(vector3.z);
			if (filter.GetFactor(normX, normZ) < SeedRandom.Value(ref seed))
			{
				continue;
			}
			if (density >= SeedRandom.Value(ref seed))
			{
				vector3.y = heightMap.GetHeight(normX, normZ);
				if (obj.Alignment == Alignment.None)
				{
					if (!SpawnObject(ref seed, array, vector3, Quaternion.identity, filter))
					{
						continue;
					}
				}
				else if (obj.Alignment == Alignment.Forward)
				{
					if (!SpawnObject(ref seed, array, vector3, Quaternion.LookRotation(tangent), filter))
					{
						continue;
					}
				}
				else if (obj.Alignment == Alignment.Inward)
				{
					if (!SpawnObject(ref seed, array, vector3, Quaternion.LookRotation(forward), filter))
					{
						continue;
					}
				}
				else
				{
					list.Add(vector3);
				}
			}
			vector = vector2;
		}
		if (list.Count > 0)
		{
			SpawnObjectsNeighborAligned(ref seed, array, list, filter);
		}
	}

	public void SpawnBridge(ref uint seed, BridgeObject obj)
	{
		if (string.IsNullOrEmpty(obj.Folder))
		{
			return;
		}
		Prefab[] array = Prefab.Load("assets/bundled/prefabs/autospawn/" + obj.Folder);
		if (array == null || array.Length == 0)
		{
			Debug.LogError("Empty decor folder: " + obj.Folder);
			return;
		}
		Vector3 startPoint = Path.GetStartPoint();
		Vector3 vector = Path.GetEndPoint() - startPoint;
		float magnitude = vector.magnitude;
		Vector3 vector2 = vector / magnitude;
		float num = magnitude / obj.Distance;
		int num2 = Mathf.RoundToInt(num);
		float num3 = 0.5f * (num - (float)num2);
		Vector3 vector3 = obj.Distance * vector2;
		Vector3 vector4 = startPoint + (0.5f + num3) * vector3;
		Quaternion rotation = Quaternion.LookRotation(vector2);
		for (int i = 0; i < num2; i++)
		{
			float num4 = WaterLevel.GetWaterOrTerrainSurface(vector4, waves: false, volumes: false) - 1f;
			if (vector4.y > num4)
			{
				SpawnObject(ref seed, array, vector4, rotation);
			}
			vector4 += vector3;
		}
	}

	public void SpawnStart(ref uint seed, BasicObject obj)
	{
		if (Start && !string.IsNullOrEmpty(obj.Folder))
		{
			Prefab[] array = Prefab.Load("assets/bundled/prefabs/autospawn/" + obj.Folder);
			if (array == null || array.Length == 0)
			{
				Debug.LogError("Empty decor folder: " + obj.Folder);
				return;
			}
			Vector3 startPoint = Path.GetStartPoint();
			Vector3 startTangent = Path.GetStartTangent();
			SpawnObject(ref seed, array, startPoint, startTangent, obj);
		}
	}

	public void SpawnEnd(ref uint seed, BasicObject obj)
	{
		if (End && !string.IsNullOrEmpty(obj.Folder))
		{
			Prefab[] array = Prefab.Load("assets/bundled/prefabs/autospawn/" + obj.Folder);
			if (array == null || array.Length == 0)
			{
				Debug.LogError("Empty decor folder: " + obj.Folder);
				return;
			}
			Vector3 endPoint = Path.GetEndPoint();
			Vector3 dir = -Path.GetEndTangent();
			SpawnObject(ref seed, array, endPoint, dir, obj);
		}
	}

	public void TrimStart(BasicObject obj)
	{
		if (!Start || string.IsNullOrEmpty(obj.Folder))
		{
			return;
		}
		Prefab[] array = Prefab.Load("assets/bundled/prefabs/autospawn/" + obj.Folder);
		if (array == null || array.Length == 0)
		{
			Debug.LogError("Empty decor folder: " + obj.Folder);
			return;
		}
		Vector3[] points = Path.Points;
		Vector3[] tangents = Path.Tangents;
		int num = points.Length / 4;
		for (int i = 0; i < num; i++)
		{
			Vector3 pos = points[Path.MinIndex + i];
			Vector3 dir = tangents[Path.MinIndex + i];
			if (CheckObjects(array, pos, dir, obj))
			{
				Path.MinIndex += i;
				break;
			}
		}
	}

	public void TrimEnd(BasicObject obj)
	{
		if (!End || string.IsNullOrEmpty(obj.Folder))
		{
			return;
		}
		Prefab[] array = Prefab.Load("assets/bundled/prefabs/autospawn/" + obj.Folder);
		if (array == null || array.Length == 0)
		{
			Debug.LogError("Empty decor folder: " + obj.Folder);
			return;
		}
		Vector3[] points = Path.Points;
		Vector3[] tangents = Path.Tangents;
		int num = points.Length / 4;
		for (int i = 0; i < num; i++)
		{
			Vector3 pos = points[Path.MaxIndex - i];
			Vector3 dir = -tangents[Path.MaxIndex - i];
			if (CheckObjects(array, pos, dir, obj))
			{
				Path.MaxIndex -= i;
				break;
			}
		}
	}

	public void TrimTopology(int topology)
	{
		Vector3[] points = Path.Points;
		int num = points.Length / 4;
		for (int i = 0; i < num; i++)
		{
			Vector3 worldPos = points[Path.MinIndex + i];
			if (!TerrainMeta.TopologyMap.GetTopology(worldPos, topology))
			{
				Path.MinIndex += i;
				break;
			}
		}
		for (int j = 0; j < num; j++)
		{
			Vector3 worldPos2 = points[Path.MaxIndex - j];
			if (!TerrainMeta.TopologyMap.GetTopology(worldPos2, topology))
			{
				Path.MaxIndex -= j;
				break;
			}
		}
	}

	public void ResetTrims()
	{
		Path.MinIndex = Path.DefaultMinIndex;
		Path.MaxIndex = Path.DefaultMaxIndex;
	}

	public void AdjustTerrainHeight(float intensity = 1f, float fade = 1f, bool scaleWidthWithLength = false)
	{
		AdjustTerrainHeight((float xn, float zn) => intensity, (float xn, float zn) => fade, scaleWidthWithLength);
	}

	public void AdjustTerrainHeight(Func<float, float, float> intensity, Func<float, float, float> fade, bool scaleWidthWithLength = false)
	{
		TerrainHeightMap heightmap = TerrainMeta.HeightMap;
		_ = TerrainMeta.TopologyMap;
		float num = 1f;
		float randomScale = RandomScale;
		float outerPadding = OuterPadding;
		float innerPadding = InnerPadding;
		float outerFade = OuterFade;
		float innerFade = InnerFade;
		float terrainOffset = TerrainOffset;
		float num2 = Width * 0.5f;
		Vector3 startPoint = Path.GetStartPoint();
		Vector3 endPoint = Path.GetEndPoint();
		Vector3 startTangent = Path.GetStartTangent();
		Vector3 normalized = startTangent.XZ3D().normalized;
		Vector3 vector = rot90 * normalized;
		Vector3 vector2 = startPoint;
		Line prev_line = new Line(startPoint, startPoint + startTangent * num);
		Vector3 vector3 = startPoint - vector * (num2 + outerPadding + outerFade);
		Vector3 vector4 = startPoint + vector * (num2 + outerPadding + outerFade);
		Vector3 vector5 = vector2;
		Vector3 v = startTangent;
		Line cur_line = prev_line;
		Vector3 vector6 = vector3;
		Vector3 vector7 = vector4;
		float num3 = Path.Length + num;
		for (float d = 0f; d < num3; d += num)
		{
			Vector3 vector8 = (Spline ? Path.GetPointCubicHermite(d + num) : Path.GetPoint(d + num));
			Vector3 tangent = Path.GetTangent(d + num);
			Line next_line = new Line(vector8, vector8 + tangent * num);
			float opacity = 1f;
			float radius = GetRadius(d, Path.Length, num2, randomScale, scaleWidthWithLength);
			float depth = GetDepth(d, Path.Length, terrainOffset, randomScale, scaleWidthWithLength);
			float offset = depth * TerrainMeta.OneOverSize.y;
			if (!Path.Circular)
			{
				float a = (startPoint - vector5).Magnitude2D();
				float b = (endPoint - vector5).Magnitude2D();
				opacity = Mathf.InverseLerp(0f, num2, Mathf.Min(a, b));
			}
			normalized = v.XZ3D().normalized;
			vector = rot90 * normalized;
			vector6 = vector5 - vector * (radius + outerPadding + outerFade);
			vector7 = vector5 + vector * (radius + outerPadding + outerFade);
			float yn = TerrainMeta.NormalizeY((vector5.y + vector2.y) * 0.5f);
			heightmap.ForEach(vector3, vector4, vector6, vector7, delegate(int x, int z)
			{
				float num4 = heightmap.Coordinate(x);
				float num5 = heightmap.Coordinate(z);
				Vector3 vector9 = TerrainMeta.Denormalize(new Vector3(num4, yn, num5));
				Vector3 vector10 = prev_line.ClosestPoint2D(vector9);
				Vector3 vector11 = cur_line.ClosestPoint2D(vector9);
				Vector3 vector12 = next_line.ClosestPoint2D(vector9);
				float num6 = (vector9 - vector10).Magnitude2D();
				float num7 = (vector9 - vector11).Magnitude2D();
				float num8 = (vector9 - vector12).Magnitude2D();
				float value = num7;
				Vector3 vector13 = vector11;
				if (!(num7 <= num6) || !(num7 <= num8))
				{
					if (num6 <= num8)
					{
						value = num6;
						vector13 = vector10;
					}
					else
					{
						value = num8;
						vector13 = vector12;
					}
				}
				float num9 = Mathf.InverseLerp(radius + outerPadding + outerFade * fade(num4, num5), radius + outerPadding, value);
				float num10 = intensity(num4, num5) * opacity * num9;
				if (num10 > 0f)
				{
					float num11 = (scaleWidthWithLength ? Mathf.Lerp(0.3f, 1f, d / 1000f) : 1f);
					float t = Mathf.InverseLerp(radius - innerPadding * num11, radius - innerPadding * num11 - innerFade * num11, value);
					float num12 = TerrainMeta.NormalizeY(vector13.y);
					float num13 = Mathf.SmoothStep(0f, offset, t);
					heightmap.SetHeight(x, z, num12 + num13, num10);
				}
			});
			vector2 = vector5;
			vector3 = vector6;
			vector4 = vector7;
			prev_line = cur_line;
			vector5 = vector8;
			v = tangent;
			cur_line = next_line;
		}
	}

	public void AdjustTerrainTexture(bool scaleWidthWithLength = false)
	{
		if (Splat == 0)
		{
			return;
		}
		TerrainSplatMap splatmap = TerrainMeta.SplatMap;
		float num = 1f;
		float randomScale = RandomScale;
		float outerPadding = OuterPadding;
		float innerPadding = InnerPadding;
		float num2 = Width * 0.5f;
		Vector3 startPoint = Path.GetStartPoint();
		Vector3 endPoint = Path.GetEndPoint();
		Vector3 startTangent = Path.GetStartTangent();
		Vector3 normalized = startTangent.XZ3D().normalized;
		Vector3 vector = rot90 * normalized;
		Vector3 v = startPoint - vector * (num2 + outerPadding);
		Vector3 v2 = startPoint + vector * (num2 + outerPadding);
		float num3 = Path.Length + num;
		for (float num4 = 0f; num4 < num3; num4 += num)
		{
			Vector3 vector2 = (Spline ? Path.GetPointCubicHermite(num4) : Path.GetPoint(num4));
			float opacity = 1f;
			float radius = GetRadius(num4, Path.Length, num2, randomScale, scaleWidthWithLength);
			if (!Path.Circular)
			{
				float a = (startPoint - vector2).Magnitude2D();
				float b = (endPoint - vector2).Magnitude2D();
				opacity = Mathf.InverseLerp(0f, num2, Mathf.Min(a, b));
			}
			startTangent = Path.GetTangent(num4);
			normalized = startTangent.XZ3D().normalized;
			vector = rot90 * normalized;
			Ray ray = new Ray(vector2, startTangent);
			Vector3 vector3 = vector2 - vector * (radius + outerPadding);
			Vector3 vector4 = vector2 + vector * (radius + outerPadding);
			float yn = TerrainMeta.NormalizeY(vector2.y);
			splatmap.ForEach(v, v2, vector3, vector4, delegate(int x, int z)
			{
				Vector3 vector5 = TerrainMeta.Denormalize(new Vector3(splatmap.Coordinate(x), z: splatmap.Coordinate(z), y: yn));
				Vector3 vector6 = RayEx.ClosestPoint(ray, vector5);
				float value = (vector5 - vector6).Magnitude2D();
				float num5 = Mathf.InverseLerp(radius + outerPadding, radius - innerPadding, value);
				splatmap.SetSplat(x, z, Splat, num5 * opacity);
			});
			v = vector3;
			v2 = vector4;
		}
	}

	public void AdjustTerrainWaterFlow(bool scaleWidthWithLength = false)
	{
		TerrainWaterFlowMap flowMap = TerrainMeta.WaterFlowMap;
		float num = 1f;
		float randomScale = RandomScale;
		float outerPadding = OuterPadding;
		float num2 = Width * 0.5f;
		Vector3 startPoint = Path.GetStartPoint();
		Vector3 dir = Path.GetStartTangent();
		Vector3 normalized = dir.XZ3D().normalized;
		Vector3 vector = rot90 * normalized;
		Vector3 v = startPoint - vector * (num2 + outerPadding);
		Vector3 v2 = startPoint + vector * (num2 + outerPadding);
		float num3 = Path.Length + num;
		for (float num4 = 0f; num4 < num3; num4 += num)
		{
			Vector3 obj = (Spline ? Path.GetPointCubicHermite(num4) : Path.GetPoint(num4));
			float radius = GetRadius(num4, Path.Length, num2, randomScale, scaleWidthWithLength);
			dir = Path.GetTangent(num4);
			normalized = dir.XZ3D().normalized;
			vector = rot90 * normalized;
			Vector3 vector2 = obj - vector * (radius + outerPadding);
			Vector3 vector3 = obj + vector * (radius + outerPadding);
			flowMap.ForEach(v, v2, vector2, vector3, delegate(int x, int z)
			{
				float normX = flowMap.Coordinate(x);
				float normZ = flowMap.Coordinate(z);
				flowMap.SetFlowDirection(normX, normZ, dir);
			});
			v = vector2;
			v2 = vector3;
		}
	}

	public void AdjustTerrainTopology(bool scaleWidthWithLength = false)
	{
		if (Topology == 0)
		{
			return;
		}
		TerrainTopologyMap topomap = TerrainMeta.TopologyMap;
		float num = 1f;
		float randomScale = RandomScale;
		float outerPadding = OuterPadding;
		float innerPadding = InnerPadding;
		float num2 = Width * 0.5f;
		Vector3 startPoint = Path.GetStartPoint();
		Vector3 endPoint = Path.GetEndPoint();
		Vector3 startTangent = Path.GetStartTangent();
		Vector3 normalized = startTangent.XZ3D().normalized;
		Vector3 vector = rot90 * normalized;
		Vector3 v = startPoint - vector * (num2 + outerPadding);
		Vector3 v2 = startPoint + vector * (num2 + outerPadding);
		float num3 = Path.Length + num;
		for (float num4 = 0f; num4 < num3; num4 += num)
		{
			Vector3 vector2 = (Spline ? Path.GetPointCubicHermite(num4) : Path.GetPoint(num4));
			float opacity = 1f;
			float radius = GetRadius(num4, Path.Length, num2, randomScale, scaleWidthWithLength);
			if (!Path.Circular)
			{
				float a = (startPoint - vector2).Magnitude2D();
				float b = (endPoint - vector2).Magnitude2D();
				opacity = Mathf.InverseLerp(0f, num2, Mathf.Min(a, b));
			}
			startTangent = Path.GetTangent(num4);
			normalized = startTangent.XZ3D().normalized;
			vector = rot90 * normalized;
			Ray ray = new Ray(vector2, startTangent);
			Vector3 vector3 = vector2 - vector * (radius + outerPadding);
			Vector3 vector4 = vector2 + vector * (radius + outerPadding);
			float yn = TerrainMeta.NormalizeY(vector2.y);
			topomap.ForEach(v, v2, vector3, vector4, delegate(int x, int z)
			{
				Vector3 vector5 = TerrainMeta.Denormalize(new Vector3(topomap.Coordinate(x), z: topomap.Coordinate(z), y: yn));
				Vector3 vector6 = RayEx.ClosestPoint(ray, vector5);
				float value = (vector5 - vector6).Magnitude2D();
				if (Mathf.InverseLerp(radius + outerPadding, radius - innerPadding, value) * opacity > 0.3f)
				{
					topomap.AddTopology(x, z, Topology);
				}
			});
			v = vector3;
			v2 = vector4;
		}
	}

	public void AdjustPlacementMap(float width)
	{
		TerrainPlacementMap placementmap = TerrainMeta.PlacementMap;
		float num = 1f;
		float radius = width * 0.5f;
		Vector3 startPoint = Path.GetStartPoint();
		Path.GetEndPoint();
		Vector3 startTangent = Path.GetStartTangent();
		Vector3 normalized = startTangent.XZ3D().normalized;
		Vector3 vector = rot90 * normalized;
		Vector3 v = startPoint - vector * radius;
		Vector3 v2 = startPoint + vector * radius;
		float num2 = Path.Length + num;
		for (float num3 = 0f; num3 < num2; num3 += num)
		{
			Vector3 vector2 = (Spline ? Path.GetPointCubicHermite(num3) : Path.GetPoint(num3));
			startTangent = Path.GetTangent(num3);
			normalized = startTangent.XZ3D().normalized;
			vector = rot90 * normalized;
			Ray ray = new Ray(vector2, startTangent);
			Vector3 vector3 = vector2 - vector * radius;
			Vector3 vector4 = vector2 + vector * radius;
			float yn = TerrainMeta.NormalizeY(vector2.y);
			placementmap.ForEach(v, v2, vector3, vector4, delegate(int x, int z)
			{
				Vector3 vector5 = TerrainMeta.Denormalize(new Vector3(placementmap.Coordinate(x), z: placementmap.Coordinate(z), y: yn));
				Vector3 vector6 = RayEx.ClosestPoint(ray, vector5);
				if ((vector5 - vector6).Magnitude2D() <= radius)
				{
					placementmap.SetBlocked(x, z);
				}
			});
			v = vector3;
			v2 = vector4;
		}
	}

	public List<PathMeshTemplate> CreateMeshTemplates(Mesh[] meshes, float normalSmoothing, bool snapToTerrain, bool snapStartToTerrain, bool snapEndToTerrain, bool scaleWidthWithLength = false, bool topAligned = false, int roundVertices = 0)
	{
		MeshCache.Data[] array = new MeshCache.Data[meshes.Length];
		for (int i = 0; i < meshes.Length; i++)
		{
			array[i] = MeshCache.Get(meshes[i]);
		}
		Bounds bounds = meshes[^1].bounds;
		_ = bounds.min;
		_ = bounds.size;
		float num = Width / bounds.size.x;
		List<PathMeshTemplate> list = new List<PathMeshTemplate>();
		int num2 = (int)(Path.Length / (num * bounds.size.z));
		int num3 = 5;
		float num4 = Path.Length / (float)num2;
		_ = RandomScale;
		_ = MeshOffset;
		_ = Width;
		_ = array[0].vertices.Length;
		_ = array[0].triangles.Length;
		TerrainHeightMap heightMap = TerrainMeta.HeightMap;
		for (int j = 0; j < num2; j += num3)
		{
			float distance = (float)j * num4 + 0.5f * (float)num3 * num4;
			Vector3 vector = (Spline ? Path.GetPointCubicHermite(distance) : Path.GetPoint(distance));
			int segmentCount = Math.Min(num3, num2 - j);
			PathMeshTemplate item = new PathMeshTemplate
			{
				srcData = array,
				normalSmoothing = normalSmoothing,
				snapToTerrain = snapToTerrain,
				snapStartToTerrain = snapStartToTerrain,
				snapEndToTerrain = snapEndToTerrain,
				scaleWidthWithLength = scaleWidthWithLength,
				topAligned = topAligned,
				roundVertices = roundVertices,
				PathList = this,
				heightmap = heightMap,
				Position = vector,
				origin = vector,
				stepIndex = j,
				segmentCount = segmentCount,
				stepSize = num4,
				dstData = new Mesh.MeshDataArray[array.Length],
				outputMeshes = new Mesh[array.Length]
			};
			list.Add(item);
		}
		return list;
	}

	public List<MeshObject> CreateMeshOld(Mesh[] meshes, float normalSmoothing, bool snapToTerrain, bool snapStartToTerrain, bool snapEndToTerrain, bool scaleWidthWithLength = false, bool topAligned = false, int roundVertices = 0)
	{
		MeshCache.Data[] array = new MeshCache.Data[meshes.Length];
		MeshData[] array2 = new MeshData[meshes.Length];
		for (int i = 0; i < meshes.Length; i++)
		{
			array[i] = MeshCache.Get(meshes[i]);
			array2[i] = new MeshData();
		}
		MeshData[] array3 = array2;
		for (int j = 0; j < array3.Length; j++)
		{
			array3[j].AllocMinimal();
		}
		Bounds bounds = meshes[^1].bounds;
		Vector3 min = bounds.min;
		Vector3 size = bounds.size;
		float num = Width / bounds.size.x;
		List<MeshObject> list = new List<MeshObject>();
		int num2 = (int)(Path.Length / (num * bounds.size.z));
		int num3 = 5;
		float num4 = Path.Length / (float)num2;
		float randomScale = RandomScale;
		float meshOffset = MeshOffset;
		float baseRadius = Width * 0.5f;
		_ = array[0].vertices.Length;
		_ = array[0].triangles.Length;
		TerrainHeightMap heightMap = TerrainMeta.HeightMap;
		for (int k = 0; k < num2; k += num3)
		{
			float distance = (float)k * num4 + 0.5f * (float)num3 * num4;
			Vector3 vector = (Spline ? Path.GetPointCubicHermite(distance) : Path.GetPoint(distance));
			for (int l = 0; l < num3 && k + l < num2; l++)
			{
				float num5 = (float)(k + l) * num4;
				for (int m = 0; m < meshes.Length; m++)
				{
					MeshCache.Data data = array[m];
					MeshData meshData = array2[m];
					int count = meshData.vertices.Count;
					for (int n = 0; n < data.vertices.Length; n++)
					{
						Vector2 item = data.uv[n];
						Vector3 vector2 = data.vertices[n];
						Vector3 vector3 = data.normals[n];
						Vector4 item2 = data.tangents[n];
						float t = (vector2.x - min.x) / size.x;
						float num6 = vector2.y - min.y;
						if (topAligned)
						{
							num6 -= size.y;
						}
						float num7 = (vector2.z - min.z) / size.z;
						float num8 = num5 + num7 * num4;
						Vector3 obj = (Spline ? Path.GetPointCubicHermite(num8) : Path.GetPoint(num8));
						Vector3 tangent = Path.GetTangent(num8);
						Vector3 normalized = tangent.XZ3D().normalized;
						Vector3 vector4 = rot90 * normalized;
						Vector3 vector5 = Vector3.Cross(tangent, vector4);
						Quaternion quaternion = Quaternion.LookRotation(normalized, vector5);
						float radius = GetRadius(num8, Path.Length, baseRadius, randomScale, scaleWidthWithLength);
						Vector3 vector6 = obj - vector4 * radius;
						Vector3 vector7 = obj + vector4 * radius;
						if (snapToTerrain)
						{
							vector6.y = heightMap.GetHeight(vector6);
							vector7.y = heightMap.GetHeight(vector7);
						}
						vector6 += vector5 * meshOffset;
						vector7 += vector5 * meshOffset;
						vector2 = Vector3.Lerp(vector6, vector7, t);
						if ((snapStartToTerrain && num8 < 0.1f) || (snapEndToTerrain && num8 > Path.Length - 0.1f))
						{
							vector2.y = heightMap.GetHeight(vector2);
						}
						else
						{
							vector2.y += num6;
						}
						vector2 -= vector;
						vector3 = quaternion * vector3;
						Vector3 vector8 = new Vector3(item2.x, item2.y, item2.z);
						vector8 = quaternion * vector8;
						item2.Set(vector8.x, vector8.y, vector8.z, item2.w);
						if (normalSmoothing > 0f)
						{
							vector3 = Vector3.Slerp(vector3, Vector3.up, normalSmoothing);
						}
						if (roundVertices > 0)
						{
							vector2.x = (float)Math.Round(vector2.x, roundVertices);
							vector2.y = (float)Math.Round(vector2.y, roundVertices);
							vector2.z = (float)Math.Round(vector2.z, roundVertices);
						}
						meshData.vertices.Add(vector2);
						meshData.normals.Add(vector3);
						meshData.tangents.Add(item2);
						meshData.uv.Add(item);
					}
					for (int num9 = 0; num9 < data.triangles.Length; num9++)
					{
						int num10 = data.triangles[num9];
						meshData.triangles.Add(count + num10);
					}
				}
			}
			list.Add(MeshObject.CreateAndRecalculateUVs(vector, array2));
			array3 = array2;
			for (int j = 0; j < array3.Length; j++)
			{
				array3[j].Clear();
			}
		}
		array3 = array2;
		for (int j = 0; j < array3.Length; j++)
		{
			array3[j].Free();
		}
		return list;
	}

	public List<MeshObject> CreateMesh(Mesh[] meshes, float normalSmoothing, bool snapToTerrain, bool snapStartToTerrain, bool snapEndToTerrain, bool scaleWidthWithLength = false, bool topAligned = false, int roundVertices = 0)
	{
		List<MeshObject> list = new List<MeshObject>();
		if (meshes.Length == 0)
		{
			return list;
		}
		foreach (PathMeshTemplate item in CreateMeshTemplates(meshes, normalSmoothing, snapToTerrain, snapStartToTerrain, snapEndToTerrain, scaleWidthWithLength, topAligned, roundVertices))
		{
			item.Generate();
			list.Add(new MeshObject
			{
				Meshes = item.outputMeshes,
				Position = item.Position
			});
		}
		return list;
	}

	public List<MeshObject> CreateMeshRiverInterior(Mesh[] meshes, bool snapToTerrain, bool snapStartToTerrain, bool snapEndToTerrain, Bounds bounds, bool scaleWidthWithLength = false, int roundVertices = 0)
	{
		MeshCache.Data[] array = new MeshCache.Data[meshes.Length];
		for (int i = 0; i < meshes.Length; i++)
		{
			array[i] = MeshCache.Get(meshes[i]);
		}
		MeshData meshData = new MeshData();
		meshData.vertices = Pool.Get<List<Vector3>>();
		meshData.triangles = Pool.Get<List<int>>();
		meshData.uv = Pool.Get<List<Vector2>>();
		MeshData meshData2 = new MeshData();
		meshData2.vertices = Pool.Get<List<Vector3>>();
		meshData2.triangles = Pool.Get<List<int>>();
		meshData2.uv = Pool.Get<List<Vector2>>();
		Dictionary<WeldVertex, int> dictionary = new Dictionary<WeldVertex, int>();
		Vector3 min = bounds.min;
		Vector3 size = bounds.size;
		float num = Width / bounds.size.x;
		int num2 = (int)(Path.Length / (num * bounds.size.z));
		int num3 = 5;
		float num4 = Path.Length / (float)num2;
		float randomScale = RandomScale;
		float meshOffset = MeshOffset;
		float baseRadius = Width * 0.5f;
		TerrainHeightMap heightMap = TerrainMeta.HeightMap;
		List<MeshObject> list = new List<MeshObject>();
		for (int j = 0; j < num2; j += num3)
		{
			float distance = (float)j * num4 + 0.5f * (float)num3 * num4;
			Vector3 vector = (Spline ? Path.GetPointCubicHermite(distance) : Path.GetPoint(distance));
			for (int k = 0; k < num3 && j + k < num2; k++)
			{
				float num5 = (float)(j + k) * num4;
				bool flag = j == 0;
				bool flag2 = k == 0;
				bool flag3 = j == num2 - 1;
				bool flag4 = k == num3 - 1 || j + k == num2 - 1;
				int num6 = (flag2 ? 4 : 3);
				for (int l = 0; l < num6; l++)
				{
					int num7 = l;
					MeshCache.Data data = array[num7];
					MeshData meshData3 = meshData;
					int count = meshData3.vertices.Count;
					float x = ((num7 == 2 && !(flag4 && flag3)) ? 1f : ((!(num7 == 3 && flag2) || flag) ? 0f : 1f));
					float y = ((num7 == 0) ? 1f : 0f);
					for (int m = 0; m < data.vertices.Length; m++)
					{
						Vector3 vector2 = data.vertices[m];
						float t = (vector2.x - min.x) / size.x;
						float num8 = vector2.y - min.y;
						float num9 = (vector2.z - min.z) / size.z;
						float num10 = num5 + num9 * num4;
						Vector3 obj = (Spline ? Path.GetPointCubicHermite(num10) : Path.GetPoint(num10));
						Vector3 tangent = Path.GetTangent(num10);
						Vector3 normalized = tangent.XZ3D().normalized;
						Vector3 vector3 = rot90 * normalized;
						Vector3 vector4 = Vector3.Cross(tangent, vector3);
						float radius = GetRadius(num10, Path.Length, baseRadius, randomScale, scaleWidthWithLength);
						Vector3 vector5 = obj - vector3 * radius;
						Vector3 vector6 = obj + vector3 * radius;
						if (snapToTerrain)
						{
							vector5.y = heightMap.GetHeight(vector5);
							vector6.y = heightMap.GetHeight(vector6);
						}
						vector5 += vector4 * meshOffset;
						vector6 += vector4 * meshOffset;
						vector2 = Vector3.Lerp(vector5, vector6, t);
						if ((snapStartToTerrain && num10 < 0.1f) || (snapEndToTerrain && num10 > Path.Length - 0.1f))
						{
							vector2.y = heightMap.GetHeight(vector2);
						}
						else
						{
							vector2.y += num8;
						}
						vector2 -= vector;
						if (roundVertices > 0)
						{
							vector2.x = (float)Math.Round(vector2.x, roundVertices);
							vector2.y = (float)Math.Round(vector2.y, roundVertices);
							vector2.z = (float)Math.Round(vector2.z, roundVertices);
						}
						meshData3.vertices.Add(vector2);
						meshData3.uv.Add(new Vector2(x, y));
					}
					for (int n = 0; n < data.triangles.Length; n++)
					{
						int num11 = data.triangles[n];
						meshData3.triangles.Add(count + num11);
					}
				}
			}
			for (int num12 = 0; num12 < meshData.triangles.Count; num12++)
			{
				int index = meshData.triangles[num12];
				Vector3 item = meshData.vertices[index];
				Vector2 item2 = meshData.uv[index];
				WeldVertex key = default(WeldVertex);
				key.x = item.x;
				key.y = item.y;
				key.z = item.z;
				key.alwaysUnderwater = meshData.uv[index].x;
				key.topSurface = meshData.uv[index].y;
				if (!dictionary.TryGetValue(key, out var value))
				{
					value = meshData2.vertices.Count;
					dictionary.Add(key, value);
					meshData2.vertices.Add(item);
					meshData2.uv.Add(item2);
				}
				meshData2.triangles.Add(value);
			}
			list.Add(MeshObject.CreateAndRecalculateUVs(vector, new MeshData[1] { meshData2 }));
			meshData.Clear();
			meshData2.Clear();
			dictionary.Clear();
		}
		meshData.Free();
		meshData2.Free();
		return list;
	}

	public static float GetRadius(float distance, float length, float baseRadius, float randomScale, bool scaleWidthWithLength)
	{
		if (scaleWidthWithLength)
		{
			float t = Mathf.Sqrt(Mathf.Max(0f, length - distance) / 100f);
			float num = ((length > 0f) ? Mathf.Lerp(3f, 1f, t) : 1f);
			float t2 = distance / 1000f;
			float num2 = Mathf.Lerp(1f, 8f, t2);
			baseRadius = baseRadius * num2 * num;
		}
		return Mathf.Lerp(baseRadius, baseRadius * randomScale, Noise.SimplexUnsigned(distance * 0.005f));
	}

	public static float GetDepth(float distance, float length, float baseDepth, float randomScale, bool scaleWidthWithLength)
	{
		if (scaleWidthWithLength)
		{
			float t = distance / 1000f;
			float num = Mathf.Lerp(1f, 3f, t);
			baseDepth *= num;
		}
		return Mathf.Lerp(baseDepth, baseDepth * randomScale, Noise.SimplexUnsigned(distance * 0.005f));
	}
}
