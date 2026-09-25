public class PlayerModelBenchmarkScene : BenchmarkScene
{
	public enum LODLevel
	{
		Lod0,
		Lod1,
		Lod2,
		Lod3,
		Culled,
		Invisible
	}

	public GameObjectRef PlayerModelPrefab;

	public int ModelCount = 100;

	public LODLevel LodLevel;

	public bool UseRandomAnims;
}
