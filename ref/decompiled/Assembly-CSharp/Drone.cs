#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class Drone : RemoteControlEntity, IRemoteControllableClientCallbacks, IRemoteControllable, IRemoteControllablePromptProvider, IRemoteControllableHostileProvider, SamSite.ISamSiteTarget
{
	public struct DroneInputState
	{
		public Vector3 movement;

		public float throttle;

		public float pitch;

		public float yaw;

		public void Reset()
		{
			movement = Vector3.zero;
			pitch = 0f;
			yaw = 0f;
		}
	}

	[ReplicatedVar(Help = "How far drones can be flown away from the controlling computer station", ShowInAdminUI = true, Default = "600")]
	public static float maxControlRange = 750f;

	[ServerVar(Help = "If greater than zero, overrides the drone's planar movement speed")]
	public static float movementSpeedOverride = 0f;

	[ServerVar(Help = "If greater than zero, overrides the drone's vertical movement speed")]
	public static float altitudeSpeedOverride = 0f;

	[ServerVar]
	public static bool disableSamTargeting;

	[ServerVar(Help = "Radius at which a SAM missile's proximity fuse detonates against drones, capped by the missile's 20m trigger sphere")]
	public static float samProximityFuseRadius = 10f;

	[ClientVar(ClientAdmin = true)]
	public static float windTimeDivisor = 10f;

	[ClientVar(ClientAdmin = true)]
	public static float windPositionDivisor = 100f;

	[ClientVar(ClientAdmin = true)]
	public static float windPositionScale = 1f;

	[ClientVar(ClientAdmin = true)]
	public static float windRotationMultiplier = 45f;

	[ClientVar(ClientAdmin = true)]
	public static float windLerpSpeed = 0.1f;

	public const Flags Flag_ThrottleUp = Flags.Reserved1;

	public const Flags Flag_Flying = Flags.Reserved2;

	public const Flags Flag_HoldingItem = Flags.Reserved3;

	private const Flags Flag_IsHostile = Flags.Reserved4;

	public const Flags Flag_DropCooldown = Flags.Reserved5;

	[Header("Drone")]
	public Rigidbody body;

	public Transform modelRoot;

	public bool killInWater = true;

	public bool killInTerrain = true;

	public bool enableGrounding = true;

	public bool keepAboveTerrain = true;

	public float groundTraceDist = 0.1f;

	public float groundCheckInterval = 0.05f;

	public float altitudeAcceleration = 10f;

	public float movementAcceleration = 10f;

	public float yawSpeed = 2f;

	public float uprightSpeed = 2f;

	public float uprightPrediction = 0.15f;

	public float uprightDot = 0.5f;

	public float leanWeight = 0.1f;

	public float leanMaxVelocity = 5f;

	public float hurtVelocityThreshold = 3f;

	public float hurtDamagePower = 3f;

	public float collisionDisableTime = 0.25f;

	public float pitchMin = -60f;

	public float pitchMax = 60f;

	public float pitchSensitivity = -5f;

	public bool disableWhenHurt;

	[Range(0f, 1f)]
	public float disableWhenHurtChance = 0.25f;

	public float playerCheckInterval = 0.1f;

	public float playerCheckRadius;

	public float deployYOffset = 0.1f;

	public Translate.Phrase computerPromptPhrase;

	public Translate.Phrase cooldownComputerPromptPhrase;

	[Header("Sound")]
	public SoundDefinition movementLoopSoundDef;

	public SoundDefinition movementStartSoundDef;

	public SoundDefinition movementStopSoundDef;

	public AnimationCurve movementLoopPitchCurve;

	public float movementSpeedReference = 50f;

	[Header("Animation")]
	public float propellerMaxSpeed = 1000f;

	public float propellerAcceleration = 3f;

	public Transform propellerA;

	public Transform propellerB;

	public Transform propellerC;

	public Transform propellerD;

	public float pitch;

	private EntityRef<DroneStorage> storageDrop;

	public Vector3? targetPosition;

	public DroneInputState currentInput;

	public float lastInputTime;

	public double lastCollision = -1000.0;

	public TimeSince lastGroundCheck;

	public bool isGrounded;

	public RealTimeSinceEx lastPlayerCheck;

	private float avgTerrainHeight;

	private BasePlayer cachedController;

	public Translate.Phrase ComputerPromptPhrase
	{
		get
		{
			if (HasFlag(Flags.Reserved5))
			{
				return cooldownComputerPromptPhrase;
			}
			return computerPromptPhrase;
		}
	}

	public override bool RequiresMouse => true;

	public override float MaxRange => maxControlRange;

	public override bool CanAcceptInput => true;

	public SamSite.SamTargetType SAMTargetType => SamSite.targetTypeDrone;

	public override bool PositionTickFixedTime
	{
		protected get
		{
			return true;
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Drone.OnRpcMessage"))
		{
			if (rpc == 795740894 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - SV_OpenStorage");
				}
				using (TimeWarning.New("SV_OpenStorage"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(795740894u, "SV_OpenStorage", this, player, 3f))
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
							SV_OpenStorage(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in SV_OpenStorage");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool IsRemoteControllableHostile()
	{
		return HasFlag(Flags.Reserved4);
	}

	public override void Spawn()
	{
		base.Spawn();
		isGrounded = true;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		foreach (BaseEntity child in children)
		{
			if (child is DroneStorage droneStorage)
			{
				storageDrop.Set(droneStorage);
				droneStorage.Drone = this;
			}
		}
	}

	protected override void OnChildAdded(BaseEntity child)
	{
		base.OnChildAdded(child);
		if (child is DroneStorage droneStorage)
		{
			storageDrop.Set(droneStorage);
			droneStorage.Drone = this;
		}
	}

	public bool IsValidSAMTarget(bool staticRespawn)
	{
		if (disableSamTargeting)
		{
			return false;
		}
		if (!base.IsBeingControlled || isGrounded)
		{
			return false;
		}
		if (!IsHostile())
		{
			return false;
		}
		return true;
	}

	public override void StopControl(CameraViewerId viewerID)
	{
		CameraViewerId? controllingViewerId = base.ControllingViewerId;
		if (viewerID == controllingViewerId)
		{
			SetFlagLocal(Flags.Reserved1, b: false);
			SetFlagLocal(Flags.Reserved2, b: false);
			pitch = 0f;
			SendNetworkUpdate();
		}
		base.StopControl(viewerID);
	}

	public override void UserInput(InputState inputState, CameraViewerId viewerID)
	{
		CameraViewerId? controllingViewerId = base.ControllingViewerId;
		if (!(viewerID != controllingViewerId))
		{
			currentInput.Reset();
			int num = (inputState.IsDown(BUTTON.FORWARD) ? 1 : 0) + (inputState.IsDown(BUTTON.BACKWARD) ? (-1) : 0);
			int num2 = (inputState.IsDown(BUTTON.RIGHT) ? 1 : 0) + (inputState.IsDown(BUTTON.LEFT) ? (-1) : 0);
			currentInput.movement = new Vector3(num2, 0f, num).normalized;
			currentInput.throttle = (inputState.IsDown(BUTTON.SPRINT) ? 1 : 0) + (inputState.IsDown(BUTTON.DUCK) ? (-1) : 0);
			currentInput.yaw = inputState.current.mouseDelta.x;
			currentInput.pitch = inputState.current.mouseDelta.y;
			if (inputState.WasJustPressed(BUTTON.FIRE_PRIMARY))
			{
				TryStorageDrop();
			}
			lastInputTime = UnityEngine.Time.time;
			bool flag = false;
			bool flag2 = false;
			bool flag3 = currentInput.throttle > 0f;
			if (flag3 != HasFlag(Flags.Reserved1))
			{
				SetFlagLocal(Flags.Reserved1, flag3);
				flag = true;
			}
			float b = pitch;
			pitch += currentInput.pitch * pitchSensitivity;
			pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
			if (!Mathf.Approximately(pitch, b))
			{
				flag2 = true;
			}
			if (flag2)
			{
				SendNetworkUpdateImmediate();
			}
			else if (flag)
			{
				SendNetworkUpdate_Flags();
			}
		}
	}

	private void OnPhysicsNeighbourChanged()
	{
		body.isKinematic = false;
		body.WakeUp();
	}

	public virtual void Update_Server()
	{
		if (!base.isServer || IsDead())
		{
			return;
		}
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			if (storageDrop.IsSet)
			{
				DroneStorage droneStorage = storageDrop.Get(serverside: true);
				flagsUpdateScope.Set(Flags.Reserved3, !droneStorage.inventory.IsEmpty());
				droneStorage.UpdateFlags();
			}
			if (base.IsBeingControlled)
			{
				flagsUpdateScope.Set(Flags.Reserved4, IsHostile());
			}
		}
		if (base.IsBeingControlled || !targetPosition.HasValue)
		{
			return;
		}
		Vector3 position = base.transform.position;
		float height = TerrainMeta.HeightMap.GetHeight(position);
		Vector3 v = targetPosition.Value - body.linearVelocity * 0.5f;
		if (keepAboveTerrain)
		{
			v.y = Mathf.Max(v.y, height + 1f);
		}
		Vector2 vector = v.XZ2D();
		Vector2 vector2 = position.XZ2D();
		(vector - vector2).XZ3D().ToDirectionAndMagnitude(out var direction, out var magnitude);
		currentInput.Reset();
		lastInputTime = UnityEngine.Time.time;
		if (position.y - height > 1f)
		{
			float num = Mathf.Clamp01(magnitude);
			currentInput.movement = base.transform.InverseTransformVector(direction) * num;
			if (magnitude > 0.5f)
			{
				float y = base.transform.rotation.eulerAngles.y;
				float y2 = Quaternion.FromToRotation(Vector3.forward, direction).eulerAngles.y;
				currentInput.yaw = Mathf.Clamp(Mathf.LerpAngle(y, y2, UnityEngine.Time.deltaTime) - y, -2f, 2f);
			}
		}
		currentInput.throttle = Mathf.Clamp(v.y - position.y, -1f, 1f);
	}

	public void FixedUpdate()
	{
		if (!base.isServer || IsDead())
		{
			return;
		}
		Vector3 position = base.transform.position;
		if (killInTerrain && AntiHack.TestInsideTerrain(position))
		{
			Kill();
			return;
		}
		if (killInWater)
		{
			float num = WaterFactor();
			if (num > 0f)
			{
				if (num > 0.99f)
				{
					Kill();
				}
				return;
			}
		}
		if ((!base.IsBeingControlled && !targetPosition.HasValue) || (isGrounded && currentInput.throttle <= 0f))
		{
			if (HasFlag(Flags.Reserved2))
			{
				SetFlagLocal(Flags.Reserved2, b: false);
				SendNetworkUpdate_Flags();
			}
			if (!body.isKinematic && body.IsSleeping() && (float)lastGroundCheck >= groundCheckInterval * 8f)
			{
				lastGroundCheck = 0f;
				if (body.SweepTest(Vector3.down, out var hitInfo, groundTraceDist, QueryTriggerInteraction.Ignore) && !hitInfo.rigidbody)
				{
					body.isKinematic = true;
				}
			}
			return;
		}
		if (playerCheckRadius > 0f && (double)lastPlayerCheck > (double)playerCheckInterval)
		{
			lastPlayerCheck = 0.0;
			List<BasePlayer> obj = Facepunch.Pool.Get<List<BasePlayer>>();
			Vis.Entities(position, playerCheckRadius, obj, 131072);
			if (obj.Count > 0)
			{
				lastCollision = TimeEx.currentTimestamp;
			}
			Facepunch.Pool.FreeUnmanaged(ref obj);
		}
		double currentTimestamp = TimeEx.currentTimestamp;
		bool num2 = lastCollision > 0.0 && currentTimestamp - lastCollision < (double)collisionDisableTime;
		if (enableGrounding)
		{
			if ((float)lastGroundCheck >= groundCheckInterval)
			{
				lastGroundCheck = 0f;
				RaycastHit hitInfo2;
				bool flag = body.SweepTest(Vector3.down, out hitInfo2, groundTraceDist, QueryTriggerInteraction.Ignore);
				if (!flag && isGrounded)
				{
					lastPlayerCheck = playerCheckInterval;
				}
				isGrounded = flag;
			}
			if (isGrounded && body.IsSleeping())
			{
				body.isKinematic = true;
			}
		}
		else
		{
			isGrounded = false;
		}
		Vector3 vector = base.transform.TransformDirection(currentInput.movement);
		body.linearVelocity.WithY(0f).ToDirectionAndMagnitude(out var direction, out var magnitude);
		float num3 = Mathf.Clamp01(magnitude / leanMaxVelocity);
		Vector3 vector2 = (Mathf.Approximately(vector.sqrMagnitude, 0f) ? ((0f - num3) * direction) : vector);
		Vector3 normalized = (Vector3.up + vector2 * leanWeight * num3).normalized;
		Vector3 up = base.transform.up;
		float num4 = Mathf.Max(Vector3.Dot(normalized, up), 0f);
		if (!num2 || isGrounded)
		{
			Vector3 vector3 = ((isGrounded && currentInput.throttle <= 0f) ? Vector3.zero : (up * (-1f * UnityEngine.Physics.gravity.y)));
			Vector3 vector4 = (isGrounded ? Vector3.zero : (vector * ((movementSpeedOverride > 0f) ? movementSpeedOverride : movementAcceleration)));
			float serviceCeiling = HotAirBalloon.serviceCeiling;
			avgTerrainHeight = Mathf.Lerp(b: Mathf.Max(HotAirBalloon.minimumAltitudeTerrain, TerrainMeta.HeightMap.GetHeight(position)), a: avgTerrainHeight, t: UnityEngine.Time.deltaTime);
			float num5 = 1f - Mathf.InverseLerp(avgTerrainHeight + serviceCeiling - 30f, avgTerrainHeight + serviceCeiling, position.y);
			Vector3 vector5 = up * (currentInput.throttle * ((altitudeSpeedOverride > 0f) ? altitudeSpeedOverride : altitudeAcceleration));
			Vector3 vector6 = num5 * (vector3 + vector5) + vector4;
			body.isKinematic = false;
			body.AddForce(vector6 * num4, ForceMode.Acceleration);
		}
		if (!num2 && !isGrounded)
		{
			Vector3 vector7 = base.transform.TransformVector(0f, currentInput.yaw * yawSpeed, 0f);
			Vector3 vector8 = Vector3.Cross(Quaternion.Euler(body.angularVelocity * uprightPrediction) * up, normalized) * uprightSpeed;
			float num6 = ((num4 < uprightDot) ? 0f : num4);
			Vector3 vector9 = vector7 * num4 + vector8 * num6;
			body.isKinematic = false;
			body.AddTorque(vector9 * num4, ForceMode.Acceleration);
		}
		bool flag2 = !num2;
		if (flag2 == HasFlag(Flags.Reserved2))
		{
			return;
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate_Flags);
		flagsUpdateScope.Set(Flags.Reserved2, flag2);
	}

	private void TryStorageDrop()
	{
		if (!isGrounded && !IsDead())
		{
			double currentTimestamp = TimeEx.currentTimestamp;
			if (!(lastCollision > 0.0) || !(currentTimestamp - lastCollision < (double)collisionDisableTime))
			{
				storageDrop.Get(serverside: true).TryServerDrop();
			}
		}
	}

	public void OnCollisionEnter(Collision collision)
	{
		if (base.isServer)
		{
			lastCollision = TimeEx.currentTimestamp;
			float magnitude = collision.relativeVelocity.magnitude;
			if (magnitude > hurtVelocityThreshold)
			{
				Hurt(Mathf.Pow(magnitude, hurtDamagePower), DamageType.Fall, null, useProtection: false);
			}
		}
	}

	public void OnCollisionStay()
	{
		if (base.isServer)
		{
			lastCollision = TimeEx.currentTimestamp;
		}
	}

	public void OnTriggerStay(Collider other)
	{
		if (base.isServer && other.CompareTag("MLRSRocketTrigger"))
		{
			TimedExplosive componentInParent = other.GetComponentInParent<TimedExplosive>();
			if (!(componentInParent == null) && !(Vector3.Distance(componentInParent.transform.position, CenterPoint()) > samProximityFuseRadius))
			{
				Hurt(base.health * 2f, DamageType.Explosion, componentInParent.creatorEntity, useProtection: false);
				componentInParent.Explode();
			}
		}
	}

	public override void Hurt(HitInfo info)
	{
		base.Hurt(info);
		if (base.isServer && disableWhenHurt && info.damageTypes.GetMajorityDamageType() != DamageType.Fall && UnityEngine.Random.value < disableWhenHurtChance)
		{
			lastCollision = TimeEx.currentTimestamp;
		}
	}

	public override void OnDied(HitInfo info)
	{
		if (storageDrop.IsSet)
		{
			storageDrop.Get(serverside: true).DropItems(info.Initiator);
		}
		base.OnDied(info);
	}

	public override float GetNetworkTime()
	{
		return UnityEngine.Time.fixedTime;
	}

	public override Vector3 GetLocalVelocityServer()
	{
		if (body == null)
		{
			return Vector3.zero;
		}
		return body.linearVelocity;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (!info.forDisk)
		{
			info.msg.drone = Facepunch.Pool.Get<ProtoBuf.Drone>();
			info.msg.drone.pitch = pitch;
		}
	}

	public override void OnPickedUp(Item createdItem, BasePlayer player)
	{
		base.OnPickedUp(createdItem, player);
		DroneStorage droneStorage = storageDrop.Get(serverside: true);
		if (droneStorage != null && !droneStorage.inventory.IsEmpty())
		{
			player.GiveItem(droneStorage.inventory.GetSlot(0), GiveItemReason.PickedUp);
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void SV_OpenStorage(RPCMessage msg)
	{
		if (CanBeLooted(msg.player))
		{
			DroneStorage droneStorage = storageDrop.Get(serverside: true);
			if (!(droneStorage == null))
			{
				droneStorage.PlayerOpenLoot(msg.player);
			}
		}
	}

	public override bool CanBeLooted(BasePlayer player)
	{
		if (base.isServer && TriggerSafeZone.IsBoundsInsideSafeZone(WorldSpaceBounds(), checkCombatZones: false))
		{
			return false;
		}
		return base.CanBeLooted(player);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.drone != null)
		{
			pitch = info.msg.drone.pitch;
		}
	}

	public virtual void Update()
	{
		Update_Server();
		if (HasFlag(Flags.Reserved2))
		{
			Vector3 eulerAngles = viewEyes.localRotation.eulerAngles;
			eulerAngles.x = Mathf.LerpAngle(eulerAngles.x, pitch, 0.1f);
			viewEyes.localRotation = Quaternion.Euler(eulerAngles);
		}
	}

	public override bool CanChangeID(BasePlayer player)
	{
		if (player != null && base.OwnerID == (ulong)player.userID)
		{
			return !HasFlag(Flags.Reserved2);
		}
		return false;
	}

	protected override bool ShouldDisplayPickupOption(BasePlayer player)
	{
		if (!HasFlag(Flags.Reserved2))
		{
			return base.ShouldDisplayPickupOption(player);
		}
		return false;
	}

	public override BasePlayer ToPlayer()
	{
		if (!base.IsBeingControlled)
		{
			return null;
		}
		if (!base.ControllingViewerId.HasValue)
		{
			return null;
		}
		ulong steamId = base.ControllingViewerId.Value.SteamId;
		if (cachedController == null || cachedController.OwnerID != steamId)
		{
			cachedController = BasePlayer.FindByID(steamId) ?? BasePlayer.FindSleeping(steamId);
		}
		return cachedController;
	}

	public override void OnPickedUpPreItemMove(Item createdItem, BasePlayer player)
	{
		base.OnPickedUpPreItemMove(createdItem, player);
		if (player != null && (ulong)player.userID == base.OwnerID)
		{
			createdItem.text = GetIdentifier();
		}
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		base.OnDeployed(parent, deployedBy, fromItem);
		base.transform.position += base.transform.up * deployYOffset;
		if (body != null)
		{
			body.linearVelocity = Vector3.zero;
			body.angularVelocity = Vector3.zero;
		}
		if (fromItem != null && !string.IsNullOrEmpty(fromItem.text) && ComputerStation.IsValidIdentifier(fromItem.text))
		{
			UpdateIdentifier(fromItem.text);
		}
	}

	public override bool IsHostile()
	{
		if (!base.IsHostile())
		{
			if (storageDrop.IsValid(serverside: true))
			{
				return storageDrop.Get(serverside: true).inventory.GetSlot(0)?.GetHeldEntity() is ThrownWeapon;
			}
			return false;
		}
		return true;
	}

	public override bool ShouldNetworkOwnerInfo()
	{
		return true;
	}

	public override bool ShouldInheritNetworkGroup()
	{
		return false;
	}

	public override float AntiHackVelocity()
	{
		return body.linearVelocity.magnitude + 1f;
	}
}
