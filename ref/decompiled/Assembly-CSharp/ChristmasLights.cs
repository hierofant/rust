#define UNITY_ASSERTIONS
using System;
using ConVar;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class ChristmasLights : StringLights
{
	public enum AnimationType
	{
		ON = 1,
		FLASHING = 2,
		CHASING = 3,
		FADE = 4,
		SLOWGLOW = 6
	}

	public AnimationType animationStyle = AnimationType.ON;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("ChristmasLights.OnRpcMessage"))
		{
			if (rpc == 115959498 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - SERVER_SetAnimationStyle");
				}
				using (TimeWarning.New("SERVER_SetAnimationStyle"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(115959498u, "SERVER_SetAnimationStyle", this, player, 3uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(115959498u, "SERVER_SetAnimationStyle", this, player, 3f))
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
							SERVER_SetAnimationStyle(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in SERVER_SetAnimationStyle");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override int ConsumptionAmount()
	{
		return 5;
	}

	[RPC_Server.CallsPerSecond(3uL)]
	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void SERVER_SetAnimationStyle(RPCMessage msg)
	{
		AnimationType animationType = (AnimationType)Mathf.Clamp(msg.read.Int32(), 1, 7);
		if (animationType != animationStyle)
		{
			animationStyle = animationType;
			SendNetworkUpdate();
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.lightString.animationStyle = (int)animationStyle;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.lightString != null)
		{
			animationStyle = (AnimationType)info.msg.lightString.animationStyle;
		}
	}
}
