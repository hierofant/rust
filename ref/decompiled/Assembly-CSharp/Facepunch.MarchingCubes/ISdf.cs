using Unity.Mathematics;

namespace Facepunch.MarchingCubes;

internal interface ISdf
{
	float Distance(in Shape s, float3 p);
}
