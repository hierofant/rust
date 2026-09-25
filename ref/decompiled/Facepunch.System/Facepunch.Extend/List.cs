using System;
using System.Collections.Generic;
using Development.Attributes;

namespace Facepunch.Extend;

public static class List
{
	public static void Compare<T>(this List<T> a, List<T> b, List<T> added, List<T> removed, List<T> remained) where T : class
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
			foreach (T item in a)
			{
				obj.Add(item);
			}
			HashSet<T> obj2 = Pool.Get<HashSet<T>>();
			foreach (T item2 in b)
			{
				if (!obj2.Contains(item2))
				{
					if (obj.Contains(item2))
					{
						remained?.Add(item2);
					}
					else
					{
						added?.Add(item2);
					}
					obj2.Add(item2);
				}
			}
			obj.Clear();
			foreach (T item3 in b)
			{
				obj.Add(item3);
			}
			foreach (T item4 in a)
			{
				if (!obj2.Contains(item4))
				{
					if (obj.Contains(item4))
					{
						remained?.Add(item4);
					}
					else
					{
						removed?.Add(item4);
					}
					obj2.Add(item4);
				}
			}
			Pool.FreeUnmanaged(ref obj2);
			Pool.FreeUnmanaged(ref obj);
		}
	}

	public static void Compare<TListA, TListB, TItemA, TItemB, TKey>(this TListA a, TListB b, HashSet<TKey> added, HashSet<TKey> removed, HashSet<TKey> remained, Func<TItemA, TKey> selectorA, Func<TItemB, TKey> selectorB) where TListA : IEnumerable<TItemA> where TListB : IEnumerable<TItemB> where TKey : IEquatable<TKey>
	{
		if (a == null)
		{
			throw new ArgumentNullException("a");
		}
		if (b == null)
		{
			throw new ArgumentNullException("b");
		}
		if (added == null)
		{
			throw new ArgumentNullException("added");
		}
		if (removed == null)
		{
			throw new ArgumentNullException("removed");
		}
		if (remained == null)
		{
			throw new ArgumentNullException("remained");
		}
		added.Clear();
		foreach (TItemB item in b)
		{
			added.Add(selectorB(item));
		}
		removed.Clear();
		foreach (TItemA item2 in a)
		{
			removed.Add(selectorA(item2));
		}
		remained.Clear();
		foreach (TKey item3 in removed)
		{
			if (added.Contains(item3))
			{
				remained.Add(item3);
			}
		}
		added.ExceptWith(remained);
		removed.ExceptWith(remained);
	}

	public static TItem FindWith<TItem, TKey>(this IReadOnlyCollection<TItem> items, Func<TItem, TKey> selector, TKey search, IEqualityComparer<TKey> comparer = null)
	{
		comparer = comparer ?? EqualityComparer<TKey>.Default;
		foreach (TItem item in items)
		{
			if (comparer.Equals(selector(item), search))
			{
				return item;
			}
		}
		return default(TItem);
	}

	public static TItem? TryFindWith<TItem, TKey>(this IReadOnlyCollection<TItem> items, Func<TItem, TKey> selector, TKey search, IEqualityComparer<TKey> comparer = null) where TItem : struct
	{
		comparer = comparer ?? EqualityComparer<TKey>.Default;
		foreach (TItem item in items)
		{
			if (comparer.Equals(selector(item), search))
			{
				return item;
			}
		}
		return null;
	}

	public static int FindIndexWith<TItem, TKey>(this IReadOnlyList<TItem> items, Func<TItem, TKey> selector, TKey search, IEqualityComparer<TKey> comparer = null)
	{
		comparer = comparer ?? EqualityComparer<TKey>.Default;
		for (int i = 0; i < items.Count; i++)
		{
			TItem arg = items[i];
			if (comparer.Equals(search, selector(arg)))
			{
				return i;
			}
		}
		return -1;
	}

	public static int FindIndex<TItem>(this IReadOnlyList<TItem> items, TItem search, IEqualityComparer<TItem> comparer = null)
	{
		comparer = comparer ?? EqualityComparer<TItem>.Default;
		for (int i = 0; i < items.Count; i++)
		{
			TItem y = items[i];
			if (comparer.Equals(search, y))
			{
				return i;
			}
		}
		return -1;
	}

	[PoolAnalyzerGetWrapper]
	public static List<T> ShallowClonePooled<T>(this List<T> items)
	{
		if (items == null)
		{
			return null;
		}
		List<T> list = Pool.Get<List<T>>();
		foreach (T item in items)
		{
			list.Add(item);
		}
		return list;
	}

	public static bool Resize<T>(this List<T> list, int newCount)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (newCount < 0)
		{
			throw new ArgumentOutOfRangeException("newCount");
		}
		if (list.Count == newCount)
		{
			return false;
		}
		if (list.Count > newCount)
		{
			while (list.Count > newCount)
			{
				list.RemoveAt(list.Count - 1);
			}
		}
		else
		{
			while (list.Count < newCount)
			{
				list.Add(default(T));
			}
		}
		return true;
	}
}
