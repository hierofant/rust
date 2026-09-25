using System;
using Rust.RenderPipeline.Runtime;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public class TerrainRenderer
{
	public enum TerrainRendererBackend
	{
		Null,
		Unity,
		GeoClipmapping
	}

	public GameObject gameObject;

	public Terrain terrain;

	public GeometryClipmapTerrain geoClipTerrain;

	public TerrainRendererBackend rendererBackend;

	private bool isUnityTerrain => rendererBackend == TerrainRendererBackend.Unity;

	public bool HasTerrain
	{
		get
		{
			if (rendererBackend != TerrainRendererBackend.Unity || !terrain)
			{
				if (rendererBackend == TerrainRendererBackend.GeoClipmapping)
				{
					return geoClipTerrain;
				}
				return false;
			}
			return true;
		}
	}

	internal Material material
	{
		get
		{
			if (rendererBackend == TerrainRendererBackend.Unity && (bool)terrain)
			{
				return terrain.materialTemplate;
			}
			if (rendererBackend == TerrainRendererBackend.GeoClipmapping && (bool)geoClipTerrain)
			{
				return geoClipTerrain.terrainMaterial;
			}
			return null;
		}
	}

	public static implicit operator bool(TerrainRenderer a)
	{
		return a?.HasTerrain ?? false;
	}

	public void SetTerrain(GeometryClipmapTerrain t)
	{
		geoClipTerrain = t;
		gameObject = t.gameObject;
		rendererBackend = TerrainRendererBackend.GeoClipmapping;
		geoClipTerrain.enabled = true;
	}

	public void SetTerrain(Terrain t)
	{
		terrain = t;
		gameObject = t.gameObject;
		rendererBackend = TerrainRendererBackend.Unity;
		terrain.enabled = true;
	}

	public ReflectionProbeUsage GetReflectionProbeUsage()
	{
		return rendererBackend switch
		{
			TerrainRendererBackend.Unity => terrain.reflectionProbeUsage, 
			TerrainRendererBackend.GeoClipmapping => geoClipTerrain.reflectionProbeUsage, 
			_ => ReflectionProbeUsage.Off, 
		};
	}

	public void SetReflectionProbeUsage(ReflectionProbeUsage value)
	{
		switch (rendererBackend)
		{
		case TerrainRendererBackend.Unity:
			terrain.reflectionProbeUsage = value;
			break;
		case TerrainRendererBackend.GeoClipmapping:
			geoClipTerrain.reflectionProbeUsage = value;
			break;
		}
	}

	public void CheckForRenderer(GameObject go)
	{
		switch (rendererBackend)
		{
		case TerrainRendererBackend.Null:
			if ((bool)terrain || go.TryGetComponent<Terrain>(out terrain))
			{
				SetTerrain(terrain);
			}
			else if ((bool)geoClipTerrain || go.TryGetComponent<GeometryClipmapTerrain>(out geoClipTerrain))
			{
				SetTerrain(geoClipTerrain);
			}
			break;
		case TerrainRendererBackend.Unity:
			if ((bool)geoClipTerrain)
			{
				geoClipTerrain.enabled = false;
			}
			if (!gameObject)
			{
				gameObject = go;
			}
			if ((bool)terrain || go.TryGetComponent<Terrain>(out terrain))
			{
				SetTerrain(terrain);
				break;
			}
			rendererBackend = TerrainRendererBackend.Null;
			gameObject = null;
			break;
		case TerrainRendererBackend.GeoClipmapping:
			if ((bool)terrain)
			{
				terrain.enabled = false;
			}
			if (!gameObject)
			{
				gameObject = go;
			}
			if ((bool)geoClipTerrain || go.TryGetComponent<GeometryClipmapTerrain>(out geoClipTerrain))
			{
				SetTerrain(geoClipTerrain);
				break;
			}
			rendererBackend = TerrainRendererBackend.Null;
			gameObject = null;
			break;
		}
	}

	public void ValidateMaterial(TerrainConfig config)
	{
		switch (rendererBackend)
		{
		case TerrainRendererBackend.Unity:
			if (terrain.materialType == Terrain.MaterialType.BuiltInStandard && terrain.materialTemplate != config.Material)
			{
				terrain.materialType = Terrain.MaterialType.Custom;
				terrain.materialTemplate = config.Material;
			}
			break;
		case TerrainRendererBackend.GeoClipmapping:
		{
			bool flag = RustRenderPipeline.IsActive() && config.GeoClipmapMaterialHoles != null;
			geoClipTerrain.terrainMaterial = (flag ? config.GeoClipmapMaterialHoles : config.GeoClipmapMaterial);
			break;
		}
		}
	}

	public void SetGeoClipMaterial(TerrainConfig config, bool useStencilHoles)
	{
	}

	public Vector3 GetPosition()
	{
		return gameObject.transform.position;
	}

	public void SwitchTerrain(TerrainRendererBackend rendererOption, TerrainData terrainData, TerrainConfig config, GameObject go)
	{
		if (rendererOption == rendererBackend)
		{
			return;
		}
		switch (rendererOption)
		{
		case TerrainRendererBackend.Unity:
			gameObject = go;
			if ((bool)terrain || go.TryGetComponent<Terrain>(out terrain))
			{
				terrain.enabled = true;
			}
			else
			{
				terrain = gameObject.AddComponent<Terrain>();
				terrain.drawInstanced = false;
				terrain.terrainData = terrainData;
				terrain.castShadows = config.CastShadows;
				terrain.materialType = Terrain.MaterialType.Custom;
				terrain.materialTemplate = config.Material;
			}
			if ((bool)geoClipTerrain)
			{
				geoClipTerrain.enabled = false;
			}
			rendererBackend = rendererOption;
			break;
		case TerrainRendererBackend.GeoClipmapping:
			gameObject = go;
			if ((bool)geoClipTerrain || go.TryGetComponent<GeometryClipmapTerrain>(out geoClipTerrain))
			{
				geoClipTerrain.enabled = true;
			}
			else
			{
				geoClipTerrain = gameObject.AddComponent<GeometryClipmapTerrain>();
				geoClipTerrain.terrainMaterial = config.GeoClipmapMaterial;
				geoClipTerrain.terrainShadows = (config.CastShadows ? ShadowCastingMode.On : ShadowCastingMode.Off);
				geoClipTerrain.terrainData = terrainData;
				geoClipTerrain.terrainLayer = gameObject.layer;
				geoClipTerrain.terrainCompute = config.GeoClipCompute;
			}
			if ((bool)terrain)
			{
				terrain.enabled = false;
			}
			rendererBackend = rendererOption;
			break;
		}
	}
}
