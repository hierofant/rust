#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class NPCTalking : NPCShopKeeper, IConversationProvider
{
	[Serializable]
	public class NPCConversationResultAction
	{
		public string action;

		public int scrapCost;

		public string broadcastMessage;

		public float broadcastRange;
	}

	public Translate.Phrase NPCName = new Translate.Phrase();

	[ConversationData.DisplayGraphViewButton]
	public ConversationData[] conversations = Array.Empty<ConversationData>();

	public static ListHashSet<IMissionProvider> serverMissionProviders = new ListHashSet<IMissionProvider>();

	public NPCConversationResultAction[] conversationResultActions;

	[NonSerialized]
	public float maxConversationDistance = 5f;

	public List<BasePlayer> conversingPlayers = new List<BasePlayer>();

	public BasePlayer lastActionPlayer;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("NPCTalking.OnRpcMessage"))
		{
			if (rpc == 2112414875 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - Server_BeginTalking");
				}
				using (TimeWarning.New("Server_BeginTalking"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2112414875u, "Server_BeginTalking", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(2112414875u, "Server_BeginTalking", this, player, 3f))
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
							Server_BeginTalking(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in Server_BeginTalking");
					}
				}
				return true;
			}
			if (rpc == 1597539152 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - Server_EndTalking");
				}
				using (TimeWarning.New("Server_EndTalking"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1597539152u, "Server_EndTalking", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(1597539152u, "Server_EndTalking", this, player, 3f))
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
							Server_EndTalking(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in Server_EndTalking");
					}
				}
				return true;
			}
			if (rpc == 2713250658u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - Server_ResponsePressed");
				}
				using (TimeWarning.New("Server_ResponsePressed"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2713250658u, "Server_ResponsePressed", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(2713250658u, "Server_ResponsePressed", this, player, 3f))
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
							Server_ResponsePressed(msg4);
						}
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
						player.Kick("RPC Error in Server_ResponsePressed");
					}
				}
				return true;
			}
			if (rpc == 458746537 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - Server_RewardChoiceSelected");
				}
				using (TimeWarning.New("Server_RewardChoiceSelected"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(458746537u, "Server_RewardChoiceSelected", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(458746537u, "Server_RewardChoiceSelected", this, player, 3f))
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
							RPCMessage msg5 = rPCMessage;
							Server_RewardChoiceSelected(msg5);
						}
					}
					catch (Exception exception4)
					{
						Debug.LogException(exception4);
						player.Kick("RPC Error in Server_RewardChoiceSelected");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public int GetConversationIndex(string conversationName)
	{
		for (int i = 0; i < conversations.Length; i++)
		{
			if (conversations[i].shortname == conversationName)
			{
				return i;
			}
		}
		return -1;
	}

	public Translate.Phrase GetNPCName()
	{
		return NPCName;
	}

	public bool ProviderBusy()
	{
		return HasFlag(Flags.Reserved1);
	}

	internal override void DoServerDestroy()
	{
		for (int num = conversingPlayers.Count - 1; num >= 0; num--)
		{
			Server_OnConversationEnded(conversingPlayers[num]);
		}
		base.DoServerDestroy();
	}

	public virtual ConversationData GetConversationFor(BasePlayer player)
	{
		return conversations[0];
	}

	public void ForceEndConversation(BasePlayer player)
	{
		ClientRPC(RpcTarget.Player("Client_EndConversation", player));
		Server_OnConversationEnded(player);
	}

	public void ForceSpeechNode(BasePlayer player, ConversationData data, int speechNodeIndex)
	{
		if (player == null)
		{
			return;
		}
		ConversationData.AbstractSpeechNodeData speechNodeData = data.GetSpeechNodeData(speechNodeIndex);
		using ConversationResponseStatesList conversationResponseStatesList = Facepunch.Pool.Get<ConversationResponseStatesList>();
		conversationResponseStatesList.list = Facepunch.Pool.Get<List<bool>>();
		if (speechNodeData != null)
		{
			for (int i = 0; i < speechNodeData.responses.Length; i++)
			{
				conversationResponseStatesList.list.Add(speechNodeData.responses[i].PassesConditions(player, this));
			}
		}
		ClientRPC(RpcTarget.Player("Client_ForceSpeechNode", player), speechNodeIndex, conversationResponseStatesList);
		Facepunch.Pool.FreeUnmanaged(ref conversationResponseStatesList.list);
	}

	public virtual void Server_OnConversationEnded(BasePlayer player)
	{
		Interface.CallHook("OnNpcConversationEnded", this, player);
		if (player != null && conversingPlayers.Contains(player))
		{
			player.ClearActiveTalkingToNpc(this);
			conversingPlayers.Remove(player);
		}
	}

	public void CleanupConversingPlayers()
	{
		for (int num = conversingPlayers.Count - 1; num >= 0; num--)
		{
			BasePlayer basePlayer = conversingPlayers[num];
			if (basePlayer == null)
			{
				conversingPlayers.RemoveAt(num);
			}
			else if (!basePlayer.IsAlive() || basePlayer.IsSleeping() || !basePlayer.IsConnected)
			{
				Server_OnConversationEnded(basePlayer);
			}
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	[RPC_Server.CallsPerSecond(1uL)]
	public void Server_BeginTalking(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		Server_BeginTalking(player);
	}

	protected virtual bool CanTalkTo(BasePlayer bp)
	{
		if (bp == null)
		{
			return false;
		}
		if (!bp.IsOnGround() && !bp.IsFlying)
		{
			return false;
		}
		return true;
	}

	public void Server_BeginTalking(BasePlayer ply)
	{
		if (!CanTalkTo(ply))
		{
			return;
		}
		CleanupConversingPlayers();
		BaseMission.MissionStatus num = ply.GetActiveMissionInstance()?.status ?? BaseMission.MissionStatus.Undefined;
		uint num2 = ply.GetActiveMissionInstance()?.missionID ?? 0;
		Server_OnConversationStarted(ply);
		bool flag = num == BaseMission.MissionStatus.Active && ply.HasCompletedMission(num2);
		ConversationData conversationFor = GetConversationFor(ply);
		if (conversationFor != null)
		{
			if (conversingPlayers.Contains(ply))
			{
				Server_OnConversationEnded(ply);
			}
			if (Interface.CallHook("OnNpcConversationStart", this, ply, conversationFor) != null)
			{
				return;
			}
			conversingPlayers.Add(ply);
			ply.SetActiveTalkingToNpc(this);
			UpdateFlags();
			using ConversationResponseStatesList conversationResponseStatesList = Facepunch.Pool.Get<ConversationResponseStatesList>();
			string text = conversationFor.entryPoint.resultingNode;
			if (flag && !ply.IsInTutorial)
			{
				text = string.Empty;
				ConversationData.AbstractConversationNodeData[] controlNodes = conversationFor.controlNodes;
				for (int i = 0; i < controlNodes.Length; i++)
				{
					if (controlNodes[i] is ConversationData.ActionEventMissionCompletedNodeData actionEventMissionCompletedNodeData)
					{
						text = actionEventMissionCompletedNodeData.GetResultingNodeForMission(num2);
						break;
					}
				}
			}
			if (string.IsNullOrWhiteSpace(text) && (ply.IsInTutorial || !flag))
			{
				Debug.LogError("Failed to start conversation " + conversationFor.name + ", entry node has no output", conversationFor);
				Server_OnConversationEnded(ply);
				return;
			}
			int nodeIndex;
			ConversationData.AbstractSpeechNodeData firstSpeechNodeFrom = conversationFor.GetFirstSpeechNodeFrom(ply, this, text, out nodeIndex);
			conversationResponseStatesList.list = Facepunch.Pool.Get<List<bool>>();
			if (ply.IsInTutorial || !flag)
			{
				if (nodeIndex < 0)
				{
					Debug.LogError("Failed to find a valid speech node starting from GUID: " + text + ", player: " + ply.name + ", provider: " + base.name, this);
				}
				else
				{
					for (int j = 0; j < firstSpeechNodeFrom.responses.Length; j++)
					{
						conversationResponseStatesList.list.Add(firstSpeechNodeFrom.responses[j].PassesConditions(ply, this));
					}
				}
			}
			if (this is IMissionProvider missionProvider && !flag)
			{
				ply.Server_SendCanAcceptMissionsFromProvider(missionProvider);
			}
			ClientRPC(RpcTarget.Player("Client_StartConversation", ply), GetConversationIndex(conversationFor.shortname), nodeIndex, flag, conversationResponseStatesList);
			return;
		}
		Server_OnConversationEnded(ply);
	}

	public virtual void Server_OnConversationStarted(BasePlayer speakingTo)
	{
	}

	public virtual void UpdateFlags()
	{
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	[RPC_Server.CallsPerSecond(1uL)]
	public void Server_EndTalking(RPCMessage msg)
	{
		Server_OnConversationEnded(msg.player);
	}

	public bool ValidConversationPlayer(BasePlayer player)
	{
		if (Vector3.Distance(player.transform.position, base.transform.position) > maxConversationDistance)
		{
			return false;
		}
		if (conversingPlayers.Contains(player))
		{
			return false;
		}
		return true;
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server.MaxDistance(3f)]
	public void Server_ResponsePressed(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		int num = msg.read.Int32();
		int num2 = msg.read.Int32();
		uint missionId = msg.read.UInt32();
		uint missionId2 = msg.read.UInt32();
		ConversationData conversationFor = GetConversationFor(player);
		if (conversationFor == null)
		{
			return;
		}
		ConversationData.AbstractSpeechNodeData abstractSpeechNodeData = conversationFor.speechNodes[num];
		string guid = string.Empty;
		IMissionProvider missionProvider = this as IMissionProvider;
		ConversationData.ResponseNode responseNode = default(ConversationData.ResponseNode);
		if (abstractSpeechNodeData is ConversationData.MissionListSpeechNodeData missionListSpeechNodeData)
		{
			if (!(this is NPCSimpleMissionProvider) || missionProvider == null)
			{
				Debug.LogError(string.Format("Response {0} pressed on mission list speech node on {1} but is not a {2}", num2, base.name, "NPCSimpleMissionProvider"));
				return;
			}
			if (missionProvider.TryGetMission(missionId2, out var mission))
			{
				bool flag = false;
				ConversationData.MissionListNodeOptionData[] resultingNodeOptions = missionListSpeechNodeData.resultingNodeOptions;
				foreach (ConversationData.MissionListNodeOptionData missionListNodeOptionData in resultingNodeOptions)
				{
					if (!(missionListNodeOptionData.selectedMission != mission))
					{
						guid = missionListNodeOptionData.resultingNode;
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					guid = missionListSpeechNodeData.defaultResultingNode;
				}
			}
		}
		else
		{
			if (abstractSpeechNodeData is ConversationData.MissionPreviewSpeechNodeData && this is NPCSimpleMissionProvider && num2 == 0 && missionProvider != null && missionProvider.TryGetMission(missionId, out var mission2) && player.Server_CanAcceptMission(missionProvider, mission2))
			{
				TryAssignMissionToPlayer(mission2, player);
			}
			responseNode = abstractSpeechNodeData.responses[num2];
			if (responseNode != null)
			{
				if (Interface.CallHook("OnNpcConversationRespond", this, player, conversationFor, responseNode) != null)
				{
					return;
				}
				if (responseNode.conditions.Length != 0)
				{
					UpdateFlags();
				}
				bool num3 = responseNode.PassesConditions(player, this);
				if (num3)
				{
					string actionString = responseNode.GetActionString();
					if (!string.IsNullOrEmpty(actionString))
					{
						OnConversationAction(player, actionString);
					}
				}
				guid = (num3 ? responseNode.resultingSpeechNode : responseNode.GetFailedSpeechNode(player, this));
			}
		}
		conversationFor.GetFirstSpeechNodeFrom(player, this, guid, out var nodeIndex);
		if (nodeIndex == -1)
		{
			ForceEndConversation(player);
			return;
		}
		ForceSpeechNode(player, conversationFor, nodeIndex);
		Interface.CallHook("OnNpcConversationResponded", this, player, conversationFor, responseNode);
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	[RPC_Server.CallsPerSecond(1uL)]
	public void Server_RewardChoiceSelected(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		uint num = msg.read.UInt32();
		int choice = msg.read.Int32();
		if (!MissionManifest.TryGetFromID(num, out var mission))
		{
			Debug.LogError($"{base.name} failed to retrieve a mission from ID {num}", this);
			return;
		}
		BaseMission.MissionInstance missionInstance = null;
		for (int i = 0; i < player.acceptedMissions.Count; i++)
		{
			BaseMission.MissionInstance missionInstance2 = player.acceptedMissions[i];
			if (missionInstance2 != null && missionInstance2.missionID == num)
			{
				BaseMission.MissionStatus status = missionInstance2.status;
				if ((status == BaseMission.MissionStatus.Accomplished || status == BaseMission.MissionStatus.Completed) && !missionInstance2.hasDispensedRewards)
				{
					missionInstance = missionInstance2;
					break;
				}
			}
		}
		if (missionInstance != null)
		{
			mission.DispenseRewards(missionInstance, player, choice);
		}
	}

	public BasePlayer GetActionPlayer()
	{
		return lastActionPlayer;
	}

	public virtual void OnConversationAction(BasePlayer player, string action)
	{
		if (action == "openvending")
		{
			InvisibleVendingMachine vendingMachine = GetVendingMachine();
			if (vendingMachine != null && Vector3.Distance(player.transform.position, base.transform.position) < 5f)
			{
				ForceEndConversation(player);
				if (Interface.CallHook("OnVendingShopOpen", vendingMachine, player) == null)
				{
					vendingMachine.PlayerOpenLoot(player, "vendingmachine.customer", doPositionChecks: false);
					Interface.CallHook("OnVendingShopOpened", vendingMachine, player);
				}
				return;
			}
		}
		ItemDefinition itemDefinition = ItemManager.FindItemDefinition("scrap");
		NPCConversationResultAction[] array = conversationResultActions;
		foreach (NPCConversationResultAction nPCConversationResultAction in array)
		{
			if (!(nPCConversationResultAction.action == action))
			{
				continue;
			}
			CleanupConversingPlayers();
			foreach (BasePlayer conversingPlayer in conversingPlayers)
			{
				if (conversingPlayer == player || conversingPlayer == null)
				{
					continue;
				}
				int speechNodeIndex = -1;
				ConversationData conversationFor = GetConversationFor(player);
				ConversationData.AbstractConversationNodeData[] controlNodes = conversationFor.controlNodes;
				for (int j = 0; j < controlNodes.Length; j++)
				{
					if (controlNodes[j] is ConversationData.ActionEventOtherPlayerInvokedData actionEventOtherPlayerInvokedData)
					{
						speechNodeIndex = conversationFor.GetSpeechNodeIndex(actionEventOtherPlayerInvokedData.resultingNode);
						break;
					}
				}
				ForceSpeechNode(conversingPlayer, conversationFor, speechNodeIndex);
			}
			int num = nPCConversationResultAction.scrapCost;
			using PooledList<Item> pooledList = Facepunch.Pool.Get<PooledList<Item>>();
			player.inventory.FindItemsByItemID(pooledList, itemDefinition.itemid);
			foreach (Item item in pooledList)
			{
				num -= item.amount;
			}
			if (num > 0)
			{
				int speechNodeIndex2 = -1;
				ConversationData conversationFor2 = GetConversationFor(player);
				ConversationData.AbstractConversationNodeData[] controlNodes = conversationFor2.controlNodes;
				for (int j = 0; j < controlNodes.Length; j++)
				{
					if (controlNodes[j] is ConversationData.ActionEventPlayerTooPoorNodeData actionEventPlayerTooPoorNodeData)
					{
						speechNodeIndex2 = conversationFor2.GetSpeechNodeIndex(actionEventPlayerTooPoorNodeData.resultingNode);
						break;
					}
				}
				ForceSpeechNode(player, conversationFor2, speechNodeIndex2);
				break;
			}
			Facepunch.Rust.Analytics.Azure.OnNPCVendor(player, this, nPCConversationResultAction.scrapCost, nPCConversationResultAction.action);
			num = nPCConversationResultAction.scrapCost;
			foreach (Item item2 in pooledList)
			{
				int num2 = Mathf.Min(num, item2.amount);
				item2.UseItem(num2);
				num -= num2;
				if (num <= 0)
				{
					break;
				}
			}
			lastActionPlayer = player;
			BroadcastEntityMessage(nPCConversationResultAction.broadcastMessage, nPCConversationResultAction.broadcastRange);
			lastActionPlayer = null;
			break;
		}
	}

	protected virtual void TryAssignMissionToPlayer(BaseMission mission, BasePlayer player)
	{
	}
}
