#define UNITY_ASSERTIONS
using Unity.Collections;
using UnityEngine;
using UnityEngine.Assertions;

namespace Rust.Rendering.IndirectInstancing;

internal struct TerrainRef
{
	public NativeArray<short>.ReadOnly data;

	public NativeArray<byte>.ReadOnly alpha;

	public Vector3 pos;

	public Vector3 size;

	public Vector3 one_over_size;

	public int res;

	public int alpha_res;

	public static Rust.Rendering.IndirectInstancing.TerrainRef FromCurrent()
	{
		Assert.IsNotNull(TerrainMeta.HeightMap, "Cannot create TerrainRef because there is no terrain!");
		Rust.Rendering.IndirectInstancing.TerrainRef result = default(Rust.Rendering.IndirectInstancing.TerrainRef);
		result.data = TerrainMeta.HeightMap.src.AsReadOnly();
		result.alpha = TerrainMeta.AlphaMap.src.AsReadOnly();
		result.pos = TerrainMeta.Position;
		result.size = TerrainMeta.Size;
		result.one_over_size = TerrainMeta.OneOverSize;
		result.res = TerrainMeta.HeightMap.res;
		result.alpha_res = TerrainMeta.AlphaMap.res;
		return result;
	}
}
