using System.Collections.Generic;
using UnityEngine;

public class NPCSimpleMissionProvider : NPCTalking, IMissionProvider
{
	public GameObjectRef MarkerPrefab;

	public List<BaseMission> availableMissions = new List<BaseMission>();

	private BufferList<BaseMission> cachedAllMissions;

	public NetworkableId ProviderID()
	{
		return net.ID;
	}

	public Vector3 ProviderPosition()
	{
		return base.transform.position;
	}

	public BufferList<BaseMission> GetAllMissions()
	{
		if (cachedAllMissions == null)
		{
			cachedAllMissions = new BufferList<BaseMission>();
			cachedAllMissions.CopyFrom(availableMissions);
		}
		return cachedAllMissions;
	}

	public string GetNameToken()
	{
		return NPCName.token;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (MarkerPrefab != null && MarkerPrefab.isValid && GetAllMissions().Count > 0)
		{
			MapMarkerMissionProvider obj = GameManager.server.CreateEntity(MarkerPrefab.resourcePath, base.transform.position, base.transform.rotation) as MapMarkerMissionProvider;
			obj.AssignMissions(ProviderID(), GetAllMissions(), NPCName.token);
			obj.Spawn();
		}
		NPCTalking.serverMissionProviders.TryAdd(this);
	}

	public override void Server_OnConversationEnded(BasePlayer player)
	{
		player.ProcessMissionEvent(BaseMission.MissionEventType.CONVERSATION, ProviderID(), 0f);
		base.Server_OnConversationEnded(player);
	}

	public override void Server_OnConversationStarted(BasePlayer speakingTo)
	{
		speakingTo.ProcessMissionEvent(BaseMission.MissionEventType.CONVERSATION, ProviderID(), 1f);
		base.Server_OnConversationStarted(speakingTo);
	}

	protected override void TryAssignMissionToPlayer(BaseMission mission, BasePlayer player)
	{
		base.TryAssignMissionToPlayer(mission, player);
		BaseMission.AssignMission(player, this, mission);
	}

	public override void OnConversationAction(BasePlayer player, string action)
	{
		if (action.StartsWith("assignmission"))
		{
			int num = action.IndexOf(" ");
			BaseMission fromShortName = MissionManifest.GetFromShortName(action.Substring(num + 1));
			if (fromShortName != null && ((IMissionProvider)this).TryGetMission(fromShortName.id, out BaseMission _))
			{
				BaseMission.AssignMission(player, this, fromShortName);
			}
		}
		base.OnConversationAction(player, action);
	}

	public bool Server_HasMissionAvailable(BasePlayer player)
	{
		BufferList<BaseMission> allMissions = GetAllMissions();
		for (int i = 0; i < allMissions.Count; i++)
		{
			if (player.Server_CanAcceptMission(this, allMissions[i]))
			{
				return true;
			}
		}
		return false;
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		NPCTalking.serverMissionProviders.Remove(this);
	}
}
