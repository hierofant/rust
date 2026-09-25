using UnityEngine;

public class TerrainMeshProcessor : MonoBehaviour, IEditorComponent, ISceneToPrefabProcess
{
	public Terrain terrain;

	public MeshTerrainRoot outputRoot;

	public Material terrainMaterial;

	public TerrainConfig terrainConfig;

	public TerrainMeshBakeSettings settingsOverride;

	public Texture2D splatControl0;

	public Texture2D splatControl1;

	public Texture2D biome;

	public Texture2D normal;

	public Texture2D alpha;

	public Texture2D height;

	public Texture2D topology;

	public void OnSceneToPrefab(SceneToPrefab sceneToPrefab)
	{
	}
}
