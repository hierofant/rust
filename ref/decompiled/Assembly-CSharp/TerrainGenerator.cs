using Oxide.Core;
using UnityEngine;

public class TerrainGenerator : SingletonComponent<TerrainGenerator>
{
	public TerrainConfig config;

	private const float HeightMapRes = 1f;

	private const float SplatMapRes = 0.5f;

	private const float BaseMapRes = 0.01f;

	public static int GetHeightMapRes()
	{
		return Mathf.Min(4096, Mathf.ClosestPowerOfTwo((int)((float)World.Size * 1f))) + 1;
	}

	public static int GetSplatMapRes()
	{
		return Mathf.Min(2048, Mathf.NextPowerOfTwo((int)((float)World.Size * 0.5f)));
	}

	public static int GetBaseMapRes()
	{
		return Mathf.Min(2048, Mathf.NextPowerOfTwo((int)((float)World.Size * 0.01f)));
	}

	public GameObject CreateTerrain()
	{
		return CreateTerrain(GetHeightMapRes(), GetSplatMapRes());
	}

	public GameObject CreateTerrain(int heightmapResolution, int alphamapResolution)
	{
		Interface.CallHook("OnTerrainCreate", this);
		TerrainData terrainData = new TerrainData();
		terrainData.baseMapResolution = GetBaseMapRes();
		terrainData.heightmapResolution = heightmapResolution;
		terrainData.alphamapResolution = alphamapResolution;
		terrainData.size = new Vector3(World.Size, 1000f, World.Size);
		Terrain terrain = null;
		GeometryClipmapTerrain geometryClipmapTerrain = null;
		terrain = Terrain.CreateTerrainGameObject(terrainData).GetComponent<Terrain>();
		terrain.transform.position = base.transform.position + new Vector3((float)(0L - (long)World.Size) * 0.5f, 0f, (float)(0L - (long)World.Size) * 0.5f);
		terrain.drawInstanced = false;
		terrain.castShadows = config.CastShadows;
		terrain.materialType = Terrain.MaterialType.Custom;
		terrain.materialTemplate = config.Material;
		terrain.gameObject.tag = base.gameObject.tag;
		terrain.gameObject.layer = base.gameObject.layer;
		terrain.gameObject.GetComponent<TerrainCollider>().sharedMaterial = config.GenericMaterial;
		GameObject obj = terrain.gameObject;
		TerrainMeta terrainMeta = obj.AddComponent<TerrainMeta>();
		obj.AddComponent<TerrainPhysics>();
		obj.AddComponent<TerrainColors>();
		obj.AddComponent<TerrainCollision>();
		obj.AddComponent<TerrainBiomeMap>();
		obj.AddComponent<TerrainAlphaMap>();
		obj.AddComponent<TerrainHeightMap>();
		obj.AddComponent<TerrainSplatMap>();
		obj.AddComponent<TerrainTopologyMap>();
		obj.AddComponent<TerrainWaterMap>();
		obj.AddComponent<TerrainPlacementMap>();
		obj.AddComponent<TerrainPath>();
		obj.AddComponent<TerrainTexturing>();
		obj.AddComponent<TerrainWaterFlowMap>();
		obj.AddComponent<TerrainHoleRenderer>();
		if ((bool)terrain)
		{
			terrainMeta.terrainRenderer.SetTerrain(terrain);
		}
		else if ((bool)geometryClipmapTerrain)
		{
			terrainMeta.terrainRenderer.SetTerrain(geometryClipmapTerrain);
		}
		terrainMeta.terrainData = terrainData;
		terrainMeta.config = config;
		Object.DestroyImmediate(base.gameObject);
		return obj;
	}
}
