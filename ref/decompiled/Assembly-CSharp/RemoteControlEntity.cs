#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class RemoteControlEntity : BaseCombatEntity, IRemoteControllable, IAdminUpdatableIdentifier
{
	public static List<IRemoteControllable> allControllables = new List<IRemoteControllable>();

	[Header("RC Entity")]
	public string rcIdentifier = "";

	public Transform viewEyes;

	public GameObjectRef IDPanelPrefab;

	public RemoteControllableControls rcControls;

	public bool CanPing => true;

	public virtual bool CanAcceptInput => false;

	public int ViewerCount { get; set; }

	public CameraViewerId? ControllingViewerId { get; set; }

	public bool IsBeingControlled
	{
		get
		{
			if (ViewerCount > 0)
			{
				return ControllingViewerId.HasValue;
			}
			return false;
		}
	}

	public virtual bool RequiresMouse => false;

	public virtual float MaxRange => 10000f;

	public RemoteControllableControls RequiredControls => rcControls;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("RemoteControlEntity.OnRpcMessage"))
		{
			if (rpc == 2025588587 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - Server_AdminUpdateIdentifier");
				}
				using (TimeWarning.New("Server_AdminUpdateIdentifier"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(2025588587u, "Server_AdminUpdateIdentifier", this, player, 3f))
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
							Server_AdminUpdateIdentifier(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in Server_AdminUpdateIdentifier");
					}
				}
				return true;
			}
			if (rpc == 1677685895 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - SERVER_RequestOpenRCPanel");
				}
				using (TimeWarning.New("SERVER_RequestOpenRCPanel"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1677685895u, "SERVER_RequestOpenRCPanel", this, player, 3uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(1677685895u, "SERVER_RequestOpenRCPanel", this, player, 3f))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(1677685895u, "SERVER_RequestOpenRCPanel", this, player, 3f))
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
							SERVER_RequestOpenRCPanel(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in SERVER_RequestOpenRCPanel");
					}
				}
				return true;
			}
			if (rpc == 1053317251 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - Server_SetID");
				}
				using (TimeWarning.New("Server_SetID"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(1053317251u, "Server_SetID", this, player, 3f))
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
							Server_SetID(msg4);
						}
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
						player.Kick("RPC Error in Server_SetID");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public Transform GetEyes()
	{
		return viewEyes;
	}

	public float GetFovScale()
	{
		return 1f;
	}

	public BaseEntity GetEnt()
	{
		return this;
	}

	public string GetIdentifier()
	{
		return rcIdentifier;
	}

	public virtual bool InitializeControl(CameraViewerId viewerID)
	{
		ViewerCount++;
		if (CanAcceptInput && !ControllingViewerId.HasValue)
		{
			ControllingViewerId = viewerID;
			return true;
		}
		return !CanAcceptInput;
	}

	public virtual void StopControl(CameraViewerId viewerID)
	{
		ViewerCount--;
		if (ControllingViewerId == viewerID)
		{
			ControllingViewerId = null;
		}
	}

	public Matrix4x4 GetEyesMatrix()
	{
		return viewEyes.localToWorldMatrix;
	}

	public virtual void UserInput(InputState inputState, CameraViewerId viewerID)
	{
	}

	public void UpdateIdentifier(string newID, bool clientSend = false)
	{
		_ = rcIdentifier;
		if (base.isServer)
		{
			if (!IDInUse(newID))
			{
				rcIdentifier = newID;
			}
			SendNetworkUpdate();
		}
	}

	public virtual void RCSetup()
	{
		if (base.isServer)
		{
			InstallControllable(this);
		}
	}

	public virtual void RCShutdown()
	{
		if (base.isServer)
		{
			RemoveControllable(this);
		}
	}

	public override void InitShared()
	{
		base.InitShared();
		RCSetup();
	}

	public override void DestroyShared()
	{
		RCShutdown();
		base.DestroyShared();
	}

	public virtual bool CanControl(ulong playerID)
	{
		object obj = Interface.CallHook("OnEntityControl", this, playerID);
		if (obj is bool)
		{
			return (bool)obj;
		}
		return true;
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	public void Server_SetID(RPCMessage msg)
	{
		string oldID = msg.read.String();
		string newID = msg.read.String();
		SetID(msg.player, oldID, newID);
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	public void Server_AdminUpdateIdentifier(RPCMessage msg)
	{
		if (!(msg.player == null) && (msg.player.IsAdmin || msg.player.IsDeveloper))
		{
			string oldID = msg.read.String();
			string newID = msg.read.String();
			SetID(msg.player, oldID, newID, bypassChecks: true);
		}
	}

	public void SetID(BasePlayer player, string oldID, string newID, bool bypassChecks = false)
	{
		if (CanChangeID(player) && CanControl(player.userID) && (string.IsNullOrEmpty(oldID) || ComputerStation.IsValidIdentifier(oldID)) && ComputerStation.IsValidIdentifier(newID) && oldID == GetIdentifier())
		{
			Debug.Log("SetID success!");
			UpdateIdentifier(newID);
		}
	}

	[RPC_Server.CallsPerSecond(3uL)]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void SERVER_RequestOpenRCPanel(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!(player == null) && CanChangeID(player))
		{
			ClientRPC(RpcTarget.Player("CLIENT_OpenRCPanel", player), GetIdentifier());
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.forDisk)
		{
			info.msg.rcEntity = Facepunch.Pool.Get<RCEntity>();
			info.msg.rcEntity.identifier = GetIdentifier();
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.rcEntity != null && ComputerStation.IsValidIdentifier(info.msg.rcEntity.identifier))
		{
			UpdateIdentifier(info.msg.rcEntity.identifier);
		}
	}

	public virtual bool CanChangeID(BasePlayer player)
	{
		if (player != null)
		{
			if ((!player.CanBuild() || !player.IsBuildingAuthed() || !player.IsHoldingEntity<Hammer>()) && !player.IsAdmin)
			{
				return player.IsDeveloper;
			}
			return true;
		}
		return false;
	}

	public static bool IDInUse(string id)
	{
		return FindByID(id) != null;
	}

	public static IRemoteControllable FindByID(string id)
	{
		foreach (IRemoteControllable allControllable in allControllables)
		{
			if (allControllable != null && allControllable.GetIdentifier() == id)
			{
				return allControllable;
			}
		}
		return null;
	}

	public static bool InstallControllable(IRemoteControllable newControllable)
	{
		if (allControllables.Contains(newControllable))
		{
			return false;
		}
		allControllables.Add(newControllable);
		return true;
	}

	public static bool RemoveControllable(IRemoteControllable newControllable)
	{
		if (!allControllables.Contains(newControllable))
		{
			return false;
		}
		allControllables.Remove(newControllable);
		return true;
	}
}
