#define UNITY_ASSERTIONS
using System;
using Unity.Collections;
using UnityEngine;

public class StableObjectArray
{
	public const int NotInCache = -1;
}
public class StableObjectArray<T> : StableObjectArray, IDisposable
{
	private T[] _objects;

	private NativeArray<int> _stableIndexLookup;

	private int[] _movingIndices;

	private int _firstFree;

	private int _count;

	private bool _canRepack;

	public int Count => _count;

	public int Capacity => _objects.Length;

	public ReadOnlySpan<T> Objects => new ReadOnlySpan<T>(_objects, 0, _count);

	public T[] UnsafeObjects => _objects;

	public NativeArray<int>.ReadOnly StableIndexLookup => _stableIndexLookup.GetSubArray(0, _count).AsReadOnly();

	public StableObjectArray(int initCapacity)
	{
		_objects = new T[initCapacity];
		_movingIndices = new int[initCapacity];
		_stableIndexLookup = new NativeArray<int>(initCapacity, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		SetupFreeList(0);
	}

	public int Add(T obj)
	{
		int num = _count++;
		if (_count > _objects.Length)
		{
			Grow();
		}
		_objects[num] = obj;
		int firstFree = _firstFree;
		int freeIndex = _movingIndices[_firstFree];
		_movingIndices[_firstFree] = num;
		_stableIndexLookup[num] = firstFree;
		_firstFree = AsSlotIndex(freeIndex);
		return firstFree;
	}

	public void RemoveAtSwapback(int stableIndex, bool invalidateStableIndex = false)
	{
		int num = --_count;
		int num2 = _stableIndexLookup[num];
		int num3 = _movingIndices[stableIndex];
		_objects[num3] = _objects[num];
		if (invalidateStableIndex)
		{
			_movingIndices[stableIndex] = num3;
			_stableIndexLookup[num3] = stableIndex;
			_movingIndices[num2] = AsFreeIndex(_firstFree);
			_firstFree = num2;
		}
		else
		{
			_movingIndices[num2] = _movingIndices[stableIndex];
			_stableIndexLookup[num3] = num2;
			_movingIndices[stableIndex] = AsFreeIndex(_firstFree);
			_firstFree = stableIndex;
		}
		_objects[num] = default(T);
		_canRepack |= !invalidateStableIndex;
	}

	public T Get(int stableIndex)
	{
		int num = _movingIndices[stableIndex];
		if (num >= 0)
		{
			return _objects[num];
		}
		return default(T);
	}

	public int GetIndexForSyncRemove(int stableIndex)
	{
		int num = _movingIndices[stableIndex];
		Debug.Assert(num >= 0, "StableIndex is invalid!");
		return num;
	}

	public bool Repack(Action<int, int> onSwap)
	{
		if (!_canRepack)
		{
			return false;
		}
		bool flag = false;
		int num = 0;
		int num2 = 0;
		while (num < _count)
		{
			int num3 = _movingIndices[num2];
			if (num3 == num2)
			{
				num++;
			}
			else if (num3 >= 0)
			{
				flag = true;
				_movingIndices[num2] = _movingIndices[num3];
				_movingIndices[num3] = num3;
				onSwap?.Invoke(num2, num3);
				if (num3 < num2 && num3 >= 0)
				{
					num++;
				}
				num2--;
			}
			num2++;
		}
		if (flag)
		{
			for (num2 = 0; num2 < _count; num2++)
			{
				_stableIndexLookup[num2] = num2;
			}
			SetupFreeList(_count);
		}
		_canRepack = false;
		return flag;
	}

	public void Dispose()
	{
		_objects = null;
		_stableIndexLookup.Dispose();
		_movingIndices = null;
	}

	private void Grow()
	{
		int num = _objects.Length;
		int num2 = Mathf.Max(num * 2, 1);
		Array.Resize(ref _objects, num2);
		Array.Resize(ref _movingIndices, num2);
		NativeArrayEx.Expand(ref _stableIndexLookup, num2, NativeArrayOptions.UninitializedMemory);
		SetupFreeList(num);
	}

	private void SetupFreeList(int from)
	{
		for (int i = from; i < _movingIndices.Length; i++)
		{
			int slot = i + 1;
			_movingIndices[i] = AsFreeIndex(slot);
		}
		_firstFree = from;
	}

	private static int AsFreeIndex(int slot)
	{
		Debug.Assert(slot >= 0);
		return -(slot + 1);
	}

	private static int AsSlotIndex(int freeIndex)
	{
		Debug.Assert(freeIndex < 0);
		return -freeIndex - 1;
	}
}
