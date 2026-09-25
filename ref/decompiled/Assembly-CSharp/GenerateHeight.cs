using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class GenerateHeight : ProceduralComponent
{
	[DllImport("RustNative", EntryPoint = "generate_height")]
	public unsafe static extern void Native_GenerateHeight(short* map, int res, Vector3 pos, Vector3 size, uint seed, float lootAngle, float lootTier0, float lootTier1, float lootTier2, float biomeAngle, float biomeArid, float biomeTemperate, float biomeTundra, float biomeArctic);

	public unsafe override void Process(uint seed)
	{
		short* unsafePtr = (short*)TerrainMeta.HeightMap.dst.GetUnsafePtr();
		int res = TerrainMeta.HeightMap.res;
		Vector3 position = TerrainMeta.Position;
		Vector3 size = TerrainMeta.Size;
		float lootAxisAngle = TerrainMeta.LootAxisAngle;
		float biomeAxisAngle = TerrainMeta.BiomeAxisAngle;
		Native_GenerateHeight(unsafePtr, res, position, size, seed, lootAxisAngle, World.Config.PercentageTier0, World.Config.PercentageTier1, World.Config.PercentageTier2, biomeAxisAngle, World.Config.PercentageBiomeArid, World.Config.PercentageBiomeTemperate, World.Config.PercentageBiomeTundra, World.Config.PercentageBiomeArctic);
	}
}
