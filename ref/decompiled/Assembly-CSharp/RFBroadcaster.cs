#define UNITY_ASSERTIONS
using System;
using ConVar;
using Network;
using Oxide.Core;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class RFBroadcaster : IOEntity, IRFObject
{
	public bool playerUsable = true;

	public int frequency;

	public GameObjectRef frequencyPanelPrefab;

	public const Flags Flag_Broadcasting = Flags.Reserved3;

	private float nextChangeTime;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("RFBroadcaster.OnRpcMessage"))
		{
			if (rpc == 1052196345 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
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
						if (!RPC_Server.MaxDistance.Test(1052196345u, "SERVER_RequestOpenPanel", this, player, 3f))
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
				if (ConVar.Global.developer > 2)
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
		if (playerUsable && player != null)
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
		return float.PositiveInfinity;
	}

	public void RFSignalUpdate(bool on)
	{
	}

	public override bool WantsPower(int inputIndex)
	{
		return true;
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(3uL)]
	public void ServerSetFrequency(RPCMessage msg)
	{
		if (CanChangeFrequency(msg.player) && !(UnityEngine.Time.time < nextChangeTime))
		{
			nextChangeTime = UnityEngine.Time.time + 2f;
			int num = RFManager.ClampFrequency(msg.read.Int32());
			if (RFManager.IsReserved(num))
			{
				RFManager.ReserveErrorPrint(msg.player);
			}
			else if (Interface.CallHook("OnRfFrequencyChange", this, num, msg.player) == null)
			{
				SetFrequency(num);
				Hurt(MaxHealth() * 0.01f, DamageType.Decay, this);
				Interface.CallHook("OnRfFrequencyChanged", this, num, msg.player);
			}
		}
	}

	public void SetFrequency(int freq)
	{
		freq = RFManager.ClampFrequency(freq);
		RFManager.ChangeFrequency(frequency, freq, this, isListener: false, IsPowered());
		frequency = freq;
		MarkDirty();
		SendNetworkUpdate();
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
		}
	}

	public override void UpdateFromInput(int inputAmount, int inputSlot)
	{
		base.UpdateFromInput(inputAmount, inputSlot);
		if (inputAmount > 0)
		{
			RFManager.AddBroadcaster(frequency, this);
			SetFlagLocal(Flags.Reserved3, b: true);
			SendNetworkUpdate_Flags();
		}
		else
		{
			StopBroadcasting();
		}
	}

	public void StopBroadcasting()
	{
		SetFlagLocal(Flags.Reserved3, b: false);
		SendNetworkUpdate_Flags();
		RFManager.RemoveBroadcaster(frequency, this);
	}

	internal override void DoServerDestroy()
	{
		RFManager.RemoveBroadcaster(frequency, this);
		base.DoServerDestroy();
	}

	[RPC_Server.CallsPerSecond(3uL)]
	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.MaxDistance(3f)]
	public void SERVER_RequestOpenPanel(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!(player == null) && CanChangeFrequency(player))
		{
			ClientRPC(RpcTarget.Player("CLIENT_OpenPanel", player), frequency);
		}
	}
}
