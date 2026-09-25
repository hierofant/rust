namespace Unity.Collections;

public static class NativeListEx
{
	public static void Expand<T>(this ref NativeList<T> list, int newCapacity, bool copyContents = true) where T : unmanaged
	{
		if (!list.IsCreated || newCapacity > list.Capacity)
		{
			if (list.IsCreated)
			{
				if (copyContents)
				{
					list.Capacity = newCapacity;
				}
				else
				{
					list.Dispose();
					list = new NativeList<T>(newCapacity, Allocator.Persistent);
				}
			}
			else
			{
				list = new NativeList<T>(newCapacity, Allocator.Persistent);
			}
		}
		if (!copyContents)
		{
			list.Clear();
		}
	}

	public static void SafeDispose<T>(this ref NativeList<T> list) where T : unmanaged
	{
		if (list.IsCreated)
		{
			list.Dispose();
		}
	}

	public static void CopyFrom<T>(this ref NativeList<T> list, in NativeArray<T>.ReadOnly from) where T : unmanaged
	{
		list.Resize(from.Length, NativeArrayOptions.UninitializedMemory);
		from.CopyTo(list.AsArray());
	}
}
