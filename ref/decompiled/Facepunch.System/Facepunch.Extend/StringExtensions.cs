using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Facepunch.Extend;

public static class StringExtensions
{
	private struct SplitState
	{
		public enum SeparatorType : byte
		{
			Space,
			SingleQuote,
			DoubleQuote
		}

		public SeparatorType Separator;

		public int Pos;

		public int Length;
	}

	private static readonly Regex regexSplitQuotes = new Regex("\"([^\"]+)\"|'([^']+)'|\\S+");

	private static char[] spaceOrQuote = new char[2] { ' ', '"' };

	private static StringBuilder _quoteSafeBuilder = new StringBuilder();

	private static char[] FilenameDelim = new char[2] { '/', '\\' };

	private static readonly char[] _badCharacters = new char[73]
	{
		'\0', '\u0001', '\u0002', '\u0003', '\u0004', '\u0005', '\u0006', '\a', '\b', '\t',
		'\v', '\f', '\r', '\u000e', '\u000f', '\u0010', '\u0012', '\u0013', '\u0014', '\u0016',
		'\u0017', '\u0018', '\u0019', '\u001a', '\u001b', '\u001c', '\u001d', '\u001e', '\u001f', '\u00a0',
		'\u00ad', '\u2000', '\u2001', '\u2002', '\u2003', '\u2004', '\u2005', '\u2006', '\u2007', '\u2008',
		'\u2009', '\u200a', '\u200b', '\u200c', '\u200d', '\u200e', '\u200f', '‐', '‑', '‒',
		'–', '—', '―', '‖', '‗', '‘', '’', '‚', '‛', '“',
		'”', '„', '‟', '\u2028', '\u2029', '\u202f', '\u205f', '\u2060', '␠', '␢',
		'␣', '\u3000', '\ufeff'
	};

	public static string QuoteSafe(this string str)
	{
		lock (_quoteSafeBuilder)
		{
			_quoteSafeBuilder.Clear();
			_quoteSafeBuilder.QuoteSafe(str);
			return _quoteSafeBuilder.ToString();
		}
	}

	private static void GatherStringQuoteSplits(StringView input, List<SplitState> results, int maxCount = int.MaxValue)
	{
		if (input.Length == 0 || maxCount == 0)
		{
			return;
		}
		int i;
		for (i = 0; i < input.Length && char.IsWhiteSpace(input[i]); i++)
		{
		}
		if (i == input.Length)
		{
			return;
		}
		SplitState splitState = StartState(input, ref i);
		i++;
		results.Add(splitState);
		while (true)
		{
			if (i < input.Length)
			{
				char c = input[i];
				bool flag = false;
				switch (splitState.Separator)
				{
				case SplitState.SeparatorType.Space:
					flag = char.IsWhiteSpace(c);
					break;
				case SplitState.SeparatorType.SingleQuote:
					flag = c == '\'';
					break;
				case SplitState.SeparatorType.DoubleQuote:
					flag = c == '"' && i > 0 && input[i - 1] != '\\';
					break;
				}
				if (flag)
				{
					int num = i;
					if (splitState.Separator != SplitState.SeparatorType.SingleQuote)
					{
						num--;
					}
					while (num > splitState.Pos && char.IsWhiteSpace(input[num]))
					{
						num--;
					}
					splitState.Length = num + 1 - splitState.Pos;
					results[results.Count - 1] = splitState;
					if (results.Count == maxCount)
					{
						goto IL_0170;
					}
					if (i < input.Length - 1)
					{
						int j;
						for (j = i + 1; j < input.Length && char.IsWhiteSpace(input[j]); j++)
						{
						}
						if (j >= input.Length)
						{
							goto IL_0170;
						}
						splitState = StartState(input, ref j);
						results.Add(splitState);
						i = j;
					}
				}
				i++;
				continue;
			}
			goto IL_0170;
			IL_0170:
			if (splitState.Length != -1)
			{
				break;
			}
			if (splitState.Separator == SplitState.SeparatorType.Space)
			{
				int num2 = input.Length - 1;
				while (num2 > splitState.Pos && char.IsWhiteSpace(input[num2]))
				{
					num2--;
				}
				splitState.Length = num2 + 1 - splitState.Pos;
				results[results.Count - 1] = splitState;
				break;
			}
			splitState.Separator = SplitState.SeparatorType.Space;
			results[results.Count - 1] = splitState;
			i = splitState.Pos;
		}
		static SplitState StartState(StringView input, ref int pos)
		{
			SplitState result;
			switch (input[pos])
			{
			case '\'':
				result = default(SplitState);
				result.Separator = SplitState.SeparatorType.SingleQuote;
				result.Pos = pos;
				result.Length = -1;
				return result;
			case '"':
				result = default(SplitState);
				result.Separator = SplitState.SeparatorType.DoubleQuote;
				result.Pos = pos + 1;
				result.Length = -1;
				return result;
			case '\\':
				if (pos + 1 < input.Length && input[pos + 1] == '"')
				{
					pos++;
				}
				break;
			}
			result = default(SplitState);
			result.Separator = SplitState.SeparatorType.Space;
			result.Pos = pos;
			result.Length = -1;
			return result;
		}
	}

