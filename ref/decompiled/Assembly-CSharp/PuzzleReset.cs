using System.Collections.Generic;
using ConVar;
using Facepunch;
using Facepunch.Extend;
using Facepunch.Rust;
using Network;
using ProtoBuf;
using Rust;
using UnityEngine;

public class PuzzleReset : FacepunchBehaviour
{
	public SpawnGroup[] respawnGroups;

	public IOEntity[] resetEnts;

	public GameObject[] resetObjects;

	public bool playersBlockReset;

	public bool CheckSleepingAIZForPlayers;

	public float playerDetectionRadius;

	public Transform playerDetectionOrigin;

	public bool ignoreAboveGroundPlayers;

	public float timeBetweenResets = 30f;

	public bool scaleWithServerPopulation;

	public bool pauseUntilLooted;

	[Tooltip("Ignore players below this height")]
	public float minimumHeightOffset;

	[HideInInspector]
	public Vector3[] resetPositions;

	public bool broadcastResetMessage;

	public Translate.Phrase resetPhrase;

	public bool radiationReset;

	public static ListHashSet<PuzzleReset> AllResets = new ListHashSet<PuzzleReset>();

	private List<SpawnGroup> _cachedSpawnGroups;

	private List<GameObject> _cachedResetObjects;

	public static Translate.Phrase BlockedWarningPhrase = new Translate.Phrase("monument.blocked.warning", "This monument is resetting, please leave the area!");

	private AIInformationZone zone;

	private TwoTierRadiationZone radiationZone;

	private float currentResetTotalTime;

	private float timePausedUnlooted;

	public float resetTimeElapsed;

	private float timeSpentEmptyWithRads;

	private float timeSpentBlockedWithRads;

	private bool hasBeenLooted;

	private float resetTickTime = 10f;

	private bool hasPlayerEnteredRange;

	private float timeSpentBlocked;

	private string lootedSpawnGroupName;

	private static string TwoTierRadSpherePath = "assets/prefabs/io/electric/generators/twotierradiationsphere.prefab";

	private static string TwoTierRadBoxPath = "assets/prefabs/io/electric/generators/twotierradiationbox.prefab";

	private List<NetworkableId> danglingSpawnedInstances;

	private bool canUseRadiationReset
	{
		get
		{
			if (radiationReset)
			{
				return ConVar.Server.monumentPuzzleResetRadiation;
			}
			return false;
		}
	}

	public float lastNormalizedRadiation { get; private set; }

	public void Save(BaseNetworkable.SaveInfo info)
	{
		info.msg.puzzleReset = Facepunch.Pool.Get<ProtoBuf.PuzzleReset>();
		info.msg.puzzleReset.playerBlocksReset = playersBlockReset;
		if (playerDetectionOrigin != null)
		{
			info.msg.puzzleReset.playerDetectionOrigin = playerDetectionOrigin.position;
		}
		info.msg.puzzleReset.playerDetectionRadius = playerDetectionRadius;
		info.msg.puzzleReset.scaleWithServerPopulation = scaleWithServerPopulation;
		info.msg.puzzleReset.timeBetweenResets = timeBetweenResets;
		info.msg.puzzleReset.checkSleepingAIZForPlayers = CheckSleepingAIZForPlayers;
		info.msg.puzzleReset.ignoreAboveGroundPlayers = ignoreAboveGroundPlayers;
		info.msg.puzzleReset.broadcastResetMessage = broadcastResetMessage;
		info.msg.puzzleReset.resetPhrase = resetPhrase?.token ?? "";
		info.msg.puzzleReset.radiationReset = radiationReset;
		info.msg.puzzleReset.pauseUntilLooted = pauseUntilLooted;
		foreach (SpawnGroup spawnGroup in GetSpawnGroups())
		{
			if (spawnGroup.shouldBlockSpawnedEntitySaving)
			{
				continue;
			}
			ProtoBuf.PuzzleReset puzzleReset = info.msg.puzzleReset;
			if (puzzleReset.danglingSpawnedInstances == null)
			{
				puzzleReset.danglingSpawnedInstances = Facepunch.Pool.Get<List<NetworkableId>>();
			}
			foreach (SpawnPointInstance spawnInstance in spawnGroup.SpawnInstances)
			{
				if (spawnInstance != null && spawnInstance.Entity.IsValid() && spawnInstance.Entity.enableSaving)
				{
					info.msg.puzzleReset.danglingSpawnedInstances.Add(spawnInstance.Entity.net.ID);
				}
			}
		}
		if (danglingSpawnedInstances != null && danglingSpawnedInstances.Count > 0)
		{
			ProtoBuf.PuzzleReset puzzleReset = info.msg.puzzleReset;
			if (puzzleReset.danglingSpawnedInstances == null)
			{
				puzzleReset.danglingSpawnedInstances = Facepunch.Pool.Get<List<NetworkableId>>();
			}
			foreach (NetworkableId danglingSpawnedInstance in danglingSpawnedInstances)
			{
				if (!info.msg.puzzleReset.danglingSpawnedInstances.Contains(danglingSpawnedInstance))
				{
					info.msg.puzzleReset.danglingSpawnedInstances.Add(danglingSpawnedInstance);
				}
			}
		}
		if (resetPositions != null && resetPositions.Length != 0)
		{
			info.msg.puzzleReset.resetPositions = Facepunch.Pool.Get<List<Vector3>>();
			for (int i = 0; i < resetPositions.Length; i++)
			{
				info.msg.puzzleReset.resetPositions.Add(resetPositions[i]);
			}
		}
	}

