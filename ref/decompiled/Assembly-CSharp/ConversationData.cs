using System;
using ConVar;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "NewConversation", menuName = "Rust/ConversationData", order = 1)]
public class ConversationData : ScriptableObject
{
	[Serializable]
	public class ResponseNode
	{
		public enum ActionType
		{
			Custom,
			None,
			AssignMission
		}

		public Translate.Phrase responseTextLocalized;

		public ConversationCondition[] conditions = Array.Empty<ConversationCondition>();

		public ActionType actionType;

		public string actionString = string.Empty;

		public BaseMission actionMission;

		public string resultingSpeechNode;

		public bool PassesConditions(BasePlayer player, IConversationProvider provider)
		{
			ConversationCondition[] array = conditions;
			for (int i = 0; i < array.Length; i++)
			{
				if (!array[i].Passes(player, provider))
				{
					return false;
				}
			}
			return true;
		}

		public string GetFailedSpeechNode(BasePlayer player, IConversationProvider provider)
		{
			ConversationCondition[] array = conditions;
			foreach (ConversationCondition conversationCondition in array)
			{
				if (!conversationCondition.Passes(player, provider))
				{
					return conversationCondition.failedSpeechNode;
				}
			}
			return "";
		}

		public string GetActionString()
		{
			switch (actionType)
			{
			case ActionType.None:
				return "";
			case ActionType.Custom:
				return actionString ?? "";
			case ActionType.AssignMission:
				if (!(actionMission != null) || string.IsNullOrWhiteSpace(actionMission.shortname))
				{
					return "";
				}
				return "assignmission " + actionMission.shortname;
			default:
				Debug.LogWarning($"Cannot get conversation action string! Unhandled action type: {actionType}");
				return "";
			}
		}
	}

	[Serializable]
	public abstract class AbstractConversationNodeData : ISerializationCallbackReceiver
	{
		public Vector2 nodePosition;

		[field: SerializeField]
		public string Guid { get; private set; }

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			GenerateNewGuidIfEmpty();
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			GenerateNewGuidIfEmpty();
		}

