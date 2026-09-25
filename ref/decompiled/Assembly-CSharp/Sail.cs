#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Assertions;

public class Sail : DecayEntity, global::IBoatBuildingPiece, IBoatPropulsion
{
	[ReplicatedVar]
	public static float MaxThrustMultiplier = 1f;

	[SerializeField]
	[Header("Sail")]
	private float maxThrust = 1000f;

	public float RaiseDuration = 1.5f;

	public float LowerDuration = 1.5f;

	public GameObject LoweredCollider;

	public GameObject RaisedCollider;

	public List<Transform> WindBlockedCheckPoints;

	public float WindBlockedCheckRadius = 0.5f;

	public float WindBlockedCheckDistance = 1.5f;

	[Header("Visuals")]
	public Transform SailVisualRoot;

	public Animator Animator;

	public GameObject RaisedFarVisual;

	public GameObject LoweredFarVisual;

	public GameObjectRef sailRotateEffect;

	public const Flags Flag_Lowered = Flags.Reserved3;

	public const Flags Flag_Lowering = Flags.Reserved12;

	public const Flags Flag_Raising = Flags.Reserved13;

	public const Flags Flag_WindBlocked = Flags.Reserved14;

	private static readonly int WindBlockedHash = Animator.StringToHash("windblocked");

	private static readonly int LoweredHash = Animator.StringToHash("lowered");

	private static readonly int LoweringHash = Animator.StringToHash("lowering");

	private static readonly int RaisingHash = Animator.StringToHash("raising");

	private TimeUntil timeUntilLoweredRaised;

	public float MaxThrust => maxThrust * MaxThrustMultiplier;

	public bool Lowering => HasFlag(Flags.Reserved12);

	public bool Raising => HasFlag(Flags.Reserved13);

	public bool Lowered => HasFlag(Flags.Reserved3);

	public bool Blowing
	{
		get
		{
			if (Lowered || Lowering || Raising)
			{
				return !WindBlocked;
			}
			return false;
		}
	}

	public bool WindBlocked => HasFlag(Flags.Reserved14);

	public Vector3 ThrustPosition => base.transform.position + base.transform.up * 1f;

	public Vector3 Direction => base.transform.forward;

	float IBoatPropulsion.MaxThrust => MaxThrust;

	public float CurrentThrust
	{
		get
		{
			if (Blowing)
			{
				if (Lowering)
				{
					return (LowerDuration - (float)timeUntilLoweredRaised) / LowerDuration * MaxThrust;
				}
				if (Raising)
				{
					return (float)timeUntilLoweredRaised / RaiseDuration * MaxThrust;
				}
				return MaxThrust;
			}
			return 0f;
		}
	}

