#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Text;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using Spatial;
using UnityEngine;
using UnityEngine.Assertions;

public class BoatBuildingStation : DecayEntity
{
	private enum BoatValidationStatus
	{
		Valid,
		Invalid_Multiple_Buildings,
		Invalid_No_Blocks,
		Invalid_No_Propulsion,
		Invalid_Missing_Item,
		Invalid_Too_Many_Blocks,
		Invalid_Too_Many_Deployables
	}

	public static readonly Translate.Phrase invalidTooManyDeployablesPhrase = new Translate.Phrase("boatbuilding.invalid.tooManyDeployables", "Deployable limit reached");

	public static readonly Translate.Phrase invalidIllegalPlacement = new Translate.Phrase("boatbuilding.invalid.illegalPlacement", "Illegal deployable placement.");

	[ReplicatedVar]
	public static int max_bbs = 1;

	private static Dictionary<ulong, List<BoatBuildingStation>> bbsPerPlayer = new Dictionary<ulong, List<BoatBuildingStation>>();

	public static Translate.Phrase bbsLimitPhrase = new Translate.Phrase("bbs_limit_update", "You are now at {0}/{1} Boat Building Stations");

	public static Translate.Phrase bbsLimitReachedPhrase = new Translate.Phrase("bbs_limit_reached", "You have reached your Boat Building Station limit!");

	private float lastInteractionTime;

	public const string ACHIEVEMENT_FINISH_BOAT_NAME = "BBS_FINISH_BOAT";

	[ServerVar]
	[Help("When disabled, any spawned static BBS will destroy themselves on spawn")]
	public static bool StaticStationsEnabled = true;

	[ServerVar]
	[Help("When set above zero, enables a global shared cooldown for boat edit/finishing.")]
	public static float GlobalEditFinishUseInterval = 0f;

	public static float NextGlobalEditFinishUseTime = 0f;

	[ServerVar]
	public static bool LogBoatBuildingEvents = false;

	[ServerVar]
	public static float AutoClosePlayerCheckInterval = 150f;

	[ServerVar]
	public static int AutoClosePlayerCheckTriggerCount = 2;

	private static Grid<BoatBuildingStation> serverStations = new Grid<BoatBuildingStation>();

	private int autoClosePassCount;

	private ulong bbsOwnerID;

	private static StringBuilder logStringBuilder = new StringBuilder();

	public bool IsStatic;

	[ReplicatedVar]
	public static float EditFinishUseInterval = 5f;

	public GameObjectRef BoatPrefab;

	public GameObject BuildArea;

	public GameObject Netting;

	public List<ItemDefinition> RequiredItems;

	public List<ItemDefinition> PropulsionItems;

	public Animator Animator;

	public HashSet<ulong> authorizedPlayers = new HashSet<ulong>();

	public string boatLockCode;

	public TriggerPlayer AutoClosePlayerTrigger;

	public List<TriggerPlayer> StationNettingPlayerTriggers;

	public TriggerBoatBuildingArea BoatBuildingAreaTrigger;

	private const float GridQueryRadius = 20f;

	private SteeringWheel cachedSteeringWheel;

	public static Dictionary<ulong, List<BoatBuildingStation>> BBSPerPlayer => bbsPerPlayer;

