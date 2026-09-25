using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Facepunch;

namespace UnityEngine;

public static class StringEx
{
	private static readonly Regex regexNumeric = new Regex("^[0-9]+$");

	private static readonly Regex regexAlphaNumeric = new Regex("^[a-zA-Z0-9]+$");

	public static readonly string[] KnownRichTextTags = new string[10] { "color", "size", "alpha", "b", "i", "u", "s", "br", "noparse", "sprite" };

	public static string Replace(this string originalString, string oldValue, string newValue, StringComparison comparisonType)
	{
		int startIndex = 0;
		while (true)
		{
			startIndex = originalString.IndexOf(oldValue, startIndex, comparisonType);
			if (startIndex == -1)
			{
				break;
			}
			originalString = originalString.Substring(0, startIndex) + newValue + originalString.Substring(startIndex + oldValue.Length);
			startIndex += newValue.Length;
		}
		return originalString;
	}

	public static bool Contains(this string haystack, string needle, CompareOptions options)
	{
		return CultureInfo.InvariantCulture.CompareInfo.IndexOf(haystack, needle, options) >= 0;
	}

	public static bool IsLower(this string str)
	{
		for (int i = 0; i < str.Length; i++)
		{
			if (char.IsUpper(str[i]))
			{
				return false;
			}
		}
		return true;
	}

