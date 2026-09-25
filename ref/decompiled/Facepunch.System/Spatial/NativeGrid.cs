using System;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;

namespace Spatial;

public struct NativeGrid<T> : IDisposable where T : unmanaged, IEquatable<T>
{
	private const float DefaultWorldSize = 8096f;

	private const int DefaultCellSize = 32;

	private const int DefaultLookupCapacity = 512;

	private const int DefaultNodeCapacity = 16;

	private Allocator gridAllocator;

	private NativeArray<NativeHashSet<T>> Cells;

	private NativeHashMap<T, NativeHashSet<T>> Lookup;

	private float2 Center;

	private int CellSize;

	private int CellColumns;

	private int TotalCells;

	public bool IsCreated
	{
		get
		{
			if (Lookup.IsCreated)
			{
				return Cells.IsCreated;
			}
			return false;
		}
	}

	public bool IsEmpty => Lookup.Count == 0;

	public NativeGrid(Allocator gridAllocator, int cellSize = 32, float worldSize = 8096f, bool preAllocateNodes = false)
	{
		this.gridAllocator = gridAllocator;
		Lookup = new NativeHashMap<T, NativeHashSet<T>>(512, gridAllocator);
		CellSize = cellSize;
		CellColumns = (int)(worldSize / (float)CellSize + 0.5f);
		TotalCells = CellColumns * CellColumns;
		Cells = new NativeArray<NativeHashSet<T>>(TotalCells, gridAllocator);
		if (preAllocateNodes)
		{
			for (int i = 0; i < TotalCells; i++)
			{
				Cells[i] = new NativeHashSet<T>(16, gridAllocator);
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
		if (!IsCreated || IsEmpty)
		{
			return result;
		}
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
		if (num > CellColumns - 1)
		{
			return CellColumns - 1;
		}
		return num;
	}

	public NativeHashSet<T> GetNodeByIndex(int indexX, int indexY)
	{
		int num = indexX * CellColumns + indexY;
		if (Hint.Unlikely(!Cells.IsCreated || num < 0 || num >= Cells.Length))
		{
			return default(NativeHashSet<T>);
		}
		return Cells[num];
	}

	private NativeHashSet<T> GetNode(float x, float y, bool create = true)
	{
		x += Center.x;
		y += Center.y;
		int num = Clamp(x / (float)CellSize);
		int num2 = Clamp(y / (float)CellSize);
		NativeHashSet<T> nativeHashSet = GetNodeByIndex(num, num2);
		if (!nativeHashSet.IsCreated && create)
		{
			nativeHashSet = new NativeHashSet<T>(16, gridAllocator);
			Cells[num * CellColumns + num2] = nativeHashSet;
		}
		return nativeHashSet;
	}

	public void Add(T obj, float x, float y)
	{
		NativeHashSet<T> node = GetNode(x, y);
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
		NativeHashSet<T> node = GetNode(x, y);
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

	public void Dispose()
	{
		if (gridAllocator == Allocator.Temp)
		{
			return;
		}
		if (Lookup.IsCreated)
		{
			Lookup.Dispose();
		}
		if (!Cells.IsCreated)
		{
			return;
		}
		for (int i = 0; i < Cells.Length; i++)
		{
			if (Cells[i].IsCreated)
			{
				Cells[i].Dispose();
			}
		}
		Cells.Dispose();
	}
}
