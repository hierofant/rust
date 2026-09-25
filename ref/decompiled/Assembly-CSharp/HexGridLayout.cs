using System;
using UnityEngine;

public static class HexGridLayout
{
	private const float Sqrt3 = 1.7320508f;

	public static readonly Vector2Int[] NeighbourDirs = new Vector2Int[6]
	{
		new Vector2Int(0, 1),
		new Vector2Int(-1, 1),
		new Vector2Int(-1, 0),
		new Vector2Int(0, -1),
		new Vector2Int(1, -1),
		new Vector2Int(1, 0)
	};

	private static int QBias(float radius)
	{
		return Mathf.CeilToInt(TerrainMeta.Size.x / (3f * radius)) + 1;
	}

	public static int Width(float radius)
	{
		return QBias(radius) + Mathf.CeilToInt(TerrainMeta.Size.x / (1.7320508f * radius)) + 2;
	}

	public static int Height(float radius)
	{
		return Mathf.CeilToInt(TerrainMeta.Size.x / (1.5f * radius)) + 2;
	}

	public static int CellCount(float radius)
	{
		return Width(radius) * Height(radius);
	}

	public static int AxialToCell(int q, int r, float radius)
	{
		int num = q + QBias(radius);
		int num2 = Width(radius);
		if (num < 0 || num >= num2 || r < 0 || r >= Height(radius))
		{
			return -1;
		}
		return r * num2 + num;
	}

	public static void CellToAxial(int cell, float radius, out int q, out int r)
	{
		int num = Width(radius);
		q = cell % num - QBias(radius);
		r = cell / num;
	}

	public static int WorldToCell(Vector3 worldPos, float radius, Vector2 offset)
	{
		float num = worldPos.x - TerrainMeta.Position.x - offset.x;
		float num2 = worldPos.z - TerrainMeta.Position.z - offset.y;
		float num3 = (0.57735026f * num - 1f / 3f * num2) / radius;
		float num4 = 2f / 3f * num2 / radius;
		float num5 = 0f - num3 - num4;
		float num6 = Mathf.Round(num3);
		float num7 = Mathf.Round(num5);
		float num8 = Mathf.Round(num4);
		float num9 = Mathf.Abs(num6 - num3);
		float num10 = Mathf.Abs(num7 - num5);
		float num11 = Mathf.Abs(num8 - num4);
		if (num9 > num10 && num9 > num11)
		{
			num6 = 0f - num7 - num8;
		}
		else if (num11 > num10)
		{
			num8 = 0f - num6 - num7;
		}
		return AxialToCell((int)num6, (int)num8, radius);
	}

	public static Vector3 CellCenter(int cell, float radius, Vector2 offset)
	{
		CellToAxial(cell, radius, out var q, out var r);
		float num = radius * 1.7320508f * ((float)q + (float)r * 0.5f);
		return new Vector3(z: radius * 1.5f * (float)r + TerrainMeta.Position.z + offset.y, x: num + TerrainMeta.Position.x + offset.x, y: 0f);
	}

	public static Vector3 Corner(Vector3 center, float radius, int i)
	{
		float f = (60f * (float)i + 30f) * (MathF.PI / 180f);
		return new Vector3(center.x + radius * Mathf.Cos(f), 0f, center.z + radius * Mathf.Sin(f));
	}
}
