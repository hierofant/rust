using System;

public static class TimeFormatUtils
{
	private static string UnitName(string singular, string plural, long count)
	{
		if (count != 1)
		{
			return Translate.Get(plural, plural);
		}
		return Translate.Get(singular, singular);
	}

	public static string FormatSeconds(long s)
	{
		long num = (long)Math.Floor((float)s / 60f);
		long num2 = (long)Math.Floor((float)num / 60f);
		long num3 = (long)Math.Floor((float)num2 / 24f);
		if (s < 60)
		{
			return string.Format("{0} {1}", s, UnitName("second", "seconds", s));
		}
		if (num < 60)
		{
			return string.Format("{0} {1}", num, UnitName("minute", "minutes", num));
		}
		if (num2 < 48)
		{
			return string.Format("{0} {1}", num2, UnitName("hour", "hours", num2));
		}
		if (num3 < 2)
		{
			return string.Format("{0} {1}, {2} {3}", num3, UnitName("day", "days", num3), num2 % 24, UnitName("hour", "hours", num2 % 24));
		}
		return string.Format("{0} {1}", num3, Translate.Get("days", "days"));
	}
}
