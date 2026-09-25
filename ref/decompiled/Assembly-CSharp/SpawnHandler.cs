using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConVar;
using UnityEngine;

public class SpawnHandler : SingletonComponent<SpawnHandler>, SpawnPopulationBase.ISpawnHandler
{
	private class DebugSpawner : SpawnPopulationBase.ISpawnHandler
	{
		private SpawnHandler handler;

		private List<(Vector3, SpawnPopulationBase.Status)> samples;

		public DebugSpawner(SpawnHandler handler, List<(Vector3, SpawnPopulationBase.Status)> samples)
		{
			this.handler = handler;
			this.samples = samples;
		}

		SpawnPopulationBase.Status SpawnPopulationBase.ISpawnHandler.TrySpawn(SpawnPopulationBase pop, Prefab<Spawnable> prefab, Vector3 pos, Quaternion rot, out GameObject spawned)
		{
			spawned = null;
			if (!handler.Validate(pop, prefab, pos, rot))
			{
				return SpawnPopulationBase.Status.PrefabRejected;
			}
			if (prefab.Component.GetComponent<BaseEntity>() == null || prefab.Component.CompareTag("CannotBeCreated"))
			{
				return SpawnPopulationBase.Status.InvalidEntity;
			}
			return SpawnPopulationBase.Status.Success;
		}

		void SpawnPopulationBase.ISpawnHandler.ReportAttempt(SpawnPopulationBase.Status status, Vector3 pos)
		{
			samples.Add((pos, status));
		}
	}

	public float TickInterval = 60f;

	public int MinSpawnsPerTick = 100;

	public int MaxSpawnsPerTick = 100;

	public LayerMask PlacementMask;

	public LayerMask PlacementCheckMask;

	public float PlacementCheckHeight = 25f;

	public LayerMask RadiusCheckMask;

	public float RadiusCheckDistance = 5f;

	public LayerMask BoundsCheckMask;

	public SpawnFilter CharacterSpawn;

	public float CharacterSpawnCutoff;

	public SpawnPopulationBase[] SpawnPopulations;

	public SpawnDistribution[] SpawnDistributions;

	public SpawnDistribution CharDistribution;

	public ListHashSet<ISpawnGroup> SpawnGroups = new ListHashSet<ISpawnGroup>();

	internal List<SpawnIndividual> SpawnIndividuals = new List<SpawnIndividual>();

	[Header("Scientist Outfits")]
	public PlayerInventoryProperties[] JungleLoadouts;

	[ReadOnly]
	public SpawnPopulationBase[] ConvarSpawnPopulations;

	public Dictionary<SpawnPopulationBase, SpawnDistribution> population2distribution;

	private bool spawnTick;

	public SpawnPopulationBase[] AllSpawnPopulations;

	private static int PlayerCount
	{
		get
		{
			if (ConVar.Spawn.loot_population_test <= 0)
			{
				return BasePlayer.activePlayerList.Count;
			}
			return ConVar.Spawn.loot_population_test;
		}
	}

	protected void OnEnable()
	{
		AllSpawnPopulations = SpawnPopulations.Concat(ConvarSpawnPopulations).ToArray();
		StartCoroutine(SpawnTick());
		StartCoroutine(SpawnGroupTick());
		StartCoroutine(SpawnIndividualTick());
	}

	public static BasePlayer.SpawnPoint GetSpawnPoint()
	{
		if (SingletonComponent<SpawnHandler>.Instance == null || SingletonComponent<SpawnHandler>.Instance.CharDistribution == null)
		{
			return null;
		}
		BasePlayer.SpawnPoint spawnPoint = new BasePlayer.SpawnPoint();
		if (!((WaterSystem.OceanLevel < 0.5f) ? GetSpawnPointStandard(spawnPoint) : FloodedSpawnHandler.GetSpawnPoint(spawnPoint, WaterSystem.OceanLevel + 1f)))
		{
			return null;
		}
		return spawnPoint;
	}

