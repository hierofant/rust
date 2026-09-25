using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BufferList<T> : IEnumerable<T>, IEnumerable
{
	public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
	{
		private readonly BufferList<T> list;

		private int index;

		public T Current => list[index];

		object IEnumerator.Current => Current;

		public Enumerator(BufferList<T> list)
		{
			this.list = list;
			index = -1;
		}

		public bool MoveNext()
		{
			index++;
			return index < list.Count;
		}

		public void Reset()
		{
			index = -1;
		}

		public void Dispose()
		{
		}
	}

	private int count;

	private T[] buffer;

	public int Count => count;

	public int Capacity => buffer.Length;

	public T[] Buffer => buffer;

	public T this[int index]
	{
		get
		{
			return buffer[index];
		}
		set
		{
			buffer[index] = value;
		}
	}

	public BufferList()
	{
		buffer = Array.Empty<T>();
	}

	public BufferList(int capacity)
	{
		buffer = ((capacity == 0) ? Array.Empty<T>() : new T[capacity]);
	}

	protected BufferList(T[] array)
	{
		buffer = array;
	}

	public void Push(T element)
	{
		Add(element);
	}

	public T Pop()
	{
		if (count == 0)
		{
			return default(T);
		}
		T result = buffer[count - 1];
		buffer[count - 1] = default(T);
		count--;
		return result;
	}

	public void Add(T element)
	{
		if (count == buffer.Length)
		{
			Resize(Math.Max(buffer.Length * 2, 8));
		}
		buffer[count] = element;
		count++;
	}

	public void AddRange(BufferList<T> elements)
	{
		if (count + elements.count > buffer.Length)
		{
			Resize(Math.Max(count + elements.count, 8));
		}
		Array.Copy(elements.buffer, 0, buffer, count, elements.count);
		count += elements.count;
	}

	public void AddSpan(Span<T> span)
	{
		count += span.Length;
		if (buffer.Length < count)
		{
			Resize(Mathf.NextPowerOfTwo(count));
		}
		span.CopyTo(buffer.AsSpan(count - span.Length));
	}

	public void AddSpan(ReadOnlySpan<T> span)
	{
		count += span.Length;
		if (buffer.Length < count)
		{
			Resize(Mathf.NextPowerOfTwo(count));
		}
		span.CopyTo(buffer.AsSpan(count - span.Length));
	}

	public Span<T> NewSpan(int length)
	{
		count += length;
		if (buffer.Length < count)
		{
			Resize(Mathf.NextPowerOfTwo(count));
		}
		return buffer.AsSpan(count - length);
	}

	public bool Remove(T element)
	{
		int num = Array.IndexOf(buffer, element);
		if (num == -1)
		{
			return false;
		}
		if (count == 0)
		{
			return false;
		}
		RemoveAt(num);
		return true;
	}

	public void RemoveAt(int index)
	{
		for (int i = index; i < count - 1; i++)
		{
			buffer[i] = buffer[i + 1];
		}
		buffer[count - 1] = default(T);
		count--;
	}

	public bool RemoveUnordered(T element)
	{
		int num = Array.IndexOf(buffer, element);
		if (num == -1)
		{
			return false;
		}
		if (count == 0)
		{
			return false;
		}
		RemoveUnordered(num);
		return true;
	}

	public void RemoveUnordered(int index)
	{
		buffer[index] = buffer[count - 1];
		buffer[count - 1] = default(T);
		count--;
	}

	public void CopyFrom(T[] array)
	{
		int num = array.Length;
		if (num > buffer.Length)
		{
			Resize(num);
		}
		Array.Copy(array, buffer, num);
		if (num < count)
		{
			Array.Clear(buffer, num, count - num);
		}
		count = num;
	}

	public void CopyFrom(List<T> list)
	{
		int num = list.Count;
		if (num > buffer.Length)
		{
			Resize(num);
		}
		list.CopyTo(buffer);
		if (num < count)
		{
			Array.Clear(buffer, num, count - num);
		}
		count = num;
	}

	public int IndexOf(T element)
	{
		return Array.IndexOf(buffer, element);
	}

	public int LastIndexOf(T element)
	{
		return Array.LastIndexOf(buffer, element);
	}

	public bool Contains(T element)
	{
		return Array.IndexOf(buffer, element) != -1;
	}

	public void Clear()
	{
		if (count != 0)
		{
			Array.Clear(buffer, 0, count);
			count = 0;
		}
	}

	public void Resize(int newSize)
	{
		Array.Resize(ref buffer, newSize);
	}

	public ReadOnlySpan<T> ContentReadOnlySpan()
	{
		return new ReadOnlySpan<T>(Buffer, 0, count);
	}

	public Span<T> ContentSpan()
	{
		return new Span<T>(Buffer, 0, count);
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
