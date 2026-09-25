using System.Text;
using UnityEngine;

public static class StringBuilderEx
{
	public static void AppendPadded(this StringBuilder sb, int n, int pad)
	{
		sb.Append(' ', Mathf.Max(pad - n.Digits(), 0));
		sb.Append(n);
	}

	public static void AppendPadded(this StringBuilder sb, uint n, int pad)
	{
		sb.Append(' ', Mathf.Max(pad - n.Digits(), 0));
		sb.Append(n);
	}

	public static void AppendPadded(this StringBuilder sb, long n, int pad)
	{
		sb.AppendPadded((int)n, pad);
	}

	public static void AppendPadded(this StringBuilder sb, ulong n, int pad)
	{
		sb.AppendPadded((uint)n, pad);
	}

	public static void AppendPadded(this StringBuilder sb, string s, int pad)
	{
		sb.Append(s);
		sb.Append(' ', Mathf.Max(pad - s.Length, 0));
	}

	public static void Clear(this StringBuilder value)
	{
		value.Length = 0;
	}
}
