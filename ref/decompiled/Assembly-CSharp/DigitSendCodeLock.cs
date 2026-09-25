#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ConVar;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class DigitSendCodeLock : CodeLock
{
	public ParticleSystem digitsViewParticleSystem;

	public List<Transform> digitsParticleAnchorsFront;

	public List<Transform> digitsParticleAnchorsBack;

	private int __sync_digitsInputted;

	[Sync]
	private int digitsInputted
	{
		[CompilerGenerated]
		get
		{
			return __sync_digitsInputted;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_digitsInputted, value))
			{
				__sync_digitsInputted = value;
				byte nameID = __GetWeaverID("digitsInputted");
				QueueSyncVar(nameID);
			}
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("DigitSendCodeLock.OnRpcMessage"))
		{
			if (rpc == 3077276815u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - OnDigitEntered");
				}
				using (TimeWarning.New("OnDigitEntered"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3077276815u, "OnDigitEntered", this, player, 4uL))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(3077276815u, "OnDigitEntered", this, player, 3f))
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
							OnDigitEntered(rpc2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in OnDigitEntered");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	[RPC_Server.CallsPerSecond(4uL)]
	private void OnDigitEntered(RPCMessage rpc)
	{
		int num = rpc.read.Int16();
		if (num >= 0 && num <= 4)
		{
			digitsInputted = num;
		}
	}

	protected override bool WriteSyncVar(byte id, NetWrite writer)
	{
		if (id == 0)
		{
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log("SyncVar Writing: digitsInputted for " + iD.ToString());
			}
			SyncVarNetWrite(writer, __sync_digitsInputted);
			return true;
		}
		return base.WriteSyncVar(id, writer);
	}

	protected override bool OnSyncVar(byte id, NetRead reader, bool fromAutoSave = false)
	{
		if (id == 0)
		{
			try
			{
				_ = __sync_digitsInputted;
				int _sync_digitsInputted = reader.Int32();
				__sync_digitsInputted = _sync_digitsInputted;
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			return true;
		}
		return base.OnSyncVar(id, reader, fromAutoSave);
	}

	private byte __GetWeaverID(string propertyName)
	{
		if (propertyName == "digitsInputted")
		{
			return 0;
		}
		return byte.MaxValue;
	}

	protected override void WriteAutoSaveSyncVars(NetWrite writer)
	{
		base.WriteAutoSaveSyncVars(writer);
	}

	protected override void ReadAutoSaveSyncVars(NetRead reader)
	{
		base.ReadAutoSaveSyncVars(reader);
	}

	protected override bool AutoSaveSyncVars(SaveInfo save)
	{
		return true;
	}

	protected override bool AutoLoadSyncVars(LoadInfo load)
	{
		return true;
	}

	protected override void ResetSyncVars()
	{
		base.ResetSyncVars();
		__sync_digitsInputted = 0;
	}

	protected override bool ShouldInvalidateCache(byte id)
	{
		if (id == 0)
		{
			return true;
		}
		return base.ShouldInvalidateCache(id);
	}
}
