#define UNITY_ASSERTIONS
using System;
using ConVar;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class EaselDeployable : DecorDeployable
{
	[Header("Painting Easel")]
	public Transform easelTopBar;

	public Transform easelBottomBar;

	public Transform cameraPaintingAnchor;

	public IEaselPaintable paintable;

	private const Flags HasPaintingFlag = Flags.Reserved3;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("EaselDeployable.OnRpcMessage"))
		{
			if (rpc == 1365820335 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - Server_StartPainting");
				}
				using (TimeWarning.New("Server_StartPainting"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1365820335u, "Server_StartPainting", this, player, 2uL))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(1365820335u, "Server_StartPainting", this, player, 6f))
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
							Server_StartPainting(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in Server_StartPainting");
					}
				}
				return true;
			}
			if (rpc == 3444709649u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - Server_StopPainting");
				}
				using (TimeWarning.New("Server_StopPainting"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3444709649u, "Server_StopPainting", this, player, 2uL))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(3444709649u, "Server_StopPainting", this, player, 6f))
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
							Server_StopPainting(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in Server_StopPainting");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public void SetPainting(IEaselPaintable newPaintable)
	{
		paintable = newPaintable;
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved3, paintable != null);
		}
		_ = paintable;
	}

	public void RemovePainting(IEaselPaintable newSign)
	{
		if (newSign != paintable)
		{
			return;
		}
		paintable = null;
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved3, b: false);
	}

	public override void OnDied(HitInfo info)
	{
		if (paintable != null)
		{
			DecayEntity component = paintable.GameObject.GetComponent<DecayEntity>();
			if (component != null)
			{
				Item item = ItemManager.Create(component.pickup.itemTarget, 1, 0uL, isServerSide: true, 0uL);
				paintable.SaveSignageToItem(item);
				item.CreateWorldObject(cameraPaintingAnchor.position);
				component.Die();
			}
		}
		base.OnDied(info);
	}

	[RPC_Server.CallsPerSecond(2uL)]
	[RPC_Server.MaxDistance(6f)]
	[RPC_Server]
	public void Server_StartPainting(RPCMessage msg)
	{
	}

	[RPC_Server.CallsPerSecond(2uL)]
	[RPC_Server]
	[RPC_Server.MaxDistance(6f)]
	public void Server_StopPainting(RPCMessage msg)
	{
	}

	public override bool SupportsChildDeployables()
	{
		return true;
	}
}
