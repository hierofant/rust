using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class FoliageGrid : SingletonComponent<FoliageGrid>, IClientComponent
{
	private const int GRID_POOL_SIZE = 48;

	private const float FOLIAGE_CULL_RADIUS = 2f;

	private const int COMPUTE_CELL_DIM = 8;

	private const int COMPUTE_TILE_DIM = 5;

	private const int COMPUTE_TILE_HALF = 2;

	private const int COMPUTE_CELL_COUNT = 64;

	private const int GRID_OFFSET = 65536;

	public static bool Paused;

	public static bool RefreshDisabled;

	public GameObjectRef BatchPrefab;

	public float CellSize = 50f;

	private const int InstanceCellSize = 16;

	[SerializeField]
	private ComputeShader foliageCompute;

	public LayerSelect FoliageLayer = 0;

	public ShadowCastingMode FoliageShadows;

	public void OnGlobalTextureMipmapLimitChange()
	{
	}
}
