#define UNITY_ASSERTIONS
using System;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using Rust;
using Rust.Safety;
using UnityEngine;
using UnityEngine.Assertions;

public class MountedWeapon : StorageContainer
{
	private struct LoadDataCache
	{
		public int ammoId;

		public int ammoCount;
	}

	[ServerVar]
	public static int antihack_level = 0;

	[ServerVar]
	public static float antihack_max_snap_degrees = 35f;

	[ServerVar]
	public static float antihack_max_degrees_per_second_yaw = 720f;

	[ServerVar]
	public static float antihack_max_degrees_per_second_pitch = 720f;

	[ReplicatedVar]
	public static bool ENABLE_CLIENT_AUTHORITY = true;

	[ReplicatedVar]
	public static bool DEBUG = false;

	private static readonly int Up = Animator.StringToHash("up");

	[Header("Mounted Weapon")]
	[SerializeField]
	private Transform _eyes;

	[SerializeField]
	private bool _usingSights;

	[SerializeField]
	private bool _flipPitch;

	[SerializeField]
	private bool _invertForward;

	[SerializeField]
	private bool _clientAuthority;

	[SerializeField]
	[ItemSelector]
	private ItemDefinition _ammoItem;

	[SerializeField]
	[Header("Mounted Weapon - Weapon")]
	private ItemDefinition _weapon;

	[SerializeField]
	private Transform _attachPoint;

	[SerializeField]
	private Transform _yawPivot;

	[SerializeField]
	private Transform _pitchPivot;

	[SerializeField]
	private GameObjectRef _screenshakeEffect;

	[SerializeField]
	private GameObjectRef _dryFireEffect;

	[ItemSelector]
	public ItemDefinition AmmoDef;

	[SerializeField]
	[Header("Mounted Weapon - Second Weapon")]
	private ItemDefinition _weapon2;

	[SerializeField]
	private Transform _attachPoint2;

	[Header("Mounted Weapon - Player General Animation")]
	[SerializeField]
	private int _turretAnimationType;

	[SerializeField]
	private Transform _leftHandIdleIKPosition;

	[SerializeField]
	private Transform _rightHandIdleIKPosition;

	[SerializeField]
	private AnimationCurve _reloadIKBlendCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[SerializeField]
	private bool _walkAroundTurret;

	[SerializeField]
	private float _minWalkAroundDistance = 1.11f;

	[SerializeField]
	private float _walkAroundDistance = 1.11f;

	[SerializeField]
	private float _reloadWalkAroundDistance = 1.11f;

	[SerializeField]
	private AnimationCurve _walkAroundDistanceCurve;

	[SerializeField]
	private AnimationCurve _reloadWalkAroundBlendCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[SerializeField]
	private bool _forceSeatPositionUpdates;

	[SerializeField]
	[Header("Mounted Weapon - Player Camera Animation")]
	private Transform _cameraAnimation;

	[SerializeField]
	private Animator _cameraAnimationController;

	[SerializeField]
	private float _fovMultiplier = 1f;

	[Header("Mounted Weapon - Viewmodel")]
	[SerializeField]
	private bool _useViewmodel;

	[SerializeField]
	private ViewModel _viewmodel;

	[Header("Mounted Weapon - Aim Movement Sounds")]
	[SerializeField]
	private SoundDefinition aimMovementSoundDef;

	[SerializeField]
	private SoundDefinition aimMovementYawSoundDef;

	[SerializeField]
	private SoundDefinition aimMovementPitchSoundDef;

	[SerializeField]
	private AnimationCurve aimMovementGainCurve;

	[SerializeField]
	private float aimMovementSpeedDecayRate = 200f;

	[SerializeField]
	private float aimMovementSpeedMax = 100f;

	public const Flags Flag_WeaponAttached = Flags.Reserved15;

	public const Flags Flag_Lights = Flags.Reserved5;

	private static readonly Translate.Phrase _ammoPhrase = new Translate.Phrase("mountedweapon.reload.tip", "You need regular 5.56 ammo in your inventory to reload.");

	private static readonly Translate.Phrase _ammoFullPhrase = new Translate.Phrase("mountedweapon.reload.full.tip", "Can't reload. Ammo is already full!");

	private Vector3 _defaultEyePosition;

	private Quaternion _defaultEyeRotation;

	private EntityRef<HeldEntity> _attachedEntity;

	private EntityRef<HeldEntity> _attachedEntity2;

	private float _reloadTime;

	private MountedWeaponSeat _seat;

	private BasePlayer _mountedPlayer;

	private Vector3 _seatRelativePosition;

	private float _targetWorldYaw;

	private float _targetWorldPitch;

	private float _worldPitch;

	private float _worldYaw;

	private float _startTime;

	private float _reloadServerTimer;

	private float _lastAimRpcTime;

	private float _lastAimYaw;

	private float _lastAimPitch;

	private int[] _reloadStartMag = new int[2];

	private int[] _reloadTaken = new int[2];

	private LoadDataCache? _loadDataCache;

	private ServersideMountedWeaponSnapshot __sync_Snapshot;

	private NetworkableId __sync_GunId;

	private NetworkableId __sync_Gun2Id;

	private bool __sync_IsReloading;

	private bool __sync_IsEmpty;

	private GameObject _worldModel => GetComponentInChildren<BaseProjectile>(includeInactive: true).gameObject;

	public ItemDefinition WeaponDef => _weapon;

	[Sync(RequireChange = false, Pack = false)]
	private ServersideMountedWeaponSnapshot Snapshot
	{
		[CompilerGenerated]
		get
		{
			return __sync_Snapshot;
		}
		[CompilerGenerated]
		set
		{
			__sync_Snapshot = value;
			byte nameID = __GetWeaverID("Snapshot");
			SV_SyncVarSend(nameID);
		}
	}

