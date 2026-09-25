using System.Collections.Generic;
using ConVar;
using Facepunch;
using Rust.Ai.Gen2.Nav;
using UnityEngine;

public class NPCSpawner : SpawnGroup
{
	public int AdditionalLOSBlockingLayer;

	public MonumentNavMesh monumentNavMesh;

	public bool shouldFillOnSpawn;

	[Header("InfoZone Config")]
	public AIInformationZone VirtualInfoZone;

	[Header("Navigator Config")]
	public AIMovePointPath Path;

	public BasePath AStarGraph;

	[Header("Human Stat Replacements")]
	public bool UseStatModifiers;

	public float SenseRange = 30f;

	public bool CheckLOS = true;

	public float TargetLostRange = 50f;

	public float AttackRangeMultiplier = 1f;

	public float ListenRange = 10f;

	public float CanUseHealingItemsChance;

	[Header("Loadout Replacements")]
	public PlayerInventoryProperties[] Loadouts;

	[Header("Parenting")]
	public BaseEntity attachToParent;

	public override void SpawnInitial()
	{
		if (!AI.npc_spawn_on_cargo_ship && GameObjectEx.ToBaseEntity(base.transform.root) is CargoShip)
		{
			base.enabled = false;
			return;
		}
		if (!AI.npc_spawn_on_junkpile && GameObjectEx.ToBaseEntity(base.transform.root) is JunkPile)
		{
			base.enabled = false;
			return;
		}
		if (DeepSeaManager.IsInsideDeepSea(base.transform.position))
		{
			List<DeepSeaIsland> obj = Facepunch.Pool.Get<List<DeepSeaIsland>>();
			Vis.Entities(base.transform.position, 10f, obj, 8454145);
			DeepSeaIsland deepSeaIsland = ((obj.Count > 0) ? obj[0] : null);
			Facepunch.Pool.FreeUnmanaged(ref obj);
			if (deepSeaIsland != null)
			{
				monumentNavMesh = deepSeaIsland.monumentNavMesh;
				if (!AI.npc_spawn_on_deep_sea_islands)
				{
					base.enabled = false;
					return;
				}
			}
		}
		fillOnSpawn = shouldFillOnSpawn;
		if (WaitingForNavMesh())
		{
			Invoke(LateSpawn, 10f);
		}
		else
		{
			base.SpawnInitial();
		}
	}

	public bool WaitingForNavMesh()
	{
		if (monumentNavMesh != null)
		{
			return monumentNavMesh.IsBuilding;
		}
		if (!AI.useUnityNavmesh)
		{
			IndependantNavmesh independantNavmesh = IndependantNavmesh.FindNavmeshAtPosition(base.transform.position);
			if (independantNavmesh != null)
			{
				return !independantNavmesh.IsBuilt();
			}
			if (!RustNavigation.Instance.IsDefaultNavmeshBuilt())
			{
				return true;
			}
		}
		if (!DungeonNavmesh.NavReady())
		{
			return true;
		}
		return !AI.move;
	}

	public void LateSpawn()
	{
		if (!WaitingForNavMesh())
		{
			SpawnInitial();
			if (AI.logIssues)
			{
				string recursiveName = TransformEx.GetRecursiveName(base.transform);
				Debug.Log("SpawnGroup spawning: \"" + recursiveName + "\"");
			}
		}
		else
		{
			Invoke(LateSpawn, 5f);
		}
	}

	protected override void PostSpawnProcess(BaseEntity entity, BaseSpawnPoint spawnPoint)
	{
		base.PostSpawnProcess(entity, spawnPoint);
		BaseNavigator component = entity.GetComponent<BaseNavigator>();
		if (AdditionalLOSBlockingLayer != 0 && entity != null && entity is HumanNPC humanNPC)
		{
			humanNPC.AdditionalLosBlockingLayer = AdditionalLOSBlockingLayer;
		}
		HumanNPC humanNPC2 = entity as HumanNPC;
		if (humanNPC2 != null)
		{
			if (Loadouts != null && Loadouts.Length != 0)
			{
				humanNPC2.EquipLoadout(Loadouts);
			}
			ModifyHumanBrainStats(humanNPC2.Brain);
		}
		if (VirtualInfoZone != null)
		{
			if (VirtualInfoZone.Virtual)
			{
				NPCPlayer nPCPlayer = entity as NPCPlayer;
				if (nPCPlayer != null)
				{
					nPCPlayer.VirtualInfoZone = VirtualInfoZone;
					if (humanNPC2 != null)
					{
						humanNPC2.VirtualInfoZone.RegisterSleepableEntity(humanNPC2.Brain);
					}
				}
			}
			else
			{
				Debug.LogError("NPCSpawner trying to set a virtual info zone without the Virtual property!");
			}
		}
		if (component != null)
		{
			component.Path = Path;
			component.AStarGraph = AStarGraph;
		}
		if ((bool)attachToParent)
		{
			entity.SetParent(attachToParent, worldPositionStays: true);
		}
	}

	private void ModifyHumanBrainStats(BaseAIBrain brain)
	{
		if (UseStatModifiers && !(brain == null))
		{
			brain.SenseRange = SenseRange;
			brain.TargetLostRange *= TargetLostRange;
			brain.AttackRangeMultiplier = AttackRangeMultiplier;
			brain.ListenRange = ListenRange;
			brain.CheckLOS = CheckLOS;
			if (CanUseHealingItemsChance > 0f)
			{
				brain.CanUseHealingItems = Random.Range(0f, 1f) <= CanUseHealingItemsChance;
			}
		}
	}
}
