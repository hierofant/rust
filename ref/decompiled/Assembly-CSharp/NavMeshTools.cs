using System;
using System.Collections;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Rust.Ai;
using Rust.Ai.Gen2;
using UnityEngine;
using UnityEngine.AI;

public static class NavMeshTools
{
	public static void Log(string message)
	{
		Debug.Log("[UnityNavmesh] " + message);
	}

	public static void LogWarning(string message)
	{
		Debug.LogWarning("[UnityNavmesh] " + message);
	}

	public static void EncapsulateNavmeshColliders(GameObject root, ref Bounds footprint, ref bool hasFootprint)
	{
		if (root == null)
		{
			return;
		}
		Collider[] componentsInChildren = root.GetComponentsInChildren<Collider>(includeInactive: false);
		foreach (Collider collider in componentsInChildren)
		{
			if (!collider.isTrigger && collider.enabled && (0x20000000 & (1 << collider.gameObject.layer)) == 0)
			{
				if (hasFootprint)
				{
					footprint.Encapsulate(collider.bounds);
					continue;
				}
				footprint = collider.bounds;
				hasFootprint = true;
			}
		}
	}

	public static IEnumerator CollectSourcesAsync(Bounds bounds, int mask, NavMeshCollectGeometry geometry, int area, bool useBakedTerrainMesh, int cellSize, List<NavMeshBuildSource> sources, Action<List<NavMeshBuildSource>> append, Action callback, Transform customNavMeshDataRoot, HashSet<Transform> ignoreRoots = null)
	{
		while (!AI.move && !AiManager.nav_wait)
		{
			yield return CoroutineEx.waitForSeconds(1f);
		}
		if (customNavMeshDataRoot != null)
		{
			customNavMeshDataRoot.gameObject.SetActive(value: true);
			yield return new WaitForEndOfFrame();
		}
		float time = UnityEngine.Time.realtimeSinceStartup;
		Log("Starting Navmesh Source Collecting");
		mask = ((!useBakedTerrainMesh) ? (mask | 0x800000) : (mask & -8388609));
		List<NavMeshBuildMarkup> list = new List<NavMeshBuildMarkup>();
		if (ignoreRoots != null)
		{
			foreach (Transform ignoreRoot in ignoreRoots)
			{
				NavMeshBuildMarkup navMeshBuildMarkup = default(NavMeshBuildMarkup);
				navMeshBuildMarkup.root = ignoreRoot;
				navMeshBuildMarkup.ignoreFromBuild = true;
				navMeshBuildMarkup.overrideIgnore = true;
				NavMeshBuildMarkup item = navMeshBuildMarkup;
				list.Add(item);
			}
		}
		int areaFromName = NavMesh.GetAreaFromName("Not Walkable");
		using PooledList<RustNavmeshModifierVolume> modifierVolumes = Facepunch.Pool.Get<PooledList<RustNavmeshModifierVolume>>();
		RustNavmeshModifierVolume.AllModifierVolumes.GetInBounds(bounds, modifierVolumes);
		foreach (RustNavmeshModifierVolume item3 in modifierVolumes)
		{
			NavMeshBuildMarkup navMeshBuildMarkup = default(NavMeshBuildMarkup);
			navMeshBuildMarkup.root = item3.transform;
			navMeshBuildMarkup.overrideArea = true;
			navMeshBuildMarkup.area = areaFromName;
			NavMeshBuildMarkup item2 = navMeshBuildMarkup;
			list.Add(item2);
		}
		NavMeshBuilder.CollectSources(bounds, mask, geometry, area, list, sources);
		if (useBakedTerrainMesh && TerrainMeta.HeightMap != null)
		{
			for (float x = 0f - bounds.extents.x; x < bounds.extents.x - (float)(cellSize / 2); x += (float)cellSize)
			{
				for (float z = 0f - bounds.extents.z; z < bounds.extents.z - (float)(cellSize / 2); z += (float)cellSize)
				{
					AsyncTerrainNavMeshBake terrainSource = new AsyncTerrainNavMeshBake(new Vector3(x, 0f, z), cellSize, cellSize, normal: false, alpha: true);
					yield return terrainSource;
					sources.Add(terrainSource.CreateNavMeshBuildSource(area));
				}
			}
		}
		append?.Invoke(sources);
		Log($"Navmesh Source Collecting took {UnityEngine.Time.realtimeSinceStartup - time:0.00} seconds");
		if (customNavMeshDataRoot != null)
		{
			customNavMeshDataRoot.gameObject.SetActive(value: false);
		}
		callback?.Invoke();
	}

	public static IEnumerator CollectSourcesAsync(IEnumerable<GameObject> roots, int mask, NavMeshCollectGeometry geometry, int area, List<NavMeshBuildSource> sources, Action<List<NavMeshBuildSource>> append, Action callback)
	{
		while (!AI.move && !AiManager.nav_wait)
		{
			yield return CoroutineEx.waitForSeconds(1f);
		}
		float realtimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
		Log("Starting Navmesh Source Collecting");
		List<NavMeshBuildMarkup> markups = new List<NavMeshBuildMarkup>();
		List<NavMeshBuildSource> list = new List<NavMeshBuildSource>();
		foreach (GameObject root in roots)
		{
			if (root == null)
			{
				continue;
			}
			NavMeshBuilder.CollectSources(root.transform, mask, geometry, area, markups, list);
			foreach (NavMeshBuildSource item in list)
			{
				sources.Add(item);
			}
		}
		append?.Invoke(sources);
		Log($"Navmesh Source Collecting took {UnityEngine.Time.realtimeSinceStartup - realtimeSinceStartup:0.00} seconds");
		callback?.Invoke();
	}
}