	[Sync(Autosave = true)]
	private NetworkableId GunId
	{
		[CompilerGenerated]
		get
		{
			return __sync_GunId;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_GunId, value))
			{
				__sync_GunId = value;
				byte nameID = __GetWeaverID("GunId");
				QueueSyncVar(nameID);
			}
		}
	}

	[Sync(Autosave = true)]
	private NetworkableId Gun2Id
	{
		[CompilerGenerated]
		get
		{
			return __sync_Gun2Id;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_Gun2Id, value))
			{
				__sync_Gun2Id = value;
				byte nameID = __GetWeaverID("Gun2Id");
				QueueSyncVar(nameID);
			}
		}
	}

	[Sync(Pack = false)]
	public bool IsReloading
	{
		[CompilerGenerated]
		get
		{
			return __sync_IsReloading;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_IsReloading, value))
			{
				__sync_IsReloading = value;
				byte nameID = __GetWeaverID("IsReloading");
				SV_SyncVarSend(nameID);
			}
		}
	}

	[Sync(Autosave = true)]
	public bool IsEmpty
	{
		[CompilerGenerated]
		get
		{
			return __sync_IsEmpty;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_IsEmpty, value))
			{
				__sync_IsEmpty = value;
				byte nameID = __GetWeaverID("IsEmpty");
				QueueSyncVar(nameID);
			}
		}
	}

	private bool HasSecondWeapon
	{
		get
		{
			if (_attachPoint2 != null)
			{
				return _weapon2 != null;
			}
			return false;
		}
	}

	public Transform PitchPivot => _pitchPivot;

	public bool PilotedByAi
	{
		get
		{
			if (_seat != null)
			{
				return _seat.GetMounted() is HumanNPC;
			}
			return false;
		}
	}

	private bool HasServerAuthority
	{
		get
		{
			if (_clientAuthority)
			{
				if (_seat != null)
				{
					return _seat.GetMounted() is HumanNPC;
				}
				return false;
			}
			return true;
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("MountedWeapon.OnRpcMessage"))
		{
			if (rpc == 2998965234u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - SV_ReceiveClientAim");
				}
				using (TimeWarning.New("SV_ReceiveClientAim"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2998965234u, "SV_ReceiveClientAim", this, player, 100uL))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(2998965234u, "SV_ReceiveClientAim", this, player, 3f))
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
							SV_ReceiveClientAim(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in SV_ReceiveClientAim");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public BaseEntity GetOwnerEntity()
	{
		return this;
	}

	public MountedWeaponSeat GetSeat()
	{
		return _seat;
	}

	public void AssignSeat(MountedWeaponSeat seat)
	{
		_seat = seat;
		_seatRelativePosition = base.transform.InverseTransformPoint(seat.transform.position);
	}

	public Quaternion EyeRotationForPlayer(BasePlayer player, Quaternion baseEyeRot)
	{
		if (IsReloading && _cameraAnimation != null)
		{
			return baseEyeRot * _cameraAnimation.localRotation;
		}
		return baseEyeRot;
	}

	public Transform GetCustomEyes()
	{
		return _eyes;
	}

	private void ShowDebug()
	{
		if (DEBUG && base.isServer)
		{
			Vector3 vector = base.transform.position + Vector3.up * 3.5f;
			UnityEngine.DDraw.BroadcastText(vector, "SERVER\n" + $"WorldYaw:   {_worldYaw:F1}\n" + $"WorldPitch: {_worldPitch:F1}\n" + $"TargetYaw:  {_targetWorldYaw:F1}\n" + $"TargetPitch:{_targetWorldPitch:F1}", Color.yellow, 0f);
			WorldAngleToTurretAngle(_worldYaw, _worldPitch, out var turretYaw, out var turretPitch);
			UnityEngine.DDraw.BroadcastText(vector + Vector3.up * 1.2f, $"Server Local\nYaw: {turretYaw:F1}\nPitch: {turretPitch:F1}", Color.white, 0f);
		}
	}

	public Vector3 EyePositionForPlayer(BasePlayer player, Quaternion lookRot, Vector3 baseEyePos)
	{
		if (IsReloading && _cameraAnimation != null)
		{
			return baseEyePos + _cameraAnimation.localPosition;
		}
		return baseEyePos;
	}

	private void Tick()
	{
		_clientAuthority = ENABLE_CLIENT_AUTHORITY;
		ShowDebug();
		if (!_seat || !_seat.AnyMounted())
		{
			return;
		}
		if (IsReloading)
		{
			LerpToZero();
		}
		Vector3 gunForward = GetGunForward();
		_seat.transform.forward = gunForward;
		if (_walkAroundTurret || _forceSeatPositionUpdates)
		{
			using (TimeWarning.New("MountedWeapon.Update.SeatPosition"))
			{
				_seatRelativePosition = base.transform.InverseTransformPoint(_seat.transform.position);
				float num = _walkAroundDistance;
				Vector2 pitchClamp = _seat.GetPitchClamp();
				float value = 0f;
				if (base.isServer)
				{
					value = _worldPitch;
				}
				if (true)
				{
					float time = Mathf.InverseLerp(pitchClamp.x, pitchClamp.y, value);
					float value2 = _walkAroundDistanceCurve.Evaluate(time);
					value2 = Mathf.Clamp01(value2);
					num = Mathf.Lerp(_minWalkAroundDistance, _walkAroundDistance, value2);
				}
				Vector3 position = base.transform.position + -GetGunForward() * num;
				position.y = base.transform.TransformPoint(_seatRelativePosition).y;
				_seat.transform.position = position;
			}
		}
		float num2 = 65f;
		if (_usingSights)
		{
			num2 = 110f;
		}
		if (!base.isServer)
		{
			return;
		}
		using (TimeWarning.New("MountedWeapon.Tick.Server"))
		{
			if (!_clientAuthority || (_seat != null && _seat.GetMounted() is HumanNPC))
			{
				_worldYaw = Mathf.LerpAngle(_worldYaw, _targetWorldYaw, UnityEngine.Time.deltaTime * num2);
				_worldPitch = Mathf.LerpAngle(_worldPitch, _targetWorldPitch, UnityEngine.Time.deltaTime * num2);
				WorldAngleToTurretAngle(_worldYaw, _worldPitch, out var turretYaw, out var turretPitch);
				Vector2 yawClamp = GetSeat().GetYawClamp();
				Vector2 pitchClamp2 = GetSeat().GetPitchClamp();
				turretYaw = Mathf.Clamp(turretYaw, yawClamp.x, yawClamp.y);
				turretPitch = Mathf.Clamp(turretPitch, pitchClamp2.x, pitchClamp2.y);
				_yawPivot.localRotation = Quaternion.Euler(0f, turretYaw, 0f);
				_pitchPivot.localRotation = Quaternion.Euler(turretPitch, 0f, 0f);
			}
		}
	}

	private void Update()
	{
		Tick();
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		_attachedEntity.uid = GunId;
		_attachedEntity2.uid = Gun2Id;
		CalculateReloadTime();
		if (!info.fromDisk)
		{
			return;
		}
		if (base.isServer)
		{
			UpdateClient(force: true);
			if (info.msg.mountedWeapon != null)
			{
				_loadDataCache = new LoadDataCache
				{
					ammoId = info.msg.mountedWeapon.ammoItemID,
					ammoCount = info.msg.mountedWeapon.ammoStackSize
				};
				SetupTurretsWithLoadData();
			}
		}
		GetZeroInWorldAngles(out var worldYaw, out var worldPitch);
		SetTargetAngles(worldYaw, worldPitch, set: true);
	}

	private void SetTargetAngles(float worldYaw, float worldPitch, bool set = false)
	{
		if (GetOwnerEntity() == null || GetSeat() == null)
		{
			return;
		}
		_targetWorldYaw = worldYaw;
		_targetWorldPitch = worldPitch;
		if (set)
		{
			if (base.isServer)
			{
				_worldYaw = worldYaw;
				_worldPitch = worldPitch;
			}
			_targetWorldYaw = worldYaw;
			_targetWorldPitch = worldPitch;
			WorldAngleToTurretAngle(_targetWorldYaw, _targetWorldPitch, out var turretYaw, out var turretPitch);
			Vector2 yawClamp = GetSeat().GetYawClamp();
			Vector2 pitchClamp = GetSeat().GetPitchClamp();
			turretYaw = Mathf.Clamp(turretYaw, yawClamp.x, yawClamp.y);
			turretPitch = Mathf.Clamp(turretPitch, pitchClamp.x, pitchClamp.y);
			_yawPivot.localRotation = Quaternion.Euler(0f, turretYaw, 0f);
			_pitchPivot.localRotation = Quaternion.Euler(turretPitch, 0f, 0f);
		}
		base.transform.rotation = Quaternion.Euler(0f, base.transform.rotation.eulerAngles.y, 0f);
	}

	private void GetZeroInWorldAngles(out float worldYaw, out float worldPitch)
	{
		TurretAngleToWorldAngle(0f, 0f, out worldYaw, out worldPitch);
	}

	private Vector3 GetGunForward()
	{
		if (PitchPivot == null)
		{
			return Vector3.forward;
		}
		Vector3 vector = PitchPivot.forward;
		if (_invertForward)
		{
			vector = -vector;
		}
		return vector;
	}

	private void CalculateReloadTime()
	{
		_reloadTime = 0f;
		BaseProjectile baseProjectile = GetWeaponEntity() as BaseProjectile;
		if (baseProjectile != null)
		{
			_reloadTime = baseProjectile.reloadTime;
		}
		BaseProjectile baseProjectile2 = GetWeaponEntity2() as BaseProjectile;
		if (baseProjectile2 != null)
		{
			_reloadTime = Mathf.Max(_reloadTime, baseProjectile2.reloadTime);
		}
	}

	private static float NormalizeAngle(float angle)
	{
		angle %= 360f;
		if (angle > 180f)
		{
			angle -= 360f;
		}
		if (angle < -180f)
		{
			angle += 360f;
		}
		return angle;
	}

	private HeldEntity GetWeaponEntity()
	{
		HeldEntity heldEntity = _attachedEntity.Get(base.isServer);
		if (heldEntity.IsValid())
		{
			return heldEntity;
		}
		return null;
	}

	private HeldEntity GetWeaponEntity2()
	{
		HeldEntity heldEntity = _attachedEntity2.Get(base.isServer);
		if (heldEntity.IsValid())
		{
			return heldEntity;
		}
		return null;
	}

	private void TurretAngleToWorldAngle(float turretYaw, float turretPitch, out float worldYaw, out float worldPitch)
	{
		float num = (_flipPitch ? (-1f) : 1f);
		Vector3 direction = Quaternion.Euler((0f - turretPitch) * num, turretYaw, 0f) * Vector3.forward;
		Vector3 normalized = base.transform.TransformDirection(direction).normalized;
		worldYaw = Mathf.Atan2(normalized.x, normalized.z) * 57.29578f;
		worldPitch = (0f - Mathf.Asin(normalized.y)) * 57.29578f;
	}

	private void WorldAngleToTurretAngle(float worldYaw, float worldPitch, out float turretYaw, out float turretPitch)
	{
		Vector3 direction = Quaternion.Euler(new Vector3(worldPitch, worldYaw, 0f)) * Vector3.forward;
		base.transform.rotation = Quaternion.Euler(0f, base.transform.rotation.eulerAngles.y, 0f);
		Vector3 eulerAngles = Quaternion.LookRotation(base.transform.InverseTransformDirection(direction).normalized, base.transform.up).eulerAngles;
		turretYaw = eulerAngles.y;
		turretPitch = (0f - eulerAngles.x) * (_flipPitch ? (-1f) : 1f);
		turretYaw = NormalizeAngle(turretYaw);
		turretPitch = NormalizeAngle(turretPitch);
	}

	private void LerpToZero()
	{
		float num = 25f;
		WorldAngleToTurretAngle(_worldYaw, _worldPitch, out var turretYaw, out var _);
		TurretAngleToWorldAngle(turretYaw, 0f, out var worldYaw, out var worldPitch);
		if (base.isServer)
		{
			_worldYaw = Mathf.MoveTowardsAngle(_worldYaw, worldYaw, UnityEngine.Time.deltaTime * num);
			_worldPitch = Mathf.MoveTowardsAngle(_worldPitch, worldPitch, UnityEngine.Time.deltaTime * num);
			SetTargetAngles(_worldYaw, _worldPitch);
			UpdateClient(force: true);
		}
	}

	private void HandleAiming(InputState inputState, BasePlayer player, bool asClient, float currentPitch, float currentYaw)
	{
		float num = 1.5f;
		bool flag = false;
		if (base.isServer)
		{
			flag = HasServerAuthority && !asClient;
		}
		if (_seat == null || !_seat.AnyMounted() || IsReloading || player == null || player.eyes == null || Snapshot == null)
		{
			return;
		}
		bool flag2 = false;
		if (!flag)
		{
			return;
		}
		if (_usingSights)
		{
			float num2 = (0f - inputState.current.mouseDelta.y) * num;
			float num3 = inputState.current.mouseDelta.x * num;
			WorldAngleToTurretAngle(currentYaw, currentPitch, out var turretYaw, out var turretPitch);
			turretYaw += num3;
			turretPitch -= num2;
			Vector2 yawClamp = _seat.GetYawClamp();
			Vector2 pitchClamp = _seat.GetPitchClamp();
			turretYaw = Mathf.Clamp(turretYaw, yawClamp.x, yawClamp.y);
			turretPitch = Mathf.Clamp(turretPitch, pitchClamp.x, pitchClamp.y);
			TurretAngleToWorldAngle(turretYaw, turretPitch, out var worldYaw, out var worldPitch);
			SetTargetAngles(worldYaw, worldPitch);
			float num4 = 0.15f;
			if (Mathf.Abs(Mathf.DeltaAngle(worldYaw, currentYaw)) > num4 || Mathf.Abs(Mathf.DeltaAngle(worldPitch, currentPitch)) > num4)
			{
				flag2 = true;
			}
		}
		else
		{
			Quaternion quaternion = Quaternion.LookRotation(Ballistics.GetBulletHitPoint(new Ray(player.eyes.position + player.eyes.HeadForward() * 0.5f, player.eyes.HeadForward()), this) - base.transform.position);
			Quaternion.Euler(currentPitch, currentYaw, 0f);
			Vector3 eulerAngles = quaternion.eulerAngles;
			float y = eulerAngles.y;
			float x = eulerAngles.x;
			WorldAngleToTurretAngle(y, x, out var turretYaw2, out var turretPitch2);
			Vector2 yawClamp2 = _seat.GetYawClamp();
			Vector2 pitchClamp2 = _seat.GetPitchClamp();
			turretYaw2 = Mathf.Clamp(turretYaw2, yawClamp2.x, yawClamp2.y);
			turretPitch2 = Mathf.Clamp(turretPitch2, pitchClamp2.x, pitchClamp2.y);
			TurretAngleToWorldAngle(turretYaw2, turretPitch2, out var worldYaw2, out var worldPitch2);
			if (Mathf.Abs(Mathf.DeltaAngle(worldYaw2, currentYaw)) > 0.05f || Mathf.Abs(Mathf.DeltaAngle(worldPitch2, currentPitch)) > 0.05f)
			{
				SetTargetAngles(worldYaw2, worldPitch2);
			}
			float num5 = 1f;
			if (Mathf.Abs(Mathf.DeltaAngle(worldYaw2, currentYaw)) > num5 || Mathf.Abs(Mathf.DeltaAngle(worldPitch2, currentPitch)) > num5)
			{
				flag2 = true;
			}
		}
		if (flag2 && !base.isClient)
		{
			UpdateClient();
		}
	}

	public void OnPlayerMounted()
	{
		UpdateClient(force: true);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		ItemContainer itemContainer = base.inventory;
		itemContainer.canAcceptItem = (Func<BasePlayer, Item, int, bool>)Delegate.Combine(itemContainer.canAcceptItem, new Func<BasePlayer, Item, int, bool>(CanAcceptItem));
		Invoke(delegate
		{
			UpdateAttachedWeapon(_weapon, _attachPoint);
			if (HasSecondWeapon)
			{
				UpdateAttachedWeapon(_weapon, _attachPoint2, second: true);
			}
		}, 0.5f);
		_startTime = UnityEngine.Time.realtimeSinceStartup;
		_reloadServerTimer = 0f;
		GetZeroInWorldAngles(out var worldYaw, out var worldPitch);
		_worldYaw = worldYaw;
		_worldPitch = worldPitch;
		SetTargetAngles(_worldYaw, _worldPitch);
		Invoke(delegate
		{
			UpdateClient(force: true);
		}, 1f);
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved5, b: false);
		}
		_lastAimRpcTime = UnityEngine.Time.realtimeSinceStartup;
		_lastAimYaw = _worldYaw;
		_lastAimPitch = _worldPitch;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.forDisk)
		{
			info.msg.mountedWeapon = Facepunch.Pool.Get<ProtoBuf.MountedWeapon>();
			BaseProjectile baseProjectile = GetWeaponEntity() as BaseProjectile;
			if (baseProjectile != null)
			{
				info.msg.mountedWeapon.ammoItemID = baseProjectile.primaryMagazine.ammoType.itemid;
				info.msg.mountedWeapon.ammoStackSize = baseProjectile.primaryMagazine.contents;
			}
		}
	}

	private void RefundAmmo(BasePlayer player, int amount)
	{
		if (amount > 0 && !(player == null))
		{
			Item item = ItemManager.Create(AmmoDef, amount, 0uL, isServerSide: true, 0uL);
			if (!item.MoveToContainer(player.inventory.containerMain))
			{
				item.Drop(player.transform.position + Vector3.up, Vector3.down);
			}
		}
	}

	public void OnPlayerDismounted(BasePlayer player)
	{
		HeldEntity weaponEntity = GetWeaponEntity();
		HeldEntity weaponEntity2 = GetWeaponEntity2();
		bool isReloading = IsReloading;
		if (weaponEntity != null)
		{
			weaponEntity.forcedOwner = null;
			if (isReloading)
			{
				IsReloading = false;
				_reloadServerTimer = 0f;
				CancelInvoke(ProcessServerReloadTimer);
				if (weaponEntity is BaseProjectile baseProjectile)
				{
					RefundAmmo(player, _reloadTaken[0]);
					baseProjectile.primaryMagazine.contents = _reloadStartMag[0];
					IsEmpty = _reloadStartMag[0] == 0;
					baseProjectile.SkipReload();
				}
			}
		}
		if (weaponEntity2 != null)
		{
			weaponEntity2.forcedOwner = null;
			if (isReloading && weaponEntity2 is BaseProjectile baseProjectile2)
			{
				RefundAmmo(player, _reloadTaken[1]);
				baseProjectile2.primaryMagazine.contents = _reloadStartMag[1];
				IsEmpty = _reloadStartMag[1] == 0;
				baseProjectile2.SkipReload();
			}
		}
		_reloadTaken[0] = (_reloadTaken[1] = 0);
		_reloadStartMag[0] = (_reloadStartMag[1] = 0);
	}

	private void SetupTurretsWithLoadData()
	{
		if (!_loadDataCache.HasValue)
		{
			return;
		}
		BaseProjectile baseProjectile = GetWeaponEntity() as BaseProjectile;
		ItemDefinition itemDefinition = ItemManager.FindItemDefinition(_loadDataCache.Value.ammoId);
		if (!(itemDefinition == null))
		{
			if (baseProjectile != null)
			{
				baseProjectile.primaryMagazine.ammoType = itemDefinition;
				baseProjectile.primaryMagazine.contents = _loadDataCache.Value.ammoCount;
			}
			BaseProjectile baseProjectile2 = GetWeaponEntity2() as BaseProjectile;
			if (baseProjectile2 != null)
			{
				baseProjectile2.primaryMagazine.ammoType = itemDefinition;
				baseProjectile2.primaryMagazine.contents = _loadDataCache.Value.ammoCount;
			}
		}
	}

	public void LightToggle(BasePlayer basePlayer)
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved5, !HasFlag(Flags.Reserved5));
	}

	public override void AdminKill()
	{
		BaseEntity baseEntity = GetParentEntity();
		if (baseEntity != null)
		{
			baseEntity.AdminKill();
		}
		base.AdminKill();
	}

	public override BasePlayer ToPlayer()
	{
		BaseEntity ownerEntity = GetOwnerEntity();
		if (ownerEntity != null && ownerEntity is BaseVehicleSeat baseVehicleSeat)
		{
			return baseVehicleSeat.GetMounted();
		}
		return null;
	}

	public bool Fire(bool isAi = false)
	{
		if (IsReloading)
		{
			return false;
		}
		BaseProjectile baseProjectile = GetWeaponEntity() as BaseProjectile;
		BaseProjectile baseProjectile2 = GetWeaponEntity2() as BaseProjectile;
		int num = 0 | (TryFireWeapon(baseProjectile) ? 1 : 0) | (TryFireWeapon(baseProjectile2) ? 1 : 0);
		if (baseProjectile != null)
		{
			IsEmpty = baseProjectile.AmmoFraction() <= 0f && (baseProjectile2 == null || baseProjectile2.AmmoFraction() <= 0f);
		}
		if (((uint)num & (isAi ? 1u : 0u)) != 0)
		{
			ClientRPC(RpcTarget.NetworkGroup("CL_OnAttack"));
		}
		return (byte)num != 0;
	}

	public void PlayerServerInput(InputState inputState, BasePlayer player)
	{
		using (TimeWarning.New("ServersideMountedWeapon.ServerInput"))
		{
			if (inputState.IsDown(BUTTON.FIRE_PRIMARY) && !IsReloading)
			{
				if (player.InSafeZone())
				{
					return;
				}
				if (Fire())
				{
					player.MarkHostileFor();
					ClientRPC(RpcTarget.NetworkGroup("CL_OnAttack"));
				}
			}
			inputState.IsDown(BUTTON.FIRE_SECONDARY);
			if (inputState.IsDown(BUTTON.RELOAD) && !IsReloading)
			{
				using PooledList<Item> pooledList = Facepunch.Pool.Get<PooledList<Item>>();
				player.inventory.FindAmmo(pooledList, AmmoTypes.RIFLE_556MM);
				bool flag = false;
				foreach (Item item in pooledList)
				{
					if (item.info.itemid == _ammoItem.itemid)
					{
						flag = true;
					}
				}
				if (flag)
				{
					BaseProjectile baseProjectile = GetWeaponEntity() as BaseProjectile;
					BaseProjectile baseProjectile2 = GetWeaponEntity2() as BaseProjectile;
					if (baseProjectile != null && baseProjectile.AmmoFraction() >= 1f && (baseProjectile2 == null || baseProjectile2.AmmoFraction() >= 1f))
					{
						player.ShowToast(GameTip.Styles.Blue_Normal, _ammoFullPhrase, false);
						return;
					}
					if (_reloadTime == 0f)
					{
						CalculateReloadTime();
					}
					_reloadServerTimer = _reloadTime;
					StartReload();
				}
				else
				{
					player.ShowToast(GameTip.Styles.Blue_Normal, _ammoPhrase, false);
				}
			}
			if (!IsReloading)
			{
				HandleAiming(inputState, player, asClient: false, _worldPitch, _worldYaw);
			}
		}
	}

	public void CheckAiReload()
	{
		bool isReloading = false;
		bool flag = false;
		if (GetWeaponEntity() is BaseProjectile baseProjectile)
		{
			if (baseProjectile.ServerIsReloading())
			{
				isReloading = true;
			}
			if (!baseProjectile.ServerIsReloading() && baseProjectile.primaryMagazine.contents <= 0)
			{
				flag = true;
				baseProjectile.ServerReload();
			}
		}
		if (GetWeaponEntity2() is BaseProjectile baseProjectile2)
		{
			if (baseProjectile2.ServerIsReloading())
			{
				isReloading = true;
			}
			if (!baseProjectile2.ServerIsReloading() && baseProjectile2.primaryMagazine.contents <= 0)
			{
				flag = true;
				baseProjectile2.ServerReload();
			}
		}
		IsReloading = isReloading;
		if (flag)
		{
			float time = UnityEngine.Time.time;
			float arg = time + _reloadTime;
			ClientRPC(RpcTarget.NetworkGroup("CL_StartReloading"), time, arg);
		}
	}

	public void AimAt(Vector3 origin, Vector3 desiredGunForward, bool forceFlip)
	{
		float worldYaw = Mathf.Atan2(desiredGunForward.x, desiredGunForward.z) * 57.29578f;
		float worldPitch = (0f - Mathf.Asin(desiredGunForward.y)) * 57.29578f;
		WorldAngleToTurretAngle(worldYaw, worldPitch, out var turretYaw, out var turretPitch);
		if (forceFlip)
		{
			turretPitch = 0f - turretPitch;
		}
		turretYaw = NormalizeAngle(turretYaw);
		turretPitch = NormalizeAngle(turretPitch);
		TurretAngleToWorldAngle(turretYaw, turretPitch, out var worldYaw2, out var worldPitch2);
		_worldYaw = Mathf.MoveTowardsAngle(_worldYaw, worldYaw2, UnityEngine.Time.deltaTime * 105f);
		_worldPitch = Mathf.MoveTowardsAngle(_worldPitch, worldPitch2, UnityEngine.Time.deltaTime * 105f);
		SetTargetAngles(_worldYaw, _worldPitch, set: true);
		if (Snapshot == null || Mathf.Abs(Mathf.DeltaAngle(Snapshot.yaw, _worldYaw)) > 3f || Mathf.Abs(Mathf.DeltaAngle(Snapshot.pitch, _worldPitch)) > 3f)
		{
			UpdateClient(force: true);
		}
	}

	private void UpdateClient(bool force = false)
	{
		Snapshot = new ServersideMountedWeaponSnapshot
		{
			time = UnityEngine.Time.realtimeSinceStartup - _startTime,
			yaw = _worldYaw,
			pitch = _worldPitch,
			force = force
		};
	}

	private bool CanAcceptItem(BasePlayer player, Item item, int targetSlot)
	{
		if (Check.IsValidWeapon(item, checkCanUseTurret: true) && targetSlot == 0)
		{
			return true;
		}
		if (item.info.category == ItemCategory.Ammunition)
		{
			return true;
		}
		return false;
	}

	private bool TryFireWeapon(HeldEntity heldEntity)
	{
		if (heldEntity == null)
		{
			return false;
		}
		if (_seat != null)
		{
			BasePlayer mounted = _seat.GetMounted();
			heldEntity.forcedOwner = mounted;
			heldEntity.useOwnerForward = false;
		}
		if (heldEntity is BaseProjectile baseProjectile)
		{
			if (baseProjectile.NextAttackTime > UnityEngine.Time.time)
			{
				return false;
			}
			if (baseProjectile.primaryMagazine.contents <= 0)
			{
				ClientRPC(RpcTarget.NetworkGroup("CL_OnDryFire"), _mountedPlayer);
				baseProjectile.StartAttackCooldown(1f);
				return false;
			}
			if (baseProjectile is ITurretNotify turretNotify)
			{
				turretNotify.WarmupTick(wantsShoot: true);
				if (!turretNotify.CanShoot())
				{
					return false;
				}
			}
		}
		heldEntity.ServerUse();
		heldEntity.useOwnerForward = false;
		heldEntity.forcedOwner = null;
		return true;
	}

	private void UpdateAttachedWeapon(ItemDefinition weapon, Transform attachPoint, bool second = false)
	{
		HeldEntity heldEntity = AutoTurret.TryAddWeaponToTurret(ItemManager.Create(second ? _weapon2 : weapon, 1, 0uL, isServerSide: true, 0uL), attachPoint, this, -0.5f);
		heldEntity.transform.localPosition = Vector3.zero;
		if (HasSecondWeapon)
		{
			Quaternion quaternion = Quaternion.LookRotation(((_attachPoint.position + _attachPoint2.position) / 2f + attachPoint.forward * 10f - attachPoint.position).normalized, base.transform.up);
			heldEntity.transform.localRotation = Quaternion.Inverse(attachPoint.rotation) * quaternion;
		}
		bool flag = heldEntity != null;
		if (flag)
		{
			if (!second)
			{
				_attachedEntity.Set(heldEntity);
				GunId = _attachedEntity.uid;
			}
			else
			{
				_attachedEntity2.Set(heldEntity);
				Gun2Id = _attachedEntity2.uid;
			}
		}
		else
		{
			HeldEntity heldEntity2 = GetWeaponEntity();
			if (second)
			{
				heldEntity2 = GetWeaponEntity2();
			}
			if (heldEntity2 != null)
			{
				heldEntity2.SetGenericVisible(wantsVis: false);
				heldEntity2.SetLightsOn(isOn: false);
				if (heldEntity2 is ITurretNotify turretNotify)
				{
					turretNotify.WarmupTick(wantsShoot: false);
				}
			}
			if (!second)
			{
				_attachedEntity.Set(null);
				GunId = _attachedEntity.uid;
			}
			else
			{
				_attachedEntity2.Set(null);
				Gun2Id = _attachedEntity2.uid;
			}
		}
		SetupTurretsWithLoadData();
		SetFlagLocal(Flags.Reserved15, flag);
		SendNetworkUpdate();
	}

	private void StartReload()
	{
		if (_seat == null)
		{
			return;
		}
		BasePlayer mounted = _seat.GetMounted();
		if (!(mounted == null))
		{
			CalculateReloadTime();
			BaseProjectile baseProjectile = GetWeaponEntity() as BaseProjectile;
			BaseProjectile baseProjectile2 = GetWeaponEntity2() as BaseProjectile;
			_reloadStartMag[0] = (baseProjectile ? baseProjectile.primaryMagazine.contents : 0);
			_reloadStartMag[1] = (baseProjectile2 ? baseProjectile2.primaryMagazine.contents : 0);
			bool num = baseProjectile != null && baseProjectile.ServerTryReload(mounted.inventory);
			bool flag = baseProjectile2 != null && baseProjectile2.ServerTryReload(mounted.inventory);
			int num2 = (baseProjectile ? baseProjectile.primaryMagazine.contents : 0);
			int num3 = (baseProjectile2 ? baseProjectile2.primaryMagazine.contents : 0);
			_reloadTaken[0] = Mathf.Max(0, num2 - _reloadStartMag[0]);
			_reloadTaken[1] = Mathf.Max(0, num3 - _reloadStartMag[1]);
			bool num4 = num || flag;
			if (_reloadTime == 0f)
			{
				CalculateReloadTime();
			}
			float time = UnityEngine.Time.time;
			float arg = time + _reloadTime;
			if (num4)
			{
				IsEmpty = false;
				IsReloading = true;
				mounted.userID.Get();
				_reloadServerTimer = 0f;
				ClientRPC(RpcTarget.NetworkGroup("CL_StartReloading"), time, arg);
				InvokeRepeating(ProcessServerReloadTimer, 0f, 0f);
			}
		}
	}

	private void ProcessServerReloadTimer()
	{
		_reloadServerTimer += UnityEngine.Time.deltaTime;
		if (_reloadServerTimer >= _reloadTime)
		{
			IsReloading = false;
			CancelInvoke(ProcessServerReloadTimer);
			_reloadServerTimer = 0f;
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	[RPC_Server.CallsPerSecond(100uL)]
	private void SV_ReceiveClientAim(RPCMessage msg)
	{
		if (!_clientAuthority)
		{
			return;
		}
		BasePlayer player = msg.player;
		if (_seat == null)
		{
			return;
		}
		BasePlayer mounted = _seat.GetMounted();
		if (mounted == null || player == null || player != mounted || mounted != player)
		{
			return;
		}
		float realtimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
		float num = Mathf.Clamp(realtimeSinceStartup - _lastAimRpcTime, 0.0001f, 0.2f);
		using ServersideMountedWeaponSnapshot serversideMountedWeaponSnapshot = msg.read.Proto<ServersideMountedWeaponSnapshot>();
		float yaw = serversideMountedWeaponSnapshot.yaw;
		float pitch = serversideMountedWeaponSnapshot.pitch;
		WorldAngleToTurretAngle(yaw, pitch, out var turretYaw, out var turretPitch);
		Vector2 yawClamp = GetSeat().GetYawClamp();
		Vector2 pitchClamp = GetSeat().GetPitchClamp();
		if (antihack_level >= 1 && (turretYaw < yawClamp.x || turretYaw > yawClamp.y || turretPitch < pitchClamp.x || turretPitch > pitchClamp.y))
		{
			turretYaw = Mathf.Clamp(turretYaw, yawClamp.x, yawClamp.y);
			turretPitch = Mathf.Clamp(turretPitch, pitchClamp.x, pitchClamp.y);
			TurretAngleToWorldAngle(turretYaw, turretPitch, out _worldYaw, out _worldPitch);
			UpdateClient(force: true);
			return;
		}
		float num2 = Mathf.Abs(Mathf.DeltaAngle(_lastAimYaw, yaw));
		float num3 = Mathf.Abs(Mathf.DeltaAngle(_lastAimPitch, pitch));
		if (antihack_level >= 2 && (num2 > antihack_max_snap_degrees || num3 > antihack_max_snap_degrees))
		{
			UpdateClient(force: true);
			return;
		}
		if (antihack_level >= 3)
		{
			float num4 = num2 / num;
			float num5 = num3 / num;
			if (num4 > antihack_max_degrees_per_second_yaw || num5 > antihack_max_degrees_per_second_pitch)
			{
				UpdateClient(force: true);
				return;
			}
		}
		_worldYaw = yaw;
		_worldPitch = pitch;
		SetTargetAngles(_worldYaw, _worldPitch, set: true);
		if (Snapshot == null || UnityEngine.Time.realtimeSinceStartup - _startTime - Snapshot.time >= 0.05f)
		{
			UpdateClient();
		}
		_lastAimRpcTime = realtimeSinceStartup;
		_lastAimYaw = yaw;
		_lastAimPitch = pitch;
	}

	protected override bool WriteSyncVar(byte id, NetWrite writer)
	{
		switch (id)
		{
		case 0:
			if (ConVar.Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log("SyncVar Writing: Snapshot for " + iD.ToString());
			}
			using (TimeWarning.New("Objects"))
			{
				if (__sync_Snapshot == null)
				{
					__sync_Snapshot = Facepunch.Pool.Get<ServersideMountedWeaponSnapshot>();
				}
				SyncVarNetWrite(writer, __sync_Snapshot);
				return true;
			}
		case 1:
			if (ConVar.Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log("SyncVar Writing: GunId for " + iD.ToString());
			}
			SyncVarNetWrite(writer, __sync_GunId);
			return true;
		case 2:
			if (ConVar.Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log("SyncVar Writing: Gun2Id for " + iD.ToString());
			}
			SyncVarNetWrite(writer, __sync_Gun2Id);
			return true;
		case 3:
			if (ConVar.Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log("SyncVar Writing: IsReloading for " + iD.ToString());
			}
			SyncVarNetWrite(writer, __sync_IsReloading);
			return true;
		case 4:
			if (ConVar.Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log("SyncVar Writing: IsEmpty for " + iD.ToString());
			}
			SyncVarNetWrite(writer, __sync_IsEmpty);
			return true;
		default:
			return base.WriteSyncVar(id, writer);
		}
	}

	protected override bool OnSyncVar(byte id, NetRead reader, bool fromAutoSave = false)
	{
		switch (id)
		{
		case 0:
			try
			{
				ServersideMountedWeaponSnapshot serversideMountedWeaponSnapshot = __sync_Snapshot;
				ServersideMountedWeaponSnapshot _sync_Snapshot = reader.Proto<ServersideMountedWeaponSnapshot>();
				__sync_Snapshot = _sync_Snapshot;
				if (fromAutoSave)
				{
					serversideMountedWeaponSnapshot = null;
				}
				serversideMountedWeaponSnapshot?.Dispose();
			}
			catch (Exception exception2)
			{
				Debug.LogException(exception2);
			}
			return true;
		case 1:
			try
			{
				_ = __sync_GunId;
				NetworkableId _sync_GunId = reader.EntityID();
				__sync_GunId = _sync_GunId;
			}
			catch (Exception exception4)
			{
				Debug.LogException(exception4);
			}
			return true;
		case 2:
			try
			{
				_ = __sync_Gun2Id;
				NetworkableId _sync_Gun2Id = reader.EntityID();
				__sync_Gun2Id = _sync_Gun2Id;
			}
			catch (Exception exception5)
			{
				Debug.LogException(exception5);
			}
			return true;
		case 3:
			try
			{
				_ = __sync_IsReloading;
				bool _sync_IsReloading = reader.Bool();
				__sync_IsReloading = _sync_IsReloading;
			}
			catch (Exception exception3)
			{
				Debug.LogException(exception3);
			}
			return true;
		case 4:
			try
			{
				_ = __sync_IsEmpty;
				bool _sync_IsEmpty = reader.Bool();
				__sync_IsEmpty = _sync_IsEmpty;
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			return true;
		default:
			return base.OnSyncVar(id, reader, fromAutoSave);
		}
	}

	private byte __GetWeaverID(string propertyName)
	{
		return propertyName switch
		{
			"Snapshot" => 0, 
			"GunId" => 1, 
			"Gun2Id" => 2, 
			"IsReloading" => 3, 
			"IsEmpty" => 4, 
			_ => byte.MaxValue, 
		};
	}

	protected override void WriteAutoSaveSyncVars(NetWrite writer)
	{
		base.WriteAutoSaveSyncVars(writer);
		WriteSyncVar(1, writer);
		WriteSyncVar(2, writer);
		WriteSyncVar(4, writer);
	}

	protected override void ReadAutoSaveSyncVars(NetRead reader)
	{
		base.ReadAutoSaveSyncVars(reader);
		OnSyncVar(1, reader, fromAutoSave: true);
		OnSyncVar(2, reader, fromAutoSave: true);
		OnSyncVar(4, reader, fromAutoSave: true);
	}

	protected override bool AutoSaveSyncVars(SaveInfo save)
	{
		NetWrite obj = Network.Net.sv.StartWrite();
		WriteAutoSaveSyncVars(obj);
		var (src, num) = obj.GetBuffer();
		if (_autosaveBuffer == null)
		{
			_autosaveBuffer = BaseEntity._autosaveBufferPool.Rent(num);
		}
		if (_autosaveBuffer.Length < num)
		{
			BaseEntity._autosaveBufferPool.Return(_autosaveBuffer);
			_autosaveBuffer = BaseEntity._autosaveBufferPool.Rent(num);
		}
		Buffer.BlockCopy(src, 0, _autosaveBuffer, 0, num);
		save.msg.baseEntity.syncVars = _autosaveBuffer;
		Facepunch.Pool.Free(ref obj);
		return true;
	}

	protected override bool AutoLoadSyncVars(LoadInfo load)
	{
		if (load.msg.baseEntity != null && load.msg.baseEntity.syncVars != null)
		{
			NetRead obj = Facepunch.Pool.Get<NetRead>();
			obj.Init(load.msg.baseEntity.syncVars.AsSpan());
			ReadAutoSaveSyncVars(obj);
			Facepunch.Pool.Free(ref obj);
		}
		return true;
	}

	protected override void ResetSyncVars()
	{
		base.ResetSyncVars();
		__sync_Snapshot = null;
		__sync_GunId = default(NetworkableId);
		__sync_Gun2Id = default(NetworkableId);
		__sync_IsReloading = false;
		__sync_IsEmpty = false;
	}

	protected override bool ShouldInvalidateCache(byte id)
	{
		return id switch
		{
			0 => true, 
			1 => true, 
			2 => true, 
			3 => true, 
			4 => true, 
			_ => base.ShouldInvalidateCache(id), 
		};
	}
}
