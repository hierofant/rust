#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Development.Attributes;
using Facepunch;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

[ResetStaticFields]
public class BaseMountable : BaseCombatEntity
{
	public enum ClippingCheckLocation
	{
		HeadOnly,
		WholeBody
	}

	public enum DismountConvarType
	{
		Misc,
		Boating,
		Flying,
		GroundVehicle,
		Horse
	}

	public enum MountStatType
	{
		None,
		Boating,
		Flying,
		Driving
	}

	public enum MountGestureType
	{
		None,
		UpperBody
	}

	public enum MountSyncType
	{
		RepositionPerFrame,
		Parent,
		ParentAIOnly
	}

	public const float MountCheckRadius = 0.25f;

	public static Translate.Phrase dismountPhrase = new Translate.Phrase("dismount", "Dismount");

	[Header("Base Mountable")]
	public MountSyncType mountSyncType;

	[Header("View")]
	public Transform eyePositionOverride;

	public Transform eyeCenterOverride;

	public bool overrideEyesRotation;

	public Vector2 pitchClamp = new Vector2(-80f, 50f);

	public Vector2 yawClamp = new Vector2(-80f, 80f);

	public bool canWieldItems = true;

	public bool relativeViewAngles = true;

	public bool disableLegsWhenMounted;

	public bool disableBreastCensorshipWhenMounted;

	[Header("Mounting")]
	public bool AllowForceMountWhenRestrained;

	[Tooltip("Allow players to mount other mountables/ladders from this vehicle")]
	public bool mountChaining = true;

	public Transform mountAnchor;

	public float mountLOSVertOffset = 0.5f;

	[Range(0f, 1f)]
	[Tooltip("The speed of the posde animation for this mountable.")]
	[Header("Mount Pose")]
	public float mountedAnimationSpeed;

	public PlayerModel.MountPoses mountPose;

	[Tooltip("Toggles the vehicleAimYaw parameter update, only used by specific mountable like the rowboat and steering wheel.")]
	public bool animateVehicleAim360;

	[Space]
	public float maxMountDistance = 1.5f;

	public Transform[] dismountPositions;

	public bool checkPlayerLosOnMount;

	public bool disableMeshCullingForPlayers;

	public bool allowHeadLook;

	public bool ignoreVehicleParent;

	public bool legacyDismount;

	public ItemModWearable wearWhileMounted;

	public bool modifiesPlayerCollider;

	public BasePlayer.CapsuleColliderInfo customPlayerCollider;

	public float clippingCheckRadius = 0.4f;

	public bool clippingAndVisChecks;

	public ClippingCheckLocation clippingChecksLocation;

	public SoundDefinition mountSoundDef;

	public SoundDefinition swapSoundDef;

	public SoundDefinition dismountSoundDef;

	public bool allowFootstepEffects;

	public DismountConvarType dismountHoldType;

	[NonSerialized]
	private EntityRef _mountedRef;

	public MountStatType mountTimeStatType;

	public MountGestureType allowedGestures;

	public bool canDrinkWhileMounted = true;

	public bool allowSleeperMounting;

	public bool shouldShowHudHealth;

	[Tooltip("Block looting of containers while mounted to this entity")]
	public bool blockLooting;

	[Help("Set this to true if the mountable is enclosed so it doesn't move inside cars and such")]
	public bool animateClothInLocalSpace = true;

	[SerializeField]
	private bool protectsFromAnimals = true;

	[Header("Camera")]
	public BasePlayer.CameraMode MountedCameraMode;

	[Header("Rigidbody (Optional)")]
	public Rigidbody rigidBody;

	public bool wantsBoundaryRepelCheck;

	[FormerlySerializedAs("needsVehicleTick")]
	public bool isMobile;

	public float SideLeanAmount = 0.2f;

	public const float playerHeight = 1.8f;

	public const float playerRadius = 0.5f;

	public BasePlayer _mounted;

	public static ListHashSet<BaseMountable> AllMountables = new ListHashSet<BaseMountable>();

	public static ListHashSet<BaseMountable> Mounted = new ListHashSet<BaseMountable>();

	[ServerVar(Help = "Toggles the usage of mountable MountedPlayerSync optimisations (only used by boat scientists currently)")]
	public static bool canPauseMountedPlayerSync = false;

	protected bool syncsMountedPlayers = true;

	public const float MOUNTABLE_TICK_RATE = 0.05f;

	public bool ProtectsFromAnimals
	{
		get
		{
			if (base.transform.parent != null && BaseNetworkableEx.Is<BaseMountable>(base.transform.parent.GetComponent<BaseMountable>(), out var castedUnityObject))
			{
				return castedUnityObject.ProtectsFromAnimals;
			}
			return protectsFromAnimals;
		}
	}

	public override float PositionTickRate
	{
		protected get
		{
			return 0.05f;
		}
	}