	public static string[] SplitQuotesStrings(this string input, int maxCount = int.MaxValue)
	{
		List<SplitState> obj = Pool.Get<List<SplitState>>();
		GatherStringQuoteSplits(input, obj, maxCount);
		string[] array = Array.Empty<string>();
		if (obj.Count > 0)
		{
			array = new string[obj.Count];
			for (int i = 0; i < obj.Count; i++)
			{
				array[i] = input.Substring(obj[i].Pos, obj[i].Length).Replace("\\\"", "\"");
			}
		}
		Pool.FreeUnmanaged(ref obj);
		return array;
	}

	public static StringView[] SplitQuotesStrings(this StringView input, int maxCount = int.MaxValue)
	{
		List<SplitState> obj = Pool.Get<List<SplitState>>();
		GatherStringQuoteSplits(input, obj, maxCount);
		StringView[] array = Array.Empty<StringView>();
		if (obj.Count > 0)
		{
			array = new StringView[obj.Count];
			for (int i = 0; i < obj.Count; i++)
			{
				array[i] = input.Substring(obj[i].Pos, obj[i].Length).Replace("\\\"", "\"");
			}
		}
		Pool.FreeUnmanaged(ref obj);
		return array;
	}

	public static decimal ToDecimal(this string str, decimal Default = 0m)
	{
		return ((StringView)str).ToDecimal(Default);
	}

	public static decimal ToDecimal(this StringView str, decimal Default = 0m)
	{
		if (decimal.TryParse(str, out var result))
		{
			return result;
		}
		return Default;
	}

	public static float ToFloat(this string str, float Default = 0f)
	{
		return ((StringView)str).ToFloat(Default);
	}

	public static float ToFloat(this StringView str, float Default = 0f)
	{
		return (float)str.ToDecimal((decimal)Default);
	}

	public static int ToInt(this string str, int Default = 0)
	{
		return ((StringView)str).ToInt(Default);
	}

	public static int ToInt(this StringView str, int Default = 0)
	{
		decimal num = str.ToDecimal(Default);
		if (!(num <= -2147483648m))
		{
			if (!(num >= 2147483647m))
			{
				return (int)num;
			}
			return int.MaxValue;
		}
		return int.MinValue;
	}

	public static long ToLong(this string str, long Default = 0L)
	{
		return ((StringView)str).ToLong(Default);
	}

	public static long ToLong(this StringView str, long Default = 0L)
	{
		decimal num = str.ToDecimal(Default);
		if (!(num <= -9223372036854775808m))
		{
			if (!(num >= 9223372036854775807m))
			{
				return (long)num;
			}
			return long.MaxValue;
		}
		return long.MinValue;
	}

	public static bool ToBool(this string str)
	{
		if (string.IsNullOrWhiteSpace(str))
		{
			return false;
		}
		return ((StringView)str).ToBool();
	}

	public static bool ToBool(this StringView str)
	{
		if (str == "0")
		{
			return false;
		}
		if (str == "1")
		{
			return true;
		}
		if (str == "False")
		{
			return false;
		}
		if (str == "True")
		{
			return true;
		}
		str = str.Trim();
		if (str.Equals("true", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		if (str.Equals("t", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		if (str.Equals("yes", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		if (str.Equals("y", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		return false;
	}

	public static string Truncate(this string str, int maxLength, string appendage = null)
	{
		if (string.IsNullOrEmpty(str))
		{
			return str;
		}
		if (str.Length <= maxLength)
		{
			return str;
		}
		if (appendage != null)
		{
			maxLength -= appendage.Length;
		}
		str = str.Substring(0, maxLength);
		if (appendage == null)
		{
			return str;
		}
		return str + appendage;
	}

	public static string TruncateFilename(this string str, int maxLength, string appendage = null)
	{
		if (string.IsNullOrEmpty(str))
		{
			return str;
		}
		if (str.Length <= maxLength)
		{
			return str;
		}
		maxLength -= 3;
		string text = str;
		int num = 0;
		while (num++ < 100)
		{
			List<string> list = str.Split(FilenameDelim).ToList();
			list.RemoveRange(list.Count - 1 - num, num);
			if (list.Count == 1)
			{
				return list.Last();
			}
			list.Insert(list.Count - 1, "...");
			text = string.Join("/", list.ToArray());
			if (text.Length < maxLength)
			{
				return text;
			}
		}
		return str.Split(FilenameDelim).ToList().Last();
	}

	public static bool Contains(this string source, string toCheck, StringComparison comp)
	{
		return source.IndexOf(toCheck, comp) >= 0;
	}

	public static string Snippet(this string source, string find, int padding)
	{
		if (string.IsNullOrEmpty(find))
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		int num;
		for (num = 0; num < source.Length; num += find.Length)
		{
			num = source.IndexOf(find, num, StringComparison.InvariantCultureIgnoreCase);
			if (num == -1)
			{
				break;
			}
			int num2 = (num - padding).Clamp(0, source.Length);
			int num3 = (num2 + find.Length + padding * 2).Clamp(0, source.Length);
			num = num3;
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(" ... ");
			}
			stringBuilder.Append(source.Substring(num2, num3 - num2));
		}
		return stringBuilder.ToString();
	}

	public static string RemoveBadCharacters(this string str)
	{
		str = new string(str.Where((char x) => !_badCharacters.Contains(x)).ToArray());
		return str;
	}

	public static string Base64Encode(this string plainText)
	{
		return Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));
	}

	public static string Base64Decode(this string base64EncodedData)
	{
		byte[] bytes = Convert.FromBase64String(base64EncodedData);
		return Encoding.UTF8.GetString(bytes);
	}
}
