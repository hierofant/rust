using System;
using System.Collections.Generic;
using Development.Attributes;

namespace Spatial;

public class Grid<T>
{
	internal class Node
	{
		public HashSet<T> Contents = new HashSet<T>();

		public event Action OnNodeContentsChanged;

		public void Add(T obj)
		{
			Contents.Add(obj);
			this.OnNodeContentsChanged?.Invoke();
		}

		public bool Remove(T obj)
		{
			bool num = Contents.Remove(obj);
			if (num)
			{
				Action onNodeContentsChanged = this.OnNodeContentsChanged;
				if (onNodeContentsChanged == null)
				{
					return num;
				}
				onNodeContentsChanged();
			}
			return num;
		}
	}

	public const float DefaultWorldSize = 8096f;

	public const int DefaultCellSize = 32;

	private float CenterX;

	private float CenterY;

	private Node[,] Nodes;

	private Dictionary<T, Node> Lookup;

	public int CellCount { get; private set; }

	public int CellSize { get; private set; }

	public Grid(int CellSize = 32, float WorldSize = 8096f)
	{
		this.CellSize = CellSize;
		CellCount = (int)(WorldSize / (float)CellSize + 0.5f);
		CenterX = WorldSize * 0.5f;
		CenterY = WorldSize * 0.5f;
		Nodes = new Node[CellCount, CellCount];
		Lookup = new Dictionary<T, Node>(512);
	}

	public int Query(float x, float y, float radius, T[] result, Func<T, bool> filter = null)
	{
		int num = Clamp((x + CenterX - radius) / (float)CellSize);
		int num2 = Clamp((x + CenterX + radius) / (float)CellSize);
		int num3 = Clamp((y + CenterY - radius) / (float)CellSize);
		int num4 = Clamp((y + CenterY + radius) / (float)CellSize);
		int num5 = 0;
		for (int i = num; i <= num2; i++)
		{
			for (int j = num3; j <= num4; j++)
			{
				if (Nodes[i, j] == null)
				{
					continue;
				}
				foreach (T content in Nodes[i, j].Contents)
				{
					if (filter == null || filter(content))
					{
						result[num5] = content;
						num5++;
						if (num5 >= result.Length)
						{
							return num5;
						}
					}
				}
			}
		}
		return num5;
	}

	[PoolAnalyzerNonCaching]
	public void Query<U>(float x, float y, float radius, List<U> result) where U : class
	{
		if (result == null)
		{
			return;
		}
		int num = Clamp((x + CenterX - radius) / (float)CellSize);
		int num2 = Clamp((x + CenterX + radius) / (float)CellSize);
		int num3 = Clamp((y + CenterY - radius) / (float)CellSize);
		int num4 = Clamp((y + CenterY + radius) / (float)CellSize);
		for (int i = num; i <= num2; i++)
		{
			for (int j = num3; j <= num4; j++)
			{
				if (Nodes[i, j] == null)
				{
					continue;
				}
				foreach (T content in Nodes[i, j].Contents)
				{
					if (content is U item)
					{
						result.Add(item);
					}
				}
			}
		}
	}

	[PoolAnalyzerNonCaching]
	public void Query(float x, float y, float radius, List<T> result)
	{
		if (result == null)
		{
			return;
		}
		int num = Clamp((x + CenterX - radius) / (float)CellSize);
		int num2 = Clamp((x + CenterX + radius) / (float)CellSize);
		int num3 = Clamp((y + CenterY - radius) / (float)CellSize);
		int num4 = Clamp((y + CenterY + radius) / (float)CellSize);
		for (int i = num; i <= num2; i++)
		{
			for (int j = num3; j <= num4; j++)
			{
				if (Nodes[i, j] != null)
				{
					result.AddRange(Nodes[i, j].Contents);
				}
			}
		}
	}

	[PoolAnalyzerNonCaching]
	public void Query(float x, float y, float radius, List<T> result, Func<T, bool> filter)
	{
		if (result == null)
		{
			return;
		}
		int num = Clamp((x + CenterX - radius) / (float)CellSize);
		int num2 = Clamp((x + CenterX + radius) / (float)CellSize);
		int num3 = Clamp((y + CenterY - radius) / (float)CellSize);
		int num4 = Clamp((y + CenterY + radius) / (float)CellSize);
		for (int i = num; i <= num2; i++)
		{
			for (int j = num3; j <= num4; j++)
			{
				if (Nodes[i, j] == null)
				{
					continue;
				}
				foreach (T content in Nodes[i, j].Contents)
				{
					if (filter == null || filter(content))
					{
						result.Add(content);
					}
				}
			}
		}
	}

	public bool Any(float x, float y, float radius, Func<T, bool> ignoreFilter, Func<T, bool> filter, out bool gridNodesEmpty)
	{
		int num = Clamp((x + CenterX - radius) / (float)CellSize);
		int num2 = Clamp((x + CenterX + radius) / (float)CellSize);
		int num3 = Clamp((y + CenterY - radius) / (float)CellSize);
		int num4 = Clamp((y + CenterY + radius) / (float)CellSize);
		gridNodesEmpty = true;
		for (int i = num; i <= num2; i++)
		{
			for (int j = num3; j <= num4; j++)
			{
				if (Nodes[i, j] == null)
				{
					continue;
				}
				foreach (T content in Nodes[i, j].Contents)
				{
					if (ignoreFilter == null || !ignoreFilter(content))
					{
						gridNodesEmpty = false;
						if (filter == null || filter(content))
						{
							return true;
						}
					}
				}
			}
		}
		return false;
	}

	public void Subscribe(float x, float y, float radius, Action callback)
	{
		int num = Clamp((x + CenterX - radius) / (float)CellSize);
		int num2 = Clamp((x + CenterX + radius) / (float)CellSize);
		int num3 = Clamp((y + CenterY - radius) / (float)CellSize);
		int num4 = Clamp((y + CenterY + radius) / (float)CellSize);
		for (int i = num; i <= num2; i++)
		{
			for (int j = num3; j <= num4; j++)
			{
				Node node = Nodes[i, j];
				if (node == null)
				{
					node = new Node();
					Nodes[i, j] = node;
				}
				node.OnNodeContentsChanged += callback;
			}
		}
	}

	private int Clamp(float input)
	{
		int num = (int)input;
		if (num < 0)
		{
			return 0;
		}
		if (num > CellCount - 1)
		{
			return CellCount - 1;
		}
		return num;
	}

	private Node GetNode(float x, float y, bool create = true)
	{
		x += CenterX;
		y += CenterY;
		int num = Clamp(x / (float)CellSize);
		int num2 = Clamp(y / (float)CellSize);
		Node node = Nodes[num, num2];
		if (node == null && create)
		{
			node = new Node();
			Nodes[num, num2] = node;
		}
		return node;
	}

	public void Add(T obj, float x, float y)
	{
		Node node = GetNode(x, y);
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
		Node node = GetNode(x, y);
		if (Lookup.TryGetValue(obj, out var value) && node != value)
		{
			value.Remove(obj);
			node.Add(obj);
			Lookup[obj] = node;
		}
	}

	public bool Remove(T obj)
	{
		Node value = null;
		if (Lookup.TryGetValue(obj, out value))
		{
			value.Remove(obj);
			Lookup.Remove(obj);
			return true;
		}
		return false;
	}
}