	public bool KilledDuringWheelFinish { get; set; }

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BoatBuildingStation.OnRpcMessage"))
		{
			if (rpc == 252213800 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - ClearArea");
				}
				using (TimeWarning.New("ClearArea"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(252213800u, "ClearArea", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(252213800u, "ClearArea", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg2 = rPCMessage;
							ClearArea(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in ClearArea");
					}
				}
				return true;
			}
			if (rpc == 2844717662u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - EditBoat");
				}
				using (TimeWarning.New("EditBoat"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2844717662u, "EditBoat", this, player, 3uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(2844717662u, "EditBoat", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg3 = rPCMessage;
							EditBoat(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in EditBoat");
					}
				}
				return true;
			}
			if (rpc == 3242354064u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - FinishBuilding");
				}
				using (TimeWarning.New("FinishBuilding"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3242354064u, "FinishBuilding", this, player, 3uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(3242354064u, "FinishBuilding", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg4 = rPCMessage;
							FinishBuilding(msg4);
						}
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
						player.Kick("RPC Error in FinishBuilding");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public static Planner.CanBuildResult? CanBuildBBS(BasePlayer player, Construction construction)
	{
		if (GameManager.server.FindPrefab(construction.prefabID)?.GetComponent<BaseEntity>() is BoatBuildingStation)
		{
			int num = 1;
			Planner.CanBuildResult value2;
			if (bbsPerPlayer.TryGetValue(player.userID, out var value))
			{
				num = value.Count + 1;
				if (value.Count >= max_bbs)
				{
					value2 = default(Planner.CanBuildResult);
					value2.Result = false;
					value2.Phrase = bbsLimitReachedPhrase;
					return value2;
				}
			}
			value2 = default(Planner.CanBuildResult);
			value2.Result = true;
			value2.Phrase = bbsLimitPhrase;
			value2.Arguments = new string[2]
			{
				num.ToString(),
				max_bbs.ToString()
			};
			return value2;
		}
		return null;
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		if (bbsPerPlayer.TryGetValue(bbsOwnerID, out var _))
		{
			bbsPerPlayer[bbsOwnerID].Remove(this);
		}
		serverStations.Remove(this);
	}

	public static int GetBBSCount(ulong userId)
	{
		if (userId == 0L)
		{
			return 0;
		}
		if (!bbsPerPlayer.TryGetValue(userId, out var value))
		{
			return 0;
		}
		return value.Count;
	}

	private void AddToBBSList(ulong id)
	{
		if (!bbsPerPlayer.ContainsKey(id))
		{
			bbsPerPlayer.Add(id, new List<BoatBuildingStation>());
		}
		if (!IsBBSInList(bbsPerPlayer[id], out var _))
		{
			bbsPerPlayer[id].Add(this);
		}
	}

	private bool IsBBSInList(List<BoatBuildingStation> bbss, out BoatBuildingStation thisBBS)
	{
		bool result = false;
		thisBBS = null;
		if (bbss.Count == 0)
		{
			return false;
		}
		if (thisBBS == null)
		{
			return false;
		}
		foreach (BoatBuildingStation item in bbss)
		{
			if (item.net.ID == net.ID)
			{
				result = true;
				thisBBS = item;
				break;
			}
		}
		return result;
	}

	public override void ServerInit()
	{
		if (!base.isServer)
		{
			return;
		}
		if (IsStatic && !StaticStationsEnabled)
		{
			Kill();
			return;
		}
		base.ServerInit();
		if (IsStatic && !Rust.Application.isLoadingSave)
		{
			base.transform.position = base.transform.position.WithY(WaterLevel.GetWaterSurface(base.transform.position, waves: false, volumes: false) + 0.57f);
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.On, b: true);
			}
			FinishBuilding();
		}
		Netting.gameObject.SetActive(value: false);
		if (!Rust.Application.isLoadingSave && !IsStatic)
		{
			LogBuildingEvent(base.transform.position, null, null, "Boat Building station deployed.");
			EnterEditMode();
		}
		else if (IsOn())
		{
			using (FlagsUpdateScope flagsUpdateScope2 = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope2.Set(Flags.On, b: false);
			}
			EnterEditMode();
		}
		SetLastInteractionTime();
		serverStations.Add(this, base.transform.position.x, base.transform.position.z);
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		if (base.isServer)
		{
			if (IsInvoking(ClearCooldown))
			{
				CancelInvoke(ClearCooldown);
			}
			ClearCooldown();
			if (IsOn())
			{
				RefreshSteeringWheelCache();
			}
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (base.isServer)
		{
			info.msg.boatBuildingStation = Facepunch.Pool.Get<ProtoBuf.BoatBuildingStation>();
			info.msg.boatBuildingStation.ownerId = bbsOwnerID;
		}
	}

	public override void OnPlaced(BasePlayer player)
	{
		if (player != null)
		{
			base.OwnerID = player.userID;
		}
		if (bbsPerPlayer.TryGetValue(player.userID, out var value) && value.Count >= max_bbs)
		{
			value[0].Kill(DestroyMode.Gib);
		}
		bbsOwnerID = player.userID;
		AddToBBSList(bbsOwnerID);
	}

	public static void StartGlobalEditFinishCoolDown()
	{
		NextGlobalEditFinishUseTime = UnityEngine.Time.time + GlobalEditFinishUseInterval;
	}

	private void SetLastInteractionTime()
	{
		lastInteractionTime = UnityEngine.Time.time;
	}

	public static void LogBuildingEvent(Vector3 pos, BasePlayer player, PlayerBoat boat, string message)
	{
		if (LogBoatBuildingEvents)
		{
			logStringBuilder.Clear();
			logStringBuilder.Append(message);
			logStringBuilder.Append(" ");
			logStringBuilder.Append(pos);
			if (player != null)
			{
				logStringBuilder.Append(". ");
				logStringBuilder.Append(player.displayName);
				logStringBuilder.Append(" - ");
				logStringBuilder.Append(player.userID.Get());
				logStringBuilder.Append(".");
			}
			if (boat != null)
			{
				logStringBuilder.Append(". Boat alive time: ");
				logStringBuilder.Append(UnityEngine.Time.time - boat.boatSpawnTime);
				logStringBuilder.Append("s");
			}
			Debug.Log(logStringBuilder.ToString());
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	[RPC_Server.CallsPerSecond(3uL)]
	public void EditBoat(RPCMessage msg)
	{
		if (!(msg.player == null))
		{
			SetLastInteractionTime();
			LogBuildingEvent(base.transform.position, msg.player, null, "Edit boat requested.");
			if (!CanEnterEditMode(msg.player, sendErrorToasts: true))
			{
				StartCooldown();
			}
			else
			{
				EnterEditMode();
			}
		}
	}

	public void EnterEditMode()
	{
		if (!IsOn())
		{
			SetLastInteractionTime();
			StartCooldown();
			ConvertPlayerBoatToConstruction();
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.On, b: true);
			}
			StartAutoCloseInvoke();
			UnStickExplosives();
		}
	}

	private void StartCooldown()
	{
		StartGlobalEditFinishCoolDown();
		if (EditFinishUseInterval <= 0f)
		{
			ClearCooldown();
			return;
		}
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Busy, b: true);
		}
		if (IsInvoking(ClearCooldown))
		{
			CancelInvoke(ClearCooldown);
		}
		Invoke(ClearCooldown, EditFinishUseInterval);
	}

	private void ClearCooldown()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Busy, b: false);
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(3uL)]
	public void FinishBuilding(RPCMessage msg)
	{
		if (!(msg.player == null))
		{
			SetLastInteractionTime();
			LogBuildingEvent(base.transform.position, msg.player, null, "Finish building requested.");
			if (CanPlayerBuild(msg.player))
			{
				FinishBuilding(msg.player);
			}
		}
	}

	public bool FinishBuilding(BasePlayer player = null)
	{
		if (!PlayerBoat.FinishEditingEnabled)
		{
			return false;
		}
		if (IsOnEditFinishCooldown())
		{
			return false;
		}
		cachedSteeringWheel = null;
		SetLastInteractionTime();
		StartCooldown();
		if (!IsOn())
		{
			return true;
		}
		List<BoatBuildingBlock> obj = GetEntitiesInBuildArea<BoatBuildingBlock>(BuildArea, 134217728, server: true);
		List<BaseEntity> obj2 = GetDeployedEntities();
		bool flag = ValidBoat(obj, obj2) == BoatValidationStatus.Valid;
		bool flag2 = flag || (obj.Count == 0 && obj2.Count == 0);
		if (flag2)
		{
			Netting.gameObject.SetActive(value: false);
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.On, b: false);
			}
			WakeUpDroppedItemsInBuildArea();
			if (flag)
			{
				BaseEntity baseEntity = CreateBoat(obj, obj2, base.gameObject);
				if (player != null && baseEntity != null)
				{
					if (Rust.GameInfo.HasAchievements)
					{
						player.GiveAchievement("BBS_FINISH_BOAT");
					}
					baseEntity.OwnerID = player.userID;
					Facepunch.Rust.Analytics.Azure.OnPlayerBoatFinish(player, obj.Count, obj2.Count);
				}
			}
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
		Facepunch.Pool.FreeUnmanaged(ref obj2);
		if (flag2)
		{
			StopAutoCloseInvoke();
		}
		UnStickExplosives();
		return flag2;
	}

