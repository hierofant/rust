using System;
using Unity.Collections;
using Unity.Mathematics;

namespace Spatial;

[Obsolete]
public struct NativeParallelGrid<T> where T : unmanaged, IEquatable<T>
{
	private const float DefaultWorldSize = 8096f;

	private const int DefaultCellSize = 32;

	private const int DefaultLookupCapacity = 512;

	private const int DefaultNodeCapacity = 16;

	private Allocator gridAllocator;

	private NativeArray<NativeParallelHashSet<T>> Cells;

	private NativeParallelHashMap<T, NativeParallelHashSet<T>> Lookup;

	private float2 Center;

	private int CellSize;

	private int CellColumns;

	private int TotalCells;

	public NativeParallelGrid(Allocator gridAllocator, int cellSize = 32, float worldSize = 8096f, bool preAllocateCells = false)
	{
		this.gridAllocator = gridAllocator;
		Lookup = new NativeParallelHashMap<T, NativeParallelHashSet<T>>(512, gridAllocator);
		CellSize = cellSize;
		CellColumns = (int)(worldSize / (float)CellSize + 0.5f);
		TotalCells = CellColumns * CellColumns;
		Cells = new NativeArray<NativeParallelHashSet<T>>(TotalCells, gridAllocator);
		if (preAllocateCells)
		{
			for (int i = 0; i < TotalCells; i++)
			{
				Cells[i] = new NativeParallelHashSet<T>(16, gridAllocator);
			}
		}
		Center = new float2(worldSize * 0.5f, worldSize * 0.5f);
	}

	public int Query(float x, float y, float radius, NativeArray<T> result)
	{
		int num = Clamp((x + Center.x - radius) / (float)CellSize);
		int num2 = Clamp((x + Center.x + radius) / (float)CellSize);
		int num3 = Clamp((y + Center.y - radius) / (float)CellSize);
		int num4 = Clamp((y + Center.y + radius) / (float)CellSize);
		int num5 = 0;
		for (int i = num; i <= num2; i++)
		{
			for (int j = num3; j <= num4; j++)
			{
				if (!GetNodeByIndex(i, j).IsCreated)
				{
					continue;
				}
				foreach (T item in GetNodeByIndex(i, j))
				{
					result[num5] = item;
					num5++;
					if (num5 >= result.Length)
					{
						return num5;
					}
				}
			}
		}
		return num5;
	}

	public void Query(float x, float y, float radius, NativeList<T> result)
	{
		if (!result.IsCreated)
		{
			return;
		}
		int num = Clamp((x + Center.x - radius) / (float)CellSize);
		int num2 = Clamp((x + Center.x + radius) / (float)CellSize);
		int num3 = Clamp((y + Center.y - radius) / (float)CellSize);
		int num4 = Clamp((y + Center.y + radius) / (float)CellSize);
		for (int i = num; i <= num2; i++)
		{
			for (int j = num3; j <= num4; j++)
			{
				if (!GetNodeByIndex(i, j).IsCreated)
				{
					continue;
				}
				foreach (T item in GetNodeByIndex(i, j))
				{
					T value = item;
					result.Add(in value);
				}
			}
		}
	}

	public NativeList<T> Query(Allocator allocator, float x, float y, float radius)
	{
		NativeList<T> result = new NativeList<T>(allocator);
		Query(x, y, radius, result);
		return result;
	}

	private int Clamp(float input)
	{
		int num = (int)input;
		if (num < 0)
		{
			return 0;
		}
		if (num > TotalCells - 1)
		{
			return TotalCells - 1;
		}
		return num;
	}

	public NativeParallelHashSet<T> GetNodeByIndex(int indexX, int indexY)
	{
		return Cells[indexX * CellColumns + indexY];
	}

	private NativeParallelHashSet<T> GetNode(float x, float y, bool create = true)
	{
		x += Center.x;
		y += Center.y;
		int num = Clamp(x / (float)CellSize);
		int num2 = Clamp(y / (float)CellSize);
		NativeParallelHashSet<T> nativeParallelHashSet = GetNodeByIndex(num, num2);
		if (!nativeParallelHashSet.IsCreated && create)
		{
			nativeParallelHashSet = new NativeParallelHashSet<T>(16, gridAllocator);
			Cells[num * TotalCells + num2] = nativeParallelHashSet;
		}
		return nativeParallelHashSet;
	}

	public void Add(T obj, float x, float y)
	{
		NativeParallelHashSet<T> node = GetNode(x, y);
		node.Add(obj);
		Lookup.Add(obj, node);
	}

	public bool AddUnique(T obj, float x, float y)
	{
		if (Contains(obj))
		{
			return false;
		}
		Add(obj, x, y);
		return true;
	}

	public bool Contains(T obj)
	{
		return Lookup.ContainsKey(obj);
	}

	public void Move(T obj, float x, float y)
	{
		NativeParallelHashSet<T> node = GetNode(x, y);
		if (Lookup.TryGetValue(obj, out var item) && node.GetHashCode() != item.GetHashCode())
		{
			item.Remove(obj);
			node.Add(obj);
			Lookup[obj] = node;
		}
	}

	public bool Remove(T obj)
	{
		if (Lookup.TryGetValue(obj, out var item))
		{
			item.Remove(obj);
			Lookup.Remove(obj);
			return true;
		}
		return false;
	}
}
