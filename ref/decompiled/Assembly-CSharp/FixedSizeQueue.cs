using System.Collections.Generic;

public class FixedSizeQueue<T> : Queue<T>
{
	private readonly int _maxSize;

	public FixedSizeQueue(int maxSize)
	{
		_maxSize = maxSize;
	}

	public new void Enqueue(T item)
	{
		base.Enqueue(item);
		while (base.Count > _maxSize)
		{
			Dequeue();
		}
	}
}
