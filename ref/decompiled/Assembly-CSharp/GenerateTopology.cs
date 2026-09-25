using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class GenerateTopology : ProceduralComponent
{
	[DllImport("RustNative", EntryPoint = "generate_topology")]
	public unsafe static extern void Native_GenerateTopology(int* map, int res, Vector3 pos, Vector3 size, uint seed, float lootAngle, float lootTier0, float lootTier1, float lootTier2, float biomeAngle, float biomeArid, float biomeTemperate, float biomeTundra, float biomeArctic, short* heightmap, int heightres, byte* biomemap, int biomeres);

	public unsafe override void Process(uint seed)
	{
		int* unsafePtr = (int*)TerrainMeta.TopologyMap.dst.GetUnsafePtr();
		int res = TerrainMeta.TopologyMap.res;
		Vector3 position = TerrainMeta.Position;
		Vector3 size = TerrainMeta.Size;
		float lootAxisAngle = TerrainMeta.LootAxisAngle;
		float biomeAxisAngle = TerrainMeta.BiomeAxisAngle;
		short* unsafePtr2 = (short*)TerrainMeta.HeightMap.src.GetUnsafePtr();
		int res2 = TerrainMeta.HeightMap.res;
		byte* unsafePtr3 = (byte*)TerrainMeta.BiomeMap.src.GetUnsafePtr();
		int res3 = TerrainMeta.BiomeMap.res;
		Native_GenerateTopology(unsafePtr, res, position, size, seed, lootAxisAngle, World.Config.PercentageTier0, World.Config.PercentageTier1, World.Config.PercentageTier2, biomeAxisAngle, World.Config.PercentageBiomeArid, World.Config.PercentageBiomeTemperate, World.Config.PercentageBiomeTundra, World.Config.PercentageBiomeArctic, unsafePtr2, res2, unsafePtr3, res3);
	}
}
