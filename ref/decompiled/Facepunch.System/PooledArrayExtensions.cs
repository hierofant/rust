using System;
using System.Collections.Generic;
using System.Linq;

public static class PooledArrayExtensions
{
	public static PooledArray<T> ToPooledArray<T>(this T[] array)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		PooledArray<T> result = new PooledArray<T>(array.Length);
		Array.Copy(array, result.Array, array.Length);
		return result;
	}

	public static PooledArray<T> ToPooledArray<T>(this List<T> list)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		PooledArray<T> result = new PooledArray<T>(list.Count);
		list.CopyTo(result.Array);
		return result;
	}

	public static PooledArray<T> ToPooledArray<T>(this IEnumerable<T> enumerable)
	{
		if (enumerable == null)
		{
			throw new ArgumentNullException("enumerable");
		}
		PooledArray<T> result = new PooledArray<T>(enumerable.Count());
		int num = 0;
		foreach (T item in enumerable)
		{
			if (num >= result.Array.Length)
			{
				throw new InvalidOperationException("The enumerable contains more items than the allocated array size.");
			}
			result.Array[num++] = item;
		}
		return result;
	}
}
