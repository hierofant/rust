using System.Runtime.InteropServices;

namespace Facepunch;

[StructLayout(LayoutKind.Sequential, Size = 64)]
public struct CacheLine<T>
{
	public T Value;
}
