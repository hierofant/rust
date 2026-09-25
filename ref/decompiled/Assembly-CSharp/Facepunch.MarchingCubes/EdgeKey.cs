using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;

namespace Facepunch.MarchingCubes;

internal readonly struct EdgeKey
{
	public readonly float3 vertex;

	public readonly int edgeId;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public EdgeKey(int3 cLo, int3 cHi, float sLo, float sHi, float iso, float3 vertexOffset, float scale, int edgeId)
	{
		float t = math.saturate(math.unlerp(sLo, sHi, iso));
		vertex = (math.lerp(cLo, cHi, t) - vertexOffset) * scale;
		this.edgeId = edgeId;
	}

	[BurstDiscard]
	public override string ToString()
	{
		return $"{edgeId} | {vertex}";
	}
}
