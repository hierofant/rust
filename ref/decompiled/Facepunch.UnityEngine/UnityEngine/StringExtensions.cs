using Facepunch;
using Facepunch.Extend;

namespace UnityEngine;

public static class StringExtensions
{
	public static string BBCodeToUnity(this string x)
	{
		x = x.Replace("[", "<");
		x = x.Replace("]", ">");
		return x;
	}

	public static Vector3 ToVector3(this string str)
	{
		return ((StringView)str).ToVector3();
	}

	public static Vector3 ToVector3(this StringView str)
	{
		str = str.Trim('(', ')', ' ');
		int num = str.IndexOfAny(" ,");
		if (num == -1)
		{
			return default(Vector3);
		}
		StringView str2 = str.Substring(0, num).Trim(' ', ',');
		str = str.Substring(num + 1).Trim(' ', ',');
		num = str.IndexOfAny(" ,");
		if (num == -1)
		{
			return default(Vector3);
		}
		StringView str3 = str.Substring(0, num).Trim(' ', ',');
		StringView str4 = str.Substring(num + 1).Trim(' ', ',');
		return new Vector3(str2.ToFloat(), str3.ToFloat(), str4.ToFloat());
	}

	public static Color ToColor(this string str)
	{
		return ((StringView)str).ToColor();
	}

	public static Color ToColor(this StringView str)
	{
		int num = str.IndexOf(",");
		if (num == -1)
		{
			return default(Color);
		}
		StringView str2 = str.Substring(0, num);
		str = str.Substring(num + 1);
		num = str.IndexOf(",");
		if (num == -1)
		{
			return default(Color);
		}
		StringView str3 = str.Substring(0, num);
		str = str.Substring(num + 1);
		num = str.IndexOf(",");
		if (num == -1)
		{
			StringView str4 = str;
			return new Color(str2.ToFloat(), str3.ToFloat(), str4.ToFloat());
		}
		StringView str5 = str.Substring(0, num);
		StringView str6 = str.Substring(num + 1);
		return new Color(str2.ToFloat(), str3.ToFloat(), str5.ToFloat(), str6.ToFloat());
	}
}