	public void Load(BaseNetworkable.LoadInfo info)
	{
		playersBlockReset = info.msg.puzzleReset.playerBlocksReset;
		if (playerDetectionOrigin != null)
		{
			playerDetectionOrigin.position = info.msg.puzzleReset.playerDetectionOrigin;
		}
		playerDetectionRadius = info.msg.puzzleReset.playerDetectionRadius;
		scaleWithServerPopulation = info.msg.puzzleReset.scaleWithServerPopulation;
		timeBetweenResets = info.msg.puzzleReset.timeBetweenResets;
		CheckSleepingAIZForPlayers = info.msg.puzzleReset.checkSleepingAIZForPlayers;
		ignoreAboveGroundPlayers = info.msg.puzzleReset.ignoreAboveGroundPlayers;
		broadcastResetMessage = info.msg.puzzleReset.broadcastResetMessage;
		if (!string.IsNullOrEmpty(info.msg.puzzleReset.resetPhrase))
		{
			Translate.Phrase phrase = Translate.GetPhrase(info.msg.puzzleReset.resetPhrase);
			if (phrase != null)
			{
				resetPhrase = phrase;
			}
		}
		if (info.msg.puzzleReset.danglingSpawnedInstances != null)
		{
			danglingSpawnedInstances = info.msg.puzzleReset.danglingSpawnedInstances.ShallowClonePooled();
			foreach (NetworkableId danglingSpawnedInstance in danglingSpawnedInstances)
			{
				Network.Net.sv.RegisterUID(danglingSpawnedInstance.Value);
			}
		}
		if (info.msg.puzzleReset.resetPositions != null && info.msg.puzzleReset.resetPositions.Count > 0)
		{
			resetPositions = new Vector3[info.msg.puzzleReset.resetPositions.Count];
			for (int i = 0; i < info.msg.puzzleReset.resetPositions.Count; i++)
			{
				resetPositions[i] = info.msg.puzzleReset.resetPositions[i];
			}
		}
		radiationReset = info.msg.puzzleReset.radiationReset;
		pauseUntilLooted = info.msg.puzzleReset.pauseUntilLooted;
		ResetTimer();
	}

	public float GetResetSpacing()
	{
		return timeBetweenResets * (scaleWithServerPopulation ? (1f - SpawnHandler.PlayerLerp(Spawn.min_rate, Spawn.max_rate)) : 1f);
	}

	private void OnEnable()
	{
		AllResets.Add(this);
	}

	private void OnDisable()
	{
		AllResets.Remove(this);
	}

	public void Start()
	{
		if (timeBetweenResets != float.PositiveInfinity)
		{
			ResetTimer();
		}
	}

	public void ResetTimer()
	{
		ResetTimeCounters();
		CancelInvoke(ResetTick);
		InvokeRandomized(ResetTick, Random.Range(0f, 1f), resetTickTime, 0.5f);
	}

	private void ResetTimeCounters()
	{
		resetTimeElapsed = 0f;
		timePausedUnlooted = 0f;
		timeSpentBlocked = 0f;
		timeSpentEmptyWithRads = 0f;
		timeSpentBlockedWithRads = 0f;
		currentResetTotalTime = 0f;
		hasPlayerEnteredRange = false;
	}

