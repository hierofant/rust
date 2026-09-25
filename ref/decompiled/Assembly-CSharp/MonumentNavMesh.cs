using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using ConVar;
using Rust;
using Rust.Ai;
using Rust.Ai.Gen2.Nav;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class MonumentNavMesh : FacepunchBehaviour, IServerComponent
{
	public int NavMeshAgentTypeIndex;

	[Tooltip("The default area associated with the NavMeshAgent index.")]
	public string DefaultAreaName = "HumanNPC";

	[Tooltip("How many cells to use squared")]
	public int CellCount = 1;

	[Tooltip("The size of each cell for async object gathering")]
	public int CellSize = 80;

	public int Height = 100;

	public float NavmeshResolutionModifier = 0.5f;

	[Tooltip("Use the bounds specified in editor instead of generating it from cellsize * cellcount")]
	public bool overrideAutoBounds;

	[Tooltip("Bounds which are auto calculated from CellSize * CellCount")]
	public Bounds Bounds;

	public NavMeshData NavMeshData;

	public NavMeshDataInstance NavMeshDataInstance;

	public LayerMask LayerMask;

	public NavMeshCollectGeometry NavMeshCollectGeometry;

	public bool forceCollectTerrain;

	public bool shouldNotifyAIZones = true;

	public Transform CustomNavMeshRoot;

	public bool IgnoreTerrain;

	public bool offsetBoundsByCenterPoint;

	[ServerVar]
	public static bool use_baked_terrain_mesh = true;

	private List<NavMeshBuildSource> sources;

	private AsyncOperation BuildingOperation;

	private bool HasBuildOperationStarted;

	private Stopwatch BuildTimer = new Stopwatch();

	private int defaultArea;

	private int agentTypeId;

	private bool isOffMainLand;

	private IndependantNavmesh independantNavmesh;

	public bool IsBuilding
	{
		get
		{
			if (AI.useUnityNavmesh)
			{
				if (HasBuildOperationStarted)
				{
					return BuildingOperation != null;
				}
				return true;
			}
			if (!isOffMainLand)
			{
				return !RustNavigation.Instance.IsDefaultNavmeshBuilt();
			}
			if (independantNavmesh != null)
			{
				return !independantNavmesh.IsBuilt();
			}
			return true;
		}
	}

	public Bounds GetBounds()
	{
		if (!overrideAutoBounds)
		{
			Bounds.size = new Vector3(CellSize * CellCount, Height, CellSize * CellCount);
		}
		Bounds result = new Bounds(Bounds.center, Bounds.size);
		if (offsetBoundsByCenterPoint)
		{
			result.center = base.transform.TransformPoint(Bounds.center);
		}
		else
		{
			result.center = base.transform.position;
		}
		return result;
	}

	private void OnEnable()
	{
		if (AI.useUnityNavmesh)
		{
			agentTypeId = NavMesh.GetSettingsByIndex(NavMeshAgentTypeIndex).agentTypeID;
			NavMeshData = new NavMeshData(agentTypeId);
			sources = new List<NavMeshBuildSource>();
			defaultArea = NavMesh.GetAreaFromName(DefaultAreaName);
			InvokeRepeating(FinishBuildingNavmesh, 0f, 1f);
		}
		else
		{
			isOffMainLand = TerrainMeta.OutOfBounds(base.transform.position);
		}
	}

	private void OnDisable()
	{
		if (AI.useUnityNavmesh && !Rust.Application.isQuitting)
		{
			CancelInvoke(FinishBuildingNavmesh);
			NavMeshDataInstance.Remove();
		}
	}

	[ContextMenu("Update Monument Nav Mesh")]
	public void UpdateNavMeshAsync()
	{
		RustNavigation.EnsureUnityNavmesh();
		if (!HasBuildOperationStarted && !AiManager.nav_disable && AI.npc_enable)
		{
			float realtimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
			NavMeshTools.Log("Starting Monument Navmesh Build with " + sources.Count + " sources");
			NavMeshBuildSettings settingsByIndex = NavMesh.GetSettingsByIndex(NavMeshAgentTypeIndex);
			settingsByIndex.overrideVoxelSize = true;
			settingsByIndex.voxelSize *= NavmeshResolutionModifier;
			BuildingOperation = NavMeshBuilder.UpdateNavMeshDataAsync(NavMeshData, settingsByIndex, sources, GetBounds());
			BuildTimer.Reset();
			BuildTimer.Start();
			HasBuildOperationStarted = true;
			float num = UnityEngine.Time.realtimeSinceStartup - realtimeSinceStartup;
			if (num > 0.1f)
			{
				NavMeshTools.LogWarning("Calling UpdateNavMesh took " + num);
			}
			if (shouldNotifyAIZones)
			{
				NotifyInformationZonesOfCompletion();
			}
		}
	}

	public IEnumerator UpdateNavMeshAndWait()
	{
		if (AiManager.nav_disable || !AI.npc_enable)
		{
			yield break;
		}
		if (AI.useUnityNavmesh)
		{
			if (HasBuildOperationStarted)
			{
				yield break;
			}
			HasBuildOperationStarted = false;
			IEnumerator enumerator = NavMeshTools.CollectSourcesAsync(GetBounds(), LayerMask, NavMeshCollectGeometry, defaultArea, use_baked_terrain_mesh && !forceCollectTerrain && !IgnoreTerrain, CellSize, sources, AppendModifierVolumes, UpdateNavMeshAsync, CustomNavMeshRoot);
			if (AiManager.nav_wait)
			{
				yield return enumerator;
			}
			else
			{
				StartCoroutine(enumerator);
			}
			if (!AiManager.nav_wait)
			{
				NavMeshTools.Log("nav_wait is false, so we're not waiting for the navmesh to finish generating. This might cause your server to sputter while it's generating.");
				yield break;
			}
			int lastPct = 0;
			while (!HasBuildOperationStarted)
			{
				yield return CoroutineEx.waitForSecondsRealtime(0.25f);
			}
			while (BuildingOperation != null)
			{
				int num = (int)(BuildingOperation.progress * 100f);
				if (lastPct != num)
				{
					NavMeshTools.Log($"{num}%");
					lastPct = num;
				}
				yield return CoroutineEx.waitForSecondsRealtime(0.25f);
				FinishBuildingNavmesh();
			}
		}
		else if (isOffMainLand)
		{
			if (!TryGetComponent<IndependantNavmesh>(out independantNavmesh))
			{
				independantNavmesh = base.gameObject.AddComponent<IndependantNavmesh>();
			}
			independantNavmesh.size = GetBounds().size;
			RustNavigation.Instance.AddNavmesh(independantNavmesh);
		}
	}

	public void NotifyInformationZonesOfCompletion()
	{
		RustNavigation.EnsureUnityNavmesh();
		foreach (AIInformationZone zone in AIInformationZone.zones)
		{
			zone.NavmeshBuildingComplete();
		}
	}

	private void AppendModifierVolumes(List<NavMeshBuildSource> sources)
	{
		RustNavigation.EnsureUnityNavmesh();
		foreach (NavMeshModifierVolume activeModifier in NavMeshModifierVolume.activeModifiers)
		{
			if (((int)LayerMask & (1 << activeModifier.gameObject.layer)) != 0 && activeModifier.AffectsAgentType(agentTypeId))
			{
				Vector3 vector = activeModifier.transform.TransformPoint(activeModifier.center);
				if (GetBounds().Contains(vector))
				{
					Vector3 lossyScale = activeModifier.transform.lossyScale;
					Vector3 size = new Vector3(activeModifier.size.x * Mathf.Abs(lossyScale.x), activeModifier.size.y * Mathf.Abs(lossyScale.y), activeModifier.size.z * Mathf.Abs(lossyScale.z));
					NavMeshBuildSource item = default(NavMeshBuildSource);
					item.shape = NavMeshBuildSourceShape.ModifierBox;
					item.transform = Matrix4x4.TRS(vector, activeModifier.transform.rotation, Vector3.one);
					item.size = size;
					item.area = activeModifier.area;
					sources.Add(item);
				}
			}
		}
	}

	public void FinishBuildingNavmesh()
	{
		RustNavigation.EnsureUnityNavmesh();
		if (BuildingOperation != null && BuildingOperation.isDone)
		{
			if (!NavMeshDataInstance.valid)
			{
				NavMeshDataInstance = NavMesh.AddNavMeshData(NavMeshData);
			}
			NavMeshTools.Log($"Monument Navmesh Build took {BuildTimer.Elapsed.TotalSeconds:0.00} seconds");
			BuildingOperation = null;
		}
	}
}
