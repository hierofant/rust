using System;
using System.Diagnostics;
using UnityEngine;

namespace Rust.Assertions;

public static class Assert
{
	public class SafetyException : Exception
	{
		public SafetyException(string message)
			: base(message)
		{
		}
	}

	public class NullEntityException : SafetyException
	{
		public NullEntityException()
			: base("Entity reference is null.")
		{
		}
	}

	public class InvalidEntityException : SafetyException
	{
		public InvalidEntityException()
			: base("Entity is invalid.")
		{
		}
	}

	public class WrongSideException : SafetyException
	{
		public WrongSideException(string side)
			: base("Entity is not a " + side + "-side entity.")
		{
		}
	}

	public class NullResourceRefException : SafetyException
	{
		public NullResourceRefException()
			: base("ResourceRef is null.")
		{
		}
	}

	public class InvalidResourceRefException : SafetyException
	{
		public InvalidResourceRefException()
			: base("ResourceRef is invalid.")
		{
		}
	}

	public static void EntityValid(BaseEntity entity)
	{
		if (!entity)
		{
			throw new NullEntityException();
		}
		if (!entity.IsValid())
		{
			throw new InvalidEntityException();
		}
	}

	public static void EntityIsServer(BaseEntity entity)
	{
		EntityValid(entity);
		if (!entity.isServer)
		{
			throw new WrongSideException("server");
		}
	}

	public static void EntityIsClient(BaseEntity entity)
	{
		EntityValid(entity);
		if (entity.isServer)
		{
			throw new WrongSideException("client");
		}
	}

	public static void RefValid<T>(ResourceRef<T> gRef) where T : UnityEngine.Object
	{
		if (gRef == null)
		{
			throw new NullResourceRefException();
		}
		if (!gRef.isValid)
		{
			throw new InvalidResourceRefException();
		}
	}

	public static void That(bool condition, string message = "Assertion failed")
	{
		if (!condition)
		{
			UnityEngine.Debug.LogError(message);
		}
	}

	[Conditional("SERVER")]
	public static void ThatOnServer(bool condition, string message = "Server assertion failed")
	{
		if (!condition)
		{
			UnityEngine.Debug.LogError(message);
		}
	}

	[Conditional("CLIENT")]
	public static void ThatOnClient(bool condition, string message = "Client assertion failed")
	{
		if (!condition)
		{
			UnityEngine.Debug.LogError(message);
		}
	}

	public static void NotNull(object obj, string message = "Value must not be null")
	{
		That(obj != null, message);
	}

	public static void Equal(object actual, object expected, string message = "Values are not equal")
	{
		That(AreEqual(actual, expected), message);
	}

	public static void NotEqual(object actual, object notExpected, string message = "Values should not be equal")
	{
		That(!AreEqual(actual, notExpected), message);
	}

	public static void InRange<T>(T value, T min, T max, string message = "Values not in range") where T : IComparable<T>
	{
		That(value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0, message);
	}

	private static bool AreEqual(object a, object b)
	{
		if (a is UnityEngine.Object || b is UnityEngine.Object)
		{
			return (UnityEngine.Object)a == (UnityEngine.Object)b;
		}
		return object.Equals(a, b);
	}
}
