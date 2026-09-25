using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class SparseGrid<T>
{
	private Dictionary<(int x, int z), HashSet<T>> grid = new Dictionary<(int, int), HashSet<T>>();

	private int cellSize;

	public SparseGrid(int cellSize = 32)
	{
		this.cellSize = cellSize;
	}

	public void Add(Vector3 position, T value)
	{
		(int, int) cellKey = GetCellKey(position);
		if (grid.TryGetValue(cellKey, out var value2))
		{
			value2.Add(value);
			return;
		}
		value2 = Pool.Get<HashSet<T>>();
		value2.Add(value);
		grid[cellKey] = value2;
	}

	public void Remove(Vector3 position, T value)
	{
		(int, int) cellKey = GetCellKey(position);
		if (grid.TryGetValue(cellKey, out var value2))
		{
			value2.Remove(value);
			if (value2.Count == 0)
			{
				Pool.FreeUnmanaged(ref value2);
				grid.Remove(cellKey);
			}
		}
	}

	public void GetNeighboors(Vector3 position, List<T> values)
	{
		(int, int) cellKey = GetCellKey(position);
		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				(int, int) key = (cellKey.Item1 + i, cellKey.Item2 + j);
				if (grid.TryGetValue(key, out var value))
				{
					values.AddRange(value);
				}
			}
		}
	}

	public void GetInBounds(Bounds bounds, List<T> values)
	{
		(int, int) cellKey = GetCellKey(bounds.min);
		(int, int) cellKey2 = GetCellKey(bounds.max);
		var (i, _) = cellKey;
		for (; i <= cellKey2.Item1; i++)
		{
			for (int j = cellKey.Item2; j <= cellKey2.Item2; j++)
			{
				(int, int) key = (i, j);
				if (grid.TryGetValue(key, out var value))
				{
					values.AddRange(value);
				}
			}
		}
	}

	public (int x, int z) GetCellKey(Vector3 position)
	{
		int item = Mathf.FloorToInt(position.x / (float)cellSize);
		int item2 = Mathf.FloorToInt(position.z / (float)cellSize);
		return (x: item, z: item2);
	}

	public void Clear()
	{
		foreach (HashSet<T> value in grid.Values)
		{
			HashSet<T> obj = value;
			Pool.FreeUnmanaged(ref obj);
		}
		grid.Clear();
	}
}
