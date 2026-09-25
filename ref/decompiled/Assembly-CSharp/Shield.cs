#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public class Shield : HeldEntity
{
	public const Flags Blocking = Flags.Reserved6;

	public float DeployDelay = 1f;

	public float ShieldOnBackToggleCooldown = 3f;

	public float ShieldStowAimDelay = 0.75f;

	public ProtectionProperties Protection;

	[Range(0f, 1f)]
	public float DamageMitigationFactor;

	public Collider ShieldCollider;

	[Tooltip("This is the collider for the shield when not actively in use")]
	public Collider sideShieldCollider;

	public float MaxBlockTime = 4f;

	public float MinBlockTime = 1f;

	public float damageToLoseOneSecond = 50f;

	[Tooltip("How long after we stop blocking before we begin charging our block")]
	public float chargeDelay = 1f;

	public GameObjectRef MeleeLocalPlayerImpactFxPrefab;

	public GameObjectRef RangedLocalPlayerImpactFxPrefab;

	public ShieldAnimationSubSystem ShieldAnimSystem;

	private float lastBlockTime;

	[ReplicatedVar]
	public static bool InfiniteShieldBlock = false;

	private Action shieldBlockTick;

	private bool serverWantsBlock;

	private static Vector3 MaximumLocalPosition = new Vector3(0.39f, 1.62f, 0.41f);

	private static Vector3 MinimumLocalPosition = new Vector3(-0.66f, 0.66f, -0.44f);

	private static Vector3 MaximumLocalRotation = new Vector3(360f, 360f, 360f);

	private static Vector3 MinimumLocalRotation = new Vector3(2.5f, 2.14f, 0.04f);

	private TimeSince serverSideShieldBlockStarted;

	private float serverSideBlockPower;

	private TimeSince lastLocalPlayerUpdateTick;

	private HeldEntity tickingHeldEntity;

	private bool lastAppliedShieldOnBack;

	private TimeSince timeSinceShieldOnBackChanged;

	public override bool IsShield => true;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Shield.OnRpcMessage"))
		{
			if (rpc == 2238556937u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - ServerToggleBlock");
				}
				using (TimeWarning.New("ServerToggleBlock"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2238556937u, "ServerToggleBlock", this, player, 10uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(2238556937u, "ServerToggleBlock", this, player))
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
							ServerToggleBlock(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in ServerToggleBlock");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool IsBlocking()
	{
		return HasFlag(Flags.Reserved6);
	}

	public override void OnAttacked(HitInfo info)
	{
		base.OnAttacked(info);
		if (base.isServer)
		{
			ServerSideAttack(info);
		}
	}

	public bool RaycastAgainstColliders(Ray r, float maxDistance)
	{
		RaycastHit hitInfo;
		if (ShieldCollider != null)
		{
			return ShieldCollider.Raycast(r, out hitInfo, maxDistance);
		}
		return false;
	}

	public bool SphereCastAgainstColliders(Vector3 center, float radius)
	{
		return Vector3.Distance(ClosestPoint(center), center) <= radius;
	}

	public string GetHitMaterialString()
	{
		return AssetNameCache.GetName(ShieldCollider.sharedMaterial);
	}

	[RPC_Server.CallsPerSecond(10uL)]
	[RPC_Server.FromOwner]
	[RPC_Server]
	private void ServerToggleBlock(RPCMessage msg)
	{
		bool flag = msg.read.Bit();
		serverWantsBlock = flag;
		if (shieldBlockTick == null)
		{
			shieldBlockTick = ShieldBlockTick;
		}
		if (!IsInvoking(shieldBlockTick))
		{
			InvokeRepeating(shieldBlockTick, 0f, 0f);
		}
	}

	private void ServerSideAttack(HitInfo info)
	{
		Item item = GetItem();
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (IsBlocking() && ownerPlayer != null)
		{
			float num = info.damageTypes.Total();
			serverSideBlockPower = Mathf.Clamp(serverSideBlockPower + num / damageToLoseOneSecond, 0f, MaxBlockTime);
			ClientRPC(RpcTarget.Player("ClientUpdateShieldPowerTime", ownerPlayer), serverSideBlockPower / MaxBlockTime);
		}
		if (item != null)
		{
			Protection.Scale(info.damageTypes);
			info.HitBone = 0u;
			float num2 = info.damageTypes.Total();
			info.damageTypes.ScaleAll(Mathf.Clamp01(1f - DamageMitigationFactor));
			float amount = num2 - info.damageTypes.Total();
			if (ownerPlayer != null)
			{
				ownerPlayer.OnAttacked(info);
			}
			item.LoseCondition(amount);
		}
		bool arg = info.Weapon != null && info.Weapon is BaseMelee;
		if (ownerPlayer != null)
		{
			ClientRPC(RpcTarget.NetworkGroup("ClientShieldHit", ownerPlayer), arg, (info.InitiatorPlayer != null) ? info.InitiatorPlayer.userID.Get() : 0);
		}
	}

	private void DestroyShield()
	{
		List<BaseEntity> obj = Facepunch.Pool.Get<List<BaseEntity>>();
		foreach (BaseEntity child in children)
		{
			obj.Add(child);
		}
		foreach (BaseEntity item in obj)
		{
			item.SetParent(null, worldPositionStays: true);
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	public override float AntiHackPadding()
	{
		if (GetOwnerPlayer() != null && GetOwnerPlayer().IsBot)
		{
			return 3f;
		}
		return 0.75f;
	}

	public override void SetHeld(bool bHeld)
	{
		base.SetHeld(bHeld);
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (ownerPlayer != null)
		{
			ownerPlayer.modelState.blocking = false;
			if (ownerPlayer.ActivePlayerInd != -1)
			{
				NativeArray<ModelState.Flag> playerModelStateFlags = BasePlayer.PlayerStates.PlayerModelStateFlags;
				playerModelStateFlags[ownerPlayer.ActivePlayerInd] &= (ModelState.Flag)(-65537);
			}
		}
	}

	private void ShieldBlockTick()
	{
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (IsDisabled() || ownerPlayer == null)
		{
			return;
		}
		HeldEntity heldEntity = ownerPlayer.GetHeldEntity();
		if (heldEntity != tickingHeldEntity)
		{
			tickingHeldEntity = heldEntity;
			serverSideBlockPower = 0f;
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		if (serverWantsBlock)
		{
			if (!IsBlocking() && serverSideBlockPower < MaxBlockTime - MinBlockTime)
			{
				serverSideShieldBlockStarted = 0f;
				flagsUpdateScope.Set(Flags.Reserved6, b: true);
			}
		}
		else if (IsBlocking() && (float)serverSideShieldBlockStarted > MinBlockTime)
		{
			flagsUpdateScope.Set(Flags.Reserved6, b: false);
		}
		ownerPlayer.modelState.blocking = IsBlocking();
		if (ownerPlayer.ActivePlayerInd != -1)
		{
			NativeArray<ModelState.Flag> playerModelStateFlags = BasePlayer.PlayerStates.PlayerModelStateFlags;
			if (ownerPlayer.modelState.blocking)
			{
				playerModelStateFlags[ownerPlayer.ActivePlayerInd] |= ModelState.Flag.Blocking;
			}
			else
			{
				playerModelStateFlags[ownerPlayer.ActivePlayerInd] &= (ModelState.Flag)(-65537);
			}
		}
		if (IsBlocking())
		{
			lastBlockTime = UnityEngine.Time.realtimeSinceStartup;
		}
		bool flag = UnityEngine.Time.realtimeSinceStartup >= lastBlockTime + chargeDelay;
		serverSideBlockPower = Mathf.MoveTowards(serverSideBlockPower, IsBlocking() ? MaxBlockTime : (flag ? 0f : serverSideBlockPower), UnityEngine.Time.deltaTime);
		if ((float)lastLocalPlayerUpdateTick > 0.5f)
		{
			ClientRPC(RpcTarget.Player("ClientUpdateShieldPowerTime", ownerPlayer), serverSideBlockPower / MaxBlockTime);
			lastLocalPlayerUpdateTick = 0f;
		}
		if (HasFlag(Flags.Reserved6) && serverSideBlockPower >= MaxBlockTime && !InfiniteShieldBlock)
		{
			flagsUpdateScope.Set(Flags.Reserved6, b: false);
		}
		if (!IsBlocking() && serverSideBlockPower <= 0f)
		{
			CancelInvoke(shieldBlockTick);
		}
	}

	public override void ServerTick(BasePlayer byPlayer)
	{
		base.ServerTick(byPlayer);
		if (IsDisabled())
		{
			return;
		}
		bool flag = byPlayer.WantsShieldOnBack();
		if (flag != lastAppliedShieldOnBack && (float)timeSinceShieldOnBackChanged > ShieldOnBackToggleCooldown)
		{
			lastAppliedShieldOnBack = flag;
			timeSinceShieldOnBackChanged = 0f;
			byPlayer.GetHeldEntity()?.UpdateShieldState(bHeld: true);
		}
		if (byPlayer.modelState != null)
		{
			Vector3 vector = byPlayer.modelState.localShieldPos;
			if (vector.IsNaNOrInfinity())
			{
				vector = Vector3.Lerp(MinimumLocalPosition, MaximumLocalPosition, 0.5f);
			}
			Vector3 vector2 = byPlayer.modelState.localShieldRot;
			if (vector2.IsNaNOrInfinity())
			{
				vector2 = Vector3.Lerp(MinimumLocalRotation, MaximumLocalRotation, 0.5f);
			}
			vector.x = Mathf.Clamp(vector.x, MinimumLocalPosition.x, MaximumLocalPosition.x);
			vector.y = Mathf.Clamp(vector.y, MinimumLocalPosition.y, MaximumLocalPosition.y);
			vector.z = Mathf.Clamp(vector.z, MinimumLocalPosition.z, MaximumLocalPosition.z);
			vector2.x = Mathf.Clamp(vector2.x, MinimumLocalRotation.x, MaximumLocalRotation.x);
			vector2.y = Mathf.Clamp(vector2.y, MinimumLocalRotation.y, MaximumLocalRotation.y);
			vector2.z = Mathf.Clamp(vector2.z, MinimumLocalRotation.z, MaximumLocalRotation.z);
			base.transform.SetLocalPositionAndRotation(vector, Quaternion.Euler(vector2));
		}
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		bool flag = (next & Flags.Broken) != Flags.Broken && (next & Flags.Reserved4) == Flags.Reserved4;
		if (!base.isServer)
		{
			return;
		}
		if ((old & Flags.Broken) == Flags.Broken != ((next & Flags.Broken) == Flags.Broken))
		{
			BasePlayer ownerPlayer = GetOwnerPlayer();
			if (ownerPlayer != null)
			{
				HeldEntity heldEntity = ownerPlayer.GetHeldEntity();
				if (heldEntity != null)
				{
					heldEntity.UpdateShieldState(bHeld: true);
				}
				if ((next & Flags.Broken) == Flags.Broken)
				{
					DestroyShield();
					if (heldEntity != null)
					{
						heldEntity.UpdateShieldState(bHeld: false);
					}
				}
			}
		}
		else if ((old & Flags.Reserved4) == Flags.Reserved4 && (next & Flags.Reserved4) != Flags.Reserved4)
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved6, b: false);
			}
			serverWantsBlock = false;
		}
		SetMainColliderState(flag && (next & Flags.Reserved6) == Flags.Reserved6);
		SetSideColliderState(flag && (next & Flags.Reserved6) != Flags.Reserved6);
	}

	public void SetMainColliderState(bool enable)
	{
		ShieldCollider.enabled = enable;
	}

	public void SetSideColliderState(bool enable)
	{
		sideShieldCollider.enabled = enable;
	}
}
