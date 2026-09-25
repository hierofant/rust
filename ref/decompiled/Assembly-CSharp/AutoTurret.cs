#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Linq;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Network;
using Network.Visibility;
using Oxide.Core;
using ProtoBuf;
using Rust;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

public class AutoTurret : ContainerIOEntity, IRemoteControllable, IHostileWarningEntity, IAdminUpdatableIdentifier
{
	public class UpdateAutoTurretScanQueue : PersistentObjectWorkQueue<AutoTurret>
	{
		protected override void RunJob(AutoTurret entity)
		{
			if (ShouldAdd(entity))
			{
				entity.TargetScan();
			}
		}

		protected override bool ShouldAdd(AutoTurret entity)
		{
			if (base.ShouldAdd(entity))
			{
				return entity.IsValid();
			}
			return false;
		}
	}

	public class UpdateAutoTurretAmmoQueue : ObjectWorkQueue<AutoTurret>
	{
		protected override void RunJob(AutoTurret entity)
		{
			if (ShouldAdd(entity))
			{
				entity.UpdateTotalAmmo();
			}
		}

		protected override bool ShouldAdd(AutoTurret entity)
		{
			if (base.ShouldAdd(entity))
			{
				return entity.IsValid();
			}
			return false;
		}
	}

	public class UpdateAutoTurretTick : ObjectWorkQueue<AutoTurret>
	{
		protected override void RunJob(AutoTurret entity)
		{
			if (ShouldAdd(entity))
			{
				entity.RunScheduledTick();
			}
		}

		protected override bool ShouldAdd(AutoTurret entity)
		{
			if (base.ShouldAdd(entity))
			{
				return entity.IsValid();
			}
			return false;
		}
	}

	private enum YawPitchMode
	{
		Separate,
		Merged
	}

	public struct AutoTurretPreserveInfo
	{
		public List<ulong> authedPlayers;

		public bool isPeacekeeper;

		public string rcIdentifier;
	}

	public GameObjectRef gun_fire_effect;

	public GameObjectRef bulletEffect;

	public float bulletSpeed = 200f;

	public AmbienceEmitter ambienceEmitter;

	public bool playAmbientSounds = true;

	public GameObject assignDialog;

	public LaserBeam laserBeam;

	public static int GlobalPowerCounter = 1;

	[NonSerialized]
	public int PowerOrder;

	public HashSet<AutoTurret> nearbyTurrets = new HashSet<AutoTurret>();

	private HashSet<AutoTurret> interferringTurrets = new HashSet<AutoTurret>();

	[ServerVar(Help = "How many milliseconds to spend on target scanning per frame")]
	public static float scan_budget_ms = 0.5f;

	[ServerVar(Help = "How many milliseconds to spend on ammo updating per frame")]
	public static float ammo_update_ms = 0.1f;

	[ServerVar(Help = "How many milliseconds to spend on a tick per frame")]
	public static float tick_update_ms = 1f;

	public static UpdateAutoTurretScanQueue updateAutoTurretScanQueue = new UpdateAutoTurretScanQueue();

	public static UpdateAutoTurretAmmoQueue updateAutoTurretAmmoQueue = new UpdateAutoTurretAmmoQueue();

	public static UpdateAutoTurretTick updateTurretTick = new UpdateAutoTurretTick();

	[Header("RC")]
	public float rcTurnSensitivity = 4f;

	public Transform RCEyes;

	public GameObjectRef IDPanelPrefab;

	public RemoteControllableControls rcControls;

	public string rcIdentifier = "";

	public TargetTrigger targetTrigger;

	public TriggerBase interferenceTrigger;

	public float maxInterference = -1f;

	public float attachedWeaponZOffsetScale = -0.5f;

	public Transform socketTransform;

	public const float ServerTickRateSeconds = 0.2f;

	public bool authDirty;

	public double nextVisCheck;

	public double lastTargetSeenTime;

	private Vector3 lastTargetAimOffset = Vector3.zero;

	private double lastDamageEventTime;

	private double lastScanTime;

	public bool targetVisible = true;

	public bool booting;

	public Vector3 targetAimDir = Vector3.forward;

	private int currentBurstShotsFired;

	public const float bulletDamage = 15f;

	public RealTimeSinceEx timeSinceLastServerTick;

	public static HashSet<AutoTurret> interferenceUpdateList = new HashSet<AutoTurret>();

	private const float SlowProjectileSpeedMultplier = 2f;

	private const float SlowProjectileSpeedThreshold = 100f;

	protected Transform cachedTransf;

	private YawPitchMode rotateMode;

	private Matrix4x4 toYawFromRoot;

	private Matrix4x4 toPitchFromRootOrYaw;

	private Matrix4x4 toRCEyesFromPitch;

	private Quaternion gunAimInitialYawRot;

	private Quaternion gunAimInitialPitchOrTotalRot;

	private Quaternion gunAimYawRotLS;

	private Quaternion gunAimPitchOrTotalRotLS;

	private Quaternion gunAimTotalRotWS;

	private Action _actionSetOnline;

	private Action _actionServerThink;

	private Action _actionServerDo;

	private Action _actionSendAimDir;

	private Action _actionUpdateAttachedWeapon;

	public Vector3 lastSentAimDir = Vector3.forward;

	public static float[] visibilityOffsets = new float[3] { 0f, 0.15f, -0.15f };

	public int peekIndex;

	[NonSerialized]
	public int numConsecutiveMisses;

	private double nextIdleAimTime;

	[NonSerialized]
	public int totalAmmo;

	public double nextAmmoCheckTime;

	public bool totalAmmoDirty = true;

	public float currentAmmoGravity;

	public float currentAmmoVelocity;

	public HeldEntity AttachedWeapon;

	private bool shouldUpdateOnOutOfAmmo;

	private float lastTickTime;

	public BaseCombatEntity target;

	public Transform eyePos;

	public Transform muzzlePos;

	public Vector3 aimDir;

	public Transform gun_yaw;

	public Transform gun_pitch;

	public float sightRange = 30f;

	public SoundDefinition turnLoopDef;

	public SoundDefinition movementChangeDef;

	public SoundDefinition ambientLoopDef;

	public SoundDefinition focusCameraDef;

	public float focusSoundFreqMin = 2.5f;

	public float focusSoundFreqMax = 7f;

	public GameObjectRef peacekeeperToggleSound;

	public GameObjectRef onlineSound;

	public GameObjectRef offlineSound;

	public GameObjectRef targetAcquiredEffect;

	public GameObjectRef targetLostEffect;

	public GameObjectRef reloadEffect;

	public float aimCone;

	public const Flags Flag_Peacekeeper = Flags.Reserved1;

	public const Flags Flag_Equipped = Flags.Reserved3;

	public const Flags Flag_MaxAuths = Flags.Reserved4;

	public const Flags Flag_ShowAlphaCover = Flags.Reserved5;

	[NonSerialized]
	public HashSet<ulong> authorizedPlayers = new HashSet<ulong>();

	[NonSerialized]
	public int consumptionAmount = 10;

	public bool CanPing => false;

	private int interferenceCount => interferringTurrets.Count;

	public virtual bool RequiresMouse => true;

	public float MaxRange => 10000f;

	public RemoteControllableControls RequiredControls => rcControls;

	public int ViewerCount { get; set; }

	public CameraViewerId? ControllingViewerId { get; set; }

	public bool IsBeingControlled
	{
		get
		{
			if (ViewerCount > 0)
			{
				return ControllingViewerId.HasValue;
			}
			return false;
		}
	}

	public double nextShotTime { get; private set; }

	public double lastShotTime { get; private set; }

	private Action actionSetOnline => SetOnline;

	private Action actionServerThink => ServerThink;

	private Action actionServerDo => ServerDo;

	private Action actionSendAimDir => SendAimDir;

	private Action actionUpdateAttachedWeapon => UpdateAttachedWeapon;

	protected override bool PreventDuplicatesInQueue
	{
		public get
		{
			return Sentry.debugPreventDuplicates;
		}
	}

	public bool IsServer => base.isServer;

