#define UNITY_ASSERTIONS
using System;
using ConVar;
using Network;
using Oxide.Core;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class Lift : AnimatedBuildingBlock
{
	public GameObjectRef triggerPrefab;

	public string triggerBone;

	public float resetDelay = 5f;

	private Collider cabinTrigger;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Lift.OnRpcMessage"))
		{
			if (rpc == 2657791441u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_UseLift");
				}
				using (TimeWarning.New("RPC_UseLift"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(2657791441u, "RPC_UseLift", this, player, 3f))
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
							RPC_UseLift(rpc2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_UseLift");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	private void RPC_UseLift(RPCMessage rpc)
	{
		if (rpc.player.CanInteract() && Interface.CallHook("OnLiftUse", this, rpc.player) == null && PlayerIsAtCabin(rpc.player))
		{
			MoveUp();
		}
	}

	private bool PlayerIsAtCabin(BasePlayer player)
	{
		if (cabinTrigger == null && !ResolveCabinTrigger())
		{
			return false;
		}
		return cabinTrigger.bounds.SqrDistance(player.eyes.position) <= 9f;
	}

	private bool ResolveCabinTrigger()
	{
		foreach (BaseEntity child in children)
		{
			if (child.prefabID != triggerPrefab.resourceID)
			{
				continue;
			}
			TriggerParent[] componentsInChildren = child.GetComponentsInChildren<TriggerParent>();
			foreach (TriggerParent triggerParent in componentsInChildren)
			{
				if (!(GameObjectEx.ToBaseEntity(triggerParent.gameObject) != child))
				{
					cabinTrigger = triggerParent.GetComponent<Collider>();
					if (cabinTrigger != null)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private void MoveUp()
	{
		if (!IsOpen() && !IsBusy())
		{
			SetFlagLocal(Flags.Open, b: true);
			SendNetworkUpdateImmediate();
		}
	}

	private void MoveDown()
	{
		if (IsOpen() && !IsBusy())
		{
			SetFlagLocal(Flags.Open, b: false);
			SendNetworkUpdateImmediate();
		}
	}

	protected override void OnAnimatorDisabled()
	{
		if (base.isServer && IsOpen())
		{
			Invoke(MoveDown, resetDelay);
		}
	}

	public override void Spawn()
	{
		base.Spawn();
		bool flag = false;
		if (!Rust.Application.isLoadingSave && !flag)
		{
			BaseEntity baseEntity = GameManager.server.CreateEntity(triggerPrefab.resourcePath, Vector3.zero, Quaternion.identity);
			baseEntity.Spawn();
			baseEntity.SetParent(this, triggerBone);
		}
	}
}
