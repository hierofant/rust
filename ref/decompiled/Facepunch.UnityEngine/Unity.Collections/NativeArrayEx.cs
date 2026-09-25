using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace Unity.Collections;

public static class NativeArrayEx
{
	public static void Add<T>(this ref NativeArray<T> array, T item, ref int size) where T : unmanaged
	{
		if (size >= array.Length)
		{
			Expand(ref array, array.Length * 2);
		}
		array[size] = item;
		size++;
	}

	public static void RemoveUnordered<T>(this ref NativeArray<T> array, int index, ref int count) where T : unmanaged
	{
		int num = count - 1;
		if (index != num)
		{
			array[index] = array[num];
		}
		count--;
	}

	public static void Expand<T>(this ref NativeArray<T> array, int newCapacity, NativeArrayOptions options = NativeArrayOptions.ClearMemory, bool copyContents = true, bool usePowerOfTwo = false) where T : struct
	{
		if (newCapacity <= array.Length)
		{
			return;
		}
		NativeArray<T> nativeArray = new NativeArray<T>(usePowerOfTwo ? Mathf.NextPowerOfTwo(newCapacity) : newCapacity, Allocator.Persistent, options);
		if (array.IsCreated)
		{
			if (copyContents)
			{
				array.CopyTo(nativeArray.GetSubArray(0, array.Length));
			}
			array.Dispose();
		}
		array = nativeArray;
	}

	public static void SafeDispose<T>(this ref NativeArray<T> array) where T : unmanaged
	{
		if (array.IsCreated)
		{
			array.Dispose();
		}
	}

	public static void SafeDispose<T>(this ref NativeArray<T> array, JobHandle dependsOn) where T : unmanaged
	{
		if (array.IsCreated)
		{
			array.Dispose(dependsOn);
		}
	}

	public unsafe static void MemClear<T>(this in NativeArray<T> array) where T : struct
	{
		UnsafeUtility.MemClear(NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(array), UnsafeUtility.SizeOf<T>() * array.Length);
	}

	public unsafe static void MemSet<T>(this in NativeArray<T> array, byte value) where T : struct
	{
		UnsafeUtility.MemSet(NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(array), value, UnsafeUtility.SizeOf<T>() * array.Length);
	}

	public unsafe static NativeBitArray AsBitArray<T>(this ref NativeArray<T> array) where T : struct
	{
		return NativeBitArrayUnsafeUtility.ConvertExistingDataToNativeBitArray(NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(array), array.Length, Allocator.None);
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	private static void CheckReinterpretLoadRange<U, T>(this ref NativeArray<T>.ReadOnly array, int sourceIndex) where U : struct where T : struct
	{
	}

	public unsafe static U ReinterpretLoad<U, T>(this ref NativeArray<T>.ReadOnly array, int sourceIndex) where U : struct where T : struct
	{
		return UnsafeUtility.ReadArrayElement<U>((byte*)array.GetUnsafeReadOnlyPtr() + (long)UnsafeUtility.SizeOf<T>() * (long)sourceIndex, 0);
	}

	public static bool TryFindRegion<T>(this ref NativeArray<T> array, T value, ref int index, ref int length) where T : struct, IEquatable<T>
	{
		index += length;
		while (index < array.Length && !EqualityComparer<T>.Default.Equals(array[index], value))
		{
			index++;
		}
		if (index == array.Length)
		{
			return false;
		}
		length = 1;
		while (index + length < array.Length && EqualityComparer<T>.Default.Equals(array[index + length], value))
		{
			length++;
		}
		return true;
	}

	public static bool GenericEquals<T>(T a, T b) where T : IEquatable<T>
	{
		return a.Equals(b);
	}
}
