using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using ConVar;
using Facepunch;
using Oxide.Core;
using ProtoBuf;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class EggHuntEvent : BaseHuntEvent
{
	public class EggHunter
	{
		public ulong userid;

		public string displayName;

		public int numEggs;
	}

	public float warmupTime = 10f;

	public float warnTime = 20f;

	public float timeAlive;

	public static EggHuntEvent serverEvent = null;

	public static EggHuntEvent clientEvent = null;

	public const int CAST_LAYERS = 10551297;

	[NonSerialized]
	public static float durationSeconds = 180f;

	public Dictionary<ulong, EggHunter> _eggHunters = new Dictionary<ulong, EggHunter>();

	public SeasonalEventType SeasonEventType;

	public ItemAmount[] placementAwards;

	private Dictionary<ulong, List<CollectableEasterEgg>> _spawnedEggs = new Dictionary<ulong, List<CollectableEasterEgg>>();

	private readonly int maxEggPerPlayer = 25;

	private int initialSpawnIndex;

	private readonly Stopwatch stopwatch = new Stopwatch();

	private const int maxBatchSize = 1024;

	private const int initialMinEggPerPlayer = 4;

	private const int initialMaxEggPerPlayer = 6;

	private float eggSpawningFrameBudget = 1.5f;

	[ServerVar(Help = "Will spawn eggs for bots, only for debug purposes - don't enable it!")]
	public static bool includeBots = false;

	public static Translate.Phrase topBunnyPhrase = new Translate.Phrase("egghunt.result.topbunny", "{0} is the top bunny with {1} eggs collected.");

	public static Translate.Phrase noPlayersPhrase = new Translate.Phrase("egghunt.result.noplayers", "Wow, no one played so no one won.");

	public static Translate.Phrase placePhrase = new Translate.Phrase("egghunt.result.place", "You placed {0} of {1} with {2} eggs collected.");

	public static Translate.Phrase rewardPhrase = new Translate.Phrase("egghunt.result.reward", "You received {0}x {1} as an award!.");

	public bool IsEventActive()
	{
		if (timeAlive > warmupTime)
		{
			return timeAlive - warmupTime < durationSeconds;
		}
		return false;
	}

	public void Update()
	{
		timeAlive += UnityEngine.Time.deltaTime;
		if (!base.isServer || base.IsDestroyed)
		{
			return;
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		if (timeAlive - warmupTime > durationSeconds - warnTime)
		{
			flagsUpdateScope.Set(Flags.Reserved1, b: true);
		}
		if (timeAlive - warmupTime > durationSeconds && !IsInvoking(Cooldown) && Interface.CallHook("OnHuntEventEnd", this) == null)
		{
			flagsUpdateScope.Set(Flags.Reserved2, b: true);
			CleanupEggs();
			PrintWinnersAndAward();
			Invoke(Cooldown, 10f);
		}
	}

	public override void DestroyShared()
	{
		base.DestroyShared();
		if (base.isServer)
		{
			serverEvent = null;
		}
		else
		{
			clientEvent = null;
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if ((bool)serverEvent && base.isServer)
		{
			serverEvent.Kill();
			serverEvent = null;
		}
		serverEvent = this;
		SpawnEggs();
		Invoke(StartEvent, warmupTime);
	}

	private void StartEvent()
	{
		if (Interface.CallHook("OnHuntEventStart", this) == null)
		{
			int count = BasePlayer.activePlayerList.Count;
			if (includeBots)
			{
				count = GetCombinedPlayerList(wantBots: true).Count;
			}
			if (initialSpawnIndex <= count)
			{
				eggSpawningFrameBudget = float.PositiveInfinity;
			}
			EnableEggs();
		}
	}

	private void EnableEggs()
	{
		foreach (KeyValuePair<ulong, List<CollectableEasterEgg>> spawnedEgg in _spawnedEggs)
		{
			foreach (CollectableEasterEgg item in spawnedEgg.Value)
			{
				item.gameObject.SetActive(value: true);
				using FlagsUpdateScope flagsUpdateScope = item.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
				flagsUpdateScope.Set(Flags.Disabled, b: false);
			}
		}
	}

	[ContextMenu("SpawnDebug")]
	public void SpawnEggs()
	{
		initialSpawnIndex = 0;
		StartCoroutine(SpawnInitialEggs());
	}

	private IEnumerator SpawnInitialEggs()
	{
		NativeArray<RaycastCommand> raycastCommands = new NativeArray<RaycastCommand>(1024, Allocator.Persistent);
		NativeArray<RaycastHit> hits = new NativeArray<RaycastHit>(1024, Allocator.Persistent);
		NativeArray<ulong> ownerIDs = new NativeArray<ulong>(1024, Allocator.Persistent);
		Queue<(Vector3 position, ulong ownerID)> pendingSpawns = new Queue<(Vector3, ulong)>();
		ListHashSet<BasePlayer> playerList = BasePlayer.activePlayerList;
		if (includeBots)
		{
			playerList = GetCombinedPlayerList(wantBots: true);
		}
		while (initialSpawnIndex != playerList.Count)
		{
			stopwatch.Reset();
			stopwatch.Start();
			int commandIndex = 0;
			for (int i = initialSpawnIndex; i < playerList.Count; i++)
			{
				BasePlayer basePlayer = playerList[i];
				if (basePlayer.isInvisible)
				{
					continue;
				}
				int num = UnityEngine.Random.Range(4, 6) + Mathf.RoundToInt(basePlayer.eggVision);
				Vector3 position = basePlayer.transform.position;
				for (int j = 0; j < num; j++)
				{
					Vector3 randomSpawnPoint = GetRandomSpawnPoint(position, Vector3.zero, 15f, 25f);
					randomSpawnPoint += Vector3.up * 100f;
					raycastCommands[commandIndex] = new RaycastCommand(randomSpawnPoint, Vector3.down, 105f, 10551297);
					ownerIDs[commandIndex] = basePlayer.userID;
					commandIndex++;
					if (commandIndex >= 1024)
					{
						break;
					}
				}
				initialSpawnIndex++;
				if (commandIndex >= 1024 || stopwatch.Elapsed.TotalMilliseconds >= (double)eggSpawningFrameBudget)
				{
					break;
				}
			}
			if (commandIndex > 0)
			{
				JobHandle handle = RaycastCommand.ScheduleBatch(raycastCommands, hits, 1);
				yield return new WaitUntil(() => handle.IsCompleted);
				handle.Complete();
				for (int k = 0; k < commandIndex; k++)
				{
					Vector3 vector = raycastCommands[k].from;
					if (hits[k].collider == null)
					{
						vector.y = TerrainMeta.HeightMap.GetHeight(vector);
					}
					else
					{
						vector = hits[k].point;
					}
					pendingSpawns.Enqueue((vector, ownerIDs[k]));
				}
			}
			yield return CoroutineEx.waitForEndOfFrame;
		}
		while (pendingSpawns.Count > 0)
		{
			stopwatch.Reset();
			stopwatch.Start();
			while (pendingSpawns.Count > 0 && stopwatch.Elapsed.TotalMilliseconds < (double)eggSpawningFrameBudget)
			{
				(Vector3 position, ulong ownerID) tuple = pendingSpawns.Dequeue();
				Vector3 item = tuple.position;
				ulong item2 = tuple.ownerID;
				CollectableEasterEgg collectableEasterEgg = SpawnEggAtPoint(item, active: false);
				collectableEasterEgg.ownerUserID = item2;
				collectableEasterEgg.SetFlagLocal(Flags.Disabled, b: true);
				collectableEasterEgg.Spawn();
				TryGetPlayerEggs(item2).Add(collectableEasterEgg);
			}
			yield return CoroutineEx.waitForEndOfFrame;
		}
		raycastCommands.Dispose();
		hits.Dispose();
		ownerIDs.Dispose();
	}

	private CollectableEasterEgg SpawnEggAtPoint(Vector3 pos, bool active)
	{
		if (((uint)TerrainMeta.TopologyMap.GetTopology(pos) & 0x14080u) != 0)
		{
			float waterLevel = WaterLevel.GetWaterLevel(pos, waves: false);
			if (waterLevel > TerrainMeta.HeightMap.GetHeight(pos) && pos.y < waterLevel)
			{
				pos.y = waterLevel;
			}
		}
		GameManager server = GameManager.server;
		string strPrefab = HuntableResourcePathCached[UnityEngine.Random.Range(0, HuntableResourcePathCached.Count)];
		Vector3 pos2 = pos;
		bool startActive = active;
		return server.CreateEntity(strPrefab, pos2, default(Quaternion), startActive) as CollectableEasterEgg;
	}

	private Vector3 GetRandomSpawnPoint(Vector3 pos, Vector3 aimDir, float minDist = 1f, float maxDist = 2f, bool raycast = false)
	{
		aimDir = ((aimDir == Vector3.zero) ? UnityEngine.Random.onUnitSphere : AimConeUtil.GetModifiedAimConeDirection(90f, aimDir));
		Vector3 vector = pos + Vector3Ex.Direction2D(pos + aimDir * 10f, pos) * UnityEngine.Random.Range(minDist, maxDist);
		if (raycast && UnityEngine.Physics.Raycast(vector + Vector3.up * 100f, Vector3.down, out var hitInfo, 105f, 10551297))
		{
			vector.y = hitInfo.point.y;
		}
		else
		{
			vector.y = TerrainMeta.HeightMap.GetHeight(vector);
		}
		return vector;
	}

	public void OnEggCollected(BasePlayer player, CollectableEasterEgg collectedEgg)
	{
		IncrementScore(player);
		if (_spawnedEggs.TryGetValue(collectedEgg.ownerUserID, out var value))
		{
			value.Remove(collectedEgg);
		}
		int num = ((!((float)Mathf.RoundToInt(player.eggVision) * 0.5f < 1f)) ? 1 : UnityEngine.Random.Range(0, 2));
		int num2 = UnityEngine.Random.Range(1 + num, 2 + num);
		List<CollectableEasterEgg> list = TryGetPlayerEggs(player.userID);
		for (int i = 0; i < num2; i++)
		{
			if (list.Count + 1 > maxEggPerPlayer)
			{
				list[0].Kill();
				list.Remove(list[0]);
			}
			Vector3 randomSpawnPoint = GetRandomSpawnPoint(player.transform.position, player.eyes.BodyForward(), 15f, 25f, raycast: true);
			CollectableEasterEgg collectableEasterEgg = SpawnEggAtPoint(randomSpawnPoint, active: true);
			collectableEasterEgg.ownerUserID = player.userID;
			collectableEasterEgg.Spawn();
			list.Add(collectableEasterEgg);
		}
	}

	private void IncrementScore(BasePlayer player)
	{
		if (!_eggHunters.TryGetValue(player.userID, out var value))
		{
			value = new EggHunter();
			value.displayName = player.displayName;
			value.userid = player.userID;
			_eggHunters.Add(player.userID, value);
		}
		value.numEggs++;
		QueueUpdate();
	}

	private void QueueUpdate()
	{
		if (!IsInvoking(DoNetworkUpdate))
		{
			Invoke(DoNetworkUpdate, 2f);
		}
	}

	private void DoNetworkUpdate()
	{
		SendNetworkUpdate();
	}

	private List<CollectableEasterEgg> TryGetPlayerEggs(ulong userID)
	{
		if (!_spawnedEggs.TryGetValue(userID, out var value))
		{
			value = new List<CollectableEasterEgg>();
			_spawnedEggs[userID] = value;
		}
		return value;
	}

	protected List<EggHunter> GetTopHunters()
	{
		List<EggHunter> list = Facepunch.Pool.Get<List<EggHunter>>();
		foreach (KeyValuePair<ulong, EggHunter> eggHunter in _eggHunters)
		{
			list.Add(eggHunter.Value);
		}
		list.Sort((EggHunter a, EggHunter b) => b.numEggs.CompareTo(a.numEggs));
		return list;
	}

	protected virtual Translate.Phrase GetTopBunnyPhrase()
	{
		return topBunnyPhrase;
	}

	protected virtual Translate.Phrase GetNoPlayersPhrase()
	{
		return noPlayersPhrase;
	}

	protected virtual Translate.Phrase GetPlacePhrase()
	{
		return placePhrase;
	}

	protected virtual Translate.Phrase GetRewardPhrase()
	{
		return rewardPhrase;
	}

	protected void PrintWinnersAndAward()
	{
		List<EggHunter> obj = GetTopHunters();
		if (obj.Count > 0)
		{
			EggHunter eggHunter = obj[0];
			Chat.Broadcast(string.Format(GetTopBunnyPhrase().translated, eggHunter.displayName, eggHunter.numEggs), "", "#eee", 0uL);
			for (int i = 0; i < obj.Count; i++)
			{
				EggHunter eggHunter2 = obj[i];
				BasePlayer basePlayer = BasePlayer.FindByID(eggHunter2.userid);
				if ((bool)basePlayer)
				{
					string translated = GetPlacePhrase().translated;
					translated = string.Format(translated, i + 1, obj.Count, obj[i].numEggs);
					basePlayer.ChatMessage(translated);
					ReportEggsCollected(obj[i].numEggs);
				}
				else
				{
					UnityEngine.Debug.LogWarning("EggHuntEvent PrintWinnersAndAward could not find player with id :" + eggHunter2.userid);
				}
			}
			ReportPlayerParticipated(obj.Count);
			for (int j = 0; j < placementAwards.Length && j < obj.Count; j++)
			{
				BasePlayer basePlayer2 = BasePlayer.FindByID(obj[j].userid);
				if ((bool)basePlayer2)
				{
					Item item = ItemManager.Create(placementAwards[j].itemDef, (int)placementAwards[j].amount, 0uL, isServerSide: true, 0uL);
					basePlayer2.GiveItem(item, GiveItemReason.PickedUp);
					string translated2 = GetRewardPhrase().translated;
					translated2 = string.Format(translated2, (int)placementAwards[j].amount, placementAwards[j].itemDef.displayName.english);
					basePlayer2.ChatMessage(translated2);
				}
			}
			Facepunch.Pool.FreeUnmanaged(ref obj);
		}
		else
		{
			Chat.Broadcast(GetNoPlayersPhrase().translated, "", "#eee", 0uL);
			Facepunch.Pool.FreeUnmanaged(ref obj);
		}
	}

	protected virtual void ReportEggsCollected(int numEggs)
	{
	}

	protected virtual void ReportPlayerParticipated(int topCount)
	{
	}

	private void CleanupEggs()
	{
		foreach (KeyValuePair<ulong, List<CollectableEasterEgg>> spawnedEgg in _spawnedEggs)
		{
			if (spawnedEgg.Value == null)
			{
				continue;
			}
			foreach (CollectableEasterEgg item in spawnedEgg.Value)
			{
				if (item != null)
				{
					item.Kill();
				}
			}
		}
	}

	private void Cooldown()
	{
		CancelInvoke(Cooldown);
		Kill();
	}

	public static ListHashSet<BasePlayer> GetCombinedPlayerList(bool wantBots)
	{
		ListHashSet<BasePlayer> listHashSet = new ListHashSet<BasePlayer>(BasePlayer.activePlayerList.Count + (wantBots ? BasePlayer.bots.Count : 0));
		foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
		{
			listHashSet.Add(activePlayer);
		}
		if (wantBots)
		{
			foreach (BasePlayer bot in BasePlayer.bots)
			{
				listHashSet.Add(bot);
			}
		}
		return listHashSet;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.eggHunt = Facepunch.Pool.Get<EggHunt>();
		List<EggHunter> obj = GetTopHunters();
		info.msg.eggHunt.hunters = Facepunch.Pool.Get<List<EggHunt.EggHunter>>();
		for (int i = 0; i < Mathf.Min(10, obj.Count); i++)
		{
			EggHunt.EggHunter eggHunter = Facepunch.Pool.Get<EggHunt.EggHunter>();
			eggHunter.displayName = obj[i].displayName;
			eggHunter.numEggs = obj[i].numEggs;
			eggHunter.playerID = obj[i].userid;
			info.msg.eggHunt.hunters.Add(eggHunter);
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}
}