	public bool IsClient => base.isClient;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("AutoTurret.OnRpcMessage"))
		{
			if (rpc == 1092560690 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - AddSelfAuthorize");
				}
				using (TimeWarning.New("AddSelfAuthorize"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1092560690u, "AddSelfAuthorize", this, player, 3f))
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
							RPCMessage rpc2 = rPCMessage;
							AddSelfAuthorize(rpc2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in AddSelfAuthorize");
					}
				}
				return true;
			}
			if (rpc == 3057055788u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - AssignToFriend");
				}
				using (TimeWarning.New("AssignToFriend"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3057055788u, "AssignToFriend", this, player, 3f))
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
							AssignToFriend(msg2);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in AssignToFriend");
					}
				}
				return true;
			}
			if (rpc == 253307592 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - ClearList");
				}
				using (TimeWarning.New("ClearList"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(253307592u, "ClearList", this, player, 3f))
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
							RPCMessage rpc3 = rPCMessage;
							ClearList(rpc3);
						}
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
						player.Kick("RPC Error in ClearList");
					}
				}
				return true;
			}
			if (rpc == 1500257773 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - FlipAim");
				}
				using (TimeWarning.New("FlipAim"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1500257773u, "FlipAim", this, player, 3f))
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
							RPCMessage rpc4 = rPCMessage;
							FlipAim(rpc4);
						}
					}
					catch (Exception exception4)
					{
						Debug.LogException(exception4);
						player.Kick("RPC Error in FlipAim");
					}
				}
				return true;
			}
			if (rpc == 3617985969u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RemoveSelfAuthorize");
				}
				using (TimeWarning.New("RemoveSelfAuthorize"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3617985969u, "RemoveSelfAuthorize", this, player, 3f))
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
							RPCMessage rpc5 = rPCMessage;
							RemoveSelfAuthorize(rpc5);
						}
					}
					catch (Exception exception5)
					{
						Debug.LogException(exception5);
						player.Kick("RPC Error in RemoveSelfAuthorize");
					}
				}
				return true;
			}
			if (rpc == 2025588587 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - Server_AdminUpdateIdentifier");
				}
				using (TimeWarning.New("Server_AdminUpdateIdentifier"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(2025588587u, "Server_AdminUpdateIdentifier", this, player, 3f))
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
							Server_AdminUpdateIdentifier(msg3);
						}
					}
					catch (Exception exception6)
					{
						Debug.LogException(exception6);
						player.Kick("RPC Error in Server_AdminUpdateIdentifier");
					}
				}
				return true;
			}
			if (rpc == 1770263114 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - SERVER_AttackAll");
				}
				using (TimeWarning.New("SERVER_AttackAll"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1770263114u, "SERVER_AttackAll", this, player, 3f))
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
							RPCMessage rpc6 = rPCMessage;
							SERVER_AttackAll(rpc6);
						}
					}
					catch (Exception exception7)
					{
						Debug.LogException(exception7);
						player.Kick("RPC Error in SERVER_AttackAll");
					}
				}
				return true;
			}
			if (rpc == 3265538831u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - SERVER_Peacekeeper");
				}
				using (TimeWarning.New("SERVER_Peacekeeper"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3265538831u, "SERVER_Peacekeeper", this, player, 3f))
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
							RPCMessage rpc7 = rPCMessage;
							SERVER_Peacekeeper(rpc7);
						}
					}
					catch (Exception exception8)
					{
						Debug.LogException(exception8);
						player.Kick("RPC Error in SERVER_Peacekeeper");
					}
				}
				return true;
			}
			if (rpc == 1677685895 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - SERVER_RequestOpenRCPanel");
				}
				using (TimeWarning.New("SERVER_RequestOpenRCPanel"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1677685895u, "SERVER_RequestOpenRCPanel", this, player, 3uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(1677685895u, "SERVER_RequestOpenRCPanel", this, player, 3f))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(1677685895u, "SERVER_RequestOpenRCPanel", this, player, 3f))
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
							SERVER_RequestOpenRCPanel(msg4);
						}
					}
					catch (Exception exception9)
					{
						Debug.LogException(exception9);
						player.Kick("RPC Error in SERVER_RequestOpenRCPanel");
					}
				}
				return true;
			}
			if (rpc == 1053317251 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - Server_SetID");
				}
				using (TimeWarning.New("Server_SetID"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(1053317251u, "Server_SetID", this, player, 3f))
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
							Server_SetID(msg5);
						}
					}
					catch (Exception exception10)
					{
						Debug.LogException(exception10);
						player.Kick("RPC Error in Server_SetID");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool PeacekeeperMode()
	{
		return HasFlag(Flags.Reserved1);
	}

	protected virtual bool ShouldApplyInterference()
	{
		return true;
	}

	private void ServerInit_Interference()
	{
		if (!Rust.Application.isLoadingSave)
		{
			AddNearbyTurrets();
		}
	}

	private void ServerDestroy_Interference()
	{
		RemoveFromNeabyTurrets();
	}

	private void PostServerLoad_Interference()
	{
		AddNearbyTurrets();
	}

	private void OnTurretPowerChanged(bool online)
	{
		if (ShouldApplyInterference())
		{
			if (online)
			{
				PowerOrder = GlobalPowerCounter++;
				RecalculateInterference();
			}
			else
			{
				OnTurretDisabled();
				SetInterferenceEnabled(state: false);
			}
		}
	}

	private void Save_Interference(ProtoBuf.AutoTurret data)
	{
		data.powerOrder = PowerOrder;
		data.interferenceList = Facepunch.Pool.Get<List<NetworkableId>>();
		foreach (AutoTurret interferringTurret in interferringTurrets)
		{
			if (interferringTurret.IsValid())
			{
				data.interferenceList.Add(interferringTurret.net.ID);
			}
		}
	}

	private void Load_Interference(ProtoBuf.AutoTurret data)
	{
		PowerOrder = data.powerOrder;
		if (PowerOrder > GlobalPowerCounter)
		{
			GlobalPowerCounter = PowerOrder;
		}
		interferringTurrets.Clear();
		if (data.interferenceList == null)
		{
			return;
		}
		foreach (NetworkableId interference in data.interferenceList)
		{
			AutoTurret autoTurret = BaseNetworkable.serverEntities.Find(interference) as AutoTurret;
			if (autoTurret != null && autoTurret != this && !autoTurret.IsDestroyed)
			{
				interferringTurrets.Add(autoTurret);
			}
		}
	}

	private void AddNearbyTurrets()
	{
		nearbyTurrets.Clear();
		List<AutoTurret> obj = Facepunch.Pool.Get<List<AutoTurret>>();
		if (Interface.CallHook("OnNearbyTurretsScan", this, obj) == null)
		{
			Vis.Entities(base.transform.position, Sentry.interferenceradius, obj, 256, QueryTriggerInteraction.Ignore);
		}
		foreach (AutoTurret item in obj)
		{
			if (item.IsServer && !(item == this) && !item.IsDestroyed)
			{
				item.nearbyTurrets.Add(this);
				nearbyTurrets.Add(item);
			}
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	private void RemoveFromNeabyTurrets()
	{
		foreach (AutoTurret nearbyTurret in nearbyTurrets)
		{
			nearbyTurret.nearbyTurrets.Remove(this);
		}
		nearbyTurrets.Clear();
	}

	private bool ShouldTurretOverload(out int estimatedInterference)
	{
		estimatedInterference = 0;
		foreach (AutoTurret nearbyTurret in nearbyTurrets)
		{
			if (nearbyTurret.IsOn() && !nearbyTurret.HasInterference())
			{
				if (nearbyTurret.interferringTurrets.Count + 1 == Sentry.maxinterference)
				{
					return true;
				}
				estimatedInterference++;
				if (estimatedInterference >= Sentry.maxinterference)
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool RecalculateInterference()
	{
		object obj = Interface.CallHook("OnInterferenceUpdate", this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		bool flag = HasInterference();
		if (ShouldTurretOverload(out var _))
		{
			SetInterferenceEnabled(state: true);
			return flag != HasInterference();
		}
		SetInterferenceEnabled(state: false);
		foreach (AutoTurret nearbyTurret in nearbyTurrets)
		{
			if (nearbyTurret.IsOn() && !nearbyTurret.HasInterference())
			{
				nearbyTurret.interferringTurrets.Add(this);
				interferringTurrets.Add(nearbyTurret);
			}
		}
		return flag != HasInterference();
	}

	private void OnTurretDisabled()
	{
		if (HasInterference())
		{
			return;
		}
		interferringTurrets.Clear();
		List<AutoTurret> obj = Facepunch.Pool.Get<List<AutoTurret>>();
		foreach (AutoTurret nearbyTurret in nearbyTurrets)
		{
			nearbyTurret.interferringTurrets.Remove(this);
			if (nearbyTurret.HasInterference() && nearbyTurret.interferenceCount < Sentry.maxinterference)
			{
				obj.Add(nearbyTurret);
			}
		}
		SortTurretsByInterferenceLevel(obj);
		foreach (AutoTurret item in obj)
		{
			item.SetInterferenceEnabled(state: false);
			item.RecalculateInterference();
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	private void ClearInterference()
	{
		foreach (AutoTurret interferringTurret in interferringTurrets)
		{
			interferringTurret.interferringTurrets.Remove(this);
		}
		interferringTurrets.Clear();
	}

	private void SortTurretsByPowerOnTime(List<AutoTurret> turrets)
	{
		turrets.Sort((AutoTurret a, AutoTurret b) => a.PowerOrder.CompareTo(b.PowerOrder));
	}

	private void SortTurretsByInterferenceLevel(List<AutoTurret> turrets)
	{
		List<AutoTurret> obj = Facepunch.Pool.Get<List<AutoTurret>>();
		obj.AddRange(turrets.OrderBy((AutoTurret x) => x.nearbyTurrets.Count((AutoTurret y) => y.IsOn() && !y.HasInterference())));
		turrets.Clear();
		turrets.AddRange(obj);
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	private void SetInterferenceEnabled(bool state)
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.OnFire, state);
	}

	public bool HasInterference()
	{
		if (ShouldApplyInterference())
		{
			return IsOnFire();
		}
		return false;
	}

	public Transform GetEyes()
	{
		return RCEyes;
	}

	public float GetFovScale()
	{
		return 1f;
	}

	public BaseEntity GetEnt()
	{
		return this;
	}

	public virtual bool CanControl(ulong playerID)
	{
		object obj = Interface.CallHook("OnEntityControl", this, playerID);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (booting)
		{
			return false;
		}
		if (IsPowered())
		{
			return !PeacekeeperMode();
		}
		return false;
	}

	public bool InitializeControl(CameraViewerId viewerID)
	{
		ViewerCount++;
		if (!ControllingViewerId.HasValue)
		{
			ControllingViewerId = viewerID;
			SetTarget(null);
			SendAimDirImmediate();
			return true;
		}
		return false;
	}

	public void StopControl(CameraViewerId viewerID)
	{
		ViewerCount--;
		if (ControllingViewerId == viewerID)
		{
			ControllingViewerId = null;
		}
	}

	public Matrix4x4 GetEyesMatrix()
	{
		return GetCenterMuzzle() * toRCEyesFromPitch;
	}

	public void UserInput(InputState inputState, CameraViewerId viewerID)
	{
		CameraViewerId? controllingViewerId = ControllingViewerId;
		if (viewerID != controllingViewerId)
		{
			return;
		}
		UpdateManualAim(inputState);
		double timeAsDouble = UnityEngine.Time.timeAsDouble;
		if (timeAsDouble < nextShotTime)
		{
			return;
		}
		if (inputState.WasJustPressed(BUTTON.RELOAD))
		{
			Reload();
		}
		else
		{
			if (EnsureReloaded())
			{
				return;
			}
			bool num = inputState.IsDown(BUTTON.FIRE_PRIMARY);
			BaseProjectile baseProjectile;
			bool flag = TryGetAttachedWeapon(out baseProjectile);
			bool flag2 = flag && IsUsingBurstFireWeapon(baseProjectile);
			int num2 = (flag2 ? baseProjectile.GetBurstModeCount() : 0);
			bool flag3 = flag2 && IsBursting(baseProjectile);
			if (num || flag3)
			{
				if (flag)
				{
					if (baseProjectile is ITurretNotify turretNotify)
					{
						turretNotify.WarmupTick(wantsShoot: true);
					}
					float damageModifier = 1f;
					float speedModifier = 1f;
					ItemDefinition ammoType = baseProjectile.primaryMagazine.ammoType;
					if ((bool)ammoType)
					{
						ItemModProjectile component = ammoType.GetComponent<ItemModProjectile>();
						if ((bool)component && component.projectileVelocity < 100f)
						{
							speedModifier = 2f;
						}
					}
					if (baseProjectile.primaryMagazine.contents > 0)
					{
						if (!flag2 || currentBurstShotsFired < num2)
						{
							FireAttachedGun(Vector3.zero, aimCone, null, damageModifier, speedModifier);
						}
						float num3;
						if (flag2)
						{
							currentBurstShotsFired++;
							if (currentBurstShotsFired < num2 && baseProjectile.primaryMagazine.contents > 0)
							{
								num3 = Mathf.Max(baseProjectile.ScaleRepeatDelay(baseProjectile.repeatDelay), baseProjectile.NextAttackTime - (float)timeAsDouble);
							}
							else
							{
								ResetBurstFireState();
								num3 = Mathf.Max(baseProjectile.TimeBetweenBursts(), Player.clientTickInterval.Get() * 2f);
							}
						}
						else
						{
							ResetBurstFireState();
							num3 = (baseProjectile.isSemiAuto ? (baseProjectile.repeatDelay * 1.5f) : baseProjectile.repeatDelay);
							num3 = baseProjectile.ScaleRepeatDelay(num3);
						}
						nextShotTime = timeAsDouble + (double)num3;
						if ((float)nextShotTime < baseProjectile.NextAttackTime && !Mathf.Approximately((float)nextShotTime, baseProjectile.NextAttackTime))
						{
							Debug.LogWarning($"Turret {base.name} next shot scheduled in {num3}s will be skipped due to it being less than attached {baseProjectile.name} attack cooldown ({baseProjectile.NextAttackTime - (float)timeAsDouble}s).", this);
						}
					}
					else
					{
						ResetBurstFireState();
						nextShotTime = timeAsDouble + 5.0;
					}
				}
				else if (HasGenericFireable())
				{
					AttachedWeapon.ServerUse();
					nextShotTime = timeAsDouble + 0.11500000208616257;
				}
				else
				{
					nextShotTime = timeAsDouble + 1.0;
				}
			}
			else if ((bool)baseProjectile && baseProjectile is ITurretNotify turretNotify2)
			{
				turretNotify2.WarmupTick(wantsShoot: false);
			}
		}
	}

	public bool UpdateManualAim(InputState inputState)
	{
		float x = (0f - inputState.current.mouseDelta.y) * rcTurnSensitivity;
		float y = inputState.current.mouseDelta.x * rcTurnSensitivity;
		Vector3 euler = Quaternion.LookRotation(aimDir, base.transform.up).eulerAngles + new Vector3(x, y, 0f);
		if (euler.x >= 0f && euler.x <= 135f)
		{
			euler.x = Mathf.Clamp(euler.x, 0f, 45f);
		}
		if (euler.x >= 225f && euler.x <= 360f)
		{
			euler.x = Mathf.Clamp(euler.x, 285f, 360f);
		}
		Vector3 vector = Quaternion.Euler(euler) * Vector3.forward;
		bool result = !Mathf.Approximately(aimDir.x, vector.x) || !Mathf.Approximately(aimDir.y, vector.y) || !Mathf.Approximately(aimDir.z, vector.z);
		aimDir = vector;
		return result;
	}

	public override void InitShared()
	{
		base.InitShared();
		RCSetup();
	}

	public override void DestroyShared()
	{
		RCShutdown();
		base.DestroyShared();
	}

	public void RCSetup()
	{
		if (base.isServer)
		{
			RemoteControlEntity.InstallControllable(this);
		}
	}

	public void RCShutdown()
	{
		if (base.isServer)
		{
			RemoteControlEntity.RemoveControllable(this);
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void Server_SetID(RPCMessage msg)
	{
		string oldID = msg.read.String();
		string newID = msg.read.String();
		SetID(msg.player, oldID, newID);
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void Server_AdminUpdateIdentifier(RPCMessage msg)
	{
		if (!(msg.player == null) && (msg.player.IsAdmin || msg.player.IsDeveloper))
		{
			string oldID = msg.read.String();
			string newID = msg.read.String();
			SetID(msg.player, oldID, newID, bypassChecks: true);
		}
	}

	public void SetID(BasePlayer player, string oldID, string newID, bool bypassChecks = false)
	{
		if ((CanChangeID(player) || bypassChecks) && (string.IsNullOrEmpty(oldID) || ComputerStation.IsValidIdentifier(oldID)) && ComputerStation.IsValidIdentifier(newID) && oldID == GetIdentifier() && Interface.CallHook("OnTurretIdentifierSet", this, player, newID) == null)
		{
			Debug.Log("SetID success!");
			UpdateIdentifier(newID);
		}
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(3uL)]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.MaxDistance(3f)]
	public void SERVER_RequestOpenRCPanel(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!(player == null) && CanChangeID(player))
		{
			ClientRPC(RpcTarget.Player("CLIENT_OpenRCPanel", player), GetIdentifier());
		}
	}

	public void UpdateIdentifier(string newID, bool clientSend = false)
	{
		_ = rcIdentifier;
		if (base.isServer)
		{
			if (!RemoteControlEntity.IDInUse(newID))
			{
				rcIdentifier = newID;
			}
			SendNetworkUpdate();
		}
	}

	public string GetIdentifier()
	{
		return rcIdentifier;
	}

	public virtual bool CanChangeID(BasePlayer player)
	{
		if (player != null)
		{
			return CanChangeSettings(player);
		}
		return false;
	}

	public override int ConsumptionAmount()
	{
		return consumptionAmount;
	}

	public void SetOnline()
	{
		SetIsOnline(online: true);
	}

	public void SetIsOnline(bool online)
	{
		BaseProjectile attachedWeapon = GetAttachedWeapon();
		if ((bool)attachedWeapon && attachedWeapon is ITurretNotify turretNotify)
		{
			turretNotify.OnAddedRemovedToTurret(online);
		}
		if (online != IsOn() && Interface.CallHook("OnTurretToggle", this) == null)
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.On, online);
			}
			OnTurretPowerChanged(online);
			booting = false;
			BaseProjectile attachedWeapon2 = GetAttachedWeapon();
			if ((bool)attachedWeapon2)
			{
				attachedWeapon2.SetLightsOn(online);
			}
			SendNetworkUpdate();
			if (IsOffline())
			{
				ResetBurstFireState();
				SetTarget(null);
				isLootable = true;
			}
			else
			{
				isLootable = false;
				authDirty = true;
			}
		}
	}

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		int result = Mathf.Min(1, GetCurrentEnergy());
		switch (outputSlot)
		{
		case 0:
			if (!HasTarget())
			{
				return 0;
			}
			return result;
		case 1:
			if (totalAmmo > 50)
			{
				return 0;
			}
			return result;
		case 2:
			if (totalAmmo != 0)
			{
				return 0;
			}
			return result;
		default:
			return 0;
		}
	}

	public override void IOStateChanged(int inputAmount, int inputSlot)
	{
		base.IOStateChanged(inputAmount, inputSlot);
		if (IsPowered() && !IsOn())
		{
			InitiateStartup();
		}
		else if ((!IsPowered() && IsOn()) || booting)
		{
			InitiateShutdown();
		}
	}

	public void InitiateShutdown()
	{
		if ((!IsOffline() || booting) && Interface.CallHook("OnTurretShutdown", this) == null)
		{
			CancelInvoke(actionSetOnline);
			booting = false;
			Effect.server.Run(offlineSound.resourcePath, this, 0u, Vector3.zero, Vector3.zero);
			SetIsOnline(online: false);
		}
	}

	public void InitiateStartup()
	{
		if (!IsOnline() && !booting && Interface.CallHook("OnTurretStartup", this) == null)
		{
			Effect.server.Run(onlineSound.resourcePath, this, 0u, Vector3.zero, Vector3.zero);
			Invoke(actionSetOnline, 2f);
			booting = true;
		}
	}

	public void SetPeacekeepermode(bool isOn)
	{
		if (PeacekeeperMode() != isOn)
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved1, isOn);
			}
			Effect.server.Run(peacekeeperToggleSound.resourcePath, this, 0u, Vector3.zero, Vector3.zero);
		}
	}

	public static bool IsValidWeapon(ItemDefinition itemDef)
	{
		ItemModEntity component = itemDef.GetComponent<ItemModEntity>();
		if (component == null)
		{
			return false;
		}
		HeldEntity component2 = component.entityPrefab.Get().GetComponent<HeldEntity>();
		if (component2 == null)
		{
			return false;
		}
		if (!component2.IsUsableByTurret)
		{
			return false;
		}
		return true;
	}

	public bool IsValidWeapon(Item item)
	{
		if (item.isBroken)
		{
			return false;
		}
		return IsValidWeapon(item.info);
	}

	public bool CanAcceptItem(BasePlayer player, Item item, int targetSlot)
	{
		Item slot = base.inventory.GetSlot(0);
		if (IsValidWeapon(item) && targetSlot == 0)
		{
			return true;
		}
		if (item.info.category == ItemCategory.Ammunition)
		{
			ItemModProjectile component = item.info.GetComponent<ItemModProjectile>();
			BaseProjectile attachedWeapon = GetAttachedWeapon();
			if (slot == null || attachedWeapon == null || component == null)
			{
				return false;
			}
			if ((attachedWeapon.primaryMagazine.definition.ammoTypes & component.ammoType) == 0)
			{
				return false;
			}
			if (targetSlot == 0)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public bool AtMaxAuthCapacity()
	{
		return HasFlag(Flags.Reserved4);
	}

	public void UpdateMaxAuthCapacity()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		if (authorizedPlayers.Count >= 200)
		{
			flagsUpdateScope.Set(Flags.Reserved4, b: true);
			return;
		}
		BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(serverside: true);
		bool b = activeGameMode != null && activeGameMode.limitTeamAuths && authorizedPlayers.Count >= activeGameMode.GetMaxRelationshipTeamSize();
		flagsUpdateScope.Set(Flags.Reserved4, b);
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void FlipAim(RPCMessage rpc)
	{
		if (!IsOnline() && IsAuthed(rpc.player) && !booting && Interface.CallHook("OnTurretRotate", this, rpc.player) == null)
		{
			base.transform.rotation = Quaternion.LookRotation(-base.transform.forward, base.transform.up);
			SendNetworkUpdate();
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void AddSelfAuthorize(RPCMessage rpc)
	{
		AddSelfAuthorize(rpc.player);
	}

	public void AddSelfAuthorize(BasePlayer player)
	{
		if (!IsOnline() && player.CanBuild() && !AtMaxAuthCapacity() && Interface.CallHook("OnTurretAuthorize", this, player) == null)
		{
			authorizedPlayers.Add(player.userID);
			Facepunch.Rust.Analytics.Azure.OnEntityAuthChanged(this, player, authorizedPlayers, "added", player.userID);
			UpdateMaxAuthCapacity();
			SendNetworkUpdate();
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	private void RemoveSelfAuthorize(RPCMessage rpc)
	{
		if (!booting && !IsOnline() && IsAuthed(rpc.player) && Interface.CallHook("OnTurretDeauthorize", this, rpc.player) == null)
		{
			authorizedPlayers.Remove(rpc.player.userID);
			authDirty = true;
			Facepunch.Rust.Analytics.Azure.OnEntityAuthChanged(this, rpc.player, authorizedPlayers, "removed", rpc.player.userID);
			UpdateMaxAuthCapacity();
			SendNetworkUpdate();
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	private void ClearList(RPCMessage rpc)
	{
		BasePlayer player = rpc.player;
		if (!(player == null) && !booting && !IsOnline() && player.CanBuild() && Interface.CallHook("OnTurretClearList", this, rpc.player) == null)
		{
			authorizedPlayers.Clear();
			authDirty = true;
			Facepunch.Rust.Analytics.Azure.OnEntityAuthChanged(this, rpc.player, authorizedPlayers, "clear", rpc.player.userID);
			UpdateMaxAuthCapacity();
			SendNetworkUpdate();
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void AssignToFriend(RPCMessage msg)
	{
		if (!AtMaxAuthCapacity() && !(msg.player == null) && msg.player.CanInteract() && CanChangeSettings(msg.player))
		{
			ulong num = msg.read.UInt64();
			if (num != 0L && !IsAuthed(num) && Interface.CallHook("OnTurretAssign", this, num, msg.player) == null)
			{
				Facepunch.Rust.Analytics.Azure.OnEntityAuthChanged(this, msg.player, authorizedPlayers, "added", num);
				authorizedPlayers.Add(num);
				UpdateMaxAuthCapacity();
				SendNetworkUpdate();
				Interface.CallHook("OnTurretAssigned", this, num, msg.player);
			}
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	private void SERVER_Peacekeeper(RPCMessage rpc)
	{
		if (IsAuthed(rpc.player) && Interface.CallHook("OnTurretModeToggle", this, rpc.player) == null)
		{
			SetPeacekeepermode(isOn: true);
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	private void SERVER_AttackAll(RPCMessage rpc)
	{
		if (IsAuthed(rpc.player) && Interface.CallHook("OnTurretModeToggle", this, rpc.player) == null)
		{
			SetPeacekeepermode(isOn: false);
		}
	}

	public virtual float TargetScanRate()
	{
		return 1f;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		ItemContainer itemContainer = base.inventory;
		itemContainer.canAcceptItem = (Func<BasePlayer, Item, int, bool>)Delegate.Combine(itemContainer.canAcceptItem, new Func<BasePlayer, Item, int, bool>(CanAcceptItem));
		if (targetTrigger != null)
		{
			TargetTrigger obj = targetTrigger;
			obj.OnEntityEnterTrigger = (Action<BaseNetworkable>)Delegate.Combine(obj.OnEntityEnterTrigger, new Action<BaseNetworkable>(OnEntityEnterTrigger));
			targetTrigger.GetComponent<SphereCollider>().radius = sightRange;
		}
		InvokeRepeating(actionServerThink, UnityEngine.Random.Range(0f, 1f), 0.2f);
		InvokeRandomized(actionServerDo, UnityEngine.Random.Range(0f, 1f), 0.03f, 0.05f);
		InvokeRandomized(actionSendAimDir, UnityEngine.Random.Range(0f, 1f), 0.2f, 0.05f);
		updateAutoTurretScanQueue.Add(this);
		if (ShouldApplyInterference())
		{
			ServerInit_Interference();
		}
		cachedTransf = base.transform;
		rotateMode = ((gun_pitch.localPosition.x == 0f && gun_pitch.localPosition.z == 0f) ? YawPitchMode.Merged : YawPitchMode.Separate);
		if (rotateMode == YawPitchMode.Merged)
		{
			toPitchFromRootOrYaw = cachedTransf.worldToLocalMatrix * gun_pitch.localToWorldMatrix;
			gunAimInitialPitchOrTotalRot = toPitchFromRootOrYaw.rotation;
		}
		else
		{
			toYawFromRoot = base.transform.root.worldToLocalMatrix * gun_yaw.localToWorldMatrix;
			gunAimInitialYawRot = toYawFromRoot.rotation;
			toPitchFromRootOrYaw = gun_yaw.worldToLocalMatrix * gun_pitch.localToWorldMatrix;
			gunAimInitialPitchOrTotalRot = toPitchFromRootOrYaw.rotation;
			gunAimTotalRotWS = gun_pitch.rotation;
		}
		if ((bool)RCEyes)
		{
			toRCEyesFromPitch = gun_pitch.worldToLocalMatrix * RCEyes.localToWorldMatrix;
		}
		if (ShouldApplyInterference())
		{
			ServerInit_Interference();
		}
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		updateAutoTurretScanQueue.Remove(this);
		if (ShouldApplyInterference())
		{
			ServerDestroy_Interference();
		}
	}

	public void OnEntityEnterTrigger(BaseNetworkable entity)
	{
		if (entity is BasePlayer player && !IsAuthed(player))
		{
			authDirty = true;
		}
	}

	public void SendAimDir()
	{
		if ((net == null || net.group == null || BaseNetworkable.HasConnections(net.group, base.transform.position)) && (HasTarget() || Vector3.Angle(lastSentAimDir, aimDir) > 1f))
		{
			SendAimDirImmediate();
		}
	}

	public void SendAimDirImmediate()
	{
		lastSentAimDir = aimDir;
		ClientRPC(RpcTarget.NetworkGroup("CLIENT_ReceiveAimDir"), aimDir);
	}

	public void SetTarget(BaseCombatEntity targ)
	{
		if (Interface.CallHook("OnTurretTarget", this, targ) != null)
		{
			return;
		}
		if (targ != target || targ.IsRealNull() != target.IsRealNull())
		{
			Effect.server.Run((targ == null) ? targetLostEffect.resourcePath : targetAcquiredEffect.resourcePath, base.transform.position, Vector3.up);
			if (outputs != null && outputs.Length != 0 && outputs[0].connectedTo.Get() != null)
			{
				MarkDirtyForceUpdateOutputs();
			}
			nextShotTime += 0.10000000149011612;
			authDirty = true;
		}
		target = targ;
		if (target.IsRealNull())
		{
			targetVisible = false;
			nextVisCheck = 0.0;
		}
		else
		{
			OnTargetSeen(target, UnityEngine.Time.realtimeSinceStartupAsDouble);
		}
	}

	private void OnTargetSeen(BaseCombatEntity seenTarget, double realtimeSinceStartupAsDouble)
	{
		lastTargetSeenTime = realtimeSinceStartupAsDouble;
		lastTargetAimOffset = AimOffset(seenTarget);
	}

	public virtual bool CheckPeekers()
	{
		return true;
	}

	public bool ObjectVisible(BaseCombatEntity obj)
	{
		object obj2 = Interface.CallHook("CanBeTargeted", obj, this);
		if (obj2 is bool)
		{
			return (bool)obj2;
		}
		Vector3 position = eyePos.transform.position;
		if (GamePhysics.CheckSphere(position, 0.1f, 136314880))
		{
			return false;
		}
		Vector3 vector = AimOffset(obj);
		float num = Vector3.Distance(vector, position);
		Vector3 vector2 = Vector3.Cross((vector - position).normalized, Vector3.up);
		if (num > sightRange)
		{
			return false;
		}
		List<RaycastHit> obj3 = Facepunch.Pool.Get<List<RaycastHit>>();
		int num2 = ((!(obj is BasePlayer)) ? 1 : 3);
		for (int i = 0; i < num2; i++)
		{
			Vector3 normalized = (vector + vector2 * visibilityOffsets[i] - position).normalized;
			obj3.Clear();
			GamePhysics.TraceAll(new Ray(position, normalized), 0f, obj3, num * 1.1f, 1218652417);
			for (int j = 0; j < obj3.Count; j++)
			{
				BaseEntity entity = RaycastHitEx.GetEntity(obj3[j]);
				if ((!(entity != null) || !entity.isClient) && (!(entity != null) || !(entity.ToPlayer() != null) || entity.EqualNetID(obj)) && (!(entity != null) || !entity.EqualNetID(this)))
				{
					if (entity != null && (entity == obj || entity.EqualNetID(obj)))
					{
						Facepunch.Pool.FreeUnmanaged(ref obj3);
						peekIndex = i;
						return true;
					}
					if (!(entity != null) || entity.ShouldBlockProjectiles())
					{
						break;
					}
				}
			}
		}
		Facepunch.Pool.FreeUnmanaged(ref obj3);
		return false;
	}

	public virtual void FireAttachedGun(Vector3 targetPos, float aimCone, BaseCombatEntity target = null, float damageModifier = 1f, float speedModifier = 1f)
	{
		if (TryGetAttachedWeapon(out var baseProjectile) && !IsOffline() && (!(baseProjectile is ITurretNotify turretNotify) || turretNotify.CanShoot()))
		{
			Matrix4x4 centerMuzzle = GetCenterMuzzle();
			if (IsBeingControlled)
			{
				centerMuzzle *= toRCEyesFromPitch;
			}
			baseProjectile.ServerUse(new HeldEntityServerUseParams(damageModifier, speedModifier, centerMuzzle, useBulletThickness: false));
		}
	}

	public virtual void FireGun(Vector3 targetPos, float aimCone, Transform muzzleToUse = null, BaseCombatEntity target = null)
	{
		if (IsOffline())
		{
			return;
		}
		if (muzzleToUse == null)
		{
			muzzleToUse = muzzlePos;
		}
		Matrix4x4 centerMuzzle = GetCenterMuzzle();
		Vector3 vector = centerMuzzle.MultiplyVector(Vector3.forward);
		Vector3 vector2 = centerMuzzle.GetPosition() - vector * 0.25f;
		Vector3 vector3 = vector;
		Vector3 modifiedAimConeDirection = AimConeUtil.GetModifiedAimConeDirection(aimCone, vector3);
		targetPos = vector2 + modifiedAimConeDirection * 300f;
		List<RaycastHit> obj = Facepunch.Pool.Get<List<RaycastHit>>();
		GamePhysics.TraceAll(new Ray(vector2, modifiedAimConeDirection), 0f, obj, 300f, 1220225809);
		bool flag = false;
		for (int i = 0; i < obj.Count; i++)
		{
			RaycastHit hit = obj[i];
			BaseEntity entity = RaycastHitEx.GetEntity(hit);
			if ((entity != null && (entity == this || entity.EqualNetID(this))) || (PeacekeeperMode() && target != null && entity != null && entity.GetComponent<BasePlayer>() != null && !entity.EqualNetID(target)))
			{
				continue;
			}
			BaseCombatEntity baseCombatEntity = entity as BaseCombatEntity;
			if (baseCombatEntity != null)
			{
				ApplyDamage(baseCombatEntity, hit.point, modifiedAimConeDirection);
				if (baseCombatEntity.EqualNetID(target))
				{
					flag = true;
				}
			}
			if (!(entity != null) || entity.ShouldBlockProjectiles())
			{
				targetPos = hit.point;
				vector3 = (targetPos - vector2).normalized;
				break;
			}
		}
		int num = 2;
		if (!flag)
		{
			numConsecutiveMisses++;
		}
		else
		{
			numConsecutiveMisses = 0;
		}
		if (target != null && targetVisible && numConsecutiveMisses > num)
		{
			ApplyDamage(target, target.transform.position - vector3 * 0.25f, vector3);
			numConsecutiveMisses = 0;
		}
		ClientRPC(RpcTarget.NetworkGroup("CLIENT_FireGun"), StringPool.Get(muzzleToUse.gameObject.name), targetPos);
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	public void ApplyDamage(BaseCombatEntity entity, Vector3 point, Vector3 normal)
	{
		float num = 15f * UnityEngine.Random.Range(0.9f, 1.1f);
		if (entity is BasePlayer && entity != target)
		{
			num *= 0.5f;
		}
		if (PeacekeeperMode() && entity == target)
		{
			target.MarkHostileFor(300f);
		}
		HitInfo obj = Facepunch.Pool.Get<HitInfo>();
		obj.Initiator = this;
		obj.HitEntity = entity;
		obj.damageTypes.Add(DamageType.Bullet, num);
		obj.HitPositionWorld = point;
		entity.OnAttacked(obj);
		if (entity is BasePlayer || entity is BaseNpc)
		{
			obj.HitNormalWorld = -normal;
			obj.HitMaterial = StringPool.Get("Flesh");
			Effect.server.ImpactEffect(obj);
		}
		Facepunch.Pool.Free(ref obj);
	}

	public void IdleTick(float dt)
	{
		BaseProjectile attachedWeapon = GetAttachedWeapon();
		if ((bool)attachedWeapon && attachedWeapon is ITurretNotify turretNotify)
		{
			turretNotify.WarmupTick(wantsShoot: false);
		}
		double realtimeSinceStartupAsDouble = UnityEngine.Time.realtimeSinceStartupAsDouble;
		if (realtimeSinceStartupAsDouble > nextIdleAimTime)
		{
			nextIdleAimTime = realtimeSinceStartupAsDouble + (double)UnityEngine.Random.Range(4f, 5f);
			Quaternion quaternion = Quaternion.LookRotation(base.transform.forward, Vector3.up);
			quaternion *= Quaternion.AngleAxis(UnityEngine.Random.Range(-45f, 45f), Vector3.up);
			targetAimDir = quaternion * Vector3.forward;
		}
		if (!HasTarget())
		{
			aimDir = Mathx.Lerp(aimDir, targetAimDir, 2f, dt);
		}
	}

	public virtual bool HasClipAmmo()
	{
		BaseProjectile attachedWeapon = GetAttachedWeapon();
		if (attachedWeapon == null)
		{
			return false;
		}
		return attachedWeapon.primaryMagazine.contents > 0;
	}

	public virtual bool HasReserveAmmo()
	{
		return totalAmmo > 0;
	}

	public int GetTotalAmmo()
	{
		int num = 0;
		BaseProjectile attachedWeapon = GetAttachedWeapon();
		if (attachedWeapon == null)
		{
			return num;
		}
		List<Item> ammos = Facepunch.Pool.Get<List<Item>>();
		base.inventory.FindAmmo(ammos, attachedWeapon.primaryMagazine.definition.ammoTypes);
		if (!attachedWeapon.primaryMagazine.allowAmmoSwitching)
		{
			BaseProjectile.StripAmmoToType(ref ammos, attachedWeapon.primaryMagazine.ammoType);
		}
		for (int i = 0; i < ammos.Count; i++)
		{
			num += ammos[i].amount;
		}
		Facepunch.Pool.Free(ref ammos, freeElements: false);
		return num;
	}

	public AmmoTypes GetValidAmmoTypes()
	{
		BaseProjectile attachedWeapon = GetAttachedWeapon();
		if (attachedWeapon == null)
		{
			return AmmoTypes.RIFLE_556MM;
		}
		return attachedWeapon.primaryMagazine.definition.ammoTypes;
	}

	public ItemDefinition GetDesiredAmmo()
	{
		BaseProjectile attachedWeapon = GetAttachedWeapon();
		if (attachedWeapon == null)
		{
			return null;
		}
		return attachedWeapon.primaryMagazine.ammoType;
	}

	public void Reload()
	{
		if (!TryGetAttachedWeapon(out var baseProjectile))
		{
			return;
		}
		ResetBurstFireState();
		_ = baseProjectile.primaryMagazine.ammoType;
		float turretReloadDuration = baseProjectile.GetTurretReloadDuration();
		nextShotTime = math.max(nextShotTime, UnityEngine.Time.timeAsDouble + (double)Mathf.Min(turretReloadDuration, 2f));
		AmmoTypes ammoTypes = baseProjectile.primaryMagazine.definition.ammoTypes;
		if (baseProjectile.primaryMagazine.contents > 0)
		{
			bool flag = false;
			if (base.inventory.capacity > base.inventory.itemList.Count)
			{
				flag = true;
			}
			else
			{
				int num = 0;
				foreach (Item item in base.inventory.itemList)
				{
					if (item.info == baseProjectile.primaryMagazine.ammoType)
					{
						num += item.MaxStackable() - item.amount;
					}
				}
				flag = num >= baseProjectile.primaryMagazine.contents;
			}
			if (!flag)
			{
				return;
			}
			base.inventory.AddItem(baseProjectile.primaryMagazine.ammoType, baseProjectile.primaryMagazine.contents, 0uL);
			baseProjectile.SetAmmoCount(0);
		}
		List<Item> ammos = Facepunch.Pool.Get<List<Item>>();
		base.inventory.FindAmmo(ammos, ammoTypes);
		if (!baseProjectile.primaryMagazine.allowAmmoSwitching)
		{
			BaseProjectile.StripAmmoToType(ref ammos, baseProjectile.primaryMagazine.ammoType);
		}
		if (ammos.Count > 0)
		{
			Effect.server.Run(reloadEffect.resourcePath, this, StringPool.Get("WeaponAttachmentPoint"), Vector3.zero, Vector3.zero);
			totalAmmoDirty = true;
			baseProjectile.primaryMagazine.ammoType = ammos[0].info;
			int num2 = 0;
			while (baseProjectile.primaryMagazine.contents < baseProjectile.primaryMagazine.capacity && num2 < ammos.Count)
			{
				if (ammos[num2].info == baseProjectile.primaryMagazine.ammoType)
				{
					int b = baseProjectile.primaryMagazine.capacity - baseProjectile.primaryMagazine.contents;
					b = Mathf.Min(ammos[num2].amount, b);
					ammos[num2].UseItem(b);
					baseProjectile.ModifyAmmoCount(b);
				}
				num2++;
			}
		}
		ItemDefinition ammoType = baseProjectile.primaryMagazine.ammoType;
		if ((bool)ammoType)
		{
			ItemModProjectile component = ammoType.GetComponent<ItemModProjectile>();
			GameObject gameObject = component.GetOverrideProjectile(baseProjectile).Get();
			if ((bool)gameObject)
			{
				if ((bool)gameObject.GetComponent<Projectile>())
				{
					currentAmmoGravity = 0f;
					currentAmmoVelocity = component.GetMaxVelocity();
				}
				else
				{
					ServerProjectile component2 = gameObject.GetComponent<ServerProjectile>();
					if ((bool)component2)
					{
						currentAmmoGravity = component2.gravityModifier;
						currentAmmoVelocity = component2.speed;
					}
				}
			}
		}
		Facepunch.Pool.Free(ref ammos, freeElements: false);
		baseProjectile.SendNetworkUpdate();
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		totalAmmoDirty = true;
		Reload();
		if (ShouldApplyInterference())
		{
			PostServerLoad_Interference();
		}
	}

	public void UpdateTotalAmmo()
	{
		int num = totalAmmo;
		totalAmmo = GetTotalAmmo();
		if (num != totalAmmo && (outputs[1].connectedTo.Get() != null || outputs[2].connectedTo.Get() != null))
		{
			MarkDirtyForceUpdateOutputs();
		}
	}

	public override void OnItemAddedOrRemoved(Item item, bool added)
	{
		base.OnItemAddedOrRemoved(item, added);
		if ((bool)item.info.GetComponent<ItemModEntity>())
		{
			if (IsInvoking(actionUpdateAttachedWeapon))
			{
				UpdateAttachedWeapon();
			}
			Invoke(actionUpdateAttachedWeapon, 0.5f);
		}
	}

	public bool EnsureReloaded(bool onlyReloadIfEmpty = true)
	{
		bool flag = HasReserveAmmo();
		if (onlyReloadIfEmpty)
		{
			if (flag && !HasClipAmmo())
			{
				Reload();
				return true;
			}
		}
		else if (flag)
		{
			Reload();
			return true;
		}
		return false;
	}

	public BaseProjectile GetAttachedWeapon()
	{
		return AttachedWeapon as BaseProjectile;
	}

	public bool TryGetAttachedWeapon(out BaseProjectile baseProjectile)
	{
		baseProjectile = GetAttachedWeapon();
		return baseProjectile != null;
	}

	public virtual bool HasFallbackWeapon()
	{
		return false;
	}

	public bool HasGenericFireable()
	{
		if (AttachedWeapon != null)
		{
			return AttachedWeapon.IsInstrument();
		}
		return false;
	}

	public void UpdateAttachedWeapon()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		ResetBurstFireState();
		HeldEntity heldEntity = TryAddWeaponToTurret(base.inventory.GetSlot(0), socketTransform, this, attachedWeaponZOffsetScale);
		bool flag = heldEntity != null;
		flagsUpdateScope.Set(Flags.Reserved3, flag);
		if (flag)
		{
			AttachedWeapon = heldEntity;
			totalAmmoDirty = true;
			Reload();
			UpdateTotalAmmo();
			if (IsOffline())
			{
				heldEntity.SetLightsOn(isOn: false);
			}
		}
		else
		{
			BaseProjectile attachedWeapon = GetAttachedWeapon();
			if (attachedWeapon != null)
			{
				attachedWeapon.SetGenericVisible(wantsVis: false);
				attachedWeapon.SetLightsOn(isOn: false);
				if (attachedWeapon is ITurretNotify turretNotify)
				{
					turretNotify.OnAddedRemovedToTurret(added: false);
				}
			}
			AttachedWeapon = null;
		}
		bool b = false;
		if (flag)
		{
			BaseProjectile component = heldEntity.GetComponent<BaseProjectile>();
			b = component != null && component.largeTurretWeapon;
		}
		flagsUpdateScope.Set(Flags.Reserved5, b);
	}

	public static HeldEntity TryAddWeaponToTurret(Item weaponItem, Transform parent, BaseEntity entityParent, float zOffsetScale)
	{
		HeldEntity heldEntity = null;
		if (weaponItem != null && (weaponItem.info.category == ItemCategory.Weapon || weaponItem.info.category == ItemCategory.Fun))
		{
			BaseEntity heldEntity2 = weaponItem.GetHeldEntity();
			if (heldEntity2 != null)
			{
				HeldEntity component = heldEntity2.GetComponent<HeldEntity>();
				if (component != null && component.IsUsableByTurret)
				{
					heldEntity = component;
				}
			}
		}
		if (heldEntity == null)
		{
			return null;
		}
		Transform transform = heldEntity.transform;
		Transform muzzleTransform = heldEntity.MuzzleTransform;
		heldEntity.SetParent(null);
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		Quaternion quaternion = transform.rotation * Quaternion.Inverse(muzzleTransform.rotation);
		heldEntity.limitNetworking = false;
		using (FlagsUpdateScope flagsUpdateScope = heldEntity.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Disabled, b: false);
		}
		heldEntity.SetParent(entityParent, StringPool.Get(parent.name));
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		transform.rotation *= quaternion;
		Vector3 vector = parent.InverseTransformPoint(muzzleTransform.position);
		transform.localPosition = Vector3.left * vector.x;
		float num = Vector3.Distance(muzzleTransform.position, transform.position);
		transform.localPosition += Vector3.forward * num * zOffsetScale;
		heldEntity.SetGenericVisible(wantsVis: true);
		heldEntity.SetLightsOn(isOn: true);
		if (heldEntity is ITurretNotify turretNotify)
		{
			turretNotify.OnAddedRemovedToTurret(added: true);
		}
		return heldEntity;
	}

	public override void OnDied(HitInfo info)
	{
		BaseProjectile attachedWeapon = GetAttachedWeapon();
		if (attachedWeapon != null)
		{
			attachedWeapon.SetGenericVisible(wantsVis: false);
			attachedWeapon.SetLightsOn(isOn: false);
			if (attachedWeapon is ITurretNotify turretNotify)
			{
				turretNotify.OnAddedRemovedToTurret(added: false);
			}
		}
		AttachedWeapon = null;
		base.OnDied(info);
	}

	public override bool OnStartBeingLooted(BasePlayer baseEntity)
	{
		if (!IsAuthed(baseEntity))
		{
			return false;
		}
		return base.OnStartBeingLooted(baseEntity);
	}

	public override void PlayerStoppedLooting(BasePlayer player)
	{
		base.PlayerStoppedLooting(player);
		UpdateTotalAmmo();
		EnsureReloaded(onlyReloadIfEmpty: false);
		UpdateTotalAmmo();
		nextShotTime = UnityEngine.Time.timeAsDouble;
	}

	public virtual float GetMaxAngleForEngagement()
	{
		return 1f;
	}

	private void ResetBurstFireState()
	{
		currentBurstShotsFired = 0;
	}

	private bool IsBursting(BaseProjectile attached)
	{
		if (attached.primaryMagazine.contents <= 0 || !IsUsingBurstFireWeapon(attached))
		{
			return false;
		}
		return currentBurstShotsFired != 0;
	}

	private bool IsUsingBurstFireWeapon()
	{
		if (!TryGetAttachedWeapon(out var baseProjectile))
		{
			return false;
		}
		return IsUsingBurstFireWeapon(baseProjectile);
	}

	private bool IsUsingBurstFireWeapon(BaseProjectile attached)
	{
		return attached.IsBurstModeOnly();
	}

	public void TargetTick()
	{
		using (TimeWarning.New("AutoTurret.ServerTick.TickCycle.TargetTick"))
		{
			double timeAsDouble = UnityEngine.Time.timeAsDouble;
			double realtimeSinceStartupAsDouble = UnityEngine.Time.realtimeSinceStartupAsDouble;
			if (realtimeSinceStartupAsDouble >= nextVisCheck)
			{
				nextVisCheck = realtimeSinceStartupAsDouble + (double)UnityEngine.Random.Range(0.2f, 0.3f);
				targetVisible = ObjectVisible(target);
				if (targetVisible)
				{
					OnTargetSeen(target, realtimeSinceStartupAsDouble);
				}
			}
			BaseProjectile baseProjectile;
			bool flag = TryGetAttachedWeapon(out baseProjectile);
			bool flag2 = flag && IsBursting(baseProjectile);
			EnsureReloaded();
			if (!(timeAsDouble >= nextShotTime) || !(targetVisible || flag2) || (!flag2 && !(Mathf.Abs(AngleToTarget(target, currentAmmoGravity != 0f)) < GetMaxAngleForEngagement())))
			{
				return;
			}
			if (flag)
			{
				if (baseProjectile is ITurretNotify turretNotify)
				{
					turretNotify.WarmupTick(wantsShoot: true);
				}
				float damageModifier = 1f;
				float speedModifier = 1f;
				ItemDefinition ammoType = baseProjectile.primaryMagazine.ammoType;
				if ((bool)ammoType)
				{
					ItemModProjectile component = ammoType.GetComponent<ItemModProjectile>();
					if ((bool)component && component.projectileVelocity < 100f)
					{
						speedModifier = 2f;
					}
				}
				if (baseProjectile.primaryMagazine.contents > 0)
				{
					if (!target.IsRealNull() && target.GetParentEntity() is TrainCar trainCar)
					{
						float magnitude = trainCar.GetWorldVelocity().magnitude;
						float num = Mathf.Pow(1f - TrainCar.TrainTurretInaccuratePerVelocity, magnitude);
						if (UnityEngine.Random.Range(0f, 1f) > num)
						{
							damageModifier = 0f;
						}
					}
					if (!target.IsRealNull())
					{
						lastTargetAimOffset = AimOffset(target);
					}
					FireAttachedGun(lastTargetAimOffset, aimCone, PeacekeeperMode() ? target : null, damageModifier, speedModifier);
					float num2 = 0f;
					if (IsUsingBurstFireWeapon(baseProjectile))
					{
						int burstModeCount = baseProjectile.GetBurstModeCount();
						currentBurstShotsFired++;
						if (currentBurstShotsFired < burstModeCount && baseProjectile.primaryMagazine.contents > 0)
						{
							num2 = Mathf.Max(baseProjectile.ScaleRepeatDelay(baseProjectile.repeatDelay), baseProjectile.NextAttackTime - (float)timeAsDouble);
						}
						else
						{
							ResetBurstFireState();
							num2 = Mathf.Max(baseProjectile.TimeBetweenBursts(), 0.4f);
						}
					}
					else
					{
						ResetBurstFireState();
						num2 = (baseProjectile.isSemiAuto ? (baseProjectile.repeatDelay * 1.5f) : baseProjectile.repeatDelay);
						num2 = baseProjectile.ScaleRepeatDelay(num2);
					}
					nextShotTime = timeAsDouble + (double)num2;
					if ((float)nextShotTime < baseProjectile.NextAttackTime && !Mathf.Approximately((float)nextShotTime, baseProjectile.NextAttackTime))
					{
						Debug.LogWarning($"Turret {base.name} next shot scheduled in {num2}s will be skipped due to it being less than attached {baseProjectile.name} attack cooldown ({baseProjectile.NextAttackTime - (float)timeAsDouble}s).", this);
					}
					shouldUpdateOnOutOfAmmo = true;
					lastShotTime = timeAsDouble;
				}
				else
				{
					ResetBurstFireState();
					nextShotTime = timeAsDouble + 5.0;
					if (shouldUpdateOnOutOfAmmo)
					{
						shouldUpdateOnOutOfAmmo = false;
						baseProjectile.SendNetworkUpdate();
					}
				}
			}
			else if (HasFallbackWeapon())
			{
				FireGun(AimOffset(target), aimCone, null, target);
				nextShotTime = timeAsDouble + 0.11500000208616257;
				lastShotTime = timeAsDouble;
			}
			else if (HasGenericFireable())
			{
				AttachedWeapon.ServerUse();
				lastShotTime = timeAsDouble;
				nextShotTime = timeAsDouble + 0.11500000208616257;
			}
			else
			{
				nextShotTime = timeAsDouble + 1.0;
			}
		}
	}

	public bool HasTarget()
	{
		if (target != null)
		{
			return target.IsAlive();
		}
		return false;
	}

	public void OfflineTick()
	{
		aimDir = Vector3.up;
	}

	public virtual bool IsEntityHostile(BaseCombatEntity ent)
	{
		if (ent is ScarecrowNPC)
		{
			return true;
		}
		if (ent is BasePet basePet && basePet.Brain.OwningPlayer != null)
		{
			if (!basePet.Brain.OwningPlayer.IsHostile())
			{
				return ent.IsHostile();
			}
			return true;
		}
		return ent.IsHostile();
	}

	public bool ShouldTarget(BaseCombatEntity targ)
	{
		if (targ is AutoTurret)
		{
			return false;
		}
		if (targ is RidableHorse)
		{
			return false;
		}
		if (targ is BasePet basePet && basePet.Brain.OwningPlayer != null && IsAuthed(basePet.Brain.OwningPlayer))
		{
			return false;
		}
		if (targ is Drone drone)
		{
			if (!drone.IsBeingControlled)
			{
				return false;
			}
			if (IsAuthed(drone.OwnerID))
			{
				return false;
			}
			if (!drone.IsHostile())
			{
				return false;
			}
		}
		return true;
	}

	public void ScheduleForTargetScan()
	{
		updateAutoTurretScanQueue.Add(this);
	}

	public void TargetScan()
	{
		double realtimeSinceStartupAsDouble = UnityEngine.Time.realtimeSinceStartupAsDouble;
		if (!target.IsRealNull())
		{
			double num = realtimeSinceStartupAsDouble - lastTargetSeenTime;
			double num2 = realtimeSinceStartupAsDouble - lastDamageEventTime;
			if (target == null || target.IsDead() || (num > 3.0 && num2 > 3.0) || Vector3.Distance(base.transform.position, target.transform.position) > sightRange || (PeacekeeperMode() && !IsEntityHostile(target)))
			{
				SetTarget(null);
			}
		}
		if (HasInterference())
		{
			if (HasTarget())
			{
				SetTarget(null);
			}
		}
		else
		{
			if (HasTarget() || IsOffline() || IsBeingControlled || aimDir == Vector3.up)
			{
				return;
			}
			bool flag = targetTrigger != null && targetTrigger.entityContents != null && !CollectionEx.IsEmpty(targetTrigger.entityContents) && realtimeSinceStartupAsDouble - lastScanTime >= (double)Sentry.scantimer;
			if (!authDirty && !flag)
			{
				return;
			}
			authDirty = false;
			lastScanTime = realtimeSinceStartupAsDouble;
			if (targetTrigger != null && targetTrigger.entityContents != null)
			{
				foreach (BaseEntity entityContent in targetTrigger.entityContents)
				{
					BaseCombatEntity baseCombatEntity = entityContent as BaseCombatEntity;
					if (baseCombatEntity == null)
					{
						continue;
					}
					if (!Sentry.targetall)
					{
						BasePlayer basePlayer = baseCombatEntity as BasePlayer;
						if (basePlayer != null && (IsAuthed(basePlayer) || Ignore(basePlayer)))
						{
							continue;
						}
					}
					if ((!PeacekeeperMode() || IsEntityHostile(baseCombatEntity)) && baseCombatEntity.IsAlive() && ShouldTarget(baseCombatEntity) && InFiringArc(baseCombatEntity) && ObjectVisible(baseCombatEntity))
					{
						SetTarget(baseCombatEntity);
						if ((object)target != null)
						{
							break;
						}
					}
				}
			}
			if (PeacekeeperMode() && target == null)
			{
				nextShotTime = UnityEngine.Time.timeAsDouble + 1.0;
			}
		}
	}

	public virtual bool Ignore(BasePlayer player)
	{
		return false;
	}

	public void ServerDo()
	{
		if (!base.isClient && !base.IsDestroyed)
		{
			float dt = (float)(double)timeSinceLastServerTick;
			timeSinceLastServerTick = 0.0;
			UpdateFacingToTarget(dt);
		}
	}

	public void ServerThink()
	{
		if (!base.isClient && !base.IsDestroyed)
		{
			updateTurretTick.Add(this);
			if (totalAmmoDirty && UnityEngine.Time.timeAsDouble > nextAmmoCheckTime)
			{
				updateAutoTurretAmmoQueue.Add(this);
				totalAmmoDirty = false;
				nextAmmoCheckTime = UnityEngine.Time.timeAsDouble + 0.5;
			}
		}
	}

	public override void OnNetworkGroupEnter(Group group)
	{
		base.OnNetworkGroupEnter(group);
	}

	public void RunScheduledTick()
	{
		if (lastTickTime == 0f)
		{
			lastTickTime = UnityEngine.Time.time;
		}
		if (!IsOnline())
		{
			OfflineTick();
		}
		else if (!IsBeingControlled)
		{
			if (HasTarget())
			{
				TargetTick();
			}
			else
			{
				float dt = UnityEngine.Time.time - lastTickTime;
				IdleTick(dt);
			}
		}
		lastTickTime = UnityEngine.Time.time;
	}

	public override void OnAttacked(HitInfo info)
	{
		base.OnAttacked(info);
		if (((IsOnline() && !HasTarget()) || !targetVisible) && !(info.Initiator as AutoTurret != null) && !(info.Initiator as SamSite != null) && !(info.Initiator as GunTrap != null))
		{
			BasePlayer basePlayer = info.Initiator as BasePlayer;
			if (!basePlayer || !IsAuthed(basePlayer))
			{
				SetTarget(info.Initiator as BaseCombatEntity);
				lastDamageEventTime = UnityEngine.Time.realtimeSinceStartupAsDouble;
			}
		}
	}

	public void UpdateFacingToTarget(float dt)
	{
		if (target != null && targetVisible && !IsBeingControlled)
		{
			using (TimeWarning.New("AutoTurret.ServerTick.UpdateFacing"))
			{
				Vector3 vector = AimOffset(target);
				Vector3 position = eyePos.position;
				if (peekIndex != 0)
				{
					Vector3 vector2 = position;
					Vector3.Distance(vector, vector2);
					Vector3 vector3 = Vector3.Cross((vector - vector2).normalized, Vector3.up);
					vector += vector3 * visibilityOffsets[peekIndex];
				}
				Vector3 vector4 = (vector - position).normalized;
				if (currentAmmoGravity != 0f)
				{
					float num = 0.2f;
					if (target is BasePlayer)
					{
						float num2 = Mathf.Clamp01(target.WaterFactor()) * 1.8f;
						if (num2 > num)
						{
							num = num2;
						}
					}
					vector = target.transform.position + Vector3.up * num;
					float angle = GetAngle(position, vector, currentAmmoVelocity, currentAmmoGravity);
					Vector3 normalized = (vector.XZ3D() - position.XZ3D()).normalized;
					vector4 = Quaternion.LookRotation(normalized) * Quaternion.Euler(angle, 0f, 0f) * Vector3.forward;
				}
				aimDir = vector4;
			}
		}
		UpdateAiming(dt);
	}

	public float GetAngle(Vector3 launchPosition, Vector3 targetPosition, float launchVelocity, float gravityScale)
	{
		float num = UnityEngine.Physics.gravity.y * gravityScale;
		float num2 = Vector3.Distance(launchPosition.XZ3D(), targetPosition.XZ3D());
		float num3 = launchPosition.y - targetPosition.y;
		float num4 = Mathf.Pow(launchVelocity, 2f);
		float num5 = Mathf.Pow(launchVelocity, 4f);
		float num6 = Mathf.Atan((num4 + Mathf.Sqrt(num5 - num * (num * Mathf.Pow(num2, 2f) + 2f * num3 * num4))) / (num * num2)) * 57.29578f;
		float num7 = Mathf.Atan((num4 - Mathf.Sqrt(num5 - num * (num * Mathf.Pow(num2, 2f) + 2f * num3 * num4))) / (num * num2)) * 57.29578f;
		if (float.IsNaN(num6) && float.IsNaN(num7))
		{
			return -45f;
		}
		if (float.IsNaN(num6))
		{
			return num7;
		}
		if (!(num6 > num7))
		{
			return num7;
		}
		return num6;
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		base.OnDeployed(parent, deployedBy, fromItem);
		AddSelfAuthorize(deployedBy);
	}

	public override ItemContainerId GetIdealContainer(BasePlayer player, Item item, ItemMoveModifier modifier)
	{
		return default(ItemContainerId);
	}

	public override int GetIdealSlot(BasePlayer player, ItemContainer container, Item item)
	{
		bool num = item.info.category == ItemCategory.Weapon;
		bool flag = item.info.category == ItemCategory.Ammunition;
		if (num)
		{
			return 0;
		}
		if (flag)
		{
			for (int i = 1; i < base.inventory.capacity; i++)
			{
				if (!base.inventory.SlotTaken(item, i))
				{
					return i;
				}
			}
		}
		return -1;
	}

	public override bool CanBeRedirectSwapped(BasePlayer player)
	{
		if (!IsAuthed(player))
		{
			SprayCan.LastReskinError = SprayCan.NotAuthorized;
			return false;
		}
		return base.CanBeRedirectSwapped(player);
	}

	public override void Reskin_Preserve(ref SprayCan.ReskinPreserveInfo preserveInfo)
	{
		base.Reskin_Preserve(ref preserveInfo);
		ref AutoTurretPreserveInfo autoTurretPreserve = ref preserveInfo.autoTurretPreserve;
		autoTurretPreserve.authedPlayers = Facepunch.Pool.Get<List<ulong>>();
		autoTurretPreserve.authedPlayers.AddRange(authorizedPlayers);
		autoTurretPreserve.isPeacekeeper = HasFlag(Flags.Reserved1);
		autoTurretPreserve.rcIdentifier = GetIdentifier();
	}

	public override void Reskin_Restore(ref SprayCan.ReskinPreserveInfo preserveInfo)
	{
		ref IItemContainerEntity.ContainerPreserveInfo containerPreserve = ref preserveInfo.containerPreserve;
		IItemContainerEntity.ContainerSet containerSet = default(IItemContainerEntity.ContainerSet);
		containerSet.ContainerIndex = -1;
		containerSet.PrefabId = 0u;
		IItemContainerEntity.ContainerSet key = containerSet;
		List<Item> list = containerPreserve.storageDict[key];
		foreach (Item item in list)
		{
			if (IsValidWeapon(item))
			{
				item.MoveToContainer(base.inventory);
				AttachedWeapon = item.GetHeldEntity() as HeldEntity;
				break;
			}
		}
		foreach (Item item2 in list)
		{
			if (item2.info.category == ItemCategory.Ammunition)
			{
				item2.MoveToContainer(base.inventory);
			}
		}
		containerPreserve.storageDict[key].Clear();
		base.Reskin_Restore(ref preserveInfo);
		ref AutoTurretPreserveInfo autoTurretPreserve = ref preserveInfo.autoTurretPreserve;
		foreach (ulong authedPlayer in autoTurretPreserve.authedPlayers)
		{
			authorizedPlayers.Add(authedPlayer);
		}
		if (HasFlag(Flags.Reserved1) != autoTurretPreserve.isPeacekeeper)
		{
			SetFlagLocal(Flags.Reserved1, autoTurretPreserve.isPeacekeeper);
		}
		if (!string.IsNullOrEmpty(autoTurretPreserve.rcIdentifier) && GetIdentifier() != autoTurretPreserve.rcIdentifier)
		{
			UpdateIdentifier(autoTurretPreserve.rcIdentifier);
		}
		Facepunch.Pool.FreeUnmanaged(ref autoTurretPreserve.authedPlayers);
	}

	public bool IsOnline()
	{
		return IsOn();
	}

	public bool IsOffline()
	{
		return !IsOnline();
	}

	public override void ResetState()
	{
		base.ResetState();
	}

	public virtual Matrix4x4 GetCenterMuzzle()
	{
		if (base.isServer)
		{
			Matrix4x4 localToWorldMatrix = cachedTransf.localToWorldMatrix;
			if (rotateMode == YawPitchMode.Separate)
			{
				return localToWorldMatrix * toYawFromRoot * Matrix4x4.Rotate(gunAimYawRotLS) * toPitchFromRootOrYaw * Matrix4x4.Rotate(gunAimPitchOrTotalRotLS);
			}
			return localToWorldMatrix * toPitchFromRootOrYaw * Matrix4x4.Rotate(gunAimPitchOrTotalRotLS);
		}
		return gun_pitch.localToWorldMatrix;
	}

	public float AngleToTarget(Vector3 targetPos, bool use2D = false)
	{
		use2D = true;
		Matrix4x4 centerMuzzle = GetCenterMuzzle();
		Vector3 position = centerMuzzle.GetPosition();
		Vector3 zero = Vector3.zero;
		zero = ((!use2D) ? (targetPos - position).normalized : Vector3Ex.Direction2D(targetPos, position));
		Vector3 vector = centerMuzzle.MultiplyVector(Vector3.forward);
		return Vector3.Angle(use2D ? vector.XZ3D().normalized : vector, zero);
	}

	public float AngleToTarget(BaseCombatEntity potentialtarget, bool use2D = false)
	{
		Vector3 targetPos = AimOffset(potentialtarget);
		return AngleToTarget(targetPos, use2D);
	}

	public virtual bool InFiringArc(BaseCombatEntity potentialtarget)
	{
		return Mathf.Abs(AngleToTarget(potentialtarget)) <= 90f;
	}

	protected override bool ShouldDisplayPickupOption(BasePlayer player)
	{
		if (IsAuthed(player))
		{
			return base.ShouldDisplayPickupOption(player);
		}
		return false;
	}

	protected override bool CanCompletePickup(BasePlayer player)
	{
		if (IsOnline())
		{
			pickupErrorToFormat = (format: PickupErrors.ItemIsOnline, arg0: pickup.itemTarget.displayName);
			return false;
		}
		return base.CanCompletePickup(player);
	}

	public override bool CanUseNetworkCache(Connection connection)
	{
		return false;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.autoturret = Facepunch.Pool.Get<ProtoBuf.AutoTurret>();
		info.msg.autoturret.aimDir = aimDir;
		if (info.forDisk || IsAuthed(info.forConnection.userid))
		{
			info.msg.autoturret.users = Facepunch.Pool.Get<List<PlayerNameID>>();
			foreach (ulong authorizedPlayer in authorizedPlayers)
			{
				PlayerNameID playerNameID = Facepunch.Pool.Get<PlayerNameID>();
				playerNameID.userid = authorizedPlayer;
				info.msg.autoturret.users.Add(playerNameID);
			}
		}
		if (info.forDisk && ShouldApplyInterference())
		{
			Save_Interference(info.msg.autoturret);
		}
		if (info.forDisk)
		{
			info.msg.rcEntity = Facepunch.Pool.Get<RCEntity>();
			info.msg.rcEntity.identifier = GetIdentifier();
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.autoturret != null)
		{
			authorizedPlayers.Clear();
			if (info.msg.autoturret.users != null)
			{
				foreach (PlayerNameID user in info.msg.autoturret.users)
				{
					authorizedPlayers.Add(user.userid);
				}
			}
			info.msg.autoturret.users = null;
			aimDir = info.msg.autoturret.aimDir;
			if (base.isServer && ShouldApplyInterference())
			{
				Load_Interference(info.msg.autoturret);
			}
		}
		if (info.msg.rcEntity != null)
		{
			UpdateIdentifier(info.msg.rcEntity.identifier);
		}
	}

	public Vector3 AimOffset(BaseCombatEntity aimat)
	{
		BasePlayer basePlayer = aimat as BasePlayer;
		if (!ObjectEx.IsUnityNull(basePlayer))
		{
			if (basePlayer.IsSleeping())
			{
				return basePlayer.transform.position + Vector3.up * 0.1f;
			}
			if (basePlayer.IsWounded())
			{
				return basePlayer.transform.position + Vector3.up * 0.25f;
			}
			if (basePlayer.TryGetActiveShield(out var foundShield) && foundShield.IsBlocking())
			{
				return foundShield.transform.position;
			}
			if (!ObjectEx.IsUnityNull(basePlayer.eyes))
			{
				return basePlayer.eyes.position;
			}
			return basePlayer.GetCenter();
		}
		if (!ObjectEx.IsUnityNull(aimat))
		{
			return aimat.CenterPoint();
		}
		return Vector3.zero;
	}

	public float GetAimSpeed()
	{
		if (HasTarget())
		{
			return 5f;
		}
		return 1f;
	}

	public void UpdateAiming(float dt)
	{
		if (aimDir == Vector3.zero)
		{
			return;
		}
		float speed = 5f;
		if (base.isServer && !IsBeingControlled)
		{
			speed = ((!HasTarget()) ? 15f : 35f);
		}
		Quaternion quaternion = Quaternion.LookRotation(aimDir);
		if (!base.isServer)
		{
			return;
		}
		Quaternion rotation = cachedTransf.rotation;
		if (rotateMode == YawPitchMode.Merged)
		{
			Quaternion quaternion2 = Quaternion.Inverse(rotation * gunAimInitialPitchOrTotalRot) * quaternion;
			if (gunAimPitchOrTotalRotLS != quaternion2)
			{
				gunAimPitchOrTotalRotLS = Mathx.Lerp(gunAimPitchOrTotalRotLS, quaternion2, speed, dt);
			}
		}
		else if (gunAimTotalRotWS != quaternion)
		{
			gunAimTotalRotWS = Mathx.Lerp(gunAimTotalRotWS, quaternion, speed, dt);
			Vector3 eulerAngles = (Quaternion.Inverse(rotation * gunAimInitialYawRot) * gunAimTotalRotWS).eulerAngles;
			gunAimYawRotLS = Quaternion.Euler(0f, eulerAngles.y, 0f);
			gunAimPitchOrTotalRotLS = Quaternion.Euler(eulerAngles.x, 0f, 0f);
		}
	}

	public bool IsAuthed(ulong id)
	{
		return authorizedPlayers.Contains(id);
	}

	public bool IsAuthed(BasePlayer player)
	{
		return IsAuthed(player.userID);
	}

	public bool AnyAuthed()
	{
		return authorizedPlayers.Count > 0;
	}

	public virtual bool CanChangeSettings(BasePlayer player)
	{
		if (IsAuthed(player) && player.CanBuild())
		{
			return IsOffline();
		}
		return false;
	}

	bool IHostileWarningEntity.WarningEnabled(BaseEntity forEntity)
	{
		if (!IsPowered())
		{
			return false;
		}
		if (!PeacekeeperMode())
		{
			return false;
		}
		BasePlayer basePlayer = forEntity as BasePlayer;
		if (basePlayer == null)
		{
			return false;
		}
		if (IsAuthed(basePlayer))
		{
			return false;
		}
		return true;
	}

	float IHostileWarningEntity.WarningRange()
	{
		return sightRange * 2f;
	}
}
