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

public class DungeonNavmesh : FacepunchBehaviour, IServerComponent
{
	public int NavMeshAgentTypeIndex;

	[Tooltip("The default area associated with the NavMeshAgent index.")]
	public string DefaultAreaName = "HumanNPC";

	public float NavmeshResolutionModifier = 1.25f;

	[Tooltip("Bounds which are auto calculated from CellSize * CellCount")]
	public Bounds Bounds;

	public NavMeshData NavMeshData;

	public NavMeshDataInstance NavMeshDataInstance;

	public LayerMask LayerMask;

	public NavMeshCollectGeometry NavMeshCollectGeometry;

	public static List<DungeonNavmesh> Instances = new List<DungeonNavmesh>();

	[ServerVar]
	public static bool use_baked_terrain_mesh = true;

	private List<NavMeshBuildSource> sources;

	private AsyncOperation BuildingOperation;

	private bool HasBuildOperationStarted;

	private Stopwatch BuildTimer = new Stopwatch();

	private int defaultArea;

	private int agentTypeId;

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
			return false;
		}
	}

	public static bool NavReady()
	{
		if (Instances == null || Instances.Count == 0)
		{
			return true;
		}
		foreach (DungeonNavmesh instance in Instances)
		{
			if (instance.IsBuilding)
			{
				return false;
			}
		}
		return true;
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
		Instances.Add(this);
	}

	private void OnDisable()
	{
		if (!Rust.Application.isQuitting)
		{
			if (AI.useUnityNavmesh)
			{
				CancelInvoke(FinishBuildingNavmesh);
				NavMeshDataInstance.Remove();
			}
			Instances.Remove(this);
		}
	}

	[ContextMenu("Update Monument Nav Mesh")]
	public void UpdateNavMeshAsync()
	{
		RustNavigation.EnsureUnityNavmesh();
		if (!HasBuildOperationStarted && !AiManager.nav_disable && AI.npc_enable)
		{
			float realtimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
			NavMeshTools.Log("Starting Dungeon Navmesh Build with " + sources.Count + " sources");
			NavMeshBuildSettings settingsByIndex = NavMesh.GetSettingsByIndex(NavMeshAgentTypeIndex);
			settingsByIndex.overrideVoxelSize = true;
			settingsByIndex.voxelSize *= NavmeshResolutionModifier;
			BuildingOperation = NavMeshBuilder.UpdateNavMeshDataAsync(NavMeshData, settingsByIndex, sources, Bounds);
			BuildTimer.Reset();
			BuildTimer.Start();
			HasBuildOperationStarted = true;
			float num = UnityEngine.Time.realtimeSinceStartup - realtimeSinceStartup;
			if (num > 0.1f)
			{
				NavMeshTools.LogWarning("Calling UpdateNavMesh took " + num);
			}
			NotifyInformationZonesOfCompletion();
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

	public void SourcesCollected()
	{
		RustNavigation.EnsureUnityNavmesh();
		int count = sources.Count;
		NavMeshTools.Log("Source count Pre cull : " + sources.Count);
		for (int num = sources.Count - 1; num >= 0; num--)
		{
			NavMeshBuildSource item = sources[num];
			Matrix4x4 matrix4x = item.transform;
			Vector3 vector = new Vector3(matrix4x[0, 3], matrix4x[1, 3], matrix4x[2, 3]);
			bool flag = false;
			foreach (AIInformationZone zone in AIInformationZone.zones)
			{
				if (Vector3Ex.Distance2D(zone.ClosestPointTo(vector), vector) <= 50f)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				sources.Remove(item);
			}
		}
		NavMeshTools.Log("Source count post cull : " + sources.Count + " total removed : " + (count - sources.Count));
	}

	public IEnumerator UpdateNavMeshAndWait(IEnumerable<GameObject> roots)
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
			Bounds.center = base.transform.position;
			Bounds.size = new Vector3(1000000f, 100000f, 100000f);
			IEnumerator enumerator = NavMeshTools.CollectSourcesAsync(roots, LayerMask.value, NavMeshCollectGeometry, defaultArea, sources, AppendModifierVolumes, UpdateNavMeshAsync);
			if (AiManager.nav_wait)
			{
				yield return enumerator;
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
			else
			{
				StartCoroutine(enumerator);
				NavMeshTools.Log("nav_wait is false, so we're not waiting for the navmesh to finish generating. This might cause your server to sputter while it's generating.");
			}
		}
		else
		{
			RegisterTunnelRegions(roots);
		}
	}

	private static void RegisterTunnelRegions(IEnumerable<GameObject> roots)
	{
		if (RustNavigation.Instance == null)
		{
			return;
		}
		foreach (GameObject root in roots)
		{
			if (!(root == null))
			{
				bool hasFootprint = false;
				Bounds footprint = default(Bounds);
				NavMeshTools.EncapsulateNavmeshColliders(root, ref footprint, ref hasFootprint);
				if (hasFootprint)
				{
					RustNavigation.Instance.AddTunnelRegion(footprint);
				}
				else if (AI.logIssues)
				{
					NavMeshTools.LogWarning("Tunnel cell " + root.name + " has no bakeable colliders, its tiles stay subject to the open sea cull.");
				}
			}
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
				if (Bounds.Contains(vector))
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
