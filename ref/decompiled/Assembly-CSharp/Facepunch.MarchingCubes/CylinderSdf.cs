using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace Facepunch.MarchingCubes;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal readonly struct CylinderSdf : Facepunch.MarchingCubes.ISdf
{
	public float Distance(in Shape s, float3 p)
	{
		return s.CylinderDistance(p);
	}

	float Facepunch.MarchingCubes.ISdf.Distance(in Shape s, float3 p)
	{
		return Distance(in s, p);
	}
}
