#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class CommandBlock : IOEntity
{
	public GameObjectRef commandPanelPrefab;

	[HideInInspector]
	public string currentCommand;

	private int currentPower;

	[HideInInspector]
	public ulong lastPlayerID;

	private static readonly Translate.Phrase disabledErrorPhrase = new Translate.Phrase("commandblock.disabled.error", "Command blocks are currently disabled");

	[ServerVar(Help = "Can command blocks execute commands")]
	public static bool commands_enabled = false;

	[ServerVar(Help = "If enabled, commands from command blocks will run using the last player who set them, allowing for a wider range of commands to be used")]
	public static bool use_player = false;

	[ServerVar(Help = "Print a log message when a command block is executed")]
	public static bool log_executions = true;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("CommandBlock.OnRpcMessage"))
		{
			if (rpc == 1558722905 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_SetCommand");
				}
				using (TimeWarning.New("RPC_SetCommand"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1558722905u, "RPC_SetCommand", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(1558722905u, "RPC_SetCommand", this, player, 3f))
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
							RPC_SetCommand(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_SetCommand");
					}
				}
				return true;
			}
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
							RPCMessage msg3 = rPCMessage;
							SERVER_RequestOpenPanel(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in SERVER_RequestOpenPanel");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.commandBlock != null)
		{
			currentCommand = info.msg.commandBlock.currentCommand;
			lastPlayerID = info.msg.commandBlock.lastPlayerID;
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.forDisk)
		{
			if (info.msg.commandBlock == null)
			{
				info.msg.commandBlock = Facepunch.Pool.Get<ProtoBuf.CommandBlock>();
			}
			info.msg.commandBlock.currentCommand = currentCommand;
			info.msg.commandBlock.lastPlayerID = lastPlayerID;
		}
	}

	public override void IOStateChanged(int inputAmount, int inputSlot)
	{
		base.IOStateChanged(inputAmount, inputSlot);
		if (!commands_enabled)
		{
			currentPower = inputAmount;
			return;
		}
		if (inputAmount == 0 || currentPower > 0)
		{
			currentPower = inputAmount;
			return;
		}
		if (string.IsNullOrEmpty(currentCommand))
		{
			currentPower = inputAmount;
			return;
		}
		if (log_executions)
		{
			Debug.Log("Executing \"" + currentCommand + "\" via CommandBlock");
		}
		ConsoleSystem.Option options = ConsoleSystem.Option.Server;
		options.FromCommandBlock = true;
		if (use_player)
		{
			BasePlayer basePlayer = BasePlayer.FindByID(lastPlayerID);
			if (basePlayer != null && basePlayer.Connection != null)
			{
				options = ConsoleSystem.Option.Server.FromConnection(basePlayer.Connection);
			}
		}
		ConsoleSystem.Run(options, currentCommand);
		currentPower = inputAmount;
	}

	public override int ConsumptionAmount()
	{
		return 0;
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(3uL)]
	[RPC_Server]
	public void SERVER_RequestOpenPanel(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!(player == null) && (player.IsAdmin || player.IsDeveloper))
		{
			if (!commands_enabled)
			{
				player.ShowToast(GameTip.Styles.Error, disabledErrorPhrase, true);
			}
			ClientRPC(RpcTarget.Player("CLIENT_OpenPanel", player), currentCommand);
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(5uL)]
	public void RPC_SetCommand(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!(player == null) && (player.IsAdmin || player.IsDeveloper))
		{
			string text = msg.read.String();
			currentCommand = text;
			lastPlayerID = player.userID.Get();
		}
	}
}