	public static BasePlayer.SpawnPoint GetSpawnPointForTeam(ulong teamId)
	{
		if (teamId == 0L)
		{
			return null;
		}
		RelationshipManager.PlayerTeam playerTeam = RelationshipManager.ServerInstance.FindTeam(teamId);
		if (playerTeam == null)
		{
			return null;
		}
		if (!playerTeam.usePartySpawn)
		{
			return null;
		}
		if (playerTeam.firstSpawnLocation == default(Vector3))
		{
			return null;
		}
		float num = 100000f;
		BasePlayer.SpawnPoint spawnPoint = null;
		for (int i = 0; i < party.maxpartyspawnattempts; i++)
		{
			BasePlayer.SpawnPoint spawnPoint2 = GetSpawnPoint();
			float num2 = Vector3Ex.Distance2D(spawnPoint2.pos, playerTeam.firstSpawnLocation);
			if (num2 < num || spawnPoint == null)
			{
				spawnPoint = spawnPoint2;
				num = num2;
			}
			if (num2 < (float)party.maxpartyspawndistance)
			{
				return spawnPoint2;
			}
		}
		return spawnPoint;
	}

	private static bool GetSpawnPointStandard(BasePlayer.SpawnPoint spawnPoint)
	{
		for (int i = 0; i < 60; i++)
		{
			if (!SingletonComponent<SpawnHandler>.Instance.CharDistribution.Sample(out spawnPoint.pos, out spawnPoint.rot, alignToNormal: false, 0f, 0.5f, SingletonComponent<SpawnHandler>.Instance.CharacterSpawn, SingletonComponent<SpawnHandler>.Instance.CharacterSpawnCutoff))
			{
				continue;
			}
			bool flag = true;
			if (TerrainMeta.Path != null)
			{
				foreach (MonumentInfo monument in TerrainMeta.Path.Monuments)
				{
					if (monument.Distance(spawnPoint.pos) < 50f)
					{
						flag = false;
						break;
					}
				}
			}
			if (flag)
			{
				return true;
			}
		}
		return false;
	}

	public void UpdateDistributions()
	{
		if (World.Size == 0)
		{
			return;
		}
		SpawnDistributions = new SpawnDistribution[AllSpawnPopulations.Length];
		population2distribution = new Dictionary<SpawnPopulationBase, SpawnDistribution>();
		Vector3 size = TerrainMeta.Size;
		Vector3 position = TerrainMeta.Position;
		int populationRes = Mathf.NextPowerOfTwo((int)((float)World.Size * 0.25f));
		for (int i = 0; i < AllSpawnPopulations.Length; i++)
		{
			SpawnPopulationBase spawnPopulationBase = AllSpawnPopulations[i];
			if (spawnPopulationBase == null)
			{
				Debug.LogError("Spawn handler contains null spawn population.");
				continue;
			}
			byte[] baseMapValues = spawnPopulationBase.GetBaseMapValues(populationRes);
			SpawnDistribution value = (SpawnDistributions[i] = new SpawnDistribution(this, baseMapValues, position, size));
			population2distribution.Add(spawnPopulationBase, value);
		}
		int char_res = Mathf.NextPowerOfTwo((int)((float)World.Size * 0.5f));
		byte[] map = new byte[char_res * char_res];
		SpawnFilter filter = CharacterSpawn;
		float cutoff = CharacterSpawnCutoff;
		Parallel.For(0, char_res, delegate(int z)
		{
			for (int j = 0; j < char_res; j++)
			{
				float normX = ((float)j + 0.5f) / (float)char_res;
				float normZ = ((float)z + 0.5f) / (float)char_res;
				float factor = filter.GetFactor(normX, normZ);
				map[z * char_res + j] = (byte)((factor > cutoff) ? (255f * factor) : 0f);
			}
		});
		CharDistribution = new SpawnDistribution(this, map, position, size);
	}

	public void FillPopulations()
	{
		if (SpawnDistributions == null)
		{
			return;
		}
		for (int i = 0; i < AllSpawnPopulations.Length; i++)
		{
			if (!(AllSpawnPopulations[i] == null))
			{
				SpawnInitial(AllSpawnPopulations[i], SpawnDistributions[i]);
			}
		}
	}

	public void DeletePopulation(string name)
	{
		SpawnPopulationBase[] allSpawnPopulations = AllSpawnPopulations;
		foreach (SpawnPopulationBase spawnPopulationBase in allSpawnPopulations)
		{
			if (spawnPopulationBase.name == name)
			{
				spawnPopulationBase.DeleteEntities();
				break;
			}
		}
	}

	public void DeleteAllPopulations()
	{
		SpawnPopulationBase[] allSpawnPopulations = AllSpawnPopulations;
		for (int i = 0; i < allSpawnPopulations.Length; i++)
		{
			allSpawnPopulations[i].DeleteEntities();
		}
	}

