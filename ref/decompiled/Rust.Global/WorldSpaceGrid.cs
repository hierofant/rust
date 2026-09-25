using UnityEngine;

public class WorldSpaceGrid<T>
{
	public T[] Cells;

	public float CellSize;

	public float CellSizeHalf;

	public float CellSizeInverse;

	public float CellArea;

	public int CellCount;

	public float CellCountOffset;

	public T this[Vector3 worldPos]
	{
		get
		{
			return this[WorldToGridCoords(worldPos)];
		}
		set
		{
			this[WorldToGridCoords(worldPos)] = value;
		}
	}

	public T this[Vector2i cellCoords]
	{
		get
		{
			return this[cellCoords.x, cellCoords.y];
		}
		set
		{
			this[cellCoords.x, cellCoords.y] = value;
		}
	}

	public T this[int x, int y]
	{
		get
		{
			return Cells[y * CellCount + x];
		}
		set
		{
			Cells[y * CellCount + x] = value;
		}
	}

	public T this[int index]
	{
		get
		{
			return Cells[index];
		}
		set
		{
			Cells[index] = value;
		}
	}

	public WorldSpaceGrid(float gridSize, float cellSize, WorldSpaceGrid.RoundingMode rounding = WorldSpaceGrid.RoundingMode.Up)
	{
		CellSize = cellSize;
		CellSizeHalf = cellSize * 0.5f;
		CellSizeInverse = 1f / cellSize;
		CellArea = cellSize * cellSize;
		CellCount = WorldSpaceGrid.CalculateCellCount(gridSize, cellSize, rounding);
		CellCountOffset = (float)CellCount * 0.5f - 0.5f;
		Cells = new T[CellCount * CellCount];
	}

	public Vector2i IndexToGridCoords(int index)
	{
		int num = index / CellCount;
		return new Vector2i(index - num * CellCount, num);
	}

	public Vector3 IndexToWorldCoords(int index)
	{
		return GridToWorldCoords(IndexToGridCoords(index));
	}

	public Vector2i WorldToGridCoords(Vector3 worldPos)
	{
		int v = Mathf.RoundToInt(worldPos.x * CellSizeInverse + CellCountOffset);
		int v2 = Mathf.RoundToInt(worldPos.z * CellSizeInverse + CellCountOffset);
		int x = Mathx.Clamp(v, 0, CellCount - 1);
		v2 = Mathx.Clamp(v2, 0, CellCount - 1);
		return new Vector2i(x, v2);
	}

	public Vector3 GridToWorldCoords(Vector2i cellPos)
	{
		float x = ((float)cellPos.x - CellCountOffset) * CellSize;
		float z = ((float)cellPos.y - CellCountOffset) * CellSize;
		return new Vector3(x, 0f, z);
	}

	public void Copy(WorldSpaceGrid<T> other)
	{
		for (int i = 0; i < CellCount; i++)
		{
			for (int j = 0; j < CellCount; j++)
			{
				this[i, j] = other[i, j];
			}
		}
	}
}
public class WorldSpaceGrid
{
	public enum RoundingMode
	{
		Up,
		Down
	}

	public static int CalculateCellCount(float gridSize, float cellSize, RoundingMode rounding = RoundingMode.Up)
	{
		float num = 1f / cellSize;
		float f = Mathf.Max(gridSize, 1000f) * num;
		if (rounding == RoundingMode.Up)
		{
			return Mathf.CeilToInt(f);
		}
		return Mathf.FloorToInt(f);
	}

	public static Vector3 ClosestGridCell(Vector3 worldPos, float gridSize, float cellSize, RoundingMode rounding = RoundingMode.Up)
	{
		float num = 1f / cellSize;
		int num2 = CalculateCellCount(gridSize, cellSize, rounding);
		float num3 = (float)num2 * 0.5f - 0.5f;
		int v = Mathf.RoundToInt(worldPos.x * num + num3);
		int v2 = Mathf.RoundToInt(worldPos.z * num + num3);
		int num4 = Mathx.Clamp(v, 0, num2 - 1);
		v2 = Mathx.Clamp(v2, 0, num2 - 1);
		float x = ((float)num4 - num3) * cellSize;
		float z = ((float)v2 - num3) * cellSize;
		return new Vector3(x, 0f, z);
	}
}