		public void GenerateNewGuidIfEmpty()
		{
			if (string.IsNullOrWhiteSpace(Guid))
			{
				Guid = System.Guid.NewGuid().ToString();
			}
		}
	}

	[Serializable]
	public abstract class AbstractSpeechNodeData : AbstractConversationNodeData
	{
		public ResponseNode[] responses = Array.Empty<ResponseNode>();

		public abstract string StatementTranslated { get; }
	}

	[Serializable]
	public abstract class AbstractActionEventNodeData : AbstractConversationNodeData
	{
		public string resultingNode;
	}

	[Serializable]
	public class EntryNodeData : AbstractConversationNodeData
	{
		public string resultingNode;
	}

	[Serializable]
	public class SpeechNodeData : AbstractSpeechNodeData
	{
		public string shortname;

		[FormerlySerializedAs("statementLocalized")]
		public Translate.Phrase statement;

		public override string StatementTranslated => statement.translated;
	}

	[Serializable]
	public class MissionListSpeechNodeData : AbstractSpeechNodeData
	{
		public Translate.Phrase statement;

		public MissionListNodeOptionData[] resultingNodeOptions = Array.Empty<MissionListNodeOptionData>();

		[FormerlySerializedAs("resultingNode")]
		public string defaultResultingNode;

		public override string StatementTranslated => statement.translated;
	}

	[Serializable]
	public class MissionListNodeOptionData
	{
		public BaseMission selectedMission;

		public string resultingNode;
	}

	[Serializable]
	public class MissionPreviewSpeechNodeData : AbstractSpeechNodeData
	{
		public override string StatementTranslated => string.Empty;
	}

	[Serializable]
	public class BranchNodeData : AbstractConversationNodeData
	{
		public ConversationCondition condition = new ConversationCondition();

		public string trueResultingNode;

		public string falseResultingNode;

		public string GetConditionalResultingNode(BasePlayer player, IConversationProvider provider)
		{
			if (!condition.Passes(player, provider))
			{
				return falseResultingNode;
			}
			return trueResultingNode;
		}
	}

	[Serializable]
	public class ActionEventOtherPlayerInvokedData : AbstractActionEventNodeData
	{
	}

	[Serializable]
	public class ActionEventPlayerTooPoorNodeData : AbstractActionEventNodeData
	{
	}

	[Serializable]
	public class ActionEventMissionCompletedNodeData : AbstractActionEventNodeData
	{
		public MissionListNodeOptionData[] resultingNodeOptions = Array.Empty<MissionListNodeOptionData>();

		public string GetResultingNodeForMission(uint missionId)
		{
			for (int i = 0; i < resultingNodeOptions.Length; i++)
			{
				if (missionId == resultingNodeOptions[i].selectedMission.id)
				{
					return resultingNodeOptions[i].resultingNode;
				}
			}
			return resultingNode;
		}
	}

	public class DisplayGraphViewButtonAttribute : PropertyAttribute
	{
	}

	[Serializable]
	public class ConversationCondition
	{
		public enum ConditionType
		{
			None = 0,
			HasHealth = 1,
			HasScrap = 2,
			ProviderBusy = 3,
			MissionComplete = 4,
			MissionAttempted = 5,
			PlayerCanAcceptMission = 6,
			ConVar = 7,
			ProviderHasMissionAvailable = 8,
			PlayerHasAnyMissionActive = 9,
			PlayerHasRecentlyCompletedMission = 10,
			PlayerHasPaidFoodToll = 11,
			IsNPCInDeepSea = 12,
			CanUpgradeApartment = 14,
			CanPurchaseApartment = 15,
			CanCheckoutApartment = 16,
			OwnsApartment = 17,
			CanAffordApartment = 18,
			CanAffordUpgrade = 19,
			IsAnyApartmentAvailable = 20,
			CanAffordMasterKey = 21
		}

		public ConditionType conditionType;

		public uint conditionAmount;

		public string conditionString = "";

		public BaseMission conditionMission;

		public bool inverse;

		public string failedSpeechNode;

		public bool Passes(BasePlayer player, IConversationProvider provider)
		{
			bool flag = false;
			switch (conditionType)
			{
			case ConditionType.HasScrap:
				flag = player.inventory.GetAmount(ItemManager.FindItemDefinition("scrap").itemid) >= conditionAmount;
				break;
			case ConditionType.HasHealth:
				flag = player.health >= (float)conditionAmount;
				break;
			case ConditionType.ProviderBusy:
				flag = provider.ProviderBusy();
				break;
			case ConditionType.MissionComplete:
				flag = player.HasCompletedMission(MissionID());
				break;
			case ConditionType.MissionAttempted:
				flag = player.HasAttemptedMission(MissionID());
				break;
			case ConditionType.PlayerCanAcceptMission:
				flag = player.Server_CanAcceptMission(provider as IMissionProvider, MissionID());
				break;
			case ConditionType.ConVar:
				flag = GetNPCConvar(conditionString);
				break;
			case ConditionType.ProviderHasMissionAvailable:
				flag = provider is IMissionProvider missionProvider2 && missionProvider2.Server_HasMissionAvailable(player);
				break;
			case ConditionType.PlayerHasAnyMissionActive:
				flag = player.IsAnyMissionActive();
				break;
			case ConditionType.PlayerHasRecentlyCompletedMission:
				flag = provider is IMissionProvider missionProvider && missionProvider.HasPlayerRecentlyCompletedMission(player);
				break;
			case ConditionType.PlayerHasPaidFoodToll:
			{
				DeepSeaManager deepSeaManager = DeepSeaManager.Get(player.isServer);
				if (deepSeaManager != null)
				{
					flag = deepSeaManager.HasPaidFoodToll(player);
				}
				break;
			}
			case ConditionType.IsNPCInDeepSea:
				flag = PointEntity<DeepSeaManager>.ServerInstance != null && DeepSeaManager.IsInsideDeepSea(provider.GetEntity());
				break;
			case ConditionType.CanUpgradeApartment:
				if (provider is ApartmentVendor apartmentVendor7)
				{
					flag = apartmentVendor7.Conversation_CanUpgradeApartment(player, (int)conditionAmount);
				}
				break;
			case ConditionType.CanPurchaseApartment:
				if (provider is ApartmentVendor apartmentVendor6)
				{
					flag = apartmentVendor6.Conversation_CanPurchaseApartment(player, (int)conditionAmount);
				}
				break;
			case ConditionType.CanCheckoutApartment:
				if (provider is ApartmentVendor apartmentVendor5)
				{
					flag = apartmentVendor5.Conversation_CanCheckoutApartment(player);
				}
				break;
			case ConditionType.OwnsApartment:
				if (provider is ApartmentVendor apartmentVendor4)
				{
					flag = apartmentVendor4.Conversation_OwnsApartment(player, (ApartmentSize)conditionAmount);
				}
				break;
			case ConditionType.CanAffordApartment:
				if (provider is ApartmentVendor apartmentVendor3)
				{
					flag = apartmentVendor3.Conversation_CanAffordApartment(player, (ApartmentSize)conditionAmount);
				}
				break;
			case ConditionType.CanAffordUpgrade:
				if (provider is ApartmentVendor apartmentVendor2)
				{
					flag = apartmentVendor2.Conversation_CanAffordUpgrade(player, (ApartmentVendor.ApartmentConversationUpgrade)conditionAmount);
				}
				break;
			case ConditionType.IsAnyApartmentAvailable:
				if (provider is ApartmentVendor apartmentVendor)
				{
					flag = apartmentVendor.Conversation_HasAnyRoomAvailable();
				}
				break;
			case ConditionType.CanAffordMasterKey:
				if (provider is NPCApartmentSecurity nPCApartmentSecurity)
				{
					flag = nPCApartmentSecurity.Conversation_CanAffordMasterKey(player);
				}
				break;
			}
			if (!inverse)
			{
				return flag;
			}
			return !flag;
		}

		private static bool GetNPCConvar(string convarString)
		{
			return convarString switch
			{
				"vendor_minicopter_enabled" => NPC_ConVars.vendor_minicopter_enabled, 
				"vendor_attack_heli_enabled" => NPC_ConVars.vendor_attack_heli_enabled, 
				"vendor_scrap_heli_enabled" => NPC_ConVars.vendor_scrap_heli_enabled, 
				"vendor_hab_enabled" => NPC_ConVars.vendor_hab_enabled, 
				"vendor_rowboat_enabled" => NPC_ConVars.vendor_rowboat_enabled, 
				"vendor_rhib_enabled" => NPC_ConVars.vendor_rhib_enabled, 
				"vendor_sub_solo_enabled" => NPC_ConVars.vendor_sub_solo_enabled, 
				"vendor_sub_duo_enabled" => NPC_ConVars.vendor_sub_duo_enabled, 
				_ => false, 
			};
		}

		private uint MissionID()
		{
			if (!(conditionMission != null))
			{
				return conditionAmount;
			}
			return conditionMission.id;
		}
	}

	public string shortname;

	public Sprite providerIcon;

	public bool canBeCancelled = true;

	public EntryNodeData entryPoint;

	[SerializeReference]
	public AbstractSpeechNodeData[] speechNodes = Array.Empty<AbstractSpeechNodeData>();

	[SerializeReference]
	public AbstractConversationNodeData[] controlNodes = Array.Empty<AbstractConversationNodeData>();

	public int GetSpeechNodeIndex(string guid)
	{
		for (int i = 0; i < speechNodes.Length; i++)
		{
			if (speechNodes[i].Guid == guid)
			{
				return i;
			}
		}
		return -1;
	}

	public int GetControlNodeIndex(string guid)
	{
		for (int i = 0; i < controlNodes.Length; i++)
		{
			if (controlNodes[i].Guid == guid)
			{
				return i;
			}
		}
		return -1;
	}

	public AbstractSpeechNodeData GetSpeechNodeData(int index)
	{
		if (index < 0)
		{
			return null;
		}
		if (speechNodes != null && index < speechNodes.Length)
		{
			return speechNodes[index];
		}
		return null;
	}

	public AbstractSpeechNodeData GetFirstSpeechNodeFrom(BasePlayer player, IConversationProvider provider, string guid, out int nodeIndex)
	{
		AbstractConversationNodeData nodeData = GetNodeData(guid);
		nodeIndex = -1;
		while (nodeData is BranchNodeData branchNodeData)
		{
			guid = branchNodeData.GetConditionalResultingNode(player, provider);
			nodeData = GetNodeData(guid);
		}
		if (nodeData is AbstractSpeechNodeData abstractSpeechNodeData)
		{
			nodeIndex = GetSpeechNodeIndex(abstractSpeechNodeData.Guid);
			return abstractSpeechNodeData;
		}
		return null;
	}

	public bool TryGetNodeData(string nodeGuid, out AbstractConversationNodeData nodeData)
	{
		nodeData = GetNodeData(nodeGuid);
		return nodeData != null;
	}

	public AbstractConversationNodeData GetNodeData(string nodeGuid)
	{
		if (string.IsNullOrWhiteSpace(nodeGuid))
		{
			return null;
		}
		if (entryPoint.Guid == nodeGuid)
		{
			return entryPoint;
		}
		AbstractSpeechNodeData[] array = speechNodes;
		foreach (AbstractSpeechNodeData abstractSpeechNodeData in array)
		{
			if (abstractSpeechNodeData.Guid == nodeGuid)
			{
				return abstractSpeechNodeData;
			}
		}
		AbstractConversationNodeData[] array2 = controlNodes;
		foreach (AbstractConversationNodeData abstractConversationNodeData in array2)
		{
			if (abstractConversationNodeData.Guid == nodeGuid)
			{
				return abstractConversationNodeData;
			}
		}
		return null;
	}

	public void FindAllMissionAssignments(BufferList<BaseMission> results)
	{
		AbstractSpeechNodeData[] array = speechNodes;
		foreach (AbstractSpeechNodeData abstractSpeechNodeData in array)
		{
			if (abstractSpeechNodeData is MissionListSpeechNodeData)
			{
				continue;
			}
			ResponseNode[] array2 = null;
			if (abstractSpeechNodeData is SpeechNodeData speechNodeData)
			{
				array2 = speechNodeData.responses;
			}
			else if (abstractSpeechNodeData is MissionPreviewSpeechNodeData missionPreviewSpeechNodeData)
			{
				array2 = missionPreviewSpeechNodeData.responses;
			}
			ResponseNode[] array3 = array2;
			foreach (ResponseNode responseNode in array3)
			{
				if (responseNode.actionType == ResponseNode.ActionType.AssignMission && responseNode.actionMission != null)
				{
					results.Add(responseNode.actionMission);
				}
			}
		}
	}
}