	public bool PassesResetCheck()
	{
		if (playersBlockReset)
		{
			if (CheckSleepingAIZForPlayers)
			{
				if (radiationReset && radiationZone != null && lastNormalizedRadiation > 0f)
				{
					return !radiationZone.HasPlayersInRange();
				}
				bool num = AIZSleeping();
				if (!num)
				{
					TryDDrawAIZone();
				}
				return num;
			}
			return !PlayersWithinDistance();
		}
		return true;
	}

	private void TryDDrawAIZone()
	{
		AIInformationZone aIZone = GetAIZone();
		if (!ConVar.Server.drawpuzzleresets || !(aIZone != null))
		{
			return;
		}
		if (aIZone.wakeZones.Count == 0)
		{
			Debug.LogWarning("Trying to draw AIZone for PuzzleReset but it has no TriggerWakeAIZ!");
			return;
		}
		_ = aIZone.wakeZones[0];
		if (aIZone.wakeZones.Count > 1)
		{
			Debug.LogWarning("Trying to draw AIZone for PuzzleReset but it has multiple TriggerWakeAIZs! Defaulting to first one found");
		}
		using PooledList<BasePlayer> pooledList = aIZone.wakeZones[0].GetPooledListOfPlayers();
		foreach (BasePlayer item in pooledList)
		{
			if (item.IsAdmin && aIZone.areaBox.Contains(item.transform.position))
			{
				item.SendConsoleCommand(DDrawCommand.Box(aIZone.areaBox.position, 10f, Color.green, aIZone.areaBox.extents * 2f, aIZone.areaBox.rotation, distanceFade: false));
				item.SendConsoleCommand(DDrawCommand.Box(aIZone.areaBox.position, 10f, Color.yellow, ScaleSizeByConVar(aIZone.areaBox.extents * 2f), aIZone.areaBox.rotation, distanceFade: false));
			}
		}
	}

	private void DDrawLootedStatus()
	{
		if (playerDetectionOrigin == null)
		{
			return;
		}
		foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
		{
			if (activePlayer.IsAdmin && IsPlayerInRange(activePlayer))
			{
				string text = $"Looted: {hasBeenLooted}";
				if (hasBeenLooted)
				{
					text = text + "\nGroup: " + (lootedSpawnGroupName ?? "Unknown");
				}
				activePlayer.SendConsoleCommand(DDrawCommand.Text(playerDetectionOrigin.transform.position, 10f, Color.green, text, 2f, distanceFade: false));
			}
		}
	}

	public AIInformationZone GetAIZone()
	{
		if (zone != null)
		{
			if (!zone.PointInside(base.transform.position))
			{
				zone = AIInformationZone.GetForPoint(base.transform.position);
			}
		}
		else
		{
			zone = AIInformationZone.GetForPoint(base.transform.position);
		}
		return zone;
	}

	public List<SpawnGroup> GetSpawnGroups()
	{
		if (_cachedSpawnGroups != null)
		{
			return _cachedSpawnGroups;
		}
		_cachedSpawnGroups = new List<SpawnGroup>();
		Vis.Components(base.transform.position, 1f, _cachedSpawnGroups, 262144);
		return _cachedSpawnGroups;
	}

