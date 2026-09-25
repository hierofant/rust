public static class FloatEx
{
	public static bool IsNaNOrInfinity(this float f)
	{
		if (!float.IsNaN(f))
		{
			return float.IsInfinity(f);
		}
		return true;
	}

	public static bool IsNaNOrInfinity(this double d)
	{
		if (!double.IsNaN(d))
		{
			return double.IsInfinity(d);
		}
		return true;
	}

	public static bool IsNaN(this float f)
	{
		return float.IsNaN(f);
	}

	public static bool IsNaN(this double d)
	{
		return double.IsNaN(d);
	}

	public static bool IsInfinity(this float f)
	{
		return float.IsInfinity(f);
	}

	public static bool IsInfinity(this double d)
	{
		return double.IsInfinity(d);
	}
}
