#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class SpinUpWeapon : BaseProjectile, ITurretNotify
{
	public float timeBetweenSpinToggle = 1f;

	public float spinUpTime = 1f;

	public GameObjectRef bulletEffect;

	public float projectileThicknessOverride = 0.5f;

	public bool showSpinProgress = true;

	public float spinningMoveSpeedScale = 0.7f;

	public float conditionLossPerSecondSpinning = 1f;

	public ItemModWearable BackpackWearable;

	public const Flags FullySpunFlag = Flags.Reserved10;

	public const Flags SpinningFlag = Flags.Reserved11;

	public const Flags ShootingFlag = Flags.Reserved12;

	private const float bulletSpeed = 375f;

	private float lastSpinToggleTime = float.NegativeInfinity;

	public override ItemModWearable WearableWhileEquipped
	{
		get
		{
			BasePlayer ownerPlayer = GetOwnerPlayer();
			if (ownerPlayer != null && ownerPlayer.inventory.HasBackpackItem())
			{
				return null;
			}
			return BackpackWearable;
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("SpinUpWeapon.OnRpcMessage"))
		{
			if (rpc == 2014484270 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - Server_SetSpinButton");
				}
				using (TimeWarning.New("Server_SetSpinButton"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2014484270u, "Server_SetSpinButton", this, player, 8uL))
						{
							return true;
						}
						if (!RPC_Server.IsActiveItem.Test(2014484270u, "Server_SetSpinButton", this, player))
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
							Server_SetSpinButton(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in Server_SetSpinButton");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override float GetOverrideProjectileThickness(Projectile projectile)
	{
		return projectileThicknessOverride;
	}

	public bool IsSpinning()
	{
		return HasFlag(Flags.Reserved11);
	}

	public bool IsFullySpun()
	{
		return HasFlag(Flags.Reserved10);
	}

	public override void ServerReload()
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved12, b: false);
		}
		base.ServerReload();
	}

	public override void ServerUse(HeldEntityServerUseParams parameters)
	{
		if (!ServerIsReloading())
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved12, b: true);
			}
			Invoke(StopMainTrigger, repeatDelay * 1.1f);
		}
		base.ServerUse(parameters);
	}

	public override void SetGenericVisible(bool visible)
	{
		base.SetGenericVisible(visible);
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved11, visible);
	}

	public override void OnHeldChanged()
	{
		base.OnHeldChanged();
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			BasePlayer ownerPlayer = GetOwnerPlayer();
			if (ownerPlayer != null && ownerPlayer.IsNpc)
			{
				flagsUpdateScope.Set(Flags.Reserved11, !IsDisabled());
			}
			else
			{
				flagsUpdateScope.Set(Flags.Reserved11, b: false);
				flagsUpdateScope.Set(Flags.Reserved10, b: false);
				lastSpinToggleTime = float.NegativeInfinity;
			}
		}
		if (IsDisabled())
		{
			CancelInvoke(UpdateConditionLoss);
			CancelInvoke(SetFullySpun);
		}
		else
		{
			InvokeRepeating(UpdateConditionLoss, 0f, 1f);
		}
	}

	public void UpdateConditionLoss()
	{
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (!(ownerPlayer == null) && !ownerPlayer.IsNpc && IsSpinning())
		{
			GetOwnerItem()?.LoseCondition(conditionLossPerSecondSpinning);
		}
	}

	public void FireFakeBulletServer(float aimconeToUse)
	{
		BasePlayer ownerPlayer = GetOwnerPlayer();
		bool flag = ownerPlayer != null;
		Vector3 obj = (flag ? ownerPlayer.eyes.BodyForward() : MuzzlePoint.forward);
		Vector3 vector = (flag ? ownerPlayer.eyes.position : MuzzlePoint.position);
		Vector3 inputVec = obj;
		inputVec = AimConeUtil.GetModifiedAimConeDirection(aimconeToUse, inputVec);
		List<Connection> obj2 = Facepunch.Pool.Get<List<Connection>>();
		foreach (Connection subscriber in net.group.subscribers)
		{
			BasePlayer basePlayer = subscriber.player as BasePlayer;
			if (!(basePlayer == null) && !ShouldNetworkTo(basePlayer))
			{
				obj2.Add(subscriber);
			}
		}
		if (obj2.Count > 0)
		{
			CreateProjectileEffectClientside(bulletEffect.resourcePath, vector + inputVec * 2f, inputVec * 375f, 0, flag ? ownerPlayer.net.connection : null, IsSilenced(), forceClientsideEffects: true, obj2);
		}
		Facepunch.Pool.FreeUnmanaged(ref obj2);
	}

	public void StopMainTrigger()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved12, b: false);
	}

	public override void DidAttackServerside()
	{
		base.DidAttackServerside();
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved12, b: true);
		}
		Invoke(StopMainTrigger, repeatDelay * 1.1f);
		if (ServerOcclusion.OcclusionEnabled)
		{
			DoFakeBullets();
		}
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(8uL)]
	[RPC_Server.IsActiveItem]
	private void Server_SetSpinButton(RPCMessage msg)
	{
		bool flag = msg.read.Bit();
		if (UnityEngine.Time.realtimeSinceStartup < lastSpinToggleTime + 1f)
		{
			return;
		}
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved11, flag);
			CancelInvoke(SetFullySpun);
			if (flag)
			{
				Invoke(SetFullySpun, spinUpTime);
			}
			else
			{
				flagsUpdateScope.Set(Flags.Reserved10, b: false);
			}
		}
		lastSpinToggleTime = UnityEngine.Time.realtimeSinceStartup;
	}

	public void SetFullySpun()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved10, b: true);
	}

	public void WarmupTick(bool wantsShoot)
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		if (wantsShoot)
		{
			flagsUpdateScope.Set(Flags.Reserved11, b: true);
			if (!IsInvoking(SetFullySpun))
			{
				Invoke(SetFullySpun, spinUpTime);
			}
			lastSpinToggleTime = UnityEngine.Time.realtimeSinceStartup;
		}
		else if (UnityEngine.Time.realtimeSinceStartup > lastSpinToggleTime + 10f)
		{
			CancelInvoke(SetFullySpun);
			flagsUpdateScope.Set(Flags.Reserved11, b: false);
			flagsUpdateScope.Set(Flags.Reserved10, b: false);
		}
	}

	public bool CanShoot()
	{
		return IsFullySpun();
	}

	public void OnAddedRemovedToTurret(bool added)
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved11, b: false);
			flagsUpdateScope.Set(Flags.Reserved10, b: false);
		}
		if (added)
		{
			lastSpinToggleTime = float.NegativeInfinity;
			return;
		}
		CancelInvoke(UpdateConditionLoss);
		CancelInvoke(SetFullySpun);
	}

	private void DoFakeBullets()
	{
		float num = repeatDelay / 4f;
		if (!IsInvoking(FakeBullet1))
		{
			Invoke(FakeBullet1, num);
		}
		if (!IsInvoking(FakeBullet2))
		{
			Invoke(FakeBullet2, num * 2f);
		}
		if (!IsInvoking(FakeBullet3))
		{
			Invoke(FakeBullet3, num * 3f);
		}
	}

	private void FakeBullet()
	{
		if (base.isServer)
		{
			FireFakeBulletServer(aimCone * 3f);
		}
	}

	private void FakeBullet1()
	{
		FakeBullet();
	}

	private void FakeBullet2()
	{
		FakeBullet();
	}

	private void FakeBullet3()
	{
		FakeBullet();
	}

	private void CancelFakeBullets()
	{
		CancelInvoke(FakeBullet1);
		CancelInvoke(FakeBullet2);
		CancelInvoke(FakeBullet3);
	}
}