	public void FillGroups()
	{
		for (int i = 0; i < SpawnGroups.Count; i++)
		{
			SpawnGroups[i].Fill();
		}
	}

	public void ClearGroups()
	{
		for (int i = 0; i < SpawnGroups.Count; i++)
		{
			SpawnGroups[i].Clear();
		}
	}

	public void ResetGroups()
	{
		ClearGroups();
		Invoke(FillGroups, 0f);
	}

	public void FillIndividuals()
	{
		for (int i = 0; i < SpawnIndividuals.Count; i++)
		{
			SpawnIndividual spawnIndividual = SpawnIndividuals[i];
			Spawn(Prefab.Load<Spawnable>(spawnIndividual.PrefabID), spawnIndividual.Position, spawnIndividual.Rotation);
		}
	}

	public void InitialSpawn()
	{
		if (ConVar.Spawn.respawn_populations && SpawnDistributions != null)
		{
			for (int i = 0; i < AllSpawnPopulations.Length; i++)
			{
				if (!(AllSpawnPopulations[i] == null))
				{
					SpawnInitial(AllSpawnPopulations[i], SpawnDistributions[i]);
				}
			}
		}
		if (ConVar.Spawn.respawn_groups)
		{
			for (int j = 0; j < SpawnGroups.Count; j++)
			{
				SpawnGroups[j].SpawnInitial();
			}
		}
	}

	public void StartSpawnTick()
	{
		spawnTick = true;
	}

	private IEnumerator SpawnTick()
	{
		while (true)
		{
			yield return CoroutineEx.waitForEndOfFrame;
			if (!spawnTick || !ConVar.Spawn.respawn_populations)
			{
				continue;
			}
			yield return CoroutineEx.waitForSeconds(ConVar.Spawn.tick_populations);
			for (int i = 0; i < AllSpawnPopulations.Length; i++)
			{
				SpawnPopulationBase spawnPopulationBase = AllSpawnPopulations[i];
				if (spawnPopulationBase == null)
				{
					continue;
				}
				SpawnDistribution spawnDistribution = SpawnDistributions[i];
				if (spawnDistribution == null)
				{
					continue;
				}
				try
				{
					if (SpawnDistributions != null)
					{
						SpawnRepeating(spawnPopulationBase, spawnDistribution);
					}
				}
				catch (Exception message)
				{
					Debug.LogError(message);
				}
				yield return CoroutineEx.waitForEndOfFrame;
			}
		}
	}

	private IEnumerator SpawnGroupTick()
	{
		while (true)
		{
			yield return CoroutineEx.waitForEndOfFrame;
			if (!spawnTick || !ConVar.Spawn.respawn_groups)
			{
				continue;
			}
			yield return CoroutineEx.waitForSeconds(1f);
			for (int i = 0; i < SpawnGroups.Count; i++)
			{
				ISpawnGroup spawnGroup = SpawnGroups[i];
				if (spawnGroup != null)
				{
					try
					{
						spawnGroup.SpawnRepeating();
					}
					catch (Exception message)
					{
						Debug.LogError(message);
					}
					yield return CoroutineEx.waitForEndOfFrame;
				}
			}
		}
	}

	private IEnumerator SpawnIndividualTick()
	{
		while (true)
		{
			yield return CoroutineEx.waitForEndOfFrame;
			if (!spawnTick || !ConVar.Spawn.respawn_individuals)
			{
				continue;
			}
			yield return CoroutineEx.waitForSeconds(ConVar.Spawn.tick_individuals);
			for (int i = 0; i < SpawnIndividuals.Count; i++)
			{
				SpawnIndividual spawnIndividual = SpawnIndividuals[i];
				try
				{
					Spawn(Prefab.Load<Spawnable>(spawnIndividual.PrefabID), spawnIndividual.Position, spawnIndividual.Rotation);
				}
				catch (Exception message)
				{
					Debug.LogError(message);
				}
				yield return CoroutineEx.waitForEndOfFrame;
			}
		}
	}

	public void SpawnInitial(SpawnPopulationBase population, SpawnDistribution distribution)
	{
		int targetCount = population.GetTargetCount(distribution);
		int count = distribution.Count;
		int numToFill = targetCount - count;
		population.Fill(this, distribution, numToFill, initialSpawn: true);
	}

