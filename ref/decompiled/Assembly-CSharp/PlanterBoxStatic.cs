using System.Collections.Generic;
using UnityEngine;

public class PlanterBoxStatic : PlanterBox
{
	[ServerVar(Help = "Chance of a favourable gene being picked [0-1]. Setting this to 0 does not ensure no favourable genes are picked up, but it greatly reduces the chances.")]
	public static float FavourableGeneChance = 0.5f;

	public List<GameObjectRef> staticPlantsSpawnlist;

	public bool randomPerSlot;

	public float respawnCheckTimer = 30f;

	[ServerVar(Help = "(Generated) Interval in seconds between respawn checks for growable plants in static planter boxes inside the deep sea zone; default 600s")]
	public static float DeepSeaRespawnCheckTimer = 600f;

	private TimeSince lastDeepSeaSpawn;

	private static ListHashSet<PlanterBoxStatic> AllStaticPlanters = new ListHashSet<PlanterBoxStatic>();

	private bool DeepSeaMode => DeepSeaManager.IsInsideDeepSea(this);

	public override void SetupTimeCaches()
	{
	}

	public override void RefreshGrowables(GrowableEntity ignoreEntity = null)
	{
	}

	public static void OnDeepSeaSpawned()
	{
		foreach (PlanterBoxStatic allStaticPlanter in AllStaticPlanters)
		{
			if (allStaticPlanter.DeepSeaMode)
			{
				allStaticPlanter.lastDeepSeaSpawn = float.MaxValue;
				allStaticPlanter.CreateStaticPlants();
			}
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		InvokeRepeating(CreateStaticPlants, 1f, DeepSeaMode ? 120f : respawnCheckTimer);
		AllStaticPlanters.Add(this);
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		AllStaticPlanters.Remove(this);
	}

	public void CreateStaticPlants()
	{
		soilSaturation = soilSaturationMax;
		GameObjectRef randomStaticPlant = GetRandomStaticPlant();
		Socket_Base[] array = PrefabAttribute.server.FindAll<Socket_Base>(prefabID);
		bool deepSeaMode = DeepSeaMode;
		if ((deepSeaMode && (float)lastDeepSeaSpawn < DeepSeaRespawnCheckTimer) || (deepSeaMode && DeepSeaManager.Get(base.isServer) != null && DeepSeaManager.Get(base.isServer).IsBusy()))
		{
			return;
		}
		foreach (Socket_Base socket_Base in array)
		{
			if (!(socket_Base is Socket_Specific_Female) || !IsSpawnPointFreeSearch(socket_Base.localPosition))
			{
				continue;
			}
			if (randomPerSlot)
			{
				randomStaticPlant = GetRandomStaticPlant();
			}
			Vector3 pos = base.transform.TransformPoint(socket_Base.localPosition);
			BaseEntity baseEntity = GameManager.server.CreateEntity(randomStaticPlant.resourcePath, pos, Quaternion.identity);
			baseEntity.SetParent(this, worldPositionStays: true);
			baseEntity.Spawn();
			GrowableEntity growableEntity = baseEntity as GrowableEntity;
			if (growableEntity != null)
			{
				growableEntity.Fertilize();
				growableEntity.SetGodQuality(qual: true);
				growableEntity.SetMaxGrowingConditions();
				growableEntity.Genes.GenerateFavourableGenes(growableEntity);
				if (deepSeaMode)
				{
					growableEntity.ChangeState(PlantProperties.State.Ripe, resetAge: false);
				}
				growableEntity.SendNetworkUpdate();
				OnPlantInserted(growableEntity, null);
				lastDeepSeaSpawn = 0f;
			}
		}
	}

	private GameObjectRef GetRandomStaticPlant()
	{
		if (staticPlantsSpawnlist == null || staticPlantsSpawnlist.Count == 0)
		{
			return null;
		}
		int index = Random.Range(0, staticPlantsSpawnlist.Count);
		return staticPlantsSpawnlist[index];
	}

	private bool IsSpawnPointFreeSearch(Vector3 localPos)
	{
		foreach (BaseEntity child in children)
		{
			if (child is GrowableEntity && Vector3.Distance(child.transform.localPosition, localPos) < 0.05f)
			{
				return false;
			}
		}
		return true;
	}
}
