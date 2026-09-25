using UnityEngine;

public class BiomeBenchmarkScene : BenchmarkScene
{
	[Header("Biome Benchmark")]
	public uint IslandWidth = 300u;

	public uint IslandHeight = 1000u;

	public uint IslandGap = 50u;

	public float FlyingSpeed = 16f;

	public GameObject WorldSetupPrefab;

	public TerrainConfig TerrainConfig;

	public TerrainBiome.Enum BiomesToTest = (TerrainBiome.Enum)(-1);

	public float StreamingPause = 4f;

	[Header("Biome Benchmark - Debug")]
	public bool DebugMode;

	[Range(0f, 4f)]
	public int DebugIslandNum;

	[Range(0f, 1f)]
	public float DebugProgress;
}
