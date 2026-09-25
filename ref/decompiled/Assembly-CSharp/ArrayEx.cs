using System;
using System.Collections.Generic;
using UnityEngine;

public static class ArrayEx
{
	public static T[] New<T>(int length)
	{
		if (length == 0)
		{
			return Array.Empty<T>();
		}
		return new T[length];
	}

	public static T GetRandom<T>(this T[] array)
	{
		if (array == null || array.Length == 0)
		{
			return default(T);
		}
		return array[UnityEngine.Random.Range(0, array.Length)];
	}

	public static T GetRandom<T>(this T[] array, uint seed)
	{
		if (array == null || array.Length == 0)
		{
			return default(T);
		}
		return array[SeedRandom.Range(ref seed, 0, array.Length)];
	}

	public static T GetRandom<T>(this T[] array, ref uint seed)
	{
		if (array == null || array.Length == 0)
		{
			return default(T);
		}
		return array[SeedRandom.Range(ref seed, 0, array.Length)];
	}

	public static void Shuffle<T>(this T[] array, uint seed)
	{
		Shuffle(array, ref seed);
	}

	public static void Shuffle<T>(this T[] array, ref uint seed)
	{
		for (int i = 0; i < array.Length; i++)
		{
			int num = SeedRandom.Range(ref seed, 0, array.Length);
			int num2 = SeedRandom.Range(ref seed, 0, array.Length);
			T val = array[num];
			array[num] = array[num2];
			array[num2] = val;
		}
	}

	public static void BubbleSort<T>(this T[] array) where T : IComparable<T>
	{
		for (int i = 1; i < array.Length; i++)
		{
			T val = array[i];
			for (int num = i - 1; num >= 0; num--)
			{
				T val2 = array[num];
				if (val.CompareTo(val2) >= 0)
				{
					break;
				}
				array[num + 1] = val2;
				array[num] = val;
			}
		}
	}

	public static int BinarySearch<TElement, TNeedle>(this TElement[] array, Func<TElement, TNeedle> selector, TNeedle needle, IComparer<TNeedle> comparer = null)
	{
		if (comparer == null)
		{
			comparer = Comparer<TNeedle>.Default;
		}
		int num = 0;
		int num2 = array.Length - 1;
		while (num <= num2)
		{
			int num3 = num + (num2 - num) / 2;
			TNeedle x = selector(array[num3]);
			int num4 = comparer.Compare(x, needle);
			if (num4 == 0)
			{
				return num3;
			}
			if (num4 < 0)
			{
				num = num3 + 1;
			}
			else
			{
				num2 = num3 - 1;
			}
		}
		return ~num;
	}
}
