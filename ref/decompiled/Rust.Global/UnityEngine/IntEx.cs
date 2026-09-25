namespace UnityEngine;

public static class IntEx
{
	public static int Digits(this int n)
	{
		if (n >= 0)
		{
			if (n < 10)
			{
				return 1;
			}
			if (n < 100)
			{
				return 2;
			}
			if (n < 1000)
			{
				return 3;
			}
			if (n < 10000)
			{
				return 4;
			}
			if (n < 100000)
			{
				return 5;
			}
			if (n < 1000000)
			{
				return 6;
			}
			if (n < 10000000)
			{
				return 7;
			}
			if (n < 100000000)
			{
				return 8;
			}
			if (n < 1000000000)
			{
				return 9;
			}
			return 10;
		}
		if (n > -10)
		{
			return 2;
		}
		if (n > -100)
		{
			return 3;
		}
		if (n > -1000)
		{
			return 4;
		}
		if (n > -10000)
		{
			return 5;
		}
		if (n > -100000)
		{
			return 6;
		}
		if (n > -1000000)
		{
			return 7;
		}
		if (n > -10000000)
		{
			return 8;
		}
		if (n > -100000000)
		{
			return 9;
		}
		if (n > -1000000000)
		{
			return 10;
		}
		return 11;
	}

	public static int Digits(this uint n)
	{
		if (n < 10)
		{
			return 1;
		}
		if (n < 100)
		{
			return 2;
		}
		if (n < 1000)
		{
			return 3;
		}
		if (n < 10000)
		{
			return 4;
		}
		if (n < 100000)
		{
			return 5;
		}
		if (n < 1000000)
		{
			return 6;
		}
		if (n < 10000000)
		{
			return 7;
		}
		if (n < 100000000)
		{
			return 8;
		}
		if (n < 1000000000)
		{
			return 9;
		}
		return 10;
	}

	public static string ToZeroPaddedString(this int value, int noOfDigits)
	{
		switch (noOfDigits)
		{
		case 1:
			return value.ToString("0");
		case 2:
			return value.ToString("00");
		case 3:
			return value.ToString("000");
		case 4:
			return value.ToString("0000");
		case 5:
			return value.ToString("00000");
		case 6:
			return value.ToString("000000");
		case 7:
			return value.ToString("0000000");
		case 8:
			return value.ToString("00000000");
		case 9:
			return value.ToString("000000000");
		case 10:
			return value.ToString("0000000000");
		default:
		{
			string text = value.ToString();
			Debug.LogError($"Number of digits {noOfDigits} is unsupported, returning {text}");
			return text;
		}
		}
	}

	public static string ToZeroPaddedString(this uint value, int noOfDigits)
	{
		switch (noOfDigits)
		{
		case 1:
			return value.ToString("0");
		case 2:
			return value.ToString("00");
		case 3:
			return value.ToString("000");
		case 4:
			return value.ToString("0000");
		case 5:
			return value.ToString("00000");
		case 6:
			return value.ToString("000000");
		case 7:
			return value.ToString("0000000");
		case 8:
			return value.ToString("00000000");
		case 9:
			return value.ToString("000000000");
		case 10:
			return value.ToString("0000000000");
		default:
		{
			string text = value.ToString();
			Debug.LogError($"Number of digits {noOfDigits} is unsupported, returning {text}");
			return text;
		}
		}
	}
}