	public void SpawnRepeating(SpawnPopulationBase population, SpawnDistribution distribution)
	{
		int targetCount = population.GetTargetCount(distribution);
		int count = distribution.Count;
		int num = targetCount - count;
		num = Mathf.RoundToInt((float)num * population.GetCurrentSpawnRate());
		num = UnityEngine.Random.Range(Mathf.Min(num, MinSpawnsPerTick), Mathf.Min(num, MaxSpawnsPerTick));
		population.Fill(this, distribution, num, initialSpawn: false);
	}

	public int EstimateMaxPopToSpawn(int toSpawn, SpawnPopulationBase pop)
	{
		toSpawn = Mathf.RoundToInt((float)toSpawn * pop.GetCurrentSpawnRate());
		return Mathf.Min(toSpawn, MaxSpawnsPerTick);
	}

	public bool Validate(SpawnPopulationBase population, Prefab<Spawnable> prefab, Vector3 pos, Quaternion rot)
	{
		if (prefab == null)
		{
			return false;
		}
		if (prefab.Component == null)
		{
			Debug.LogError("[Spawn] Missing component 'Spawnable' on " + prefab.Name);
			return false;
		}
		Vector3 scale = Vector3.one;
		DecorComponent[] components = PrefabAttribute.server.FindAll<DecorComponent>(prefab.ID);
		prefab.Object.transform.ApplyDecorComponents(components, ref pos, ref rot, ref scale);
		if (!prefab.ApplyTerrainFilters(pos, rot, scale))
		{
			return false;
		}
		if (!prefab.ApplyWaterChecks(pos, rot, scale))
		{
			return false;
		}
		if (!prefab.ApplyTerrainAnchors(ref pos, rot, scale, TerrainAnchorMode.MinimizeMovement, population.GetSpawnFilter()))
		{
			return false;
		}
		if (!prefab.ApplyTerrainChecks(pos, rot, scale, population.GetSpawnFilter()))
		{
			return false;
		}
		if (!prefab.ApplyEnvironmentVolumeChecks(pos, rot, scale))
		{
			return false;
		}
		if (!prefab.ApplyBoundsChecks(pos, rot, scale, BoundsCheckMask))
		{
			return false;
		}
		if (!prefab.Component.CanSpawnInSafeZone && IsInSafeZone(pos))
		{
			return false;
		}
		return true;
	}

	private static bool IsInSafeZone(Vector3 pos)
	{
		foreach (TriggerSafeZone allSafeZone in TriggerSafeZone.allSafeZones)
		{
			Collider collider = allSafeZone?.triggerCollider;
			if (!(collider == null) && collider.bounds.Contains(pos) && (collider.ClosestPoint(pos) - pos).sqrMagnitude <= 0.0001f)
			{
				return true;
			}
		}
		return false;
	}

	void SpawnPopulationBase.ISpawnHandler.ReportAttempt(SpawnPopulationBase.Status status, Vector3 pos)
	{
	}

	SpawnPopulationBase.Status SpawnPopulationBase.ISpawnHandler.TrySpawn(SpawnPopulationBase population, Prefab<Spawnable> prefab, Vector3 pos, Quaternion rot, out GameObject spawned)
	{
		spawned = null;
		if (!Validate(population, prefab, pos, rot))
		{
			return SpawnPopulationBase.Status.PrefabRejected;
		}
		if (Global.developer > 1)
		{
			Debug.Log("[Spawn] Spawning " + prefab.Name);
		}
		BaseEntity baseEntity = prefab.SpawnEntity(pos, rot, active: false);
		if (baseEntity == null)
		{
			Debug.LogWarning("[Spawn] Couldn't create prefab as entity - " + prefab.Name);
			return SpawnPopulationBase.Status.InvalidEntity;
		}
		Spawnable component = baseEntity.GetComponent<Spawnable>();
		if (component.Population != population)
		{
			component.Population = population;
		}
		PoolableEx.AwakeFromInstantiate(baseEntity.gameObject);
		baseEntity.Spawn();
		spawned = baseEntity.gameObject;
		return SpawnPopulationBase.Status.Success;
	}

