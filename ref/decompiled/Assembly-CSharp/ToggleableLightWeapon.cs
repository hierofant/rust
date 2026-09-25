#define UNITY_ASSERTIONS
using System;
using ConVar;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class ToggleableLightWeapon : BaseMelee
{
	[Header("ToggleableLightWeapon")]
	public bool ExtinguishUnderwater;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("ToggleableLightWeapon.OnRpcMessage"))
		{
			if (rpc == 2235491565u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - Extinguish");
				}
				using (TimeWarning.New("Extinguish"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(2235491565u, "Extinguish", this, player))
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
							Extinguish(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in Extinguish");
					}
				}
				return true;
			}
			if (rpc == 3010584743u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - Ignite");
				}
				using (TimeWarning.New("Ignite"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(3010584743u, "Ignite", this, player))
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
							Ignite(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in Ignite");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public virtual void SetIsOn(bool isOn)
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.On, isOn);
	}

	[RPC_Server.IsActiveItem]
	[RPC_Server]
	protected void Ignite(RPCMessage msg)
	{
		if (msg.player.CanInteract())
		{
			SetIsOn(isOn: true);
		}
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	protected void Extinguish(RPCMessage msg)
	{
		if (msg.player.CanInteract())
		{
			SetIsOn(isOn: false);
		}
	}

	public override void OnHeldChanged()
	{
		if (IsDisabled())
		{
			SetIsOn(isOn: false);
		}
	}
}
