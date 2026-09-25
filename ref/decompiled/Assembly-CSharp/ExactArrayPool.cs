using System;
using System.Collections.Concurrent;

public class ExactArrayPool<T>
{
	private readonly ConcurrentDictionary<int, ConcurrentQueue<T[]>> _buffers = new ConcurrentDictionary<int, ConcurrentQueue<T[]>>();

	private static readonly Func<int, ConcurrentQueue<T[]>> QueueFactory = (int size) => new ConcurrentQueue<T[]>();

	public T[] Rent(int size)
	{
		if (!_buffers.GetOrAdd(size, QueueFactory).TryDequeue(out var result))
		{
			return new T[size];
		}
		return result;
	}

	public void Return(T[] array)
	{
		_buffers.GetOrAdd(array.Length, QueueFactory).Enqueue(array);
	}
}