	private GameObject Spawn(Prefab<Spawnable> prefab, Vector3 pos, Quaternion rot)
	{
		if (!CheckBounds(prefab.Object, pos, rot, Vector3.one))
		{
			return null;
		}
		BaseEntity baseEntity = prefab.SpawnEntity(pos, rot);
		if (baseEntity == null)
		{
			Debug.LogWarning("[Spawn] Couldn't create prefab as entity - " + prefab.Name);
			return null;
		}
		baseEntity.Spawn();
		return baseEntity.gameObject;
	}

	public bool CheckBounds(GameObject gameObject, Vector3 pos, Quaternion rot, Vector3 scale)
	{
		return CheckBounds(gameObject, pos, rot, scale, BoundsCheckMask);
	}

	public static bool CheckBounds(GameObject gameObject, Vector3 pos, Quaternion rot, Vector3 scale, LayerMask mask)
	{
		if (gameObject == null)
		{
			return true;
		}
		if ((int)mask != 0)
		{
			BaseEntity component = gameObject.GetComponent<BaseEntity>();
			if (component != null && UnityEngine.Physics.CheckBox(pos + rot * Vector3.Scale(component.bounds.center, scale), Vector3.Scale(component.bounds.extents, scale), rot, mask))
			{
				return false;
			}
		}
		return true;
	}

	public void EnforceLimits(bool forceAll = false)
	{
		if (SpawnDistributions == null)
		{
			return;
		}
		for (int i = 0; i < AllSpawnPopulations.Length; i++)
		{
			if (!(AllSpawnPopulations[i] == null))
			{
				SpawnPopulationBase spawnPopulationBase = AllSpawnPopulations[i];
				SpawnDistribution distribution = SpawnDistributions[i];
				if (forceAll || spawnPopulationBase.EnforcePopulationLimits)
				{
					EnforceLimits(spawnPopulationBase, distribution);
				}
			}
		}
	}

	public void EnforceLimits(SpawnPopulationBase population, SpawnDistribution distribution)
	{
		int targetCount = population.GetTargetCount(distribution);
		Spawnable[] array = FindAll(population);
		if (array.Length <= targetCount)
		{
			return;
		}
		Debug.Log(population?.ToString() + " has " + array.Length + " objects, but max allowed is " + targetCount);
		int count = array.Length - targetCount;
		Debug.Log(" - deleting " + count + " objects");
		foreach (Spawnable item in array.Take(count))
		{
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(item.gameObject);
			if (baseEntity.IsValid())
			{
				baseEntity.Kill();
			}
			else
			{
				GameManager.Destroy(item.gameObject);
			}
		}
	}

	public Spawnable[] FindAll(SpawnPopulationBase population)
	{
		return (from x in UnityEngine.Object.FindObjectsByType<Spawnable>(FindObjectsSortMode.None)
			where x.gameObject.activeInHierarchy && x.Population == population
			select x).ToArray();
	}

	public void AddRespawn(SpawnIndividual individual)
	{
		SpawnIndividuals.Add(individual);
	}

	public void AddInstance(Spawnable spawnable)
	{
		if (spawnable.Population != null)
		{
			spawnable.Population.AddInstance(spawnable);
			if (!population2distribution.TryGetValue(spawnable.Population, out var value))
			{
				Debug.LogWarning("[SpawnHandler] trying to add instance to invalid population: " + spawnable.Population);
			}
			else
			{
				value.AddInstance(spawnable);
			}
		}
	}

	public void RemoveInstance(Spawnable spawnable)
	{
		if (spawnable.Population != null)
		{
			spawnable.Population.RemoveInstance(spawnable);
			if (!population2distribution.TryGetValue(spawnable.Population, out var value))
			{
				Debug.LogWarning("[SpawnHandler] trying to remove instance from invalid population: " + spawnable.Population);
			}
			else
			{
				value.RemoveInstance(spawnable);
			}
		}
	}

	public static float PlayerFraction()
	{
		float num = Mathf.Max(Server.maxplayers, 1);
		if (ConVar.Spawn.population_cap_rate > 0 && Server.maxplayers > ConVar.Spawn.population_cap_rate)
		{
			num = ConVar.Spawn.population_cap_rate;
		}
		return Mathf.Clamp01((float)PlayerCount / num);
	}

	public static float PlayerLerp(float min, float max)
	{
		return Mathf.Lerp(min, max, PlayerFraction());
	}