	public virtual bool IsSummerDlcVehicle => false;

	protected virtual bool BypassClothingMountBlocks => false;

	public virtual bool BlocksDoors => true;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BaseMountable.OnRpcMessage"))
		{
			if (rpc == 1735799362 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_WantsDismount");
				}
				using (TimeWarning.New("RPC_WantsDismount"))
				{
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg2 = rPCMessage;
							RPC_WantsDismount(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_WantsDismount");
					}
				}
				return true;
			}
			if (rpc == 4014300952u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_WantsMount");
				}
				using (TimeWarning.New("RPC_WantsMount"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(4014300952u, "RPC_WantsMount", this, player, 3f))
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
							RPC_WantsMount(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in RPC_WantsMount");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public virtual bool CanHoldItems()
	{
		return canWieldItems;
	}

	public virtual BasePlayer.CameraMode GetMountedCameraMode()
	{
		return MountedCameraMode;
	}

	public virtual bool DirectlyMountable()
	{
		return true;
	}

	public virtual Transform GetEyeOverride()
	{
		if (eyePositionOverride != null)
		{
			return eyePositionOverride;
		}
		return base.transform;
	}

	public virtual bool ModifiesThirdPersonCamera()
	{
		return false;
	}

	public virtual Vector2 GetPitchClamp()
	{
		return pitchClamp;
	}

	public virtual Vector2 GetYawClamp()
	{
		return yawClamp;
	}

	public virtual bool AnyMounted()
	{
		return IsBusy();
	}

	public bool IsMounted()
	{
		return AnyMounted();
	}

	public override void ResetState()
	{
		base.ResetState();
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (base.isServer && !info.forDisk && (bool)_mounted && _mounted.IsValid())
		{
			info.msg.baseMountable = Facepunch.Pool.Get<ProtoBuf.BaseMountable>();
			info.msg.baseMountable.mounted = _mounted.net.ID;
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
	}

	public virtual BasePlayer GetMounted()
	{
		if (base.isServer)
		{
			return _mounted;
		}
		return null;
	}

	public virtual Vector3 EyePositionForPlayer(BasePlayer player, Quaternion lookRot)
	{
		if (player.GetMounted() != this)
		{
			return Vector3.zero;
		}
		return GetEyeOverride().position;
	}

	public virtual Quaternion EyeRotationForPlayer(BasePlayer player)
	{
		if (player.GetMounted() != this || !overrideEyesRotation)
		{
			return Quaternion.identity;
		}
		return GetEyeOverride().rotation;
	}

	public virtual Vector3 EyeCenterForPlayer(BasePlayer player, Quaternion lookRot)
	{
		if (player.GetMounted() != this)
		{
			return Vector3.zero;
		}
		return eyeCenterOverride.transform.position;
	}

	public virtual float WaterFactorForPlayer(BasePlayer player, out WaterLevel.WaterInfo info)
	{
		Bounds bounds = player.WorldSpaceBounds().ToBounds();
		if (bounds.size == Vector3.zero)
		{
			bounds.size = new Vector3(0.1f, 0.1f, 0.1f);
		}
		info = WaterLevel.GetWaterInfo(bounds, waves: true, volumes: true, this);
		return WaterLevel.Factor(in info, bounds);
	}

	public override float AntiHackVelocity()
	{
		BaseEntity baseEntity = GetParentEntity();
		if ((bool)baseEntity)
		{
			return baseEntity.AntiHackVelocity();
		}
		return base.AntiHackVelocity();
	}

	public virtual bool PlayerIsMounted(BasePlayer player)
	{
		if (player.IsValid())
		{
			return player.GetMounted() == this;
		}
		return false;
	}

	public virtual BaseVehicle VehicleParent()
	{
		if (ignoreVehicleParent)
		{
			return null;
		}
		return GetParentEntity() as BaseVehicle;
	}

	public virtual bool HasValidDismountPosition(BasePlayer player)
	{
		BaseVehicle baseVehicle = VehicleParent();
		if (baseVehicle != null && !baseVehicle.childMountableHandleDismountPoints)
		{
			return baseVehicle.HasValidDismountPosition(player);
		}
		Transform[] array = dismountPositions;
		foreach (Transform transform in array)
		{
			if (!(transform == null) && ValidDismountPosition(player, transform.transform.position))
			{
				return true;
			}
		}
		return false;
	}

	protected virtual bool IgnoreChildEntitiesForDismountClipChecks()
	{
		return false;
	}

	protected virtual bool DismountCheckSkipVehicles()
	{
		return true;
	}

	public virtual bool ValidDismountPosition(BasePlayer player, Vector3 disPos)
	{
		bool debugDismounts = Debugging.DebugDismounts;
		Vector3 dismountCheckStart = GetDismountCheckStart(player);
		if (debugDismounts)
		{
			Debug.Log($"ValidDismountPosition debug: Checking dismount point {disPos} from {dismountCheckStart}.");
		}
		Vector3 start = disPos + new Vector3(0f, 0.5f, 0f);
		Vector3 end = disPos + new Vector3(0f, 1.3f, 0f);
		Collider col = null;
		if (!GamePhysics.CheckCapsule(base.isServer ? GamePhysics.Realm.Server : GamePhysics.Realm.Client, start, end, 0.5f, 1537286401))
		{
			Vector3 position = disPos + base.transform.up * 0.5f;
			if (IsVisibleAndCanSee(position))
			{
				Vector3 newPos = disPos + BasePlayer.NoClipOffset();
				if (debugDismounts)
				{
					Debug.Log($"ValidDismountPosition debug: Dismount point {disPos} is visible.");
				}
				if (legacyDismount || !AntiHack.TestNoClipping(player, dismountCheckStart, newPos, BasePlayer.NoClipRadius(ConVar.AntiHack.noclip_margin_dismount), ConVar.AntiHack.noclip_backtracking, out col, overlapVehicleLayer: false, this, forceCast: false, IgnoreChildEntitiesForDismountClipChecks(), DismountCheckSkipVehicles()))
				{
					if (debugDismounts)
					{
						Debug.Log($"<color=green>ValidDismountPosition debug: Dismount point {disPos} is valid</color>.");
					}
					return true;
				}
				if (debugDismounts)
				{
					Debug.Log($"<color=red>ValidDismountPosition debug: Dismount point {disPos} is invalid due to antihack</color>.");
				}
			}
			else if (debugDismounts)
			{
				Debug.Log($"<color=red>ValidDismountPosition debug: Dismount point {disPos} is invalid due to IsVisibleAndCanSee</color>.");
			}
		}
		if (debugDismounts && debugDismounts)
		{
			Debug.Log($"<color=red>ValidDismountPosition debug: Dismount point {disPos} is invalid</color>", col);
		}
		return false;
	}

	protected override bool ShouldDisplayPickupOption(BasePlayer player)
	{
		if (!AnyMounted())
		{
			return base.ShouldDisplayPickupOption(player);
		}
		return false;
	}

	public void EnableMountedPlayerSync()
	{
		syncsMountedPlayers = true;
	}

	public void DisableMountedPlayerSync()
	{
		syncsMountedPlayers = false;
	}

	public virtual void MounteeTookDamage(BasePlayer mountee, HitInfo info)
	{
	}

	public virtual void LightToggle(BasePlayer player)
	{
	}

	public virtual void OnWeaponFired(BaseProjectile weapon)
	{
	}

	public virtual bool CanSwapToThis(BasePlayer player)
	{
		object obj = Interface.CallHook("CanSwapToSeat", player, this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		return true;
	}

	public override void OnDied(HitInfo info)
	{
		DismountAllPlayers();
		base.OnDied(info);
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void RPC_WantsMount(RPCMessage msg)
	{
		WantsMount(msg.player);
	}

	public void WantsMount(BasePlayer player)
	{
		if (!player.IsValid() || !player.CanInteract() || Interface.CallHook("OnPlayerWantsMount", player, this) != null)
		{
			return;
		}
		if (!DirectlyMountable())
		{
			BaseVehicle baseVehicle = VehicleParent();
			if (baseVehicle != null && baseVehicle.IsVehicleMountPoint(this))
			{
				baseVehicle.WantsMount(player);
				return;
			}
		}
		AttemptMount(player);
	}

	public virtual void AttemptMount(BasePlayer player, bool doMountChecks = true)
	{
		if (_mounted != null || IsDead() || !player.CanMountMountablesNow() || IsTransferring() || IsSeatClipping(this) || ClothingBlocksMounting(player))
		{
			return;
		}
		if (doMountChecks)
		{
			if (checkPlayerLosOnMount)
			{
				Vector3 position = player.eyes.position;
				Vector3 vector = mountAnchor.position + base.transform.up * mountLOSVertOffset;
				Ray ray = new Ray(position, (vector - position).normalized);
				using PooledList<RaycastHit> pooledList = Facepunch.Pool.Get<PooledList<RaycastHit>>();
				GamePhysics.TraceAllUnordered(ray, 0.25f, pooledList, Vector3.Distance(position, vector), 1218519297);
				foreach (RaycastHit item in pooledList)
				{
					BaseEntity entity = RaycastHitEx.GetEntity(item);
					if (!(entity == null) && !(entity == this) && !(entity == VehicleParent()))
					{
						return;
					}
				}
			}
			if (!HasValidDismountPosition(player))
			{
				return;
			}
			if ((checkPlayerLosOnMount || ConVar.AntiHack.check_mount_distance >= 2) && ConVar.AntiHack.check_mount_distance >= 1)
			{
				float distanceFromMountAnchor = GetDistanceFromMountAnchor(player);
				float num = maxMountDistance;
				if (GetParentEntity() is BaseMountable baseMountable)
				{
					num = Mathf.Max(num, baseMountable.maxMountDistance);
				}
				if (distanceFromMountAnchor > num)
				{
					Debug.Log($"Player {player.name} is too far from mount anchor: {distanceFromMountAnchor} > {num}");
					return;
				}
			}
		}
		MountPlayer(player);
	}

	public virtual bool AttemptDismount(BasePlayer player)
	{
		if (player != _mounted)
		{
			return false;
		}
		if (IsTransferring())
		{
			return false;
		}
		if (!AllowPlayerInstigatedDismount(player))
		{
			return false;
		}
		if (VehicleParent() != null && !VehicleParent().AllowPlayerInstigatedDismount(player))
		{
			return false;
		}
		DismountPlayer(player);
		return true;
	}

	public virtual bool AllowPlayerInstigatedDismount(BasePlayer player)
	{
		return true;
	}

	[RPC_Server]
	public void RPC_WantsDismount(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!HasValidDismountPosition(player))
		{
			Interface.CallHook("OnPlayerDismountFailed", player, this);
		}
		else if (Interface.CallHook("OnPlayerWantsDismount", player, this) == null && (!(player != null) || !player.IsRestrained))
		{
			AttemptDismount(player);
		}
	}

	public bool ShouldRepositionPerFrame()
	{
		return mountSyncType != MountSyncType.Parent && (mountSyncType != MountSyncType.ParentAIOnly || !(_mounted is HumanNPC));
	}

	public void MountPlayer(BasePlayer player)
	{
		if (_mounted != null || mountAnchor == null || Interface.CallHook("CanMountEntity", player, this) != null)
		{
			return;
		}
		player.EnsureDismounted();
		_mounted = player;
		Transform transform = mountAnchor;
		player.SetMounted(this);
		if (blockLooting)
		{
			player.inventory.loot.Clear();
		}
		if (!ShouldRepositionPerFrame())
		{
			if (player.GetParentEntity() != this)
			{
				player.SetParent(this, worldPositionStays: true, sendImmediate: true);
			}
			player.transform.localPosition = Vector3.zero;
			player.transform.localRotation = Quaternion.identity;
			player.transform.hasChanged = true;
		}
		else
		{
			player.MovePosition(transform.position);
			player.transform.rotation = transform.rotation;
			player.ServerRotation = transform.rotation;
		}
		player.OverrideViewAngles(transform.rotation.eulerAngles);
		_mounted.eyes.NetworkUpdate(transform.rotation);
		player.SendNetworkUpdateImmediate();
		Facepunch.Rust.Analytics.Azure.OnMountEntity(player, this, VehicleParent());
		OnPlayerMounted();
		Interface.CallHook("OnEntityMounted", this, player);
		if (allowedGestures == MountGestureType.None && player.InGesture)
		{
			player.Server_CancelGesture();
		}
		else if (allowedGestures == MountGestureType.UpperBody && player.InGesture && player.CurrentGestureIsFullBody)
		{
			player.Server_CancelGesture();
		}
		if (this.IsValid() && player.IsValid())
		{
			player.ProcessMissionEvent(BaseMission.MissionEventType.MOUNT_ENTITY, net.ID, 1f);
		}
		SendNetworkUpdate();
	}

	public virtual void OnPlayerMounted()
	{
		if (_mounted != null)
		{
			Mounted.TryAdd(this);
		}
		UpdateMountFlags();
	}

	public virtual void OnPlayerDismounted(BasePlayer player)
	{
		Mounted.Remove(this);
		UpdateMountFlags();
	}

	public virtual void UpdateMountFlags()
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Busy, _mounted != null);
		}
		BaseVehicle baseVehicle = VehicleParent();
		if (baseVehicle != null)
		{
			baseVehicle.UpdateMountFlags();
		}
	}

	public virtual void DismountAllPlayers()
	{
		if ((bool)_mounted)
		{
			DismountPlayer(_mounted);
		}
	}

	public void DismountPlayer(BasePlayer player, bool lite = false)
	{
		if (_mounted == null || _mounted != player || Interface.CallHook("CanDismountEntity", player, this) != null)
		{
			return;
		}
		if (!ShouldRepositionPerFrame())
		{
			_mounted.SetParent(null, worldPositionStays: true, sendImmediate: true);
		}
		BaseVehicle baseVehicle = VehicleParent();
		if (lite)
		{
			if (baseVehicle != null)
			{
				baseVehicle.PrePlayerDismount(player, this);
			}
			_mounted.DismountObject();
			_mounted = null;
			if (baseVehicle != null)
			{
				baseVehicle.PlayerDismounted(player, this);
			}
			OnPlayerDismounted(player);
			Interface.CallHook("OnEntityDismounted", this, player);
			return;
		}
		if (!GetDismountPosition(player, out var res) || Distance(res) > 10f)
		{
			if (baseVehicle != null)
			{
				baseVehicle.PrePlayerDismount(player, this);
			}
			res = player.transform.position;
			_mounted.DismountObject();
			_mounted.MovePosition(res);
			_mounted.transform.rotation = Quaternion.identity;
			_mounted.ClientRPC(RpcTarget.Player("ForcePositionTo", _mounted), res);
			BasePlayer mounted = _mounted;
			_mounted = null;
			Debug.LogWarning("Killing player due to invalid dismount point :" + player.displayName + " / " + player.userID.Get() + " on obj : " + base.gameObject.name);
			mounted.Hurt(1000f, DamageType.Suicide, mounted, useProtection: false);
			if (baseVehicle != null)
			{
				baseVehicle.PlayerDismounted(player, this);
			}
			OnPlayerDismounted(player);
			return;
		}
		if (baseVehicle != null)
		{
			baseVehicle.PrePlayerDismount(player, this);
		}
		if (AntiHack.TestNoClipping(_mounted, res, res, BasePlayer.NoClipRadius(ConVar.AntiHack.noclip_margin), ConVar.AntiHack.noclip_backtracking, out var _, overlapVehicleLayer: true))
		{
			_mounted.PauseVehicleNoClipDetection(5f);
		}
		_mounted.DismountObject();
		_mounted.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
		_mounted.OverrideViewAngles(Vector3.zero);
		_mounted.MovePosition(res);
		_mounted.ForceUpdateTriggers();
		_mounted.SendNetworkUpdateImmediate();
		_mounted.SendModelState(force: true);
		_mounted = null;
		if (baseVehicle != null)
		{
			baseVehicle.PlayerDismounted(player, this);
		}
		if (player.net != null)
		{
			if ((bool)player.GetParentEntity())
			{
				BaseEntity baseEntity = player.GetParentEntity();
				player.ClientRPC(RpcTarget.Player("ForcePositionToParentOffset", player), baseEntity.transform.InverseTransformPoint(res), baseEntity.net.ID);
			}
			else
			{
				player.ClientRPC(RpcTarget.Player("ForcePositionTo", player), res);
				player.ClientRPC(RpcTarget.NetworkGroup("ForceResetRotation", player));
			}
		}
		Facepunch.Rust.Analytics.Azure.OnDismountEntity(player, this, baseVehicle);
		Interface.CallHook("OnEntityDismounted", this, player);
		OnPlayerDismounted(player);
		SendNetworkUpdate();
	}

	public virtual bool GetDismountPosition(BasePlayer player, out Vector3 res, bool silent = false)
	{
		BaseVehicle baseVehicle = VehicleParent();
		if (baseVehicle != null && !baseVehicle.childMountableHandleDismountPoints && baseVehicle.IsVehicleMountPoint(this))
		{
			return baseVehicle.GetDismountPosition(player, out res);
		}
		int num = 0;
		Transform[] array = dismountPositions;
		foreach (Transform transform in array)
		{
			if (!(transform == null))
			{
				if (ValidDismountPosition(player, transform.transform.position))
				{
					res = transform.transform.position;
					return true;
				}
				num++;
			}
		}
		if (!silent)
		{
			Debug.LogWarning("Failed to find dismount position for player :" + player.displayName + " / " + player.userID.Get() + " on obj : " + base.gameObject.name);
		}
		res = player.transform.position;
		return false;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (isMobile && !(this is BaseVehicleMountPoint { RequiresVehicleFixedUpdateOnSeat: false }))
		{
			AllMountables.Add(this);
		}
	}

	internal override void DoServerDestroy()
	{
		DismountAllPlayers();
		AllMountables.Remove(this);
		base.DoServerDestroy();
	}

	public static void FixedUpdateCycle()
	{
		for (int num = AllMountables.Count - 1; num >= 0; num--)
		{
			BaseMountable baseMountable = AllMountables[num];
			if (baseMountable == null)
			{
				AllMountables.RemoveAt(num);
			}
			else if (baseMountable.isSpawned)
			{
				baseMountable.VehicleFixedUpdate();
			}
		}
		for (int num2 = AllMountables.Count - 1; num2 >= 0; num2--)
		{
			BaseMountable baseMountable2 = AllMountables[num2];
			if (baseMountable2 == null)
			{
				AllMountables.RemoveAt(num2);
			}
			else if (baseMountable2.isSpawned)
			{
				baseMountable2.PostVehicleFixedUpdate();
			}
		}
	}

	public static void PlayerSyncCycle()
	{
		for (int num = Mounted.Count - 1; num >= 0; num--)
		{
			BaseMountable baseMountable = Mounted[num];
			if (baseMountable == null || baseMountable.GetMounted() == null)
			{
				Mounted.RemoveAt(num);
			}
			else if (baseMountable.isSpawned)
			{
				baseMountable.MountedPlayerSync();
			}
		}
	}

	public virtual void VehicleFixedUpdate()
	{
		using (TimeWarning.New("BaseMountable.VehicleFixedUpdate"))
		{
			if (!wantsBoundaryRepelCheck || !(rigidBody != null) || rigidBody.IsSleeping() || rigidBody.isKinematic)
			{
				return;
			}
			float world_boundary_force_start_distance = vehicle.world_boundary_force_start_distance;
			float world_boundary_force_offset = vehicle.world_boundary_force_offset;
			bool num = PointEntity<DeepSeaManager>.ServerInstance != null;
			bool flag = num && DeepSeaManager.IsInsideDeepSea(base.transform.position);
			Vector3 center = (flag ? DeepSeaManager.DeepSeaBounds.center.WithY(0f) : Vector3.zero);
			float num2 = (flag ? float.MaxValue : Mathf.Max(0f, ValidBounds.TestDist(this, base.transform.position) - world_boundary_force_offset));
			if (num)
			{
				DeepSeaPortal.PortalModeEnum portalMode = ((!flag) ? DeepSeaPortal.PortalModeEnum.Entrance : DeepSeaPortal.PortalModeEnum.Exit);
				if (DeepSeaManager.IsInsideAnyPortal(base.transform.position, portalMode, out var deepSeaPortal))
				{
					OBB oBB = deepSeaPortal.WorldSpaceBounds();
					Transform transform = deepSeaPortal.transform;
					float num3 = Vector3.Dot(base.transform.position - transform.position, transform.forward);
					float num4 = oBB.extents.z - num3;
					if (num4 < vehicle.deepseaportal_boundary_force_start_distance)
					{
						bool num5 = deepSeaPortal.PortalDirection == DeepSeaManager.GetEntrancePortalDirection();
						bool flag2;
						if (flag)
						{
							(flag2, _) = DeepSeaManager.CanTeleportToMainIsland(this);
						}
						else
						{
							(flag2, _) = DeepSeaManager.CanTeleportToDeepSea(this);
						}
						if (!num5 || !flag2)
						{
							num2 = (flag ? num4 : Mathf.Min(num2, num4));
						}
					}
				}
			}
			if (num2 < world_boundary_force_start_distance)
			{
				ApplyRepelForce(num2, world_boundary_force_start_distance, center);
			}
		}
	}

	private void ApplyRepelForce(float distToWorldEdge, float forceStartDistance, Vector3 center)
	{
		if (distToWorldEdge > forceStartDistance)
		{
			return;
		}
		Vector3 normalized = (base.transform.position - center).normalized;
		float num = Vector3.Dot(rigidBody.linearVelocity, normalized);
		if (num > 0f)
		{
			float num2 = 1f - distToWorldEdge / forceStartDistance;
			rigidBody.linearVelocity -= normalized * num * (num2 * num2);
			if (distToWorldEdge < forceStartDistance * 0.25f)
			{
				float num3 = 1f - distToWorldEdge / (forceStartDistance * 0.25f);
				rigidBody.AddForce(-normalized * 20f * num3, ForceMode.Acceleration);
			}
		}
	}

	public virtual void PostVehicleFixedUpdate()
	{
	}

	public virtual void MountedPlayerSync()
	{
		if ((syncsMountedPlayers || !canPauseMountedPlayerSync) && ShouldRepositionPerFrame())
		{
			_mounted.transform.rotation = mountAnchor.transform.rotation;
			_mounted.ServerRotation = mountAnchor.transform.rotation;
			_mounted.MovePosition(mountAnchor.transform.position);
		}
	}

	public virtual void PlayerServerInput(InputState inputState, BasePlayer player)
	{
	}

	public virtual float GetComfort()
	{
		return 0f;
	}

	public virtual void ScaleDamageForPlayer(BasePlayer player, HitInfo info)
	{
	}

	public bool TryFireProjectile(StorageContainer ammoStorage, AmmoTypes ammoType, Vector3 firingPos, Vector3 firingDir, BasePlayer shooter, float launchOffset, float minSpeed, out ServerProjectile projectile)
	{
		projectile = null;
		if (ammoStorage == null)
		{
			return false;
		}
		ItemContainer inventory = ammoStorage.inventory;
		if (inventory == null)
		{
			return false;
		}
		return TryFireProjectile(inventory, ammoType, firingPos, firingDir, shooter, launchOffset, minSpeed, out projectile);
	}

	public virtual void FilterServerProjectileAmmo(List<Item> ammoList)
	{
	}

	public bool TryFireProjectile(ItemContainer ammoContainer, AmmoTypes ammoType, Vector3 firingPos, Vector3 firingDir, BasePlayer shooter, float launchOffset, float minSpeed, out ServerProjectile projectile)
	{
		projectile = null;
		if (ammoContainer == null)
		{
			return false;
		}
		bool result = false;
		List<Item> obj = Facepunch.Pool.Get<List<Item>>();
		ammoContainer.FindAmmo(obj, ammoType);
		FilterServerProjectileAmmo(obj);
		for (int num = obj.Count - 1; num >= 0; num--)
		{
			if (obj[num].amount <= 0)
			{
				obj.RemoveAt(num);
			}
		}
		if (obj.Count > 0)
		{
			Item ammoItem = obj[obj.Count - 1];
			result = FireProjectile(ammoItem, firingPos, firingDir, shooter, launchOffset, minSpeed, out projectile);
		}
		Facepunch.Pool.Free(ref obj, freeElements: false);
		return result;
	}

	public bool FireProjectile(Item ammoItem, Vector3 firingPos, Vector3 firingDir, BasePlayer shooter, float launchOffset, float minSpeed, out ServerProjectile projectile)
	{
		ItemModProjectile component = ammoItem.info.GetComponent<ItemModProjectile>();
		if (FireProjectile(component.GetOverrideProjectile(this), firingPos, firingDir, shooter, launchOffset, minSpeed, out projectile))
		{
			ammoItem.UseItem();
			return true;
		}
		return false;
	}

	public bool FireProjectile(GameObjectRef projectilePrefab, Vector3 firingPos, Vector3 firingDir, BasePlayer shooter, float launchOffset, float minSpeed, out ServerProjectile projectile)
	{
		if (UnityEngine.Physics.Raycast(firingPos, firingDir, out var hitInfo, launchOffset, 1237003025))
		{
			launchOffset = hitInfo.distance - 0.1f;
		}
		BaseEntity baseEntity = GameManager.server.CreateEntity(projectilePrefab.resourcePath, firingPos + firingDir * launchOffset);
		projectile = baseEntity.GetComponent<ServerProjectile>();
		Vector3 vector = projectile.initialVelocity + firingDir * projectile.speed;
		if (minSpeed > 0f)
		{
			float num = Vector3.Dot(vector, firingDir) - minSpeed;
			if (num < 0f)
			{
				vector += firingDir * (0f - num);
			}
		}
		projectile.InitializeVelocity(vector);
		if (shooter.IsValid())
		{
			baseEntity.creatorEntity = shooter;
			baseEntity.OwnerID = shooter.userID;
		}
		baseEntity.Spawn();
		Facepunch.Rust.Analytics.Azure.OnExplosiveLaunched(shooter, baseEntity, this);
		return true;
	}

	public override void DisableTransferProtection()
	{
		base.DisableTransferProtection();
		BasePlayer mounted = GetMounted();
		if (mounted != null && mounted.IsTransferProtected())
		{
			mounted.DisableTransferProtection();
		}
	}

	protected virtual int GetClipCheckMask()
	{
		return 1210122497;
	}

	public virtual bool IsSeatClipping(BaseMountable mountable)
	{
		if (!clippingAndVisChecks)
		{
			return false;
		}
		if (mountable == null)
		{
			return false;
		}
		int clipCheckMask = GetClipCheckMask();
		Vector3 position = mountable.eyePositionOverride.transform.position;
		Vector3 position2 = mountable.transform.position;
		Vector3 normalized = (position - position2).normalized;
		float num = clippingCheckRadius;
		if (mountable.modifiesPlayerCollider)
		{
			num = Mathf.Min(num, mountable.customPlayerCollider.radius);
		}
		Vector3 startPos = position - normalized * (num - 0.2f);
		return IsSeatClipping(mountable, startPos, num, clipCheckMask, position2, normalized);
	}

	private void DebugSeatClipping(BaseMountable mountable, bool clipped, Vector3 startPos, Vector3 endPos, float radius, int mask, bool headOnly)
	{
		if (Debugging.DebugClippingChecks && clipped)
		{
			Collider[] array = (headOnly ? UnityEngine.Physics.OverlapSphere(startPos, radius, mask, QueryTriggerInteraction.Ignore) : UnityEngine.Physics.OverlapCapsule(startPos, endPos, radius, mask, QueryTriggerInteraction.Ignore));
			foreach (Collider collider in array)
			{
				Transform root = collider.transform.root;
				Debug.Log("[Mount Debug] Clipping blocked by " + collider.name + " (" + root.name + ") on " + mountable.ShortPrefabName);
			}
		}
	}

	public virtual Vector3 GetMountRagdollVelocity(BasePlayer player)
	{
		return Vector3.zero;
	}

	protected virtual bool IsSeatClipping(BaseMountable mountable, Vector3 startPos, float radius, int mask, Vector3 seatPos, Vector3 direction)
	{
		bool flag;
		if (clippingChecksLocation == ClippingCheckLocation.HeadOnly)
		{
			flag = GamePhysics.CheckSphere(GamePhysics.Realm.Server, startPos, radius, mask, QueryTriggerInteraction.Ignore);
			if (Debugging.DebugClippingChecks)
			{
				DebugSeatClipping(mountable, flag, startPos, startPos, radius, mask, headOnly: true);
			}
			return flag;
		}
		Vector3 vector = seatPos + direction * (radius + 0.05f);
		flag = GamePhysics.CheckCapsule(GamePhysics.Realm.Server, startPos, vector, radius, mask, QueryTriggerInteraction.Ignore);
		if (Debugging.DebugClippingChecks)
		{
			DebugSeatClipping(mountable, flag, startPos, vector, radius, mask, headOnly: false);
		}
		return flag;
	}

	public virtual bool IsInstrument()
	{
		return false;
	}

	public virtual Vector3 GetDismountCheckStart(BasePlayer player)
	{
		Vector3 result = GetMountedPosition() + BasePlayer.NoClipOffset();
		Vector3 vector = ((mountAnchor == null) ? base.transform.forward : mountAnchor.transform.forward);
		Vector3 vector2 = ((mountAnchor == null) ? base.transform.up : mountAnchor.transform.up);
		if (mountPose == PlayerModel.MountPoses.Chair)
		{
			result += -vector * 0.32f;
			result += vector2 * 0.25f;
		}
		else if (mountPose == PlayerModel.MountPoses.SitGeneric)
		{
			result += -vector * 0.26f;
			result += vector2 * 0.25f;
		}
		else if (mountPose == PlayerModel.MountPoses.SitGeneric)
		{
			result += -vector * 0.26f;
		}
		return result;
	}

	public virtual Vector3 GetMountedPosition()
	{
		if (mountAnchor == null)
		{
			return base.transform.position;
		}
		return mountAnchor.transform.position;
	}

	public virtual float GetSpeed()
	{
		if (!isMobile)
		{
			return 0f;
		}
		return Vector3.Dot(GetLocalVelocity(), base.transform.forward);
	}

	public bool CanPlayerSeeMountPoint(Ray ray, BasePlayer player, float maxDistance)
	{
		if (player == null)
		{
			return false;
		}
		if (mountAnchor == null)
		{
			return false;
		}
		if (UnityEngine.Physics.SphereCast(ray, 0.25f, out var hitInfo, maxDistance, 1218652417))
		{
			BaseEntity entity = RaycastHitEx.GetEntity(hitInfo);
			if (entity != null)
			{
				if (entity == this || EqualNetID(entity))
				{
					return true;
				}
				if (entity is BasePlayer basePlayer)
				{
					BaseMountable mounted = basePlayer.GetMounted();
					if (mounted == this)
					{
						return true;
					}
					if (mounted != null && mounted.VehicleParent() == this)
					{
						return true;
					}
				}
				BaseEntity baseEntity = entity.GetParentEntity();
				if (RaycastHitEx.IsOnLayer(hitInfo, Rust.Layer.Vehicle_Detailed) && (baseEntity == this || EqualNetID(baseEntity)))
				{
					return true;
				}
			}
		}
		return false;
	}

	public float GetDistanceFromMountAnchor(BasePlayer player)
	{
		return Vector3.Distance(player.transform.position, mountAnchor.position);
	}

	public bool NearMountPoint(BasePlayer player)
	{
		if (player == null)
		{
			return false;
		}
		if (mountAnchor == null)
		{
			return false;
		}
		if (GetDistanceFromMountAnchor(player) > maxMountDistance)
		{
			return false;
		}
		return CanPlayerSeeMountPoint(player.eyes.HeadRay(), player, 2f);
	}

	public bool ClothingBlocksMounting(BasePlayer player)
	{
		if (BypassClothingMountBlocks)
		{
			return false;
		}
		foreach (Item item in player.inventory.containerWear.itemList)
		{
			if (item.info.ItemModWearable != null && item.info.ItemModWearable.preventsMounting)
			{
				return true;
			}
		}
		return false;
	}

	public static Vector3 ConvertVector(Vector3 vec)
	{
		for (int i = 0; i < 3; i++)
		{
			if (vec[i] > 180f)
			{
				vec[i] -= 360f;
			}
			else if (vec[i] < -180f)
			{
				vec[i] += 360f;
			}
		}
		return vec;
	}

	public override bool CanBeRedirectSwapped(BasePlayer player)
	{
		if (AnyMounted())
		{
			BasePlayer mounted = GetMounted();
			SprayCan.LastReskinError = SprayCan.PlayerIsMounted;
			SprayCan.LastReskinErrorArgString = NameHelper.GetPlayerNameStreamSafe(player, mounted);
			return false;
		}
		return base.CanBeRedirectSwapped(player);
	}
}