	private void UnStickExplosives()
	{
		for (int num = children.Count - 1; num >= 0; num--)
		{
			BaseEntity baseEntity = children[num];
			if (!(baseEntity == null) && baseEntity is TimedExplosive timedExplosive)
			{
				timedExplosive.UnStick();
			}
		}
	}

	private void WakeUpDroppedItemsInBuildArea()
	{
		List<DroppedItem> obj = GetEntitiesInBuildArea<DroppedItem>(BuildArea, -2146959360, server: true);
		foreach (DroppedItem item in obj)
		{
			item.OnPhysicsNeighbourChanged();
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	private void ConvertPlayerBoatToConstruction()
	{
		List<PlayerBoat> obj = GetPlayerBoats();
		if (obj.Count != 1)
		{
			Facepunch.Pool.FreeUnmanaged(ref obj);
			return;
		}
		PlayerBoat playerBoat = obj[0];
		cachedSteeringWheel = playerBoat.GetSteeringWheel();
		playerBoat.PowerDown(force: true);
		playerBoat.rigidBody.isKinematic = true;
		UnityEngine.Object.Destroy(playerBoat.rigidBody);
		bool autoSyncTransforms = UnityEngine.Physics.autoSyncTransforms;
		try
		{
			UnityEngine.Physics.autoSyncTransforms = false;
			playerBoat.transform.position = playerBoat.transform.position.WithY(Env.oceanlevel);
			playerBoat.transform.localEulerAngles = new Vector3(0f, playerBoat.transform.localEulerAngles.y, 0f);
			playerBoat.SendNetworkUpdate();
			playerBoat.DistributeHealthAcrossBlocks();
			playerBoat.SwitchToConstruction();
			playerBoat.KilledForEditMode = true;
			playerBoat.OrphanChildEntities();
		}
		finally
		{
			if (autoSyncTransforms)
			{
				UnityEngine.Physics.SyncTransforms();
			}
			UnityEngine.Physics.autoSyncTransforms = autoSyncTransforms;
		}
		Interface.CallHook("OnPlayerBoatEditStarted", playerBoat, this);
		playerBoat.Kill();
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(1uL)]
	[RPC_Server]
	public void ClearArea(RPCMessage msg)
	{
		if (!(msg.player == null))
		{
			LogBuildingEvent(base.transform.position, msg.player, null, "Clear Area requested at BoatBuildingStation.");
			SetLastInteractionTime();
			if (!CanClearArea(msg.player))
			{
				StartCooldown();
			}
			else
			{
				ClearArea();
			}
		}
	}

	private void ClearArea()
	{
		List<BoatBuildingBlock> obj = GetEntitiesInBuildArea<BoatBuildingBlock>(BuildArea, 134217728, server: true);
		List<BaseEntity> obj2 = GetDeployedEntities();
		for (int num = obj2.Count - 1; num >= 0; num--)
		{
			BaseEntity baseEntity = obj2[num];
			if (!(baseEntity == null) && !PlayerBoat.IsChildOfFinishedPlayerBoat(baseEntity))
			{
				baseEntity.Kill();
			}
		}
		for (int num2 = obj.Count - 1; num2 >= 0; num2--)
		{
			BoatBuildingBlock boatBuildingBlock = obj[num2];
			if (!(boatBuildingBlock == null) && !PlayerBoat.IsChildOfFinishedPlayerBoat(boatBuildingBlock))
			{
				boatBuildingBlock.Kill();
			}
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
		Facepunch.Pool.FreeUnmanaged(ref obj2);
		StartCooldown();
	}

	private BaseEntity CreateBoat(List<BoatBuildingBlock> blocks, List<BaseEntity> ents, GameObject stationGameObject)
	{
		if (!BoatPrefab.isValid)
		{
			return null;
		}
		Quaternion quaternion = CalculateBoatForward(blocks, ents, stationGameObject);
		GetBoatBlocksOBBExtents(blocks, quaternion * Vector3.forward, out var center, out var halfExtents, out var _);
		PlayerBoat obj = GameManager.server.CreateEntity(BoatPrefab.resourcePath, center.WithY(Env.oceanlevel), quaternion) as PlayerBoat;
		obj.Spawn();
		obj.OnCreatedAtBBS(this);
		obj.Init(blocks, ents, halfExtents, loading: false);
		return obj;
	}

	private Quaternion CalculateBoatForward(List<BoatBuildingBlock> blocks, List<BaseEntity> ents, GameObject station)
	{
		foreach (BaseEntity ent in ents)
		{
			if (ent is SteeringWheel)
			{
				return ent.gameObject.transform.rotation;
			}
		}
		return station.transform.rotation * Quaternion.AngleAxis(180f, Vector3.up);
	}

	private void StartAutoCloseInvoke()
	{
		if (IsInvoking(CheckAutoClose))
		{
			CancelInvoke(CheckAutoClose);
		}
		autoClosePassCount = 0;
		InvokeRandomized(CheckAutoClose, AutoClosePlayerCheckInterval, AutoClosePlayerCheckInterval, AutoClosePlayerCheckInterval * 0.1f);
	}

	private void StopAutoCloseInvoke()
	{
		if (IsInvoking(CheckAutoClose))
		{
			CancelInvoke(CheckAutoClose);
		}
	}

	private void CheckAutoClose()
	{
		if (AutoClosePlayerTrigger.contents == null || AutoClosePlayerTrigger.contents.Count == 0)
		{
			if (GetEntitiesInBuildArea<BaseEntity>(BuildArea, -1, base.isServer).Count > 1)
			{
				autoClosePassCount = 0;
				return;
			}
			autoClosePassCount++;
			if (autoClosePassCount >= AutoClosePlayerCheckTriggerCount)
			{
				LogBuildingEvent(base.transform.position, null, null, "Finish building requested by CheckAutoClose.");
				if (FinishBuilding())
				{
					StopAutoCloseInvoke();
				}
			}
		}
		else
		{
			autoClosePassCount = 0;
		}
	}

	public override void OnDied(HitInfo info)
	{
		LogBuildingEvent(base.transform.position, null, null, "BoatBuildingStation.OnDied rquesting FinishBuilding");
		FinishBuilding();
		KillAllBoatBuildingEntities();
		base.OnDied(info);
	}

	public override void OnKilled()
	{
		if (!KilledDuringWheelFinish)
		{
			LogBuildingEvent(base.transform.position, null, null, "BoatBuildingStation.OnKilled rquesting FinishBuilding");
			FinishBuilding();
			KillAllBoatBuildingEntities();
		}
		base.OnKilled();
	}

	private void KillAllBoatBuildingEntities()
	{
		List<BoatBuildingBlock> obj = GetEntitiesInBuildArea<BoatBuildingBlock>(BuildArea, 134217728, base.isServer);
		List<BaseEntity> obj2 = GetDeployedEntities();
		for (int num = obj.Count - 1; num >= 0; num--)
		{
			BoatBuildingBlock boatBuildingBlock = obj[num];
			if (!(boatBuildingBlock == null) && !boatBuildingBlock.HasParent())
			{
				boatBuildingBlock.DieInstantly();
			}
		}
		for (int num2 = obj2.Count - 1; num2 >= 0; num2--)
		{
			BaseEntity baseEntity = obj2[num2];
			if (!(baseEntity == null) && !baseEntity.HasParent())
			{
				baseEntity.Kill();
			}
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
		Facepunch.Pool.FreeUnmanaged(ref obj2);
	}

	[ServerVar]
	public static void print_stats(ConsoleSystem.Arg arg)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("BOAT BUILDING STATIONS:");
		BoatBuildingStation[] array = Util.FindAll<BoatBuildingStation>();
		foreach (BoatBuildingStation boatBuildingStation in array)
		{
			if (!(boatBuildingStation == null))
			{
				stringBuilder.AppendLine("Last Interaction: " + $"{UnityEngine.Time.time - boatBuildingStation.lastInteractionTime}s. Pos: " + $"{boatBuildingStation.transform.position}");
			}
		}
		arg.ReplyWith(stringBuilder.ToString());
	}

	public static BoatBuildingStation GetStationOverlappingPosition(Vector3 position, bool isServer, float padding = 0f)
	{
		if (isServer)
		{
			return GetFromGrid(serverStations);
		}
		return null;
		BoatBuildingStation GetFromGrid(Grid<BoatBuildingStation> grid)
		{
			using PooledList<BoatBuildingStation> pooledList = Facepunch.Pool.Get<PooledList<BoatBuildingStation>>();
			grid.Query(position.x, position.z, 20f, pooledList);
			foreach (BoatBuildingStation item in pooledList)
			{
				if (item.IsInsideBuildArea(position, padding))
				{
					return item;
				}
			}
			return null;
		}
	}

	public static BoatBuildingStation GetStationIntersectingOBB(OBB obb, bool isServer)
	{
		if (isServer)
		{
			return GetFromGrid(serverStations);
		}
		return null;
		BoatBuildingStation GetFromGrid(Grid<BoatBuildingStation> grid)
		{
			Vector3 position = obb.position;
			float radius = Mathf.Max(obb.extents.x, obb.extents.y, obb.extents.z);
			using PooledList<BoatBuildingStation> pooledList = Facepunch.Pool.Get<PooledList<BoatBuildingStation>>();
			grid.Query(position.x, position.z, radius, pooledList);
			foreach (BoatBuildingStation item in pooledList)
			{
				if (item.IntersectsBuildArea(obb))
				{
					return item;
				}
			}
			return null;
		}
	}

	public static BoatBuildingStation GetForPosition(Vector3 position)
	{
		List<TriggerBoatBuildingArea> obj = Facepunch.Pool.Get<List<TriggerBoatBuildingArea>>();
		Vis.Components(position, 4f, obj, 262144);
		foreach (TriggerBoatBuildingArea item in obj)
		{
			BoatBuildingStation boatBuildingStation = GameObjectEx.ToBaseEntity(item.gameObject) as BoatBuildingStation;
			if (!(boatBuildingStation == null))
			{
				Facepunch.Pool.FreeUnmanaged(ref obj);
				return boatBuildingStation;
			}
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
		return null;
	}

	public static BoatBuildingStation GetForPlayer(BasePlayer player)
	{
		return GetForPosition(player.transform.position);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (base.isServer && info.msg.boatBuildingStation != null)
		{
			bbsOwnerID = info.msg.boatBuildingStation.ownerId;
			AddToBBSList(bbsOwnerID);
		}
	}

	public bool HasPlayerInsideNettingBlockerTrigger()
	{
		foreach (TriggerPlayer stationNettingPlayerTrigger in StationNettingPlayerTriggers)
		{
			if (!(stationNettingPlayerTrigger == null) && stationNettingPlayerTrigger.contents != null && stationNettingPlayerTrigger.contents.Count > 0)
			{
				return true;
			}
		}
		return false;
	}

	public bool CanPlayerBuild(BasePlayer player)
	{
		SteeringWheel steeringWheel = GetSteeringWheel();
		if (steeringWheel == null)
		{
			return true;
		}
		return steeringWheel.IsAuthed(player);
	}

	public bool CanPlayerDemolish(BasePlayer player)
	{
		SteeringWheel steeringWheel = GetSteeringWheel();
		if (steeringWheel == null)
		{
			return false;
		}
		return steeringWheel.IsAuthed(player);
	}

	public SteeringWheel GetSteeringWheel(bool cached = true)
	{
		if (!cached)
		{
			RefreshSteeringWheelCache();
		}
		return cachedSteeringWheel;
	}

	public void RefreshSteeringWheelCache()
	{
		cachedSteeringWheel = null;
		List<SteeringWheel> obj = Facepunch.Pool.Get<List<SteeringWheel>>();
		Vis.Entities(GetBuildAreaOBB(BuildArea), obj, 256);
		foreach (SteeringWheel item in obj)
		{
			if (!(item == null))
			{
				cachedSteeringWheel = item;
				break;
			}
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	public void OnSteeringWheelPlaced(SteeringWheel wheel)
	{
		cachedSteeringWheel = wheel;
	}

	public void OnSteeringWheelRemoved(SteeringWheel wheel)
	{
		if (cachedSteeringWheel == wheel)
		{
			cachedSteeringWheel = null;
		}
	}

	public bool IsInsideBuildArea(Vector3 pos, float padding = 0f)
	{
		if (BuildArea == null || BuildArea.transform == null)
		{
			return false;
		}
		return GetBuildAreaOBB(BuildArea, padding).Contains(pos);
	}

	public bool IntersectsBuildArea(OBB obb)
	{
		return GetBuildAreaOBB(BuildArea).Intersects(obb);
	}

	public bool IsOnEditFinishCooldown()
	{
		if (!IsBusy())
		{
			return IsOnGlobalEditFinishCoolDown();
		}
		return true;
	}

	public static bool IsOnGlobalEditFinishCoolDown()
	{
		if (GlobalEditFinishUseInterval <= 0f)
		{
			return false;
		}
		return UnityEngine.Time.time < NextGlobalEditFinishUseTime;
	}

	public bool CanEnterEditMode(BasePlayer player, bool sendErrorToasts)
	{
		if (!PlayerBoat.EditEnabled)
		{
			return false;
		}
		if (IsOnEditFinishCooldown())
		{
			return false;
		}
		if (HasPlayerInsideNettingBlockerTrigger())
		{
			return false;
		}
		PlayerBoat playerBoat = null;
		List<BaseVehicle> obj = GetAllVehicles();
		int num = 0;
		int count = obj.Count;
		foreach (BaseVehicle item in obj)
		{
			if (item is PlayerBoat playerBoat2)
			{
				playerBoat = playerBoat2;
				num++;
			}
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
		if (num > 1 || num != count)
		{
			return false;
		}
		if (playerBoat != null && (!playerBoat.CanStartEditing(player, sendErrorToasts) || !IsBoatFullyContained(playerBoat)))
		{
			return false;
		}
		return true;
	}

	private bool IsBoatFullyContained(PlayerBoat boat)
	{
		if (boat == null)
		{
			return false;
		}
		if (boat.children == null || boat.children.Count == 0)
		{
			return false;
		}
		OBB buildAreaOBB = GetBuildAreaOBB(BuildArea);
		foreach (BoatBuildingBlock item in boat.BoatBuildingBlocks.Cached)
		{
			if (!(item == null) && item.Floor && !item.IsFullyInsideOBB(buildAreaOBB))
			{
				return false;
			}
		}
		return true;
	}

	private bool IsSingularBuilding(List<BoatBuildingBlock> blocks)
	{
		BuildingManager.Building building = null;
		foreach (BoatBuildingBlock block in blocks)
		{
			BuildingManager.Building building2 = block.GetBuilding();
			if (building == null)
			{
				building = building2;
			}
			if (building2 == null || building2 != building)
			{
				return false;
			}
			building = building2;
		}
		return true;
	}

	private bool CanClearArea(BasePlayer player = null)
	{
		if (player == null)
		{
			return false;
		}
		if (IsOnEditFinishCooldown())
		{
			return false;
		}
		if (!CanPlayerBuild(player))
		{
			return false;
		}
		if (HasPlayerInBuildArea())
		{
			return false;
		}
		return true;
	}

	private BoatValidationStatus ValidBoat(List<BoatBuildingBlock> blocks, List<BaseEntity> deployables)
	{
		BoatValidationStatus boatValidationStatus = ValidateBoatBlocks(blocks);
		if (boatValidationStatus != 0)
		{
			return boatValidationStatus;
		}
		BoatValidationStatus boatValidationStatus2 = ValidateBoatDeployables(deployables);
		if (boatValidationStatus2 != 0)
		{
			return boatValidationStatus2;
		}
		BoatValidationStatus boatValidationStatus3 = HasRequiredItems(blocks, deployables);
		if (boatValidationStatus3 != 0)
		{
			return boatValidationStatus3;
		}
		BoatValidationStatus boatValidationStatus4 = HasPropulsion(deployables);
		if (boatValidationStatus4 != 0)
		{
			return boatValidationStatus4;
		}
		return BoatValidationStatus.Valid;
	}

	private BoatValidationStatus ValidateBoatBlocks(List<BoatBuildingBlock> blocks)
	{
		if (blocks.Count <= 0)
		{
			return BoatValidationStatus.Invalid_No_Blocks;
		}
		if (PlayerBoat.MaxBlockCount > 0 && blocks.Count > PlayerBoat.MaxBlockCount)
		{
			return BoatValidationStatus.Invalid_Too_Many_Blocks;
		}
		if (!IsSingularBuilding(blocks))
		{
			return BoatValidationStatus.Invalid_Multiple_Buildings;
		}
		return BoatValidationStatus.Valid;
	}

	private BoatValidationStatus ValidateBoatDeployables(List<BaseEntity> deployables)
	{
		if (PlayerBoat.MaxDeployableCount > 0 && deployables.Count > PlayerBoat.MaxDeployableCount)
		{
			return BoatValidationStatus.Invalid_Too_Many_Deployables;
		}
		return BoatValidationStatus.Valid;
	}

	private BoatValidationStatus HasRequiredItems(List<BoatBuildingBlock> blocks, List<BaseEntity> ents)
	{
		foreach (ItemDefinition requiredItem in RequiredItems)
		{
			if (!HasRequiredItem(requiredItem, ents))
			{
				return BoatValidationStatus.Invalid_Missing_Item;
			}
		}
		return BoatValidationStatus.Valid;
	}

	private BoatValidationStatus HasPropulsion(List<BaseEntity> ents)
	{
		foreach (ItemDefinition propulsionItem in PropulsionItems)
		{
			if (HasRequiredItem(propulsionItem, ents))
			{
				return BoatValidationStatus.Valid;
			}
		}
		return BoatValidationStatus.Invalid_No_Propulsion;
	}

	public bool HasRequiredItem(ItemDefinition item, List<BaseEntity> ents)
	{
		GameObjectRef gameObjectRef = item.GetComponent<ItemModDeployable>()?.entityPrefab;
		if (gameObjectRef == null)
		{
			return false;
		}
		uint num = gameObjectRef.GetEntity().prefabID;
		foreach (BaseEntity ent in ents)
		{
			if (ent.prefabID == num)
			{
				return true;
			}
		}
		return false;
	}

	public List<BaseEntity> GetDeployedEntities()
	{
		List<BaseEntity> entitiesInBuildArea = GetEntitiesInBuildArea<BaseEntity>(BuildArea, 2097408, base.isServer);
		if (entitiesInBuildArea.Count > 0)
		{
			for (int num = entitiesInBuildArea.Count - 1; num >= 0; num--)
			{
				BaseEntity baseEntity = entitiesInBuildArea[num];
				if (baseEntity == null || baseEntity is BoatBuildingStation || baseEntity is BuildingBlock || baseEntity.GetParentEntity() != null)
				{
					entitiesInBuildArea.RemoveAt(num);
				}
			}
		}
		return entitiesInBuildArea;
	}

	public bool HasPlayerInBuildArea()
	{
		if (BoatBuildingAreaTrigger.contents == null)
		{
			return false;
		}
		if (BoatBuildingAreaTrigger.contents.Count == 0)
		{
			return false;
		}
		foreach (GameObject content in BoatBuildingAreaTrigger.contents)
		{
			if (GameObjectEx.ToBaseEntity(content) is BasePlayer { IsNpc: false })
			{
				return true;
			}
		}
		return false;
	}

	public int GetPlayerBoatCount()
	{
		List<PlayerBoat> obj = GetPlayerBoats();
		int count = obj.Count;
		Facepunch.Pool.FreeUnmanaged(ref obj);
		return count;
	}

	private List<PlayerBoat> GetPlayerBoats()
	{
		return GetEntitiesInBuildArea<PlayerBoat>(BuildArea, 134217728, base.isServer);
	}

	private List<BaseVehicle> GetAllVehicles()
	{
		return GetEntitiesInBuildArea<BaseVehicle>(BuildArea, -1, base.isServer);
	}

	private List<BaseVehicle> GetNonPlayerBoatVehicles()
	{
		List<BaseVehicle> entitiesInBuildArea = GetEntitiesInBuildArea<BaseVehicle>(BuildArea, 134225920, base.isServer);
		for (int num = entitiesInBuildArea.Count - 1; num >= 0; num--)
		{
			BaseVehicle baseVehicle = entitiesInBuildArea[num];
			if (baseVehicle == null || baseVehicle is PlayerBoat)
			{
				entitiesInBuildArea.RemoveAt(num);
			}
		}
		return entitiesInBuildArea;
	}

	public static List<T> GetEntitiesInBuildArea<T>(GameObject buildArea, int layerMask, bool server) where T : BaseEntity
	{
		List<T> list = Facepunch.Pool.Get<List<T>>();
		Vis.Entities(GetBuildAreaOBB(buildArea), list, layerMask);
		if (list.Count > 0)
		{
			for (int num = list.Count - 1; num >= 0; num--)
			{
				T val = list[num];
				if (val == null || val.isServer != server)
				{
					list.RemoveAt(num);
				}
			}
		}
		return list;
	}

	public static OBB GetBuildAreaOBB(GameObject buildArea, float padding = 0f)
	{
		Vector3 position = buildArea.transform.position;
		Vector3 size = buildArea.transform.lossyScale + Vector3.one * (padding * 2f);
		Quaternion rotation = buildArea.transform.rotation;
		return new OBB(position, size, rotation);
	}

	public static void GetBoatBlocksOBBExtents(List<BoatBuildingBlock> blocks, Vector3 forward, out Vector3 center, out Vector3 halfExtents, out Quaternion rot)
	{
		List<Vector3> obj = Facepunch.Pool.Get<List<Vector3>>();
		foreach (BoatBuildingBlock block in blocks)
		{
			if (!(block == null) && !block.isClient)
			{
				OBB oBB = block.WorldSpaceBounds();
				obj.Add(oBB.GetPoint(-1f, -1f, -1f));
				obj.Add(oBB.GetPoint(-1f, -1f, 1f));
				obj.Add(oBB.GetPoint(1f, 1f, -1f));
				obj.Add(oBB.GetPoint(1f, 1f, 1f));
			}
		}
		GetOBBExtents(obj, forward, out center, out halfExtents, out rot);
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	private static void GetOBBExtents(List<Vector3> points, Vector3 forward, out Vector3 center, out Vector3 halfExtents, out Quaternion rotation)
	{
		forward = forward.normalized;
		Vector3 normalized = Vector3.Cross(Vector3.up, forward).normalized;
		Vector3 upwards = Vector3.Cross(forward, normalized);
		rotation = Quaternion.LookRotation(forward, upwards);
		Matrix4x4 inverse = Matrix4x4.Rotate(rotation).inverse;
		Vector3 vector = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		Vector3 vector2 = new Vector3(float.MinValue, float.MinValue, float.MinValue);
		foreach (Vector3 point in points)
		{
			Vector3 rhs = inverse.MultiplyPoint3x4(point);
			vector = Vector3.Min(vector, rhs);
			vector2 = Vector3.Max(vector2, rhs);
		}
		Vector3 vector3 = (vector + vector2) * 0.5f;
		halfExtents = (vector2 - vector) * 0.5f;
		center = rotation * vector3;
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
	}

	public void OnDrawGizmos()
	{
		Gizmos.color = Color.magenta;
		Matrix4x4 matrix = Matrix4x4.TRS(BuildArea.transform.position, BuildArea.transform.rotation, BuildArea.transform.lossyScale);
		Matrix4x4 matrix2 = Gizmos.matrix;
		Gizmos.matrix = matrix;
		Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
		Gizmos.matrix = matrix2;
	}
}
