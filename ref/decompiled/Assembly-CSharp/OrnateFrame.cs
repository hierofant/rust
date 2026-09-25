#define UNITY_ASSERTIONS
using System;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using Network;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

public class OrnateFrame : PhotoFrame
{
	[Header("Ornate Frame Dependencies")]
	public TextMeshPro frameTextComponent;

	public GameObjectRef configureFrameDialog;

	private string __sync_FrameText;

	private Color __sync_TextColour;

	[Sync(Autosave = true)]
	public string FrameText
	{
		[CompilerGenerated]
		get
		{
			return __sync_FrameText;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_FrameText, value))
			{
				__sync_FrameText = value;
				byte nameID = __GetWeaverID("FrameText");
				QueueSyncVar(nameID);
			}
		}
	}

	[Sync(Autosave = true)]
	public Color TextColour
	{
		[CompilerGenerated]
		get
		{
			return __sync_TextColour;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_TextColour, value))
			{
				__sync_TextColour = value;
				byte nameID = __GetWeaverID("TextColour");
				QueueSyncVar(nameID);
			}
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("OrnateFrame.OnRpcMessage"))
		{
			if (rpc == 3398916869u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_ConfigureFrame");
				}
				using (TimeWarning.New("RPC_ConfigureFrame"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3398916869u, "RPC_ConfigureFrame", this, player, 2uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(3398916869u, "RPC_ConfigureFrame", this, player, 3f))
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
							RPC_ConfigureFrame(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_ConfigureFrame");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public virtual bool CanUpdateFrame(BasePlayer player)
	{
		if (player.IsAdmin || player.IsDeveloper)
		{
			return true;
		}
		if (!player.CanBuild())
		{
			return false;
		}
		if (IsLocked())
		{
			return (ulong)player.userID == base.OwnerID;
		}
		return true;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		TextColour = Color.black;
	}

	[RPC_Server.CallsPerSecond(2uL)]
	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_ConfigureFrame(RPCMessage msg)
	{
		string frameText = msg.read.String();
		Color textColour = msg.read.Color();
		SetFrameText(frameText);
		SetTextColour(textColour);
	}

	public void SetFrameText(string text)
	{
		FrameText = text;
	}

	public void SetTextColour(Color colour)
	{
		TextColour = colour;
	}

	protected override bool WriteSyncVar(byte id, NetWrite writer)
	{
		switch (id)
		{
		case 1:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log("SyncVar Writing: FrameText for " + iD.ToString());
			}
			SyncVarNetWrite(writer, __sync_FrameText);
			return true;
		case 2:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log("SyncVar Writing: TextColour for " + iD.ToString());
			}
			SyncVarNetWrite(writer, __sync_TextColour);
			return true;
		default:
			return base.WriteSyncVar(id, writer);
		}
	}

	protected override bool OnSyncVar(byte id, NetRead reader, bool fromAutoSave = false)
	{
		switch (id)
		{
		case 1:
			try
			{
				_ = __sync_FrameText;
				string _sync_FrameText = reader.String();
				__sync_FrameText = _sync_FrameText;
			}
			catch (Exception exception2)
			{
				Debug.LogException(exception2);
			}
			return true;
		case 2:
			try
			{
				_ = __sync_TextColour;
				Color _sync_TextColour = reader.Color();
				__sync_TextColour = _sync_TextColour;
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			return true;
		default:
			return base.OnSyncVar(id, reader, fromAutoSave);
		}
	}

	private byte __GetWeaverID(string propertyName)
	{
		if (!(propertyName == "FrameText"))
		{
			if (propertyName == "TextColour")
			{
				return 2;
			}
			return byte.MaxValue;
		}
		return 1;
	}

	protected override void WriteAutoSaveSyncVars(NetWrite writer)
	{
		base.WriteAutoSaveSyncVars(writer);
		WriteSyncVar(1, writer);
		WriteSyncVar(2, writer);
	}

	protected override void ReadAutoSaveSyncVars(NetRead reader)
	{
		base.ReadAutoSaveSyncVars(reader);
		OnSyncVar(1, reader, fromAutoSave: true);
		OnSyncVar(2, reader, fromAutoSave: true);
	}

	protected override bool AutoSaveSyncVars(SaveInfo save)
	{
		NetWrite obj = Network.Net.sv.StartWrite();
		WriteAutoSaveSyncVars(obj);
		var (src, num) = obj.GetBuffer();
		if (_autosaveBuffer == null)
		{
			_autosaveBuffer = BaseEntity._autosaveBufferPool.Rent(num);
		}
		if (_autosaveBuffer.Length < num)
		{
			BaseEntity._autosaveBufferPool.Return(_autosaveBuffer);
			_autosaveBuffer = BaseEntity._autosaveBufferPool.Rent(num);
		}
		Buffer.BlockCopy(src, 0, _autosaveBuffer, 0, num);
		save.msg.baseEntity.syncVars = _autosaveBuffer;
		Facepunch.Pool.Free(ref obj);
		return true;
	}

	protected override bool AutoLoadSyncVars(LoadInfo load)
	{
		if (load.msg.baseEntity != null && load.msg.baseEntity.syncVars != null)
		{
			NetRead obj = Facepunch.Pool.Get<NetRead>();
			obj.Init(load.msg.baseEntity.syncVars.AsSpan());
			ReadAutoSaveSyncVars(obj);
			Facepunch.Pool.Free(ref obj);
		}
		return true;
	}

	protected override void ResetSyncVars()
	{
		base.ResetSyncVars();
		__sync_FrameText = null;
		__sync_TextColour = default(Color);
	}

	protected override bool ShouldInvalidateCache(byte id)
	{
		return id switch
		{
			1 => true, 
			2 => true, 
			_ => base.ShouldInvalidateCache(id), 
		};
	}
}
