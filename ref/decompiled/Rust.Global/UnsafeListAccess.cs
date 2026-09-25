#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Scripting;

public static class UnsafeListAccess
{
	[Preserve]
	private class ListPrivateFieldAccess<T>
	{
		internal T[] _items;

		internal int _size;

		internal int _version;
	}

	private static ListPrivateFieldAccess<T> GetPrivateFieldsUnsafe<T>(this List<T> list)
	{
		Debug.Assert(list != null);
		return Unsafe.As<ListPrivateFieldAccess<T>>(list);
	}

	private static T[] GetInternalArrayUnsafe<T>(this List<T> list)
	{
		return list.GetPrivateFieldsUnsafe()._items;
	}

	public static Span<T> ListAsSpan<T>(this List<T> list)
	{
		return list.GetInternalArrayUnsafe().AsSpan(0, list.Count);
	}

	public static ReadOnlySpan<T> ListAsReadOnlySpan<T>(this List<T> list)
	{
		return list.GetInternalArrayUnsafe().AsSpan(0, list.Count);
	}

	public static ReadOnlySpan<U> ListAsReadOnlySpanOf<T, U>(this List<T> list) where T : class, U
	{
		U[] internalArrayUnsafe = list.GetInternalArrayUnsafe();
		return new ReadOnlySpan<U>(internalArrayUnsafe, 0, list.Count);
	}
}