	public float ThrustRatio => CurrentThrust / MaxThrust;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Sail.OnRpcMessage"))
		{
			if (rpc == 842631481 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - LowerSail");
				}
				using (TimeWarning.New("LowerSail"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(842631481u, "LowerSail", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(842631481u, "LowerSail", this, player, 3f))
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
							LowerSail(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in LowerSail");
					}
				}
				return true;
			}
			if (rpc == 1744516204 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RaiseSail");
				}
				using (TimeWarning.New("RaiseSail"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1744516204u, "RaiseSail", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(1744516204u, "RaiseSail", this, player, 3f))
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
							RaiseSail(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in RaiseSail");
					}
				}
				return true;
			}
			if (rpc == 2730316685u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RotateSail");
				}
				using (TimeWarning.New("RotateSail"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2730316685u, "RotateSail", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(2730316685u, "RotateSail", this, player, 3f))
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
							RotateSail(msg4);
						}
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
						player.Kick("RPC Error in RotateSail");
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
		ResetFlags();
		CacheIsWindBlocked();
	}

	private void ResetFlags()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved14, b: false);
		flagsUpdateScope.Set(Flags.Reserved3, b: false);
		flagsUpdateScope.Set(Flags.Reserved13, b: false);
		flagsUpdateScope.Set(Flags.Reserved12, b: false);
		flagsUpdateScope.Set(Flags.Busy, b: false);
	}

	public override void Hurt(HitInfo info)
	{
		PlayerBoat parentPlayerBoat = PlayerBoat.GetParentPlayerBoat(this);
		if (parentPlayerBoat != null && !parentPlayerBoat.IsDestructibleWreck)
		{
			parentPlayerBoat.OnBoatDeployableHurt(this, info);
		}
		else
		{
			base.Hurt(info);
		}
	}

	public override void InitShared()
	{
		base.InitShared();
		CancelInvoke(CacheIsWindBlocked);
		InvokeRandomized(CacheIsWindBlocked, 0f, 5f, 2f);
	}

	private void CacheIsWindBlocked()
	{
		if (!Lowered)
		{
			return;
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		if (!IsOutside())
		{
			flagsUpdateScope.Set(Flags.Reserved14, b: true);
			return;
		}
		bool b = IsLocationWindBlocked(WindBlockedCheckPoints, base.transform.position, base.transform.rotation, WindBlockedCheckRadius, WindBlockedCheckDistance, this);
		flagsUpdateScope.Set(Flags.Reserved14, b);
	}

	public bool WouldSailBeBlockedByNewSailInLocation(Vector3 newSailPos, Quaternion newSailRot)
	{
		using PooledList<BoxCollider> pooledList = Facepunch.Pool.Get<PooledList<BoxCollider>>();
		GetComponentsInChildren(includeInactive: true, pooledList);
		Matrix4x4 matrix4x = Matrix4x4.TRS(newSailPos, newSailRot, Vector3.one);
		foreach (BoxCollider item in pooledList)
		{
			if (item.isTrigger)
			{
				continue;
			}
			OBB oBB = new OBB(item.transform.localPosition, item.transform.localRotation, new Bounds(item.center, item.size));
			oBB.position = matrix4x.MultiplyPoint3x4(oBB.position);
			oBB.rotation = matrix4x.rotation * oBB.rotation;
			foreach (Transform windBlockedCheckPoint in WindBlockedCheckPoints)
			{
				Ray ray = new Ray(windBlockedCheckPoint.position, windBlockedCheckPoint.rotation * Vector3.forward);
				if (oBB.Trace(ray, out var _, WindBlockedCheckDistance))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool IsLocationWindBlocked(List<Transform> checkPoints, Vector3 worldPosition, Quaternion worldRotation, float radius, float distance, BaseEntity toIgnore, bool ignoreClient = true)
	{
		int layerMask = 136323328;
		List<RaycastHit> obj = Facepunch.Pool.Get<List<RaycastHit>>();
		Matrix4x4 matrix4x = Matrix4x4.TRS(worldPosition, worldRotation, Vector3.one);
		foreach (Transform checkPoint in checkPoints)
		{
			obj.Clear();
			GamePhysics.TraceAllUnordered(new Ray(matrix4x.MultiplyPoint3x4(checkPoint.localPosition), worldRotation * checkPoint.localRotation * Vector3.forward), radius, obj, distance, layerMask, QueryTriggerInteraction.UseGlobal, toIgnore);
			for (int i = 0; i < obj.Count; i++)
			{
				BaseEntity entity = RaycastHitEx.GetEntity(obj[i]);
				if (!((bool)entity && entity.isClient && ignoreClient))
				{
					Facepunch.Pool.FreeUnmanaged(ref obj);
					return true;
				}
			}
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
		return false;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (base.isServer && info.fromDisk)
		{
			Raise(null, instant: true);
		}
		ToggleColliders();
	}

	private void ToggleColliders()
	{
		RaisedCollider.SetActive(!Lowered);
		LoweredCollider.SetActive(Lowered);
	}

	public bool CanBeRaised(BasePlayer player)
	{
		object obj = Interface.CallHook("CanRaiseSail", this, player);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (player != null && !PlayerBoat.IsPlayerAuthedOnChildEntity(this, player, authedIfNoPrivOrLock: true))
		{
			return false;
		}
		if (!PlayerBoat.IsChildOfInteractablePlayerBoat(this))
		{
			return false;
		}
		if (IsBusy())
		{
			return false;
		}
		if (!Lowered)
		{
			return false;
		}
		return true;
	}

	public bool CanBeLowered(BasePlayer player)
	{
		object obj = Interface.CallHook("CanLowerSail", this, player);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (player != null && !PlayerBoat.IsPlayerAuthedOnChildEntity(this, player, authedIfNoPrivOrLock: true))
		{
			return false;
		}
		if (!PlayerBoat.IsChildOfInteractablePlayerBoat(this))
		{
			return false;
		}
		if (IsBusy())
		{
			return false;
		}
		if (Lowered)
		{
			return false;
		}
		return true;
	}

	public bool CanRotate(BasePlayer player)
	{
		object obj = Interface.CallHook("CanRotateSail", this, player);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (IsBusy())
		{
			return false;
		}
		if (Lowered || Lowering)
		{
			return false;
		}
		if (!PlayerBoat.IsPlayerAuthedOnChildEntity(this, player, authedIfNoPrivOrLock: true))
		{
			return false;
		}
		DeployVolume[] volumes = PrefabAttribute.server.FindAll<DeployVolume>(prefabID);
		return DeployVolume.Check(base.transform.position, base.transform.rotation * Quaternion.AngleAxis(180f, Vector3.up), volumes, ~(1 << base.gameObject.layer));
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(5uL)]
	public void LowerSail(RPCMessage msg)
	{
		Lower(msg.player);
	}

	public void Lower(BasePlayer player)
	{
		if (CanBeLowered(player))
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Busy, b: true);
				flagsUpdateScope.Set(Flags.Reserved12, b: true);
			}
			WaitForLower();
		}
	}

	private void WaitForLower()
	{
		CancelInvoke(OnFullyLowered);
		timeUntilLoweredRaised = LowerDuration;
		Invoke(OnFullyLowered, LowerDuration);
	}

	private void OnFullyLowered()
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Busy, b: false);
			flagsUpdateScope.Set(Flags.Reserved12, b: false);
			flagsUpdateScope.Set(Flags.Reserved13, b: false);
			flagsUpdateScope.Set(Flags.Reserved3, b: true);
		}
		OnRaisedOrLowered();
	}

	private void OnRaisedOrLowered()
	{
		CacheIsWindBlocked();
		ToggleColliders();
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(5uL)]
	public void RaiseSail(RPCMessage msg)
	{
		Raise(msg.player);
	}

	public void Raise(BasePlayer player, bool instant = false)
	{
		if (instant)
		{
			OnFullyRaised();
		}
		else if (CanBeRaised(player))
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Busy, b: true);
				flagsUpdateScope.Set(Flags.Reserved13, b: true);
			}
			WaitForRaise();
		}
	}

	private void WaitForRaise()
	{
		CancelInvoke(OnFullyRaised);
		timeUntilLoweredRaised = RaiseDuration;
		Invoke(OnFullyRaised, RaiseDuration);
	}

	private void OnFullyRaised()
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Busy, b: false);
			flagsUpdateScope.Set(Flags.Reserved13, b: false);
			flagsUpdateScope.Set(Flags.Reserved12, b: false);
			flagsUpdateScope.Set(Flags.Reserved3, b: false);
		}
		OnRaisedOrLowered();
	}

	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RotateSail(RPCMessage msg)
	{
		RotateSail(msg.player);
	}

	private void RotateSail(BasePlayer player)
	{
		if (CanRotate(player))
		{
			base.transform.localRotation *= Quaternion.AngleAxis(180f, Vector3.up);
			CacheIsWindBlocked();
			SendNetworkUpdateImmediate();
			if (sailRotateEffect.isValid)
			{
				Effect.server.Run(sailRotateEffect.resourcePath, this);
			}
		}
	}

	void global::IBoatBuildingPiece.OnAddedToBoat(PlayerBoat boat)
	{
		Raise(null);
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		if ((old & Flags.Reserved3) == Flags.Reserved3 != ((next & Flags.Reserved3) == Flags.Reserved3))
		{
			ToggleColliders();
		}
	}

	protected override bool ShouldDisplayPickupOption(BasePlayer player)
	{
		if (base.ShouldDisplayPickupOption(player))
		{
			return !PlayerBoat.IsChildOfFinishedPlayerBoat(this);
		}
		return false;
	}
}
