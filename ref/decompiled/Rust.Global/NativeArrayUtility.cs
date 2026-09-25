using Unity.Burst;
using Unity.Collections;

[BurstCompile]
public static class NativeArrayUtility
{
	public static NativeArray<T> CopyToNativeArray<T>(this T[] array, Allocator allocator) where T : struct
	{
		NativeArray<T> result = new NativeArray<T>(array.Length, allocator);
		for (int i = 0; i < array.Length; i++)
		{
			result[i] = array[i];
		}
		return result;
	}

	[BurstCompile]
	public static void Fill<T>(ref NativeArray<T> nativeArray, T value) where T : unmanaged
	{
		for (int i = 0; i < nativeArray.Length; i++)
		{
			nativeArray[i] = value;
		}
	}
}
