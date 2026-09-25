#define UNITY_ASSERTIONS
using UnityEngine;
using UnityEngine.Assertions;

public static class CustomAssert
{
	private static string WithExpectedActual(string message, object expected, object actual)
	{
		if (!string.IsNullOrEmpty(message))
		{
			return message + "\n" + $"Expected: {expected}\nActual: {actual}";
		}
		return $"Expected: {expected}\nActual: {actual}";
	}

	private static void Fail(string details, string context)
	{
		if (Assert.raiseExceptions)
		{
			throw new AssertionException(details, context);
		}
		if (details == null)
		{
			details = "Assertion has failed";
		}
		if (context != null)
		{
			details = context + "\n" + details;
		}
		Debug.LogAssertion(details);
	}

	public static void That(bool condition, string message = null)
	{
		if (!condition)
		{
			Fail("Assertion has failed", message);
		}
	}

	public static void IsGreater(float actual, float expected, string message = null)
	{
		if (!(actual > expected))
		{
			Fail(WithExpectedActual(null, $"> {expected}", actual), message);
		}
	}

	public static void IsGreaterOrEqual(float actual, float expected, string message = null)
	{
		if (!(actual >= expected))
		{
			Fail(WithExpectedActual(null, $">= {expected}", actual), message);
		}
	}

	public static void IsLess(float actual, float expected, string message = null)
	{
		if (!(actual < expected))
		{
			Fail(WithExpectedActual(null, $"< {expected}", actual), message);
		}
	}

	public static void IsLessOrEqual(float actual, float expected, string message = null)
	{
		if (!(actual <= expected))
		{
			Fail(WithExpectedActual(null, $"<= {expected}", actual), message);
		}
	}

	public static void IsOn(BaseEntity entity, string message = null)
	{
		bool flag = entity.IsOn();
		if (!flag)
		{
			Fail(WithExpectedActual(entity.name + " On flag mismatch", true, flag), message);
		}
	}

	public static void IsOff(BaseEntity entity, string message = null)
	{
		bool flag = entity.IsOn();
		if (flag)
		{
			Fail(WithExpectedActual(entity.name + " On flag mismatch", false, flag), message);
		}
	}

	public static void IsBusy(BaseEntity entity, string message = null)
	{
		bool flag = entity.IsBusy();
		if (!flag)
		{
			Fail(WithExpectedActual(entity.name + " Busy flag mismatch", true, flag), message);
		}
	}

	public static void HasPower(IOEntity entity, string message = null)
	{
		bool flag = entity.HasFlag(BaseEntity.Flags.Reserved8);
		if (!flag)
		{
			Fail(WithExpectedActual(entity.name + " HasPower flag mismatch", true, flag), message);
		}
	}

	public static void HasNoPower(IOEntity entity, string message = null)
	{
		bool flag = entity.HasFlag(BaseEntity.Flags.Reserved8);
		if (flag)
		{
			Fail(WithExpectedActual(entity.name + " HasPower flag mismatch", false, flag), message);
		}
	}

	public static void IsOpen(BaseEntity entity, string message = null)
	{
		bool flag = entity.IsOpen();
		if (!flag)
		{
			Fail(WithExpectedActual(entity.name + " Open flag mismatch", true, flag), message);
		}
	}

	public static void IsClosed(BaseEntity entity, string message = null)
	{
		bool flag = entity.IsOpen();
		if (flag)
		{
			Fail(WithExpectedActual(entity.name + " Open flag mismatch", false, flag), message);
		}
	}
}
