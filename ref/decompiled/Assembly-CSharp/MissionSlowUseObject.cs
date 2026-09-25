#define UNITY_ASSERTIONS
using System;
using ConVar;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class MissionSlowUseObject : BaseEntity
{
	public float InteractTime = 5f;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("MissionSlowUseObject.OnRpcMessage"))
		{
			if (rpc == 2005407348 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - ServerUse");
				}
				using (TimeWarning.New("ServerUse"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(2005407348u, "ServerUse", this, player, 3f))
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
							ServerUse(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in ServerUse");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool CanPlayerUse(BasePlayer player)
	{
		if (player.TryGetActiveMissionInstance(out var instance))
		{
			BaseMission.MissionObjectiveEntry[] objectives = instance.GetMission().objectives;
			for (int i = 0; i < objectives.Length; i++)
			{
				MissionObjective objective = objectives[i].objective;
				if (objective is MissionObjective_ActivateLongUseObject missionObjective_ActivateLongUseObject && !objective.IsCompleted(i, instance) && objective.CanProgress(i, instance) && missionObjective_ActivateLongUseObject.RequiredEntity.prefabID == prefabID)
				{
					return true;
				}
			}
		}
		return false;
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void ServerUse(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (CanPlayerUse(player))
		{
			player.ProcessMissionEvent(BaseMission.MissionEventType.LONG_USE_OBJECT, net.ID, 1f);
		}
	}
}
