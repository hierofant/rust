using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace Facepunch.MarchingCubes;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal readonly struct ConeSdf : Facepunch.MarchingCubes.ISdf
{
	public float Distance(in Shape s, float3 p)
	{
		return s.ConeDistance(p);
	}

	float Facepunch.MarchingCubes.ISdf.Distance(in Shape s, float3 p)
	{
		return Distance(in s, p);
	}
}
