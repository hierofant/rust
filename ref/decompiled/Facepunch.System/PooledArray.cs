using System;
using System.Buffers;
using Facepunch;

public struct PooledArray<T> : IDisposable, Pool.IPooled
{
	public readonly T this[int index]
	{
		get
		{
			return Array[index];
		}
		set
		{
			Array[index] = value;
		}
	}

	public T[] Array { get; private set; }

	public PooledArray(int size)
	{
		Array = System.Buffers.ArrayPool<T>.Shared.Rent(size);
	}

	void IDisposable.Dispose()
	{
		if (Array != null)
		{
			System.Buffers.ArrayPool<T>.Shared.Return(Array, clearArray: true);
			Array = null;
		}
	}

	void Pool.IPooled.EnterPool()
	{
	}

	void Pool.IPooled.LeavePool()
	{
	}

	public static implicit operator T[](PooledArray<T> pooledArray)
	{
		return pooledArray.Array;
	}
}
