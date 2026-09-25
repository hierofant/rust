#define UNITY_ASSERTIONS
using System;
using ConVar;
using Network;
using Oxide.Core;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class ReactiveTarget : Signage
{
	[Header("Reactive Target")]
	public Animator myAnimator;

	public GameObjectRef bullseyeEffect;

	public GameObjectRef knockdownEffect;

	public float activationPowerTime = 0.5f;

	public int activationPowerAmount = 1;

	public string mainBoneCollider = "target_collider";

	public string bullseyeBoneCollider = "target_collider_bullseye";

	public bool isPaintableTarget;

	public Transform movableColliderRoot;

	public Vector3 movableColliderKnockedDownAngle = new Vector3(-55f, 0f, 0f);

	public GameObject movablePlayerForceTrigger;

	private float lastToggleTime = float.NegativeInfinity;

	public const Flags Flag_KnockedDown = Flags.Reserved1;

	public float knockdownHealth = 100f;

	private int inputAmountReset;

	private int inputAmountLower;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("ReactiveTarget.OnRpcMessage"))
		{
			if (rpc == 1798082523 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_Lower");
				}
				using (TimeWarning.New("RPC_Lower"))
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
							RPC_Lower(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_Lower");
					}
				}
				return true;
			}
			if (rpc == 2169477377u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_Reset");
				}
				using (TimeWarning.New("RPC_Reset"))
				{
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg3 = rPCMessage;
							RPC_Reset(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in RPC_Reset");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override bool CanUpdateSign(BasePlayer player)
	{
		if (!isPaintableTarget)
		{
			return false;
		}
		return base.CanUpdateSign(player);
	}

	public void SetCollidersUpright()
	{
		movableColliderRoot.localEulerAngles = Vector3.zero;
		if (movablePlayerForceTrigger != null)
		{
			movablePlayerForceTrigger.SetActive(value: false);
		}
	}

	public void SetCollidersKnockedDown()
	{
		movableColliderRoot.localEulerAngles = movableColliderKnockedDownAngle;
		if (movablePlayerForceTrigger != null)
		{
			movablePlayerForceTrigger.SetActive(value: true);
		}
	}

	public void OnHitShared(HitInfo info)
	{
		if (IsKnockedDown() || IsLowered())
		{
			return;
		}
		bool num = info.HitBone == StringPool.Get(mainBoneCollider);
		bool flag = info.HitBone == StringPool.Get(bullseyeBoneCollider);
		if ((!num && !flag) || !base.isServer)
		{
			return;
		}
		float num2 = info.damageTypes.Total();
		if (flag)
		{
			num2 *= 2f;
			Effect.server.Run(bullseyeEffect.resourcePath, this, StringPool.Get(bullseyeBoneCollider), Vector3.zero, Vector3.zero);
		}
		knockdownHealth -= num2;
		if (knockdownHealth <= 0f)
		{
			Effect.server.Run(knockdownEffect.resourcePath, this, StringPool.Get(bullseyeBoneCollider), Vector3.zero, Vector3.zero);
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.On, b: false);
				flagsUpdateScope.Set(Flags.Reserved1, b: true);
			}
			QueueReset();
			SendPowerBurst();
			SendNetworkUpdate();
		}
		else
		{
			ClientRPC(RpcTarget.NetworkGroup("HitEffect"), info.Initiator.net.ID);
		}
		Hurt(1f, DamageType.Suicide, info.Initiator, useProtection: false);
	}

	public bool IsKnockedDown()
	{
		if (IsLowered())
		{
			return HasFlag(Flags.Reserved1);
		}
		return false;
	}

	public bool IsLowered()
	{
		return !HasFlag(Flags.On);
	}

	public override void OnAttacked(HitInfo info)
	{
		OnHitShared(info);
		base.OnAttacked(info);
	}

	public bool CanToggle()
	{
		float num = 1f;
		num = ((inputAmountReset > 0) ? 0.25f : 1f);
		return UnityEngine.Time.time > lastToggleTime + num;
	}

	public bool CanLower()
	{
		if (inputAmountLower <= inputAmountReset)
		{
			return inputAmountReset == 0;
		}
		return true;
	}

	public bool CanReset()
	{
		if (inputAmountReset <= inputAmountLower)
		{
			return inputAmountLower == 0;
		}
		return true;
	}

	public void QueueReset()
	{
		float time = ((inputAmountReset > 0) ? 0.25f : 6f);
		Invoke(ResetTarget, time);
	}

	public void ResetTarget()
	{
		if (IsLowered() && CanToggle() && CanReset())
		{
			CancelInvoke(ResetTarget);
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.On, b: true);
				flagsUpdateScope.Set(Flags.Reserved1, b: false);
			}
			knockdownHealth = 100f;
			SendPowerBurst();
			Interface.CallHook("OnReactiveTargetReset", this);
		}
	}

	private void LowerTarget()
	{
		if (!IsKnockedDown() && CanToggle() && CanLower())
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.On, b: false);
			}
			SendPowerBurst();
		}
	}

	private void SendPowerBurst()
	{
		lastToggleTime = UnityEngine.Time.time;
		MarkDirtyForceUpdateOutputs();
		Invoke(base.MarkDirtyForceUpdateOutputs, activationPowerTime * 1.01f);
	}

	public override int ConsumptionAmount()
	{
		return 1;
	}

	public override bool IsRootEntity()
	{
		return true;
	}

	public override void UpdateFromInput(int inputAmount, int inputSlot)
	{
		switch (inputSlot)
		{
		case 0:
			base.UpdateFromInput(inputAmount, inputSlot);
			break;
		case 1:
			inputAmountReset = inputAmount;
			if (inputAmount > 0)
			{
				ResetTarget();
			}
			break;
		case 2:
			inputAmountLower = inputAmount;
			if (inputAmount > 0)
			{
				LowerTarget();
			}
			break;
		}
	}

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		if (IsLowered())
		{
			if (IsPowered())
			{
				return base.GetPassthroughAmount();
			}
			if (IsKnockedDown() && UnityEngine.Time.time < lastToggleTime + activationPowerTime)
			{
				return activationPowerAmount;
			}
		}
		return 0;
	}

	[RPC_Server]
	public void RPC_Reset(RPCMessage msg)
	{
		ResetTarget();
	}

	[RPC_Server]
	public void RPC_Lower(RPCMessage msg)
	{
		LowerTarget();
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		if ((next & Flags.On) != Flags.On)
		{
			SetCollidersKnockedDown();
		}
		else
		{
			SetCollidersUpright();
		}
	}
}
