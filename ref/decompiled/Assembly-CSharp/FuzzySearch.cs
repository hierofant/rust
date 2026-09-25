using System;
using Facepunch;
using UnityEngine;

public static class FuzzySearch
{
	[ClientVar(Help = "How similar the server name must be to the search filter to be included in the results. (0 is exact match, 1 is no match at all)")]
	public static float search_similarity_modifier = 0.14f;

	private static ArrayPool<int> _levenshteinDistancePool;

	private static int DamerauLevenshteinDistance(string source, string target)
	{
		if (_levenshteinDistancePool == null)
		{
			_levenshteinDistancePool = new ArrayPool<int>(256);
		}
		if (string.IsNullOrEmpty(source))
		{
			if (!string.IsNullOrEmpty(target))
			{
				return target.Length;
			}
			return 0;
		}
		if (string.IsNullOrEmpty(target))
		{
			return source.Length;
		}
		if (source.Length > target.Length)
		{
			string text = target;
			string text2 = source;
			source = text;
			target = text2;
		}
		int length = source.Length;
		int length2 = target.Length;
		int[] array = _levenshteinDistancePool.Rent(length2 + 1);
		int[] array2 = _levenshteinDistancePool.Rent(length2 + 1);
		int[] array3 = _levenshteinDistancePool.Rent(length2 + 1);
		try
		{
			for (int i = 0; i <= length2; i++)
			{
				array[i] = i;
			}
			for (int j = 1; j <= length; j++)
			{
				array2[0] = j;
				for (int k = 1; k <= length2; k++)
				{
					int num = ((source[j - 1] != target[k - 1]) ? 1 : 0);
					int val = array[k] + 1;
					int val2 = array2[k - 1] + 1;
					int num2 = Math.Min(val2: array[k - 1] + num, val1: Math.Min(val, val2));
					if (j > 1 && k > 1 && source[j - 1] == target[k - 2] && source[j - 2] == target[k - 1])
					{
						num2 = Math.Min(num2, array3[k - 2] + num);
					}
					array2[k] = num2;
				}
				int[] array4 = array;
				int[] array5 = array2;
				int[] array6 = array3;
				array3 = array4;
				array = array5;
				array2 = array6;
			}
			return array[length2];
		}
		finally
		{
			_levenshteinDistancePool.Return(array);
			_levenshteinDistancePool.Return(array2);
			_levenshteinDistancePool.Return(array3);
		}
	}

	public static bool IsSimilarWithThreshold(string source, string target, float threshold)
	{
		bool flag = string.IsNullOrEmpty(source);
		bool flag2 = string.IsNullOrEmpty(target);
		if (flag && flag2)
		{
			return true;
		}
		if (flag || flag2)
		{
			return false;
		}
		string text = source.ToLower().Trim();
		string text2 = target.ToLower().Trim();
		int num = DamerauLevenshteinDistance(text, text2);
		if (num <= 3)
		{
			return true;
		}
		int num2 = Mathf.Max(text.Length, text2.Length);
		return 1f - (float)num / (float)num2 >= threshold;
	}

	public static bool IsSimilar(string source, string target)
	{
		return IsSimilarWithThreshold(source, target, search_similarity_modifier);
	}

	public static bool IsSimilarFuzzy(this string source, string target)
	{
		return IsSimilar(source, target);
	}

	public static bool IsSimilarFuzzy(this string source, string target, float threshold)
	{
		return IsSimilarWithThreshold(source, target, threshold);
	}
}
