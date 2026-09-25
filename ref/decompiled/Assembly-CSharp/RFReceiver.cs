#define UNITY_ASSERTIONS
using System;
using ConVar;
using Network;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Assertions;

public class RFReceiver : IOEntity, IRFObject
{
	public int frequency;

	public GameObjectRef frequencyPanelPrefab;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("RFReceiver.OnRpcMessage"))
		{
			if (rpc == 1052196345 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - SERVER_RequestOpenPanel");
				}
				using (TimeWarning.New("SERVER_RequestOpenPanel"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1052196345u, "SERVER_RequestOpenPanel", this, player, 3uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(1052196345u, "SERVER_RequestOpenPanel", this, player, 3f))
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
							SERVER_RequestOpenPanel(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in SERVER_RequestOpenPanel");
					}
				}
				return true;
			}
			if (rpc == 2778616053u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - ServerSetFrequency");
				}
				using (TimeWarning.New("ServerSetFrequency"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2778616053u, "ServerSetFrequency", this, player, 3uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(2778616053u, "ServerSetFrequency", this, player, 3f))
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
							ServerSetFrequency(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in ServerSetFrequency");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	private bool CanChangeFrequency(BasePlayer player)
	{
		if (player != null)
		{
			return player.CanBuild();
		}
		return false;
	}

	public int GetFrequency()
	{
		return frequency;
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	public float GetMaxRange()
	{
		return 100000f;
	}

	public void RFSignalUpdate(bool on)
	{
		if (!base.IsDestroyed && IsOn() != on && !(!IsPowered() && on))
		{
			SetFlagLocal(Flags.On, on);
			SendNetworkUpdate_Flags();
			MarkDirty();
		}
	}

	internal override void DoServerDestroy()
	{
		RFManager.RemoveListener(frequency, this);
		base.DoServerDestroy();
	}

	public override void ResetIOState()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.On, b: false);
	}

	public override bool WantsPower(int inputIndex)
	{
		return IsOn();
	}

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		if (!IsOn())
		{
			return 0;
		}
		return GetCurrentEnergy();
	}

	public override void UpdateFromInput(int inputAmount, int inputSlot)
	{
		base.UpdateFromInput(inputAmount, inputSlot);
		if (inputAmount > 0 && !IsOn())
		{
			RFManager.AddListener(frequency, this);
		}
		else if (inputAmount == 0)
		{
			RFManager.RemoveListener(frequency, this);
			SetFlagLocal(Flags.On, b: false);
			SendNetworkUpdate_Flags();
		}
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(3uL)]
	[RPC_Server.IsVisible(3f)]
	public void ServerSetFrequency(RPCMessage msg)
	{
		if (!(msg.player == null) && msg.player.CanBuild())
		{
			int num = RFManager.ClampFrequency(msg.read.Int32());
			if (Interface.CallHook("OnRfFrequencyChange", this, num, msg.player) == null)
			{
				SetFrequency(num);
				Interface.CallHook("OnRfFrequencyChanged", this, num, msg.player);
			}
		}
	}

	public void SetFrequency(int freq)
	{
		freq = RFManager.ClampFrequency(freq);
		RFManager.ChangeFrequency(frequency, freq, this, isListener: true, IsPowered());
		frequency = freq;
		MarkDirty();
		SendNetworkUpdate();
	}

	[RPC_Server.CallsPerSecond(3uL)]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void SERVER_RequestOpenPanel(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!(player == null) && CanChangeFrequency(player))
		{
			ClientRPC(RpcTarget.Player("CLIENT_OpenPanel", player), frequency);
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.forDisk)
		{
			info.msg.ioEntity.genericInt1 = frequency;
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.ioEntity != null)
		{
			frequency = info.msg.ioEntity.genericInt1;
			if (info.fromDisk && base.isServer)
			{
				RFSignalUpdate(on: false);
			}
		}
	}
}
