using System;
using System.Collections;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class ListHashSet<T> : IEnumerable<T>, IEnumerable, IList<T>, ICollection<T>
{
	public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
	{
		private readonly BufferList<T> list;

		private int index;

		public T Current => list[index];

		object IEnumerator.Current => Current;

		public Enumerator(ListHashSet<T> set)
		{
			list = set.vals;
			index = -1;
		}

		public bool MoveNext()
		{
			index++;
			return index < list.Count;
		}

		public void Reset()
		{
			index = -1;
		}

		public void Dispose()
		{
		}
	}

	private Dictionary<T, int> val2idx;

	private Dictionary<int, T> idx2val;

	private BufferList<T> vals;

	public BufferList<T> Values => vals;

	public int Count => vals.Count;

	public bool IsReadOnly => false;

	public T this[int index]
	{
		get
		{
			return vals[index];
		}
		set
		{
			vals[index] = value;
		}
	}

	public ListHashSet()
		: this(8)
	{
	}

	public ListHashSet(int capacity)
	{
		val2idx = new Dictionary<T, int>(capacity);
		idx2val = new Dictionary<int, T>(capacity);
		vals = new BufferList<T>(capacity);
	}

	public void Add(T val)
	{
		int count = vals.Count;
		val2idx.Add(val, count);
		idx2val.Add(count, val);
		vals.Add(val);
	}

	public bool TryAdd(T val)
	{
		if (Contains(val))
		{
			return false;
		}
		Add(val);
		return true;
	}

	public void AddRange(List<T> list)
	{
		for (int i = 0; i < list.Count; i++)
		{
			Add(list[i]);
		}
	}

	public void AddRange(IEnumerable<T> enumerable)
	{
		foreach (T item in enumerable)
		{
			Add(item);
		}
	}

	public bool Contains(T val)
	{
		return val2idx.ContainsKey(val);
	}

	public bool Remove(T val)
	{
		if (!val2idx.TryGetValue(val, out var value))
		{
			return false;
		}
		Remove(value, val);
		return true;
	}

	public void RemoveAt(int idx)
	{
		if (idx2val.TryGetValue(idx, out var value))
		{
			Remove(idx, value);
		}
	}

	public int IndexOf(T item)
	{
		if (!val2idx.TryGetValue(item, out var value))
		{
			return -1;
		}
		return value;
	}

	public void ReplaceAt(int index, T item)
	{
		T key = vals[index];
		val2idx.Remove(key);
		val2idx.Add(item, index);
		vals[index] = item;
		idx2val[index] = item;
	}

	public void Insert(int index, T item)
	{
		vals.Add(default(T));
		for (int num = vals.Count - 1; num > index; num--)
		{
			T val = vals[num - 1];
			vals[num] = val;
			val2idx[val] = num;
			idx2val[num] = val;
		}
		vals[index] = item;
		val2idx[item] = index;
		idx2val[index] = item;
	}

	public void Clear()
	{
		if (Count != 0)
		{
			val2idx.Clear();
			idx2val.Clear();
			vals.Clear();
		}
	}

	private void Remove(int idx_remove, T val_remove)
	{
		int key = vals.Count - 1;
		T val = idx2val[key];
		vals.RemoveUnordered(idx_remove);
		val2idx[val] = idx_remove;
		idx2val[idx_remove] = val;
		val2idx.Remove(val_remove);
		idx2val.Remove(key);
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		for (int i = 0; i < vals.Count; i++)
		{
			array[arrayIndex + i] = vals[i];
		}
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public static void Compare(ListHashSet<T> a, ListHashSet<T> b, List<T> added, List<T> removed, List<T> remained)
	{
		if (a == null && b == null)
		{
			return;
		}
		if (a == null)
		{
			added?.AddRange(b);
		}
		else if (b == null)
		{
			removed?.AddRange(a);
		}
		else
		{
			if (a.Count == 0 && b.Count == 0)
			{
				return;
			}
			HashSet<T> obj = Pool.Get<HashSet<T>>();
			foreach (T item in b)
			{
				if (!obj.Contains(item))
				{
					if (a.Contains(item))
					{
						remained?.Add(item);
					}
					else
					{
						added?.Add(item);
					}
					obj.Add(item);
				}
			}
			foreach (T item2 in a)
			{
				if (!obj.Contains(item2))
				{
					if (b.Contains(item2))
					{
						remained?.Add(item2);
					}
					else
					{
						removed?.Add(item2);
					}
					obj.Add(item2);
				}
			}
			Pool.FreeUnmanaged(ref obj);
		}
	}

	public static void PartialCompare(ListHashSet<T> a, ListHashSet<T> b, List<T> added, int addedLimit, List<T> removed, int removedLimit, List<T> remained)
	{
		if (a == null && b == null)
		{
			return;
		}
		if (addedLimit == 0)
		{
			added = null;
		}
		if (removedLimit == 0)
		{
			removed = null;
		}
		if (a == null)
		{
			if (added != null)
			{
				for (int i = 0; i < addedLimit; i++)
				{
					added.Add(b[i]);
				}
			}
		}
		else if (b == null)
		{
			if (removed != null)
			{
				for (int j = 0; j < removedLimit; j++)
				{
					removed.Add(a[j]);
				}
			}
		}
		else
		{
			if (a.Count == 0 && b.Count == 0)
			{
				return;
			}
			HashSet<T> obj = Pool.Get<HashSet<T>>();
			foreach (T item in b)
			{
				if (!obj.Contains(item))
				{
					bool flag = false;
					if (a.Contains(item))
					{
						remained?.Add(item);
					}
					else if (added != null)
					{
						added.Add(item);
						flag = --addedLimit <= 0;
					}
					obj.Add(item);
					if (flag)
					{
						break;
					}
				}
			}
			foreach (T item2 in a)
			{
				if (obj.Contains(item2))
				{
					continue;
				}
				if (b.Contains(item2))
				{
					remained?.Add(item2);
				}
				else if (removed != null)
				{
					removed.Add(item2);
					if (--removedLimit <= 0)
					{
						break;
					}
				}
				obj.Add(item2);
			}
			Pool.FreeUnmanaged(ref obj);
		}
	}

	public T GetRandom()
	{
		int count = vals.Count;
		if (count == 0)
		{
			return default(T);
		}
		return vals[UnityEngine.Random.Range(0, count)];
	}
}