	public static string ToPrintable(this string str, int maxLength = int.MaxValue)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (!string.IsNullOrEmpty(str))
		{
			TextElementEnumerator textElementEnumerator = StringInfo.GetTextElementEnumerator(str);
			int num = Mathf.Min(str.Length, maxLength);
			for (int i = 0; i < num; i++)
			{
				if (!textElementEnumerator.MoveNext())
				{
					break;
				}
				string textElement = textElementEnumerator.GetTextElement();
				if (!char.IsControl(textElement, 0))
				{
					stringBuilder.Append(textElement);
				}
			}
		}
		return stringBuilder.ToString();
	}

	public static bool IsNumeric(this string str)
	{
		return regexNumeric.IsMatch(str);
	}

	public static bool IsAlphaNumeric(this string str)
	{
		return regexAlphaNumeric.IsMatch(str);
	}

	public static string FilterRichText(this string str, bool invert, params string[] tags)
	{
		if (tags == null || tags.Length == 0)
		{
			return str;
		}
		if (str.Contains("\\"))
		{
			str = str.Replace("\\", "\\\u200b");
		}
		if (!str.Contains("<") && !str.Contains(">"))
		{
			return str;
		}
		StringBuilder stringBuilder = new StringBuilder();
		string[] array = str.Split('<');
		for (int i = 0; i < array.Length; i++)
		{
			bool flag = false;
			if (string.IsNullOrWhiteSpace(array[i]))
			{
				if (i == array.Length - 1 || string.IsNullOrWhiteSpace(array[i + 1]))
				{
					stringBuilder.Append("<");
				}
				continue;
			}
			if (i == 0)
			{
				stringBuilder.Append(array[i]);
				continue;
			}
			foreach (string other in tags)
			{
				int num = ((array[i][0] == '/') ? 1 : 0);
				int k;
				for (k = num; k < array[i].Length - num; k++)
				{
					char c = array[i][k];
					if (c != '-' && (uint)((c | 0x20) - 97) > 25u)
					{
						break;
					}
				}
				if (new StringView(array[i]).Substring(num, k - num).Equals(other, StringComparison.CurrentCultureIgnoreCase))
				{
					flag = true;
					break;
				}
			}
			if (flag == invert)
			{
				stringBuilder.Append("<");
				stringBuilder.Append(array[i]);
			}
			else
			{
				stringBuilder.Append("<\u200b");
				stringBuilder.Append(array[i].Replace(">", "\u200b>"));
			}
		}
		return stringBuilder.ToString();
	}

	public static string EscapeRichText(this string str, bool altMode = false)
	{
		if (str.Contains("<"))
		{
			str = ((!altMode) ? str.Replace("<", "<\u200b") : str.Replace("<", "〈"));
		}
		if (str.Contains(">"))
		{
			str = ((!altMode) ? str.Replace(">", "\u200b>") : str.Replace(">", "〉"));
		}
		if (str.Contains("\\"))
		{
			str = ((!altMode) ? str.Replace("\\", "\\\u200b") : str.Replace("\\", "＼"));
		}
		return str;
	}

	public static int VisibleLength(this string str)
	{
		if (string.IsNullOrEmpty(str))
		{
			return 0;
		}
		int num = 0;
		int num2 = 0;
		while (num2 < str.Length)
		{
			if (str[num2] == '<')
			{
				int num3 = str.IndexOf('>', num2 + 1, Math.Min(64, str.Length - num2 - 1));
				if (num3 > num2 && IsKnownRichTextTag(str, num2 + 1, num3))
				{
					num2 = num3 + 1;
					continue;
				}
			}
			num++;
			num2++;
		}
		return num;
	}

	private static bool IsKnownRichTextTag(string str, int start, int end)
	{
		if (start < end && str[start] == '/')
		{
			start++;
		}
		if (start >= end)
		{
			return false;
		}
		if (str[start] == '#')
		{
			return true;
		}
		int i;
		for (i = start; i < end && str[i] != '='; i++)
		{
		}
		int num = i - start;
		string[] knownRichTextTags = KnownRichTextTags;
		foreach (string text in knownRichTextTags)
		{
			if (text.Length == num && string.CompareOrdinal(str, start, text, 0, num) == 0)
			{
				return true;
			}
		}
		return false;
	}

	public static IEnumerable<string> SplitToLines(this string input)
	{
		if (input == null)
		{
			yield break;
		}
		using StringReader reader = new StringReader(input);
		string text;
		while ((text = reader.ReadLine()) != null)
		{
			yield return text;
		}
	}

	public static IEnumerable<string> SplitToChunks(this string str, int chunkLength)
	{
		if (string.IsNullOrEmpty(str))
		{
			throw new ArgumentException("string cannot be null");
		}
		if (chunkLength < 1)
		{
			throw new ArgumentException("chunk length needs to be more than 0");
		}
		for (int i = 0; i < str.Length; i += chunkLength)
		{
			if (chunkLength + i >= str.Length)
			{
				chunkLength = str.Length - i;
			}
			yield return str.Substring(i, chunkLength);
		}
	}

	public static uint ManifestHash(this string str)
	{
		if (string.IsNullOrEmpty(str))
		{
			return 0u;
		}
		if (!str.IsLower())
		{
			str = str.ToLower();
		}
		return BitConverter.ToUInt32(new MD5CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(str)), 0);
	}

	public static byte[] Sha256(this string str)
	{
		if (str == null)
		{
			throw new NullReferenceException("Input string cannot be null");
		}
		return new SHA256CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(str));
	}

	public static string HexString(this byte[] bytes)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (byte b in bytes)
		{
			stringBuilder.Append(b.ToString("X2"));
		}
		return stringBuilder.ToString();
	}

	public static bool StartsWithAny(this string str, string[] values)
	{
		foreach (string value in values)
		{
			if (str.StartsWith(value))
			{
				return true;
			}
		}
		return false;
	}

	public static bool EqualsAfterLastSeparator(string left, string right, char separator, StringComparison comparison = StringComparison.Ordinal)
	{
		if (left == null || right == null)
		{
			throw new ArgumentNullException();
		}
		ReadOnlySpan<char> readOnlySpan = left.AsSpan();
		ReadOnlySpan<char> readOnlySpan2 = right.AsSpan();
		int num = readOnlySpan.LastIndexOf(separator);
		int num2 = readOnlySpan2.LastIndexOf(separator);
		ReadOnlySpan<char> span;
		ReadOnlySpan<char> readOnlySpan3;
		if (num < 0)
		{
			span = readOnlySpan;
		}
		else
		{
			readOnlySpan3 = readOnlySpan;
			int num3 = num + 1;
			span = readOnlySpan3.Slice(num3, readOnlySpan3.Length - num3);
		}
		ReadOnlySpan<char> readOnlySpan4;
		if (num2 < 0)
		{
			readOnlySpan4 = readOnlySpan2;
		}
		else
		{
			readOnlySpan3 = readOnlySpan2;
			int num3 = num2 + 1;
			readOnlySpan4 = readOnlySpan3.Slice(num3, readOnlySpan3.Length - num3);
		}
		ReadOnlySpan<char> other = readOnlySpan4;
		return MemoryExtensions.Equals(span, other, comparison);
	}
}
