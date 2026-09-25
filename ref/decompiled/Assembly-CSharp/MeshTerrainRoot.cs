using UnityEngine;

[ExecuteInEditMode]
public class MeshTerrainRoot : ListComponent<MeshTerrainRoot>, IClientComponent
{
	public GameObject TerrainBlendSearchRoot;

	public Mesh TerrainMeshAsset;

	public Material TerrainMaterial;

	public Vector3 TerrainSize;

	public TerrainConfig TerrainConfig;

	public Bounds LocalBounds;
}
