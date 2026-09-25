#define UNITY_ASSERTIONS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Network;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class SatelliteControlComputer : BaseMountable
{
	public enum ControlState
	{
		Offline,
		Ready,
		Controlling,
		Descending,
		Cooldown
	}

	private List<SatelliteData> currentSatellites;

	private int satelliteSeed;

	private int selectedSatelliteIndex = -1;

	private float controlPhaseEndTime;

	private int fuelRemaining;

	private bool targetingSearchActive;

	private Vector2 lateralOffset;

	private float radiusModifier;

	private int failedThrustersMask;

	private CrashTargeting targeting;

	private Vector3 lastThrusterCrashPos;

	private bool hasThrusterCrashPos;

	private NetworkableId preventBuildingVolumeId;

	private EntityRef fuelStorageInstance;

	private readonly List<(ItemDefinition def, int amount)> paidPowerCost = new List<(ItemDefinition, int)>();

	private const float TargetingWorldExtentFraction = 0.45f;

	private const int ThrusterFail_NoCrashSite = 0;

	private const int ThrusterFail_NoFuel = 1;

	private const int ThrusterFail_OutOfBounds = 2;

	private SatelliteCrash crashPrefabComponent;

	private bool crashPrefabResolved;

	public static readonly Translate.Phrase OfflinePhrase = new Translate.Phrase("satcomp.offline", "SYSTEM OFFLINE — INSERT ITEMS");

	public static readonly Translate.Phrase ReadyPhrase = new Translate.Phrase("satcomp.inrange", "SATELLITES IN RANGE. SELECT TARGET");

	public static readonly Translate.Phrase ControllingPhrase = new Translate.Phrase("satcomp.deorbitburn", "DEORBIT BURN ACTIVE. FIRE THRUSTERS");

	public static readonly Translate.Phrase CooldownPhrase = new Translate.Phrase("satcomp.recalibrating", "RECALIBRATING. PLEASE WAIT");

	public static readonly Translate.Phrase DescendingPhrase = new Translate.Phrase("satcomp.impactin", "SATELLITE DESCENDING. IMPACT IN:");

	public static readonly Translate.Phrase NeedFuelPhrase = new Translate.Phrase("satcomp.needfuel", "Insert the required items to power the terminal");

	public static readonly Translate.Phrase MissingItemsPhrase = new Translate.Phrase("satcomp.missingitems", "Insert {0}");

	public static readonly Translate.Phrase EventActivePhrase = new Translate.Phrase("satcomp.eventactive", "A satellite is already descending");

	public static readonly Translate.Phrase NoGridPowerPhrase = new Translate.Phrase("satcomp.nogridpower", "No grid power — bring the power plant online");

	public static readonly Translate.Phrase NoCrashSitePhrase = new Translate.Phrase("satcomp.nocrashsite", "Unsuitable location");

	public static readonly Translate.Phrase NotEnoughFuelForDistancePhrase = new Translate.Phrase("satcomp.nofueldistance", "Not enough fuel to move that far");

	public static readonly Translate.Phrase OutOfBoundsPhrase = new Translate.Phrase("satcomp.outofbounds", "Edge of map reached");

	public static readonly Translate.Phrase LaunchAbortedPhrase = new Translate.Phrase("satcomp.launchaborted", "Launch aborted — no viable crash site");

	[Header("Satellite Computer")]
	public GameObjectRef menuPrefab;

	[Header("Spectator Screen")]
	[Tooltip("Pre-spawned world-space monitor UI (a child of this entity's prefab, sitting just behind the glass) that renders the read-only crash map for everyone nearby. Initialised in ClientInit.")]
	public SatelliteSpectatorScreenUI spectatorScreen;

	[Tooltip("Pre-spawned world-space monitor UI (a prefab child like the spectator screen) that shows only the countdown to impact. Initialised in ClientInit.")]
	public SatelliteCountdownScreenUI countdownScreen;

	[Header("Power")]
	[Tooltip("Items the player must have, and which are consumed, to power up the terminal. amount is the minimum required/consumed; set maxAmount higher to roll a random cost in that range each session (leave maxAmount at -1/0 for a fixed cost).")]
	public List<ItemAmountRanged> powerCost = new List<ItemAmountRanged>();

	private List<int> resolvedPowerCost = new List<int>();

	[Tooltip("Child SatelliteFuelStorage prefab the player loads the power-up items into. Spawned and parented to this computer on first init.")]
	public GameObjectRef fuelStoragePrefab;

	[Header("Satellites")]
	[Tooltip("Number of satellites to generate each session")]
	public int satelliteCount = 6;

	[Header("Satellite Prefab")]
	public GameObjectRef satelliteCrashPrefab;

	[Tooltip("Prevent-building volume spawned at the locked crash site to reserve the area during descent")]
	public GameObjectRef preventBuildingPrefab;

	[Tooltip("Pre-spawned child of this entity's prefab (must live inside the entity prefab, not the launch site — monument-authored references don't reach the client entity). Leave disabled in the prefab; enabled only while a satellite is descending. Optional — skipped when unset.")]
	[Header("Siren")]
	public Transform launchSiren;

	[Tooltip("Inverse of launchSiren — enabled whenever launchSiren is not. Same prefab rules: must be a pre-spawned child of this entity's prefab. Optional — skipped when unset.")]
	public Transform launchSirenOff;

	[Header("Audio")]
	public SoundDefinition selectSatellite;

	public SoundDefinition controlSatellite;

	public SoundDefinition lockSatelliteTrajectory;

	public const Flags Flag_HasPower = Flags.Reserved8;

	public const Flags Flag_Controlling = Flags.Reserved9;

	public const Flags Flag_Cooldown = Flags.Reserved10;

	public static SatelliteControlComputer ActiveDescending { get; private set; }

	public Vector3 LockedCrashPosition => targeting.finalCrashPos;

	private ControlState State => GetControlState(targeting.isDescending);

	private static bool SendDebugCrashPos => false;

	public bool IsOffline
	{
		get
		{
			if (!HasFlag(Flags.Reserved8) && !HasFlag(Flags.Reserved10))
			{
				return !HasFlag(Flags.Reserved9);
			}
			return false;
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("SatelliteControlComputer.OnRpcMessage"))
		{
			if (rpc == 167225468 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					UnityEngine.Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_FireThruster");
				}
				using (TimeWarning.New("RPC_FireThruster"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(167225468u, "RPC_FireThruster", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(167225468u, "RPC_FireThruster", this, player, 3f))
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
							RPC_FireThruster(msg2);
						}
					}
					catch (Exception exception)
					{
						UnityEngine.Debug.LogException(exception);
						player.Kick("RPC Error in RPC_FireThruster");
					}
				}
				return true;
			}
			if (rpc == 1788197568 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					UnityEngine.Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_LockTrajectory");
				}
				using (TimeWarning.New("RPC_LockTrajectory"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1788197568u, "RPC_LockTrajectory", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(1788197568u, "RPC_LockTrajectory", this, player, 3f))
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
							RPC_LockTrajectory(msg3);
						}
					}
					catch (Exception exception2)
					{
						UnityEngine.Debug.LogException(exception2);
						player.Kick("RPC Error in RPC_LockTrajectory");
					}
				}
				return true;
			}
			if (rpc == 2618836397u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					UnityEngine.Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_OpenFuelStorage");
				}
				using (TimeWarning.New("RPC_OpenFuelStorage"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2618836397u, "RPC_OpenFuelStorage", this, player, 2uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(2618836397u, "RPC_OpenFuelStorage", this, player, 3f))
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
							RPC_OpenFuelStorage(msg4);
						}
					}
					catch (Exception exception3)
					{
						UnityEngine.Debug.LogException(exception3);
						player.Kick("RPC Error in RPC_OpenFuelStorage");
					}
				}
				return true;
			}
			if (rpc == 1907581554 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					UnityEngine.Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_PowerUp");
				}
				using (TimeWarning.New("RPC_PowerUp"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1907581554u, "RPC_PowerUp", this, player, 2uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(1907581554u, "RPC_PowerUp", this, player, 3f))
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
							RPCMessage msg5 = rPCMessage;
							RPC_PowerUp(msg5);
						}
					}
					catch (Exception exception4)
					{
						UnityEngine.Debug.LogException(exception4);
						player.Kick("RPC Error in RPC_PowerUp");
					}
				}
				return true;
			}
			if (rpc == 1342336167 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					UnityEngine.Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_RequestControlState");
				}
				using (TimeWarning.New("RPC_RequestControlState"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1342336167u, "RPC_RequestControlState", this, player, 5uL))
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
							RPCMessage msg6 = rPCMessage;
							RPC_RequestControlState(msg6);
						}
					}
					catch (Exception exception5)
					{
						UnityEngine.Debug.LogException(exception5);
						player.Kick("RPC Error in RPC_RequestControlState");
					}
				}
				return true;
			}
			if (rpc == 273918495 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					UnityEngine.Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_RequestSatelliteList");
				}
				using (TimeWarning.New("RPC_RequestSatelliteList"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(273918495u, "RPC_RequestSatelliteList", this, player, 2uL))
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
							RPCMessage msg7 = rPCMessage;
							RPC_RequestSatelliteList(msg7);
						}
					}
					catch (Exception exception6)
					{
						UnityEngine.Debug.LogException(exception6);
						player.Kick("RPC Error in RPC_RequestSatelliteList");
					}
				}
				return true;
			}
			if (rpc == 814681278 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					UnityEngine.Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_SelectSatellite");
				}
				using (TimeWarning.New("RPC_SelectSatellite"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(814681278u, "RPC_SelectSatellite", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(814681278u, "RPC_SelectSatellite", this, player, 3f))
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
							RPCMessage msg8 = rPCMessage;
							RPC_SelectSatellite(msg8);
						}
					}
					catch (Exception exception7)
					{
						UnityEngine.Debug.LogException(exception7);
						player.Kick("RPC Error in RPC_SelectSatellite");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		InvokeRepeating(ServerTick, 1f, 1f);
		targetingSearchActive = false;
		if (!Rust.Application.isLoadingSave)
		{
			SpawnFuelStorage();
			RollPowerCost();
		}
		else
		{
			Invoke(RestorePreventBuildingVolumeAfterLoad, 1f);
		}
	}

	protected override void OnChildAdded(BaseEntity child)
	{
		base.OnChildAdded(child);
		if (base.isServer && fuelStoragePrefab.isValid && child.prefabID == fuelStoragePrefab.GetEntity().prefabID)
		{
			fuelStorageInstance.Set(child);
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		if (currentSatellites == null && (HasFlag(Flags.Reserved8) || HasFlag(Flags.Reserved9)))
		{
			if (satelliteSeed != 0)
			{
				currentSatellites = SatelliteData.GenerateList(satelliteCount, satelliteSeed);
			}
			else
			{
				GenerateSatellites();
			}
			if (HasFlag(Flags.Reserved9))
			{
				lateralOffset = new Vector2(targeting.center.x - base.transform.position.x, targeting.center.z - base.transform.position.z);
				radiusModifier = targeting.radius - Satellite.default_crash_radius;
			}
		}
	}

	private void SpawnFuelStorage()
	{
		if (GetFuelStorage() != null)
		{
			return;
		}
		if (!fuelStoragePrefab.isValid)
		{
			UnityEngine.Debug.LogWarning("[Satellite] fuelStoragePrefab not set — players can't load power-up items.");
			return;
		}
		BaseEntity baseEntity = GameManager.server.CreateEntity(fuelStoragePrefab.resourcePath, base.transform.position, base.transform.rotation);
		if (!(baseEntity == null))
		{
			baseEntity.SetParent(this, worldPositionStays: true);
			baseEntity.Spawn();
			fuelStorageInstance.Set(baseEntity);
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		float time = info.cachedTime.Time;
		info.msg.satelliteControlComputer = Facepunch.Pool.Get<ProtoBuf.SatelliteControlComputer>();
		info.msg.satelliteControlComputer.cooldownRemaining = ((targeting.cooldownEndTime > 0f) ? (targeting.cooldownEndTime - time) : 0f);
		info.msg.satelliteControlComputer.controlRemaining = ((controlPhaseEndTime > 0f) ? (controlPhaseEndTime - time) : 0f);
		info.msg.satelliteControlComputer.selectedSatelliteIndex = selectedSatelliteIndex;
		info.msg.satelliteControlComputer.fuelRemaining = fuelRemaining;
		info.msg.satelliteControlComputer.isDescending = targeting.isDescending;
		info.msg.satelliteControlComputer.descentRemaining = ((targeting.descentEndTime > 0f) ? (targeting.descentEndTime - time) : 0f);
		info.msg.satelliteControlComputer.targetingCenter = targeting.center;
		info.msg.satelliteControlComputer.targetingRadius = targeting.radius;
		if (!info.forDisk)
		{
			return;
		}
		info.msg.satelliteControlComputer.satelliteSeed = ((currentSatellites != null) ? satelliteSeed : 0);
		info.msg.satelliteControlComputer.finalCrashPos = targeting.finalCrashPos;
		info.msg.satelliteControlComputer.finalCrashRadius = targeting.finalCrashRadius;
		info.msg.satelliteControlComputer.resolvedPowerCost = Facepunch.Pool.Get<List<int>>();
		foreach (int item in resolvedPowerCost)
		{
			info.msg.satelliteControlComputer.resolvedPowerCost.Add(item);
		}
	}

	public override void AttemptMount(BasePlayer player, bool doMountChecks = true)
	{
		if (!IsOffline)
		{
			base.AttemptMount(player, doMountChecks);
		}
	}

	public override void OnPlayerMounted()
	{
		base.OnPlayerMounted();
		BasePlayer mounted = GetMounted();
		if (!(mounted == null))
		{
			SendControlStateToPlayer(mounted);
		}
	}

	private void SendControlStateToPlayer(BasePlayer player)
	{
		if (player == null)
		{
			return;
		}
		if (HasFlag(Flags.Reserved9))
		{
			float arg = Mathf.Max(0f, controlPhaseEndTime - UnityEngine.Time.time);
			if (currentSatellites != null)
			{
				SendSatelliteListToPlayer(player);
			}
			ClientRPC(RpcTarget.Player("RPC_ControlPhaseStarted", player), arg, fuelRemaining, Satellite.default_crash_radius, selectedSatelliteIndex);
			SendTargetingToPlayer(player, -1, lastThrusterCrashPos, hasThrusterCrashPos);
			SendInvalidThrustersToPlayer(player);
		}
		else if (HasFlag(Flags.Reserved8) && currentSatellites != null)
		{
			SendSatelliteListToPlayer(player);
		}
		if (targeting.isDescending && HasFlag(Flags.Reserved10))
		{
			SendTrajectoryLockedToPlayer(player);
		}
	}

	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server]
	public void RPC_RequestControlState(RPCMessage msg)
	{
		if (!(msg.player == null) && !(msg.player != GetMounted()))
		{
			SendControlStateToPlayer(msg.player);
		}
	}

	private void ServerTick()
	{
		if (HasFlag(Flags.Reserved10) && UnityEngine.Time.time >= targeting.cooldownEndTime)
		{
			SetFlagLocal(Flags.Reserved10, b: false);
			targeting.cooldownEndTime = 0f;
			GenerateSatellites();
			RollPowerCost();
			SendNetworkUpdate();
		}
		if (HasFlag(Flags.Reserved9) && !targetingSearchActive && UnityEngine.Time.time >= controlPhaseEndTime)
		{
			EndControlPhase(GetMounted());
		}
	}

	[RPC_Server.CallsPerSecond(2uL)]
	[RPC_Server]
	public void RPC_RequestSatelliteList(RPCMessage msg)
	{
		if (HasFlag(Flags.Reserved8) && currentSatellites != null && !(msg.player == null) && !(msg.player != GetMounted()))
		{
			SendSatelliteListToPlayer(msg.player);
		}
	}

	[RPC_Server.CallsPerSecond(2uL)]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void RPC_PowerUp(RPCMessage msg)
	{
		if (HasFlag(Flags.Reserved8) || HasFlag(Flags.Reserved10))
		{
			return;
		}
		if (targeting.isDescending)
		{
			if (msg.player != null)
			{
				msg.player.ShowToast(GameTip.Styles.Red_Normal, EventActivePhrase, false);
			}
			return;
		}
		BasePlayer player = msg.player;
		if (player == null)
		{
			return;
		}
		if (Satellite.require_powerplant && Powergrid.enabled && PowergridManager.GetCurrentStage(isServer: true) < 4)
		{
			player.ShowToast(GameTip.Styles.Red_Normal, NoGridPowerPhrase, false);
			return;
		}
		if (!Satellite.free_power)
		{
			string missingPowerCostText = GetMissingPowerCostText();
			if (missingPowerCostText != null)
			{
				player.ShowToast(GameTip.Styles.Red_Normal, MissingItemsPhrase, false, missingPowerCostText);
				return;
			}
			TakePowerCost();
		}
		SetFlagLocal(Flags.Reserved8, b: true);
		GenerateSatellites();
		SendNetworkUpdate();
	}

	private SatelliteFuelStorage GetFuelStorage()
	{
		if (fuelStorageInstance.Get(base.isServer) is SatelliteFuelStorage result)
		{
			return result;
		}
		if (children != null)
		{
			foreach (BaseEntity child in children)
			{
				if (child is SatelliteFuelStorage satelliteFuelStorage)
				{
					fuelStorageInstance.Set(satelliteFuelStorage);
					return satelliteFuelStorage;
				}
			}
		}
		return null;
	}

	private void RollPowerCost()
	{
		resolvedPowerCost.Clear();
		foreach (ItemAmountRanged item in powerCost)
		{
			resolvedPowerCost.Add((item?.itemDef != null) ? Mathf.CeilToInt(item.GetAmount()) : 0);
		}
	}

	private void EnsurePowerCostRolled()
	{
		if (resolvedPowerCost.Count != powerCost.Count)
		{
			RollPowerCost();
		}
	}

	private string GetMissingPowerCostText()
	{
		EnsurePowerCostRolled();
		SatelliteFuelStorage fuelStorage = GetFuelStorage();
		StringBuilder stringBuilder = null;
		for (int i = 0; i < powerCost.Count; i++)
		{
			ItemAmountRanged itemAmountRanged = powerCost[i];
			if (itemAmountRanged?.itemDef == null)
			{
				continue;
			}
			int num = resolvedPowerCost[i];
			int num2 = ((fuelStorage != null && fuelStorage.inventory != null) ? fuelStorage.inventory.GetAmount(itemAmountRanged.itemDef.itemid, onlyUsableAmounts: true) : 0);
			int num3 = num - num2;
			if (num3 > 0)
			{
				if (stringBuilder == null)
				{
					stringBuilder = new StringBuilder();
				}
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append(num3).Append(" more ").Append(itemAmountRanged.itemDef.displayName.english);
			}
		}
		return stringBuilder?.ToString();
	}

	private void TakePowerCost()
	{
		EnsurePowerCostRolled();
		paidPowerCost.Clear();
		SatelliteFuelStorage fuelStorage = GetFuelStorage();
		if (fuelStorage == null || fuelStorage.inventory == null)
		{
			return;
		}
		for (int i = 0; i < powerCost.Count; i++)
		{
			ItemAmountRanged itemAmountRanged = powerCost[i];
			if (!(itemAmountRanged?.itemDef == null))
			{
				int num = fuelStorage.inventory.Take(null, itemAmountRanged.itemDef.itemid, resolvedPowerCost[i]);
				if (num > 0)
				{
					paidPowerCost.Add((itemAmountRanged.itemDef, num));
				}
			}
		}
	}

	private void RefundPowerCost()
	{
		if (paidPowerCost.Count == 0)
		{
			return;
		}
		SatelliteFuelStorage fuelStorage = GetFuelStorage();
		foreach (var item4 in paidPowerCost)
		{
			ItemDefinition item = item4.def;
			int item2 = item4.amount;
			Item item3 = ItemManager.Create(item, item2, 0uL, isServerSide: true, 0uL);
			if (item3 != null && (fuelStorage == null || fuelStorage.inventory == null || !item3.MoveToContainer(fuelStorage.inventory)))
			{
				item3.Drop(base.transform.position + Vector3.up, Vector3.zero);
			}
		}
		paidPowerCost.Clear();
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(2uL)]
	[RPC_Server.IsVisible(3f)]
	public void RPC_OpenFuelStorage(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!(player == null) && player.CanInteract())
		{
			SatelliteFuelStorage fuelStorage = GetFuelStorage();
			if (!(fuelStorage == null))
			{
				fuelStorage.PlayerOpenLoot(player);
			}
		}
	}

	private void GenerateSatellites()
	{
		int num = (int)(UnityEngine.Time.realtimeSinceStartup * 1000f) ^ net.ID.Value.GetHashCode();
		if (num == 0)
		{
			num = 1;
		}
		satelliteSeed = num;
		currentSatellites = SatelliteData.GenerateList(satelliteCount, num);
		selectedSatelliteIndex = -1;
	}

	private void SendSatelliteListToPlayer(BasePlayer player)
	{
		if (currentSatellites != null && currentSatellites.Count != 0)
		{
			string arg = SatelliteData.SerializeList(currentSatellites);
			ClientRPC(RpcTarget.Player("RPC_ReceiveSatelliteList", player), arg);
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(5uL)]
	public void RPC_SelectSatellite(RPCMessage msg)
	{
		if (State == ControlState.Ready && !(msg.player == null) && !(msg.player != GetMounted()))
		{
			int num = msg.read.Int32();
			if (currentSatellites != null && num >= 0 && num < currentSatellites.Count)
			{
				selectedSatelliteIndex = num;
				SatelliteData satelliteData = currentSatellites[num];
				radiusModifier = 0f;
				failedThrustersMask = 0;
				hasThrusterCrashPos = false;
				fuelRemaining = satelliteData.fuel;
				float control_window = Satellite.control_window;
				controlPhaseEndTime = UnityEngine.Time.time + control_window;
				SetFlagLocal(Flags.Reserved9, b: true);
				SendNetworkUpdate();
				ClientRPC(RpcTarget.Player("RPC_ControlPhaseStarted", msg.player), control_window, fuelRemaining, Satellite.default_crash_radius, selectedSatelliteIndex);
				TryInitialOffsetAttempt(msg.player, 0);
			}
		}
	}

	private void TryInitialOffsetAttempt(BasePlayer player, int attempt)
	{
		ChooseInitialLateralOffset();
		RecalculateTargeting();
		float crashSiteClearance = GetCrashSiteClearance();
		BeginTargetingSearch(targeting.center, targeting.radius, crashSiteClearance, default(Vector3), hasPreferred: false, logResult: false, delegate(bool found, Vector3 foundPos, int samplesTested, int tcsInArea)
		{
			int num = Mathf.Max(1, Satellite.initial_offset_attempts);
			if (!found && attempt + 1 < num)
			{
				if (Satellite.debug)
				{
					UnityEngine.Debug.Log($"[Satellite] Initial offset attempt {attempt + 1}/{num} found no crash site — re-rolling.");
				}
				TryInitialOffsetAttempt(player, attempt + 1);
			}
			else
			{
				if (!found && Satellite.debug)
				{
					UnityEngine.Debug.LogWarning($"[Satellite] No valid initial crash site after {num} offset attempt(s) — starting on the last target.");
				}
				lastThrusterCrashPos = foundPos;
				hasThrusterCrashPos = found;
				SendTargetingToPlayer(player, -1, lastThrusterCrashPos, hasThrusterCrashPos);
			}
		});
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(5uL)]
	public void RPC_FireThruster(RPCMessage msg)
	{
		if (!HasFlag(Flags.Reserved9) || (fuelRemaining <= 0 && !Satellite.free_fuel) || msg.player == null || msg.player != GetMounted())
		{
			return;
		}
		int num = msg.read.Int32();
		if (!TryGetThruster(num, out var thruster))
		{
			return;
		}
		if (targetingSearchActive)
		{
			if (Satellite.debug)
			{
				UnityEngine.Debug.Log($"[Satellite] Thruster {thruster.effect} ignored — a crash-site search is still in progress.");
			}
			NotifyThrusterRejected(msg.player);
			return;
		}
		if ((failedThrustersMask & (1 << num)) != 0)
		{
			if (Satellite.debug)
			{
				UnityEngine.Debug.Log($"[Satellite] Thruster {thruster.effect} cached as failed for this target — auto-failing, no fuel charged.");
			}
			NotifyThrusterRejected(msg.player);
			return;
		}
		Vector2 prevLateral = lateralOffset;
		float prevRadiusMod = radiusModifier;
		Vector3 center = targeting.center;
		if (ApplyThrusterEffect(thruster))
		{
			if (IsMoveBlockedByBounds(center))
			{
				lateralOffset = prevLateral;
				radiusModifier = prevRadiusMod;
				NotifyThrusterRejected(msg.player, 2);
			}
			else
			{
				RecalculateTargeting();
				TryThrusterStep(msg.player, thruster, num, prevLateral, prevRadiusMod, 1);
			}
		}
	}

	private bool IsMoveBlockedByBounds(Vector3 prevCenter)
	{
		float num = TerrainMeta.Size.x * 0.45f;
		float f = base.transform.position.x + lateralOffset.x;
		float f2 = base.transform.position.z + lateralOffset.y;
		if (Mathf.Abs(f) <= num && Mathf.Abs(f2) <= num)
		{
			return false;
		}
		Vector3 vector = CalculateTargetingCenter();
		if (Mathf.Approximately(vector.x, prevCenter.x))
		{
			return Mathf.Approximately(vector.z, prevCenter.z);
		}
		return false;
	}

	private void TryThrusterStep(BasePlayer player, SatelliteData.ThrusterInfo thruster, int thrusterLabelIndex, Vector2 prevLateral, float prevRadiusMod, int stepsTaken)
	{
		float clearance = GetCrashSiteClearance();
		BeginTargetingSearch(targeting.center, targeting.radius, clearance, lastThrusterCrashPos, hasThrusterCrashPos, logResult: false, delegate(bool hasSafeSpot, Vector3 foundCrashPos, int samplesTested, int tcsInArea)
		{
			if (Satellite.debug)
			{
				LogThrusterFireDiagnostics(thruster, clearance, samplesTested, tcsInArea, hasSafeSpot);
			}
			if (!hasSafeSpot)
			{
				int num = Mathf.Max(0, Satellite.thruster_extra_steps);
				bool flag = stepsTaken <= num;
				if (flag && !Satellite.free_fuel && fuelRemaining < stepsTaken + 1)
				{
					RollbackThrusterMove(prevLateral, prevRadiusMod, thrusterLabelIndex);
					NotifyThrusterRejected(player, 1);
				}
				else if (flag && ApplyThrusterEffect(thruster))
				{
					RecalculateTargeting();
					TryThrusterStep(player, thruster, thrusterLabelIndex, prevLateral, prevRadiusMod, stepsTaken + 1);
				}
				else
				{
					RollbackThrusterMove(prevLateral, prevRadiusMod, thrusterLabelIndex);
					NotifyThrusterRejected(player);
				}
			}
			else
			{
				failedThrustersMask = 0;
				if (!Satellite.free_fuel)
				{
					fuelRemaining = Mathf.Max(0, fuelRemaining - stepsTaken);
				}
				lastThrusterCrashPos = foundCrashPos;
				hasThrusterCrashPos = true;
				SendTargetingToPlayer(player, thrusterLabelIndex, foundCrashPos, haveCrashPos: true);
				if (!Satellite.free_fuel && fuelRemaining <= 0)
				{
					EndControlPhase(player);
				}
				SendNetworkUpdate();
			}
		});
	}

	private void RollbackThrusterMove(Vector2 prevLateral, float prevRadiusMod, int thrusterLabelIndex)
	{
		lateralOffset = prevLateral;
		radiusModifier = prevRadiusMod;
		RecalculateTargeting();
		failedThrustersMask |= 1 << thrusterLabelIndex;
	}

	private bool TryGetThruster(int labelIndex, out SatelliteData.ThrusterInfo thruster)
	{
		thruster = null;
		if (currentSatellites == null || selectedSatelliteIndex < 0 || selectedSatelliteIndex >= currentSatellites.Count)
		{
			return false;
		}
		foreach (SatelliteData.ThrusterInfo thruster2 in currentSatellites[selectedSatelliteIndex].thrusters)
		{
			if (thruster2.labelIndex == labelIndex)
			{
				thruster = thruster2;
				return true;
			}
		}
		return false;
	}

	private void NotifyThrusterRejected(BasePlayer player, int reason = 0)
	{
		SendInvalidThrustersToPlayer(player);
		SendThrusterFailedToPlayer(player, reason);
	}

	private void LogThrusterFireDiagnostics(SatelliteData.ThrusterInfo thruster, float clearance, int samplesTested, int tcsInArea, bool hasSafeSpot)
	{
		bool flag = tcsInArea >= 0;
		int num = (flag ? tcsInArea : CrashSpotSearch.CountToolCupboards(targeting.center, targeting.radius + clearance));
		string text = ((!Satellite.obstruction_tc_reorder) ? "reorder off" : (flag ? "reorder on (TC lookup)" : "reorder on (skipped, cached spot)"));
		string text2 = ((samplesTested == 0) ? "reused cached spot (1 check)" : $"scanned {samplesTested} sample(s)");
		UnityEngine.Debug.Log($"[Satellite] Thruster {thruster.effect} → center {targeting.center}, radius {targeting.radius:F0}m, " + $"clearance {clearance:F0}m, {text}, TCs in area: {num}, {text2}, hasSafeSpot: {hasSafeSpot}");
	}

	private bool ApplyThrusterEffect(SatelliteData.ThrusterInfo thruster)
	{
		switch (thruster.effect)
		{
		case SatelliteData.ThrusterEffect.Left:
			lateralOffset.x -= Satellite.lateral_distance;
			return true;
		case SatelliteData.ThrusterEffect.Right:
			lateralOffset.x += Satellite.lateral_distance;
			return true;
		case SatelliteData.ThrusterEffect.Forward:
			lateralOffset.y += Satellite.lateral_distance;
			return true;
		case SatelliteData.ThrusterEffect.Backward:
			lateralOffset.y -= Satellite.lateral_distance;
			return true;
		case SatelliteData.ThrusterEffect.Tighten:
			if (Satellite.default_crash_radius + radiusModifier <= Satellite.min_crash_radius)
			{
				return false;
			}
			radiusModifier -= Satellite.shrink_expand_radius;
			ApplyRandomNudge();
			return true;
		case SatelliteData.ThrusterEffect.Widen:
			if (radiusModifier >= 0f)
			{
				return false;
			}
			radiusModifier += Satellite.shrink_expand_radius;
			return true;
		case SatelliteData.ThrusterEffect.RotateCW:
			RotateOffset(Satellite.rotation_strength);
			return true;
		case SatelliteData.ThrusterEffect.RotateCCW:
			RotateOffset(0f - Satellite.rotation_strength);
			return true;
		default:
			return false;
		}
	}

	private void RotateOffset(float degrees)
	{
		float f = degrees * (MathF.PI / 180f);
		float num = Mathf.Cos(f);
		float num2 = Mathf.Sin(f);
		float x = lateralOffset.x;
		float y = lateralOffset.y;
		lateralOffset.x = x * num + y * num2;
		lateralOffset.y = (0f - x) * num2 + y * num;
	}

	private void ApplyRandomNudge()
	{
		float f = UnityEngine.Random.Range(0f, 360f) * (MathF.PI / 180f);
		Vector2 vector = new Vector2(Mathf.Cos(f), Mathf.Sin(f));
		float num = UnityEngine.Random.Range(Satellite.nudge_distance_min, Satellite.nudge_distance_max);
		lateralOffset += vector * num;
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(1uL)]
	public void RPC_LockTrajectory(RPCMessage msg)
	{
		if (HasFlag(Flags.Reserved9) && !(msg.player == null) && !(msg.player != GetMounted()) && !targetingSearchActive)
		{
			EndControlPhase(msg.player);
			Analytics.Azure.OnSatelliteLockTrajectory(msg.player, targeting.center);
		}
	}

	private void RecalculateTargeting()
	{
		targeting.center = CalculateTargetingCenter();
		lateralOffset = new Vector2(targeting.center.x - base.transform.position.x, targeting.center.z - base.transform.position.z);
		targeting.radius = Mathf.Max(Satellite.default_crash_radius + radiusModifier, Satellite.min_crash_radius);
	}

	private void ChooseInitialLateralOffset()
	{
		float num = TerrainMeta.Size.x * 0.45f;
		float crashSiteClearance = GetCrashSiteClearance();
		int num2 = Mathf.Max(1, Satellite.initial_offset_attempts);
		for (int i = 0; i < num2; i++)
		{
			Vector3 vector = new Vector3(UnityEngine.Random.Range(0f - num, num), 0f, UnityEngine.Random.Range(0f - num, num));
			vector.y = TerrainMeta.HeightMap.GetHeight(vector);
			lateralOffset = new Vector2(vector.x - base.transform.position.x, vector.z - base.transform.position.z);
			if (CrashSpotSearch.IsSpotOk(vector, crashSiteClearance))
			{
				return;
			}
		}
		if (Satellite.debug)
		{
			UnityEngine.Debug.LogWarning($"[Satellite] No valid initial map position found in {num2} tries — starting from the last candidate (may be unsuitable).");
		}
	}

	private void SendTargetingToPlayer(BasePlayer player, int labelIndex, Vector3 crashPos = default(Vector3), bool haveCrashPos = false)
	{
		if (!(player == null))
		{
			float x = targeting.center.x;
			float z = targeting.center.z;
			bool flag = SendDebugCrashPos && haveCrashPos;
			float arg = (flag ? crashPos.x : 0f);
			float arg2 = (flag ? crashPos.z : 0f);
			ClientRPC(RpcTarget.Player("RPC_ThrusterFired", player), labelIndex, fuelRemaining, x, z, targeting.radius, flag ? 1 : 0, arg, arg2);
		}
	}

	private void SendInvalidThrustersToPlayer(BasePlayer player)
	{
		if (!(player == null))
		{
			ClientRPC(RpcTarget.Player("RPC_InvalidThrusters", player), failedThrustersMask);
		}
	}

	private void SendThrusterFailedToPlayer(BasePlayer player, int reason = 0)
	{
		if (!(player == null))
		{
			ClientRPC(RpcTarget.Player("RPC_ThrusterFailed", player), reason);
		}
	}

	private void SendTrajectoryLockedToPlayer(BasePlayer player)
	{
		if (!(player == null))
		{
			float arg = Mathf.Max(0f, targeting.descentEndTime - UnityEngine.Time.time);
			bool sendDebugCrashPos = SendDebugCrashPos;
			float arg2 = (sendDebugCrashPos ? targeting.finalCrashPos.x : 0f);
			float arg3 = (sendDebugCrashPos ? targeting.finalCrashPos.z : 0f);
			ClientRPC(RpcTarget.Player("RPC_TrajectoryLocked", player), arg, targeting.center.x, targeting.center.z, targeting.finalCrashRadius, sendDebugCrashPos ? 1 : 0, arg2, arg3);
		}
	}

	private static float RollCooldownSeconds()
	{
		float num = SatelliteCrash.DayLengthMinutes * 60f / 24f;
		float minInclusive = Mathf.Min(Satellite.cooldown_hours_min, Satellite.cooldown_hours_max);
		float maxInclusive = Mathf.Max(Satellite.cooldown_hours_min, Satellite.cooldown_hours_max);
		return UnityEngine.Random.Range(minInclusive, maxInclusive) * num;
	}

	private void EndControlPhase(BasePlayer notifyPlayer = null)
	{
		if (!HasFlag(Flags.Reserved9))
		{
			return;
		}
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.Local))
		{
			flagsUpdateScope.Set(Flags.Reserved9, b: false);
			flagsUpdateScope.Set(Flags.Reserved8, b: false);
			flagsUpdateScope.Set(Flags.Reserved10, b: true);
		}
		controlPhaseEndTime = 0f;
		targeting.cooldownEndTime = UnityEngine.Time.time + RollCooldownSeconds();
		SendNetworkUpdate();
		if (currentSatellites == null || selectedSatelliteIndex < 0 || selectedSatelliteIndex >= currentSatellites.Count)
		{
			UnityEngine.Debug.LogWarning("[Satellite] Control phase ended with no satellite data (restored from a save?) — cancelling the launch.");
			return;
		}
		SatelliteData sat = currentSatellites[selectedSatelliteIndex];
		targeting.finalCrashRadius = targeting.radius;
		Vector3 center = targeting.center;
		float radius = targeting.radius;
		float crashSiteClearance = GetCrashSiteClearance();
		BeginTargetingSearch(center, radius, crashSiteClearance, lastThrusterCrashPos, hasThrusterCrashPos, logResult: true, delegate(bool found, Vector3 foundPos, int samplesTested, int tcsInArea)
		{
			if (!found)
			{
				UnityEngine.Debug.LogWarning($"[Satellite] No safe crash spot within {radius:F0}m of {center} — launch aborted.");
				RefundPowerCost();
				if (notifyPlayer != null)
				{
					notifyPlayer.ShowToast(GameTip.Styles.Red_Normal, LaunchAbortedPhrase, false);
				}
			}
			else
			{
				targeting.finalCrashPos = foundPos;
				paidPowerCost.Clear();
				SpawnPreventBuildingVolume(targeting.finalCrashPos);
				SpawnOrbitEntity(sat, targeting.finalCrashPos);
				SatelliteCrash satelliteCrash = GetCrashPrefabComponent();
				float scheduledDescentSeconds = SatelliteCrash.GetScheduledDescentSeconds((satelliteCrash != null) ? satelliteCrash.descentDayFraction : 1f);
				targeting.descentEndTime = UnityEngine.Time.time + scheduledDescentSeconds;
				targeting.isDescending = true;
				ActiveDescending = this;
				SendNetworkUpdate();
				if (notifyPlayer != null)
				{
					notifyPlayer.AddClanScore(ClanScoreEventType.LaunchSatellite);
					SendTrajectoryLockedToPlayer(notifyPlayer);
				}
			}
		});
	}

	private void BeginTargetingSearch(Vector3 center, float radius, float clearance, Vector3 preferred, bool hasPreferred, bool logResult, Action<bool, Vector3, int, int> onComplete)
	{
		targetingSearchActive = true;
		StartCoroutine(TargetingSearchRoutine(center, radius, clearance, preferred, hasPreferred, logResult, onComplete));
	}

	private IEnumerator TargetingSearchRoutine(Vector3 center, float radius, float clearance, Vector3 preferred, bool hasPreferred, bool logResult, Action<bool, Vector3, int, int> onComplete)
	{
		CrashSpotSearch search = new CrashSpotSearch(center, radius, clearance, preferred, hasPreferred, logResult);
		int framesSpanned = 1;
		double totalScanMs = 0.0;
		Stopwatch sw = new Stopwatch();
		sw.Restart();
		bool flag;
		Vector3 arg;
		int num;
		int tcsInArea;
		if (search.TryReusePreferred(out var result))
		{
			totalScanMs += sw.Elapsed.TotalMilliseconds;
			flag = true;
			arg = result;
			num = 0;
			tcsInArea = search.TcsInArea;
		}
		else
		{
			CrashSpotSearch.Status status;
			while (true)
			{
				sw.Restart();
				do
				{
					status = search.Step();
				}
				while (status == CrashSpotSearch.Status.InProgress && sw.Elapsed.TotalMilliseconds < (double)Satellite.targeting_budget_ms);
				totalScanMs += sw.Elapsed.TotalMilliseconds;
				if (status != 0)
				{
					break;
				}
				framesSpanned++;
				yield return null;
				if (this == null || base.IsDestroyed)
				{
					targetingSearchActive = false;
					yield break;
				}
			}
			flag = status == CrashSpotSearch.Status.Found;
			arg = search.Result;
			num = search.SamplesTested;
			tcsInArea = search.TcsInArea;
		}
		if (Satellite.debug)
		{
			UnityEngine.Debug.Log(string.Format("[Satellite] Crash-site search {0} {1} frame(s) ", (framesSpanned > 1) ? "spread across" : "completed in", framesSpanned) + $"({num} sample(s) tested, {totalScanMs:F2}ms total compute, budget {Satellite.targeting_budget_ms:F2}ms/frame, found: {flag}).");
		}
		targetingSearchActive = false;
		onComplete?.Invoke(flag, arg, num, tcsInArea);
	}

	private Vector3 CalculateTargetingCenter()
	{
		Vector3 position = base.transform.position;
		position.x += lateralOffset.x;
		position.z += lateralOffset.y;
		float num = TerrainMeta.Size.x * 0.45f;
		position.x = Mathf.Clamp(position.x, 0f - num, num);
		position.z = Mathf.Clamp(position.z, 0f - num, num);
		position.y = TerrainMeta.HeightMap.GetHeight(position);
		return position;
	}

	private SatelliteCrash GetCrashPrefabComponent()
	{
		if (!crashPrefabResolved)
		{
			crashPrefabResolved = true;
			GameObject gameObject = (satelliteCrashPrefab.isValid ? satelliteCrashPrefab.Get() : null);
			crashPrefabComponent = ((gameObject != null) ? gameObject.GetComponent<SatelliteCrash>() : null);
		}
		return crashPrefabComponent;
	}

	private float GetCrashSiteClearance()
	{
		return Satellite.site_clearance_radius;
	}

	private void SpawnPreventBuildingVolume(Vector3 position)
	{
		if (!preventBuildingPrefab.isValid)
		{
			UnityEngine.Debug.LogWarning("[Satellite] preventBuildingPrefab not set — the crash site won't be reserved during the descent.");
			return;
		}
		BaseEntity baseEntity = GameManager.server.CreateEntity(preventBuildingPrefab.resourcePath, position);
		if (!(baseEntity == null))
		{
			baseEntity.Spawn();
			preventBuildingVolumeId = baseEntity.net.ID;
		}
	}

	private void CleanupPreventBuildingVolume(bool drawCrashDebugSphere = false)
	{
		if (preventBuildingVolumeId.IsValid)
		{
			BaseNetworkable baseNetworkable = BaseNetworkable.serverEntities.Find(preventBuildingVolumeId);
			if (baseNetworkable != null && !baseNetworkable.IsDestroyed)
			{
				baseNetworkable.Kill();
			}
			preventBuildingVolumeId = default(NetworkableId);
		}
	}

	private void RestorePreventBuildingVolumeAfterLoad()
	{
		if (targeting.isDescending && !preventBuildingVolumeId.IsValid)
		{
			SpawnPreventBuildingVolume(targeting.finalCrashPos);
		}
	}

	private void SpawnOrbitEntity(SatelliteData sat, Vector3 crashTarget)
	{
		if (!satelliteCrashPrefab.isValid)
		{
			return;
		}
		BaseEntity baseEntity = GameManager.server.CreateEntity(satelliteCrashPrefab.resourcePath, crashTarget);
		if (!(baseEntity == null))
		{
			SatelliteCrash component = baseEntity.GetComponent<SatelliteCrash>();
			if (component != null)
			{
				component.InitOrbit(sat, crashTarget, net.ID, fuelRemaining);
			}
			baseEntity.Spawn();
		}
	}

	public void OnSatelliteCrashed(bool crashed = true)
	{
		Analytics.Azure.OnSatelliteCrashed(crashed, targeting.finalCrashPos);
		targeting.isDescending = false;
		if (ActiveDescending == this)
		{
			ActiveDescending = null;
		}
		CleanupPreventBuildingVolume(crashed);
		SendNetworkUpdateImmediate();
	}

	protected ControlState GetControlState(bool isDescending)
	{
		if (HasFlag(Flags.Reserved9))
		{
			return ControlState.Controlling;
		}
		if (HasFlag(Flags.Reserved10))
		{
			if (!isDescending)
			{
				return ControlState.Cooldown;
			}
			return ControlState.Descending;
		}
		if (HasFlag(Flags.Reserved8))
		{
			return ControlState.Ready;
		}
		return ControlState.Offline;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.satelliteControlComputer != null)
		{
			ProtoBuf.SatelliteControlComputer satelliteControlComputer = info.msg.satelliteControlComputer;
			targeting = CrashTargeting.FromProto(satelliteControlComputer, UnityEngine.Time.time);
			if (targeting.isDescending)
			{
				ActiveDescending = this;
			}
			controlPhaseEndTime = ((satelliteControlComputer.controlRemaining > 0f) ? (UnityEngine.Time.time + satelliteControlComputer.controlRemaining) : 0f);
			selectedSatelliteIndex = satelliteControlComputer.selectedSatelliteIndex;
			fuelRemaining = satelliteControlComputer.fuelRemaining;
			satelliteSeed = satelliteControlComputer.satelliteSeed;
			resolvedPowerCost.Clear();
			if (satelliteControlComputer.resolvedPowerCost != null)
			{
				resolvedPowerCost.AddRange(satelliteControlComputer.resolvedPowerCost);
			}
		}
	}

	[Menu.ShowIf("Menu_PowerUp_ShowIf")]
	[Menu.Icon("power")]
	[Menu.Description("satcomp.powerup_desc", "Power up the satellite terminal")]
	[Menu("satcomp.powerup", "Power Up Terminal")]
	public void Menu_PowerUp(BasePlayer player)
	{
	}

	public bool Menu_PowerUp_ShowIf(BasePlayer player)
	{
		return IsOffline;
	}

	[Menu.ShowIf("Menu_LoadFuel_ShowIf")]
	[Menu.Icon("open")]
	[Menu("satcomp.loadfuel", "Open Inventory", Order = 10)]
	[Menu.Description("satcomp.loadfuel_desc", "Open the terminal's storage")]
	public void Menu_LoadFuel(BasePlayer player)
	{
	}

	public bool Menu_LoadFuel_ShowIf(BasePlayer player)
	{
		return true;
	}
}
