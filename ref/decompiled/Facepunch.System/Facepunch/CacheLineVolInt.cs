using System.Runtime.InteropServices;

namespace Facepunch;

[StructLayout(LayoutKind.Sequential, Size = 64)]
public struct CacheLineVolInt
{
	public volatile int Value;
}