	public static float PlayerExcess()
	{
		float num = Mathf.Max(ConVar.Spawn.player_base, 1f);
		float num2 = PlayerCount;
		if (num2 > (float)ConVar.Spawn.population_cap_rate && ConVar.Spawn.population_cap_rate > 0)
		{
			num2 = ConVar.Spawn.population_cap_rate;
		}
		if (num2 <= num)
		{
			return 0f;
		}
		return (num2 - num) / num;
	}

	public static float PlayerScale(float scalar)
	{
		return Mathf.Max(1f, PlayerExcess() * scalar);
	}

	public void DumpReport(string filename)
	{
		File.AppendAllText(filename, "\r\n\r\nSpawnHandler Report:\r\n\r\n" + GetReport());
	}

	public string GetReport(bool detailed = true, string filter = null)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (AllSpawnPopulations == null)
		{
			stringBuilder.AppendLine("Spawn population array is null.");
		}
		if (SpawnDistributions == null)
		{
			stringBuilder.AppendLine("Spawn distribution array is null.");
		}
		if (AllSpawnPopulations != null && SpawnDistributions != null)
		{
			for (int i = 0; i < AllSpawnPopulations.Length; i++)
			{
				if (AllSpawnPopulations[i] == null)
				{
					continue;
				}
				SpawnPopulationBase spawnPopulationBase = AllSpawnPopulations[i];
				SpawnDistribution spawnDistribution = SpawnDistributions[i];
				if (filter != null && !spawnPopulationBase.name.Contains(filter))
				{
					continue;
				}
				if (spawnPopulationBase != null)
				{
					spawnPopulationBase.GetReportString(stringBuilder, detailed);
					if (spawnDistribution != null)
					{
						int count = spawnDistribution.Count;
						int targetCount = spawnPopulationBase.GetTargetCount(spawnDistribution);
						stringBuilder.AppendLine("- Population: " + count + "/" + targetCount);
						int toSpawn = targetCount - count;
						toSpawn = EstimateMaxPopToSpawn(toSpawn, spawnPopulationBase);
						toSpawn = spawnPopulationBase.EstimateMaxAttempts(toSpawn);
						stringBuilder.Append("- Max attempts for next tick: ");
						stringBuilder.Append(toSpawn);
						stringBuilder.AppendLine();
						int failedFillsInARow = spawnPopulationBase.FailedFillsInARow;
						if (failedFillsInARow > 0)
						{
							stringBuilder.Append("- Failed to reach target population in a row: ");
							stringBuilder.Append(failedFillsInARow);
							stringBuilder.Append("(Recent average spawns: ");
							stringBuilder.Append(spawnPopulationBase.AvgSpawnCount);
							stringBuilder.AppendLine(")");
						}
					}
					else
					{
						stringBuilder.AppendLine("- Distribution #" + i + " is not set.");
					}
				}
				else
				{
					stringBuilder.AppendLine("Population #" + i + " is not set.");
				}
				stringBuilder.AppendLine();
			}
		}
		return stringBuilder.ToString();
	}

	public void GenerateDebugMaps(string name, int simCount, out int spawned, out int attempts)
	{
		spawned = 0;
		attempts = 0;
		SpawnPopulationBase spawnPopulationBase = null;
		SpawnDistribution distribution = null;
		for (int i = 0; i < AllSpawnPopulations.Length; i++)
		{
			SpawnPopulationBase spawnPopulationBase2 = AllSpawnPopulations[i];
			if (spawnPopulationBase2.name == name)
			{
				spawnPopulationBase = spawnPopulationBase2;
				distribution = SpawnDistributions[i];
				break;
			}
		}
		if (spawnPopulationBase == null)
		{
			return;
		}
		int num = Mathf.NextPowerOfTwo((int)((float)World.Size * 0.25f));
		ExportToPNG(spawnPopulationBase.GetBaseMapValues(num), num, TextureFormat.R8, spawnPopulationBase.name + ".png");
		byte[] array = new byte[World.Size * World.Size * 3];
		List<(Vector3, SpawnPopulationBase.Status)> list = new List<(Vector3, SpawnPopulationBase.Status)>(simCount);
		DebugSpawner spawnHandler = new DebugSpawner(this, list);
		spawnPopulationBase.SubFill(spawnHandler, distribution, simCount, initialSpawn: false);
		attempts = list.Count;
		foreach (var item3 in list)
		{
			Vector3 item = item3.Item1;
			SpawnPopulationBase.Status item2 = item3.Item2;
			Vector2i vector2i = (Vector2i)(item.XZ2D() + new Vector2(World.Size / 2, World.Size / 2));
			long num2 = (vector2i.y * World.Size + vector2i.x) * 3;
			Color color;
			switch (item2)
			{
			case SpawnPopulationBase.Status.Success:
				color = Color.green;
				spawned++;
				break;
			case SpawnPopulationBase.Status.InvalidSample:
				color = Color.red;
				break;
			case SpawnPopulationBase.Status.PrefabRejected:
				color = Color.yellow;
				break;
			case SpawnPopulationBase.Status.PrefabPickFailed:
				color = Color.blue;
				break;
			case SpawnPopulationBase.Status.InvalidEntity:
				color = Color.cyan;
				break;
			case SpawnPopulationBase.Status.InvalidSpawnPosOverride:
				color = new Color(1f, 0.41f, 0f);
				break;
			case SpawnPopulationBase.Status.DensityOverflow:
				color = new Color(1f, 0f, 0.91f);
				break;
			default:
				color = Color.magenta;
				break;
			}
			array[num2] = (byte)(color.r * 255f);
			array[num2 + 1] = (byte)(color.g * 255f);
			array[num2 + 2] = (byte)(color.b * 255f);
		}
		ExportToPNG(array, (int)World.Size, TextureFormat.RGB24, spawnPopulationBase.name + "-samples.png");
	}

	public int GenerateOreNodeMap(out int inSafeZone)
	{
		int size = (int)World.Size;
		byte[] array = new byte[size * size * 3];
		inSafeZone = 0;
		int num = 0;
		foreach (OreResourceEntity item in BaseNetworkable.serverEntities.OfType<OreResourceEntity>())
		{
			Vector3 position = item.transform.position;
			bool flag = IsInSafeZone(position);
			Color color = (flag ? Color.red : Color.green);
			Vector2i vector2i = (Vector2i)(position.XZ2D() + new Vector2(size / 2, size / 2));
			long num2 = (vector2i.y * size + vector2i.x) * 3;
			array[num2] = (byte)(color.r * 255f);
			array[num2 + 1] = (byte)(color.g * 255f);
			array[num2 + 2] = (byte)(color.b * 255f);
			num++;
			if (flag)
			{
				inSafeZone++;
			}
		}
		foreach (TriggerSafeZone allSafeZone in TriggerSafeZone.allSafeZones)
		{
			Collider collider = allSafeZone?.triggerCollider;
			if (!(collider == null))
			{
				Bounds bounds = collider.bounds;
				PlotCircle(array, size, bounds.center, bounds.extents.x, Color.blue);
			}
		}
		ExportToPNG(array, size, TextureFormat.RGB24, "ore-nodes.png");
		return num;
	}

	private static void ExportToPNG(byte[] data, int size, TextureFormat format, string filename)
	{
		Texture2D texture2D = new Texture2D(size, size, format, mipChain: false);
		texture2D.SetPixelData(data, 0);
		texture2D.Apply();
		byte[] bytes = texture2D.EncodeToPNG();
		if (!Directory.Exists(Server.rootFolder + "/debug"))
		{
			Directory.CreateDirectory(Server.rootFolder + "/debug");
		}
		File.WriteAllBytes(Server.rootFolder + "/debug/" + filename, bytes);
	}

	private static void PlotCircle(byte[] pixels, int size, Vector3 worldCenter, float worldRadius, Color color)
	{
		Vector2i vector2i = (Vector2i)(worldCenter.XZ2D() + new Vector2(size / 2, size / 2));
		int num = Mathf.RoundToInt(worldRadius);
		int num2 = Mathf.Max(64, num * 4);
		for (int i = 0; i < num2; i++)
		{
			float f = (float)i / (float)num2 * MathF.PI * 2f;
			int num3 = vector2i.x + Mathf.RoundToInt(Mathf.Cos(f) * (float)num);
			int num4 = vector2i.y + Mathf.RoundToInt(Mathf.Sin(f) * (float)num);
			if (num3 >= 0 && num3 < size && num4 >= 0 && num4 < size)
			{
				long num5 = ((long)num4 * (long)size + num3) * 3;
				pixels[num5] = (byte)(color.r * 255f);
				pixels[num5 + 1] = (byte)(color.g * 255f);
				pixels[num5 + 2] = (byte)(color.b * 255f);
			}
		}
	}
}
