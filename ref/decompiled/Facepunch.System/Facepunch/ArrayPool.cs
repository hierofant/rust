using System.Collections.Concurrent;

namespace Facepunch;

public class ArrayPool<T>
{
	private int count;

	private ConcurrentQueue<T[]>[] buffer;

	public ArrayPool(int maxSize)
	{
		count = SizeToIndex(maxSize) + 1;
		buffer = new ConcurrentQueue<T[]>[count];
		for (int i = 0; i < count; i++)
		{
			buffer[i] = new ConcurrentQueue<T[]>();
		}
	}

	public ConcurrentQueue<T[]>[] GetBuffer()
	{
		return buffer;
	}

	public T[] Rent(int minSize)
	{
		int num = SizeToIndex(minSize);
		if (!buffer[num].TryDequeue(out var result))
		{
			return new T[IndexToSize(num)];
		}
		return result;
	}

	public void Return(T[] array)
	{
		int num = SizeToIndex(array.Length);
		buffer[num].Enqueue(array);
	}

	public int SizeToIndex(int size)
	{
		size = NextPowerOfTwo(size);
		int num = 0;
		while ((size >>= 1) != 0)
		{
			num++;
		}
		return num;
	}

	public int IndexToSize(int index)
	{
		return 1 << index;
	}

	private static int NextPowerOfTwo(int n)
	{
		if (n == 0)
		{
			return 1;
		}
		n--;
		n |= n >> 1;
		n |= n >> 2;
		n |= n >> 4;
		n |= n >> 8;
		n |= n >> 16;
		return n + 1;
	}
}
