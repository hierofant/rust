#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class FishMount : StorageContainer
{
	public Animator[] FishRoots = new Animator[0];

	public GameObjectRef FishInteractSound = new GameObjectRef();

	public float UseCooldown = 3f;

	public const Flags HasFish = Flags.Reserved1;

	private int currentFishItemIndex = -1;

	private int GetCurrentFishItemIndex
	{
		get
		{
			Item slot = base.inventory.GetSlot(0);
			if (slot != null && slot.info.TryGetComponent<ItemModFishable>(out var component))
			{
				return component.FishMountIndex;
			}
			return -1;
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("FishMount.OnRpcMessage"))
		{
			if (rpc == 3280542489u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - UseFish");
				}
				using (TimeWarning.New("UseFish"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3280542489u, "UseFish", this, player, 3f))
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
							UseFish(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in UseFish");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.msg.simpleInt == null)
		{
			info.msg.simpleInt = Facepunch.Pool.Get<SimpleInt>();
		}
		info.msg.simpleInt.value = currentFishItemIndex;
	}

	public override bool ItemFilter(BasePlayer player, Item item, int targetSlot)
	{
		if (item.info.TryGetComponent<ItemModFishable>(out var component) && component.CanBeMounted)
		{
			return true;
		}
		return false;
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		currentFishItemIndex = GetCurrentFishItemIndex;
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Busy, b: false);
	}

	public override void OnItemAddedOrRemoved(Item item, bool added)
	{
		base.OnItemAddedOrRemoved(item, added);
		currentFishItemIndex = GetCurrentFishItemIndex;
		SetFlagLocal(Flags.Reserved1, currentFishItemIndex >= 0);
		SendNetworkUpdate();
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void UseFish(RPCMessage msg)
	{
		if (HasFlag(Flags.Reserved1) && !IsBusy())
		{
			Effect.server.Run(FishInteractSound.resourcePath, base.transform.position);
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Busy, b: true);
			}
			Invoke(ClearBusy, UseCooldown);
			ClientRPC(RpcTarget.NetworkGroup("PlayAnimation"));
		}
	}

	private void ClearBusy()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Busy, b: false);
	}
}
