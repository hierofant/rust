using System;

namespace Facepunch;

public static class TimeSpanExt
{
	public static TimeSpan FromMicroseconds(double ms)
	{
		return new TimeSpan((long)(ms * 1000.0) * 10000 / 1000);
	}
}
