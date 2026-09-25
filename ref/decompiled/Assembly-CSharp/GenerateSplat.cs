using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class GenerateSplat : ProceduralComponent
{
	[DllImport("RustNative", EntryPoint = "generate_splat")]
	public unsafe static extern void Native_GenerateSplat(byte* map, int res, Vector3 pos, Vector3 size, uint seed, float lootAngle, float biomeAngle, short* heightmap, int heightres, byte* biomemap, int biomeres, int* topologymap, int topologyres);

	public unsafe override void Process(uint seed)
	{
		byte* unsafePtr = (byte*)TerrainMeta.SplatMap.dst.GetUnsafePtr();
		int res = TerrainMeta.SplatMap.res;
		Vector3 position = TerrainMeta.Position;
		Vector3 size = TerrainMeta.Size;
		float lootAxisAngle = TerrainMeta.LootAxisAngle;
		float biomeAxisAngle = TerrainMeta.BiomeAxisAngle;
		short* unsafePtr2 = (short*)TerrainMeta.HeightMap.src.GetUnsafePtr();
		int res2 = TerrainMeta.HeightMap.res;
		byte* unsafePtr3 = (byte*)TerrainMeta.BiomeMap.src.GetUnsafePtr();
		int res3 = TerrainMeta.BiomeMap.res;
		int* unsafePtr4 = (int*)TerrainMeta.TopologyMap.src.GetUnsafePtr();
		int res4 = TerrainMeta.TopologyMap.res;
		Native_GenerateSplat(unsafePtr, res, position, size, seed, lootAxisAngle, biomeAxisAngle, unsafePtr2, res2, unsafePtr3, res3, unsafePtr4, res4);
	}
}