	public List<GameObject> GetResetObjects()
	{
		if (_cachedResetObjects != null)
		{
			return _cachedResetObjects;
		}
		_cachedResetObjects = new List<GameObject>();
		List<PuzzleResetObject> obj = Facepunch.Pool.Get<List<PuzzleResetObject>>();
		Vis.Components(base.transform.position, 1f, obj, 262144);
		foreach (PuzzleResetObject item in obj)
		{
			_cachedResetObjects.Add(item.gameObject.transform.parent.gameObject);
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
		return _cachedResetObjects;
	}

	private bool AIZSleeping()
	{
		AIInformationZone aIZone = GetAIZone();
		if (aIZone == null)
		{
			return false;
		}
		return aIZone.Sleeping;
	}

	private bool PlayersWithinDistance(bool includeSleepers = false)
	{
		return AnyPlayersWithinDistance(playerDetectionOrigin, playerDetectionRadius, minimumHeightOffset, ignoreAboveGroundPlayers, includeSleepers);
	}

	public static bool AnyPlayersWithinDistance(Transform origin, float radius, float heightOffset, bool ignoreAboveGroundPlayers = false, bool includeSleepers = false)
	{
		float num = origin.position.y - radius + heightOffset;
		IEnumerable<BasePlayer> enumerable;
		if (!includeSleepers)
		{
			IEnumerable<BasePlayer> activePlayerList = BasePlayer.activePlayerList;
			enumerable = activePlayerList;
		}
		else
		{
			enumerable = BasePlayer.allPlayerList;
		}
		foreach (BasePlayer item in enumerable)
		{
			if (!item.IsAlive() || item.isInvisible || (!includeSleepers && item.IsSleeping()) || item.transform.position.y < num || Vector3.Distance(item.transform.position, origin.position) >= radius || (ignoreAboveGroundPlayers && item.IsUnderground()))
			{
				continue;
			}
			if (ConVar.Server.drawpuzzleresets && item.IsConnected && item.IsAdmin)
			{
				item.SendConsoleCommand(DDrawCommand.Sphere(origin.position, 10f, Color.green, radius, distanceFade: false));
				item.SendConsoleCommand(DDrawCommand.Sphere(origin.position, 10f, Color.yellow, ScaleRadiusByConVar(radius), distanceFade: false));
				if (heightOffset > 0f)
				{
					Vector3 vector = origin.position + new Vector3(0f, 0f - radius + heightOffset, 0f);
					item.SendConsoleCommand(DDrawCommand.Line(vector + new Vector3(radius, 0f, 0f), vector - new Vector3(radius, 0f, 0f), 10f, Color.red, distanceFade: false, zTest: true));
					item.SendConsoleCommand(DDrawCommand.Line(vector + new Vector3(0f, 0f, radius), vector - new Vector3(0f, 0f, radius), 10f, Color.red, distanceFade: false, zTest: true));
				}
			}
			return true;
		}
		return false;
	}

	public void ResetTick()
	{
		float num = resetTickTime * Debugging.puzzleResetTimeMultiplier;
		currentResetTotalTime += num;
		if (pauseUntilLooted && ConVar.Server.pauseunlootedpuzzles)
		{
			hasBeenLooted = HasPuzzleBeenPartialLooted(out var lootedSpawnGroup);
			lootedSpawnGroupName = ((lootedSpawnGroup == null) ? null : lootedSpawnGroup.name);
			if (ConVar.Server.drawpuzzleresets)
			{
				DDrawLootedStatus();
			}
			if (!hasBeenLooted)
			{
				timePausedUnlooted += num;
				return;
			}
		}
		if (canUseRadiationReset)
		{
			bool num2 = !PassesResetCheck();
			if (num2)
			{
				hasPlayerEnteredRange = true;
			}
			if (!hasPlayerEnteredRange)
			{
				num = 0f;
			}
			resetTimeElapsed += num;
			if (num2)
			{
				timeSpentBlocked += num;
				timeSpentBlockedWithRads += num;
				hasPlayerEnteredRange = true;
			}
			else if (lastNormalizedRadiation > 0f)
			{
				timeSpentEmptyWithRads += num;
			}
		}
		else if (PassesResetCheck())
		{
			resetTimeElapsed += num;
		}
		else
		{
			timeSpentBlocked += num;
		}
		float resetSpacing = GetResetSpacing();
		if (resetTimeElapsed > resetSpacing && (!canUseRadiationReset || timeSpentEmptyWithRads > ConVar.Server.monumentPuzzleResetRadiationPlayerEmptyTime))
		{
			DoReset();
			ResetTimeCounters();
		}
		float num3 = resetSpacing - resetTimeElapsed;
		if (!canUseRadiationReset || (!(num3 < ConVar.Server.monumentPuzzleResetRadiationPreResetTime) && !ConVar.Server.monumentpuzzleresetradiationoverride))
		{
			return;
		}
		SetRadiusRadiationAmount(ConVar.Server.monumentpuzzleresetradiationoverride ? 0.95f : (1f - Mathf.Clamp01(num3 / ConVar.Server.monumentPuzzleResetRadiationPreResetTime)));
		if (ConVar.Server.monumentPuzzleResetWarnings && radiationZone != null)
		{
			using (PooledList<BasePlayer> sentPlayers2 = Facepunch.Pool.Get<PooledList<BasePlayer>>())
			{
				NotifyRadiationZone(radiationZone.InnerRadiation, sentPlayers2);
				NotifyRadiationZone(radiationZone.OuterRadiation, sentPlayers2);
			}
		}
		void NotifyRadiationZone(TriggerRadiation r, List<BasePlayer> sentPlayers)
		{
			if (r.entityContents == null)
			{
				return;
			}
			foreach (BaseEntity entityContent in r.entityContents)
			{
				if (entityContent is BasePlayer { IsNpc: false } basePlayer && !sentPlayers.Contains(basePlayer) && (!ignoreAboveGroundPlayers || basePlayer.IsUnderground()))
				{
					sentPlayers.Add(basePlayer);
					basePlayer.ShowToast(GameTip.Styles.Error, BlockedWarningPhrase, false);
				}
			}
		}
	}

	private bool HasPuzzleBeenPartialLooted(out SpawnGroup lootedSpawnGroup)
	{
		lootedSpawnGroup = null;
		if (GetSpawnGroups().Count == 0)
		{
			return true;
		}
		foreach (SpawnGroup spawnGroup in GetSpawnGroups())
		{
			if (spawnGroup == null || spawnGroup.WantsTimedSpawn() || spawnGroup.resetBehavior == SpawnGroupResetBehavior.Exclude || (spawnGroup.DoesGroupContainNPCs() && spawnGroup.resetBehavior != SpawnGroupResetBehavior.Include))
			{
				continue;
			}
			if (spawnGroup.ObjectsActive < Mathf.Min(spawnGroup.maxPopulation, spawnGroup.SpawnPointCount))
			{
				lootedSpawnGroup = spawnGroup;
				return true;
			}
			foreach (SpawnPointInstance spawnInstance in spawnGroup.SpawnInstances)
			{
				if (spawnInstance.Entity is LootContainer { HasBeenLooted: not false })
				{
					lootedSpawnGroup = spawnGroup;
					return true;
				}
			}
		}
		return false;
	}

	public void CleanupSleepers()
	{
		if (playerDetectionOrigin == null || BasePlayer.sleepingPlayerList == null)
		{
			return;
		}
		for (int num = BasePlayer.sleepingPlayerList.Count - 1; num >= 0; num--)
		{
			BasePlayer basePlayer = BasePlayer.sleepingPlayerList[num];
			if (!(basePlayer == null) && basePlayer.IsSleeping() && Vector3.Distance(basePlayer.transform.position, playerDetectionOrigin.position) <= playerDetectionRadius && (!ignoreAboveGroundPlayers || basePlayer.IsUnderground()))
			{
				basePlayer.Hurt(1000f, DamageType.Suicide, basePlayer, useProtection: false);
			}
		}
	}

	public void TryForceReset()
	{
		if (PlayersWithinDistance(includeSleepers: true))
		{
			foreach (GameObject resetObject in GetResetObjects())
			{
				if (resetObject.TryGetComponent<RadiationSphere>(out var component))
				{
					component.RestartRadiation();
				}
			}
			return;
		}
		DoReset(skipAlerts: true);
		ResetTimer();
	}

	public void DoReset(bool skipAlerts = false)
	{
		SetRadiusRadiationAmount(0f);
		CleanupSleepers();
		IOEntity component = GetComponent<IOEntity>();
		if (component != null)
		{
			ResetIOEntRecursive(component, UnityEngine.Time.frameCount);
			component.MarkDirty();
		}
		if (resetPositions != null)
		{
			Vector3[] array = resetPositions;
			foreach (Vector3 position in array)
			{
				Vector3 position2 = base.transform.TransformPoint(position);
				List<IOEntity> obj = Facepunch.Pool.Get<List<IOEntity>>();
				Vis.Entities(position2, 0.5f, obj, 1235288065, QueryTriggerInteraction.Ignore);
				foreach (IOEntity item in obj)
				{
					if (item.IsRootEntity() && item.isServer)
					{
						ResetIOEntRecursive(item, UnityEngine.Time.frameCount);
						item.MarkDirty();
					}
				}
				Facepunch.Pool.FreeUnmanaged(ref obj);
			}
		}
		if (danglingSpawnedInstances != null)
		{
			foreach (NetworkableId danglingSpawnedInstance in danglingSpawnedInstances)
			{
				BaseNetworkable baseNetworkable = BaseNetworkable.serverEntities.Find(danglingSpawnedInstance);
				if (baseNetworkable.IsValid())
				{
					baseNetworkable.Kill();
				}
			}
			Facepunch.Pool.FreeUnmanaged(ref danglingSpawnedInstances);
		}
		foreach (SpawnGroup spawnGroup in GetSpawnGroups())
		{
			if (!(spawnGroup == null))
			{
				spawnGroup.Clear();
				spawnGroup.DelayedSpawn();
			}
		}
		foreach (GameObject resetObject in GetResetObjects())
		{
			if ((!skipAlerts || !resetObject.TryGetComponent<OilRigResetNotification>(out var _)) && resetObject != null)
			{
				resetObject.SendMessage("OnPuzzleReset", SendMessageOptions.DontRequireReceiver);
			}
		}
		if (broadcastResetMessage && !skipAlerts)
		{
			foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
			{
				if (!activePlayer.IsNpc && activePlayer.IsConnected && !activePlayer.IsInTutorial)
				{
					activePlayer.ShowToast(GameTip.Styles.Server_Event, resetPhrase, false);
				}
			}
		}
		Analytics.Azure.OnPuzzleReset(this, currentResetTotalTime, timeSpentBlocked, timeSpentBlockedWithRads, timePausedUnlooted);
	}

	public void DebugApplyPuzzleResetTime(float time)
	{
		float num = resetTickTime;
		resetTickTime = time;
		ResetTick();
		resetTickTime = num;
	}

	public static void ResetIOEntRecursive(IOEntity target, int resetIndex)
	{
		if (target.lastResetIndex == resetIndex)
		{
			return;
		}
		target.lastResetIndex = resetIndex;
		target.ResetIOState();
		IOEntity.IOSlot[] outputs = target.outputs;
		foreach (IOEntity.IOSlot iOSlot in outputs)
		{
			if (iOSlot.connectedTo.Get() != null && iOSlot.connectedTo.Get() != target)
			{
				ResetIOEntRecursive(iOSlot.connectedTo.Get(), resetIndex);
			}
		}
	}

	private void SetRadiusRadiationAmount(float normalisedAmount)
	{
		if (!canUseRadiationReset)
		{
			normalisedAmount = 0f;
		}
		InitialiseRadiationTriggers();
		if (radiationZone == null)
		{
			return;
		}
		radiationZone.gameObject.SetActive(normalisedAmount > 0f);
		radiationZone.SetRadiationLevel(normalisedAmount * ConVar.Server.monumentPuzzleResetRadiationAmount, normalisedAmount * 10f);
		radiationZone.SetBypassArmor(state: true);
		radiationZone.SetIgnoreAboveGroundPlayers(ignoreAboveGroundPlayers);
		lastNormalizedRadiation = normalisedAmount;
		if (!(normalisedAmount >= 1f) || radiationZone.InnerRadiation.entityContents == null)
		{
			return;
		}
		using PooledList<BaseEntity> pooledList = Facepunch.Pool.Get<PooledList<BaseEntity>>();
		pooledList.AddRange(radiationZone.InnerRadiation.entityContents);
		foreach (BaseEntity item in pooledList)
		{
			if (item is BasePlayer basePlayer && (!ignoreAboveGroundPlayers || basePlayer.IsUnderground()))
			{
				basePlayer.Hurt(25f, DamageType.Radiation, null, useProtection: false);
			}
		}
	}

	private void InitialiseRadiationTriggers()
	{
		if (!canUseRadiationReset)
		{
			return;
		}
		if (radiationZone == null)
		{
			if (CheckSleepingAIZForPlayers)
			{
				if (GetAIZone() != null)
				{
					GameObject gameObject = GameManager.server.CreatePrefab(TwoTierRadBoxPath, base.transform);
					radiationZone = gameObject.GetComponent<TwoTierRadiationZone>();
				}
			}
			else
			{
				GameObject gameObject2 = GameManager.server.CreatePrefab(TwoTierRadSpherePath, base.transform);
				radiationZone = gameObject2.GetComponent<TwoTierRadiationZone>();
				if (playerDetectionOrigin != null)
				{
					gameObject2.transform.localPosition = playerDetectionOrigin.localPosition;
					gameObject2.transform.localRotation = playerDetectionOrigin.localRotation;
					gameObject2.transform.localScale = Vector3.one;
				}
			}
		}
		if (CheckSleepingAIZForPlayers)
		{
			Vector3 size = ScaleSizeByConVar(zone.areaBox.extents * 2f);
			Vector3 center = base.transform.InverseTransformPoint(zone.areaBox.position);
			radiationZone.Apply(new Bounds(center, zone.areaBox.extents * 2f), new Bounds(center, size));
			radiationZone.transform.rotation = zone.areaBox.rotation;
			return;
		}
		float num = playerDetectionRadius;
		float num2 = ScaleRadiusByConVar(playerDetectionRadius);
		float num3 = num2 - num;
		radiationZone.transform.localPosition = playerDetectionOrigin.localPosition;
		radiationZone.Apply(num, num2);
		radiationZone.InnerRadiation.MinLocalHeight = 0f - num + minimumHeightOffset;
		_ = playerDetectionRadius;
		radiationZone.OuterRadiation.MinLocalHeight = 0f - num2 + num3 + minimumHeightOffset;
		radiationZone.InnerRadiation.ApplyLocalHeightCheck = true;
		radiationZone.OuterRadiation.ApplyLocalHeightCheck = true;
	}

	public bool IsPlayerInRange(BasePlayer bp)
	{
		if (CheckSleepingAIZForPlayers)
		{
			if (GetAIZone().areaBox.Contains(bp.transform.position))
			{
				return true;
			}
		}
		else
		{
			float num = playerDetectionOrigin.position.y - playerDetectionRadius + minimumHeightOffset;
			if (bp.transform.position.y < num)
			{
				return false;
			}
			if (bp.Distance(playerDetectionOrigin.position) <= playerDetectionRadius)
			{
				return true;
			}
		}
		return false;
	}

	public void GetDebugInfo(List<string> readout)
	{
		float resetSpacing = GetResetSpacing();
		readout.Add($"Reset time: {resetTimeElapsed}/{resetSpacing}");
		if (canUseRadiationReset)
		{
			float num = resetSpacing - ConVar.Server.monumentPuzzleResetRadiationPreResetTime - resetTimeElapsed;
			if (num > 0f)
			{
				readout.Add($"Rads begin in {num}");
			}
			else
			{
				readout.Add($"Rads active: {lastNormalizedRadiation}");
				readout.Add($"Time spent empty with rads:{timeSpentEmptyWithRads}/{ConVar.Server.monumentPuzzleResetRadiationPlayerEmptyTime}");
			}
			if (!hasPlayerEnteredRange)
			{
				readout.Add("No player has entered range, will not tick");
			}
			else
			{
				readout.Add("Player has entered range, ticking...");
			}
		}
	}

	public MonumentInfo GetClosestMonumentInfo()
	{
		if (TerrainMeta.Path != null && TerrainMeta.Path.Monuments != null)
		{
			float num = float.MaxValue;
			MonumentInfo result = null;
			{
				foreach (MonumentInfo monument in TerrainMeta.Path.Monuments)
				{
					float num2 = Vector3.Distance(base.transform.position, monument.transform.position);
					if (num2 < num)
					{
						num = num2;
						result = monument;
					}
				}
				return result;
			}
		}
		return null;
	}

	private static float ScaleRadiusByConVar(float radius)
	{
		return Mathf.Min(radius * ConVar.Server.monumentPuzzleResetRadiationRadiusMultiplier, radius + ConVar.Server.monumentPuzzleResetRadiationMaxRadiusIncrease);
	}

	private static Vector3 ScaleSizeByConVar(Vector3 size)
	{
		size.x = Mathf.Min(size.x * ConVar.Server.monumentPuzzleResetRadiationRadiusMultiplier, size.x + ConVar.Server.monumentPuzzleResetRadiationMaxRadiusIncrease * 2f);
		size.y = Mathf.Min(size.y * ConVar.Server.monumentPuzzleResetRadiationRadiusMultiplier, size.y + ConVar.Server.monumentPuzzleResetRadiationMaxRadiusIncrease * 2f);
		size.z = Mathf.Min(size.z * ConVar.Server.monumentPuzzleResetRadiationRadiusMultiplier, size.z + ConVar.Server.monumentPuzzleResetRadiationMaxRadiusIncrease * 2f);
		return size;
	}
}
