using System;
using System.Collections.Generic;
using UnityEngine;

public class GenerateRiverLayout : ProceduralComponent
{
	public const float Width = 8f;

	public const float InnerPadding = 1f;

	public const float OuterPadding = 1f;

	public const float InnerFade = 16f;

	public const float OuterFade = 64f;

	public const float RandomScale = 0.75f;

	public const float MeshOffset = -0.5f;

	public const float TerrainOffset = -1.5f;

	private static Quaternion rot90 = Quaternion.Euler(0f, 90f, 0f);

	public override void Process(uint seed)
	{
		if (World.Networked)
		{
			TerrainMeta.Path.Rivers.Clear();
			TerrainMeta.Path.Rivers.AddRange(World.GetPaths("River"));
		}
		else
		{
			if (!World.Config.Rivers)
			{
				return;
			}
			List<PathList> list = new List<PathList>();
			TerrainHeightMap heightMap = TerrainMeta.HeightMap;
			TerrainTopologyMap topologyMap = TerrainMeta.TopologyMap;
			TerrainBiomeMap biomeMap = TerrainMeta.BiomeMap;
			List<Vector3> list2 = new List<Vector3>();
			int num = 3;
			if (World.Size <= 4000)
			{
				num = 2;
			}
			Vector3[] array = new Vector3[4]
			{
				new Vector3(-1f, 0f, -1f),
				new Vector3(-1f, 0f, 1f),
				new Vector3(1f, 0f, -1f),
				new Vector3(1f, 0f, 1f)
			};
			for (float num2 = TerrainMeta.Center.z + 250f; num2 < TerrainMeta.Max.z - 750f; num2 += 5f)
			{
				for (float num3 = TerrainMeta.Center.x + 250f; num3 < TerrainMeta.Max.x - 750f; num3 += 5f)
				{
					Vector3[] array2 = array;
					for (int i = 0; i < array2.Length; i++)
					{
						Vector3 vector = array2[i];
						Vector3 vector2 = new Vector3(vector.x * num3, 0f, vector.z * num2);
						float num4 = (vector2.y = heightMap.GetHeight(vector2));
						if (vector2.y <= 15f)
						{
							continue;
						}
						Vector3 normal = heightMap.GetNormal(vector2);
						if (normal.y <= 0.01f || normal.y >= 0.99f)
						{
							continue;
						}
						bool flag = false;
						foreach (PathList item in list)
						{
							Vector3[] points = item.Path.Points;
							foreach (Vector3 vector3 in points)
							{
								if ((vector2 - vector3).SqrMagnitude2D() < 270400f)
								{
									flag = true;
									break;
								}
							}
							if (flag)
							{
								break;
							}
						}
						if (flag)
						{
							continue;
						}
						Vector2 normalized = normal.XZ2D().normalized;
						float value = Vector3.Angle(Vector3.up, normal);
						list2.Add(vector2);
						float baseRadius = 4f;
						int num5 = 0;
						for (int k = 0; k < 5000; k++)
						{
							int num6 = k * 4;
							Vector2 vector4 = normalized.Rotate(Mathf.Sin((float)num6 * (MathF.PI / 180f) * 0.5f) * Mathf.InverseLerp(30f, 10f, value) * 60f);
							vector2.x += vector4.x * 4f;
							vector2.z += vector4.y * 4f;
							bool flag2 = false;
							for (int l = 0; l < list2.Count - 10; l++)
							{
								Vector3 vector5 = new Line(list2[l], list2[l + 1]).ClosestPoint(vector2);
								if ((vector2 - vector5).SqrMagnitude2D() < 16900f)
								{
									flag2 = true;
									break;
								}
							}
							if (flag2)
							{
								break;
							}
							float height = heightMap.GetHeight(vector2);
							if (height > num4 + 32f)
							{
								break;
							}
							float num7 = Mathf.Min(height, num4);
							float t = Mathf.Lerp(0.15f, 0.95f, Mathf.InverseLerp(10f, 0f, num7));
							vector2.y = Mathf.Lerp(vector2.y, num7, t);
							float radius = PathList.GetRadius(num6, 0f, baseRadius, 0.75f, scaleWidthWithLength: true);
							float radius2 = PathList.GetRadius(num6, num6, baseRadius, 0.75f, scaleWidthWithLength: true);
							int num8 = Mathf.RoundToInt(radius2 / 4f);
							Vector3 vector6 = new Vector3(vector4.x, 0f, vector4.y);
							Vector3 vector7 = vector6 * (radius * 1.5f);
							Vector3 vector8 = vector6 * (radius2 + 1f + 64f);
							Vector3 vector9 = rot90 * vector6;
							Vector3 vector10 = vector9 * (radius * 1.5f);
							Vector3 vector11 = vector9 * (radius2 + 1f + 64f);
							int topology = topologyMap.GetTopology(vector2, radius + 1f + 64f);
							int num9 = topologyMap.GetTopology(vector2) & topologyMap.GetTopology(vector2 - vector7) & topologyMap.GetTopology(vector2 + vector7) & topologyMap.GetTopology(vector2 + vector8) & topologyMap.GetTopology(vector2 - vector10) & topologyMap.GetTopology(vector2 - vector11) & topologyMap.GetTopology(vector2 + vector10) & topologyMap.GetTopology(vector2 + vector11);
							int topology2 = topologyMap.GetTopology(vector2);
							int num10 = 3742724;
							int num11 = 128;
							int num12 = 128;
							if ((topology & num10) != 0)
							{
								break;
							}
							if ((num9 & num11) != 0)
							{
								list2.Add(vector2);
								if (list2.Count >= 62)
								{
									PathList pathList = new PathList("River " + (TerrainMeta.Path.Rivers.Count + list.Count), list2.ToArray());
									pathList.Spline = true;
									pathList.Width = 8f;
									pathList.InnerPadding = 1f;
									pathList.OuterPadding = 1f;
									pathList.InnerFade = 16f;
									pathList.OuterFade = 64f;
									pathList.RandomScale = 0.75f;
									pathList.MeshOffset = -0.5f;
									pathList.TerrainOffset = -1.5f;
									pathList.Topology = 16384;
									pathList.Splat = 128;
									pathList.Start = true;
									pathList.End = true;
									list.Add(pathList);
								}
								break;
							}
							if ((topology2 & num12) != 0 || vector2.y < 0f)
							{
								if (num5++ >= num8)
								{
									break;
								}
							}
							else if (num5 > 0)
							{
								break;
							}
							if (k % 4 == 0)
							{
								list2.Add(vector2);
							}
							normal = heightMap.GetNormal(vector2);
							value = Vector3.Angle(Vector3.up, normal);
							normalized = Vector2.Lerp(normalized, normal.XZ2D().normalized, 0.025f).normalized;
							num4 = num7;
						}
						list2.Clear();
					}
				}
			}
			list.Sort((PathList a, PathList b) => b.Path.Points.Length.CompareTo(a.Path.Points.Length));
			int num13 = (int)(World.Size / 16);
			bool[,] array3 = new bool[num13, num13];
			int num14 = 0;
			for (int m = 0; m < list.Count; m++)
			{
				PathList pathList2 = list[m];
				bool flag3 = biomeMap.GetBiomeMaxType(pathList2.Path.GetEndPoint()) == 16;
				if (num14 >= num && !flag3)
				{
					list.RemoveAt(m--);
					continue;
				}
				bool flag4 = false;
				for (int n = 0; n < m; n++)
				{
					PathList pathList3 = list[n];
					Vector3[] array2 = pathList2.Path.Points;
					foreach (Vector3 vector12 in array2)
					{
						Vector3[] points = pathList3.Path.Points;
						foreach (Vector3 vector13 in points)
						{
							if ((vector12 - vector13).sqrMagnitude < 270400f)
							{
								list.RemoveAt(m--);
								flag4 = true;
							}
							if (flag4)
							{
								break;
							}
						}
						if (flag4)
						{
							break;
						}
					}
					if (flag4)
					{
						break;
					}
				}
				if (flag4)
				{
					continue;
				}
				for (int num15 = 0; num15 < pathList2.Path.Points.Length; num15++)
				{
					Vector3 vector14 = pathList2.Path.Points[num15];
					int num16 = Mathf.Clamp((int)(TerrainMeta.NormalizeX(vector14.x) * (float)num13), 0, num13 - 1);
					int num17 = Mathf.Clamp((int)(TerrainMeta.NormalizeZ(vector14.z) * (float)num13), 0, num13 - 1);
					if (array3[num17, num16])
					{
						list.RemoveAt(m--);
						flag4 = true;
						break;
					}
				}
				if (flag4)
				{
					continue;
				}
				int num18 = -1;
				int num19 = -1;
				for (int num20 = 0; num20 < pathList2.Path.Points.Length; num20++)
				{
					Vector3 vector15 = pathList2.Path.Points[num20];
					int num21 = Mathf.Clamp((int)(TerrainMeta.NormalizeX(vector15.x) * (float)num13), 0, num13 - 1);
					int num22 = Mathf.Clamp((int)(TerrainMeta.NormalizeZ(vector15.z) * (float)num13), 0, num13 - 1);
					if (num18 != -1)
					{
						array3[num22, num18] = true;
					}
					if (num19 != -1)
					{
						array3[num19, num21] = true;
					}
					array3[num22, num21] = true;
					num18 = num21;
					num19 = num22;
				}
				if (!flag3)
				{
					num14++;
				}
			}
			for (int num23 = 0; num23 < list.Count; num23++)
			{
				list[num23].Name = "River " + (TerrainMeta.Path.Rivers.Count + num23);
			}
			foreach (PathList item2 in list)
			{
				item2.Path.Smoothen(4, new Vector3(1f, 0f, 1f));
				item2.Path.Smoothen(8, new Vector3(0f, 1f, 0f));
				item2.Path.Resample(7.5f);
				item2.Path.RecalculateTangents();
			}
			TerrainMeta.Path.Rivers.AddRange(list);
		}
	}
}
