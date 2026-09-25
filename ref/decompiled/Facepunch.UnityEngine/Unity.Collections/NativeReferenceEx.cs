namespace Unity.Collections;

public static class NativeReferenceEx
{
	public static void SafeDispose<T>(this ref NativeReference<T> reference) where T : unmanaged
	{
		if (reference.IsCreated)
		{
			reference.Dispose();
		}
	}
}
