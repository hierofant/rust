using System.Collections.Generic;
using Facepunch;
using ProtoBuf;
using UnityEngine;

public class MapMarkerMissionProvider : MapMarker
{
	private NetworkableId missionProviderNetId;

	private BufferList<BaseMission> missions = new BufferList<BaseMission>();

	private string nameToken;

	public void AssignMissions(NetworkableId providerNetId, BufferList<BaseMission> missionsToAssign, string nameToken)
	{
		missionProviderNetId = providerNetId;
		missions.Clear();
		for (int i = 0; i < missionsToAssign.Count; i++)
		{
			missions.Add(missionsToAssign[i]);
		}
		this.nameToken = nameToken;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.missionMapMarker = Pool.Get<ProtoBuf.MissionMapMarker>();
		info.msg.missionMapMarker.missionIds = Pool.Get<List<uint>>();
		foreach (BaseMission mission in missions)
		{
			info.msg.missionMapMarker.missionIds.Add(mission.id);
		}
		info.msg.missionMapMarker.missionProviderNetId = missionProviderNetId;
		info.msg.missionMapMarker.nameToken = nameToken;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.missionMapMarker == null)
		{
			return;
		}
		missions.Clear();
		if (info.msg.missionMapMarker.missionIds != null)
		{
			for (int i = 0; i < info.msg.missionMapMarker.missionIds.Count; i++)
			{
				if (MissionManifest.TryGetFromID(info.msg.missionMapMarker.missionIds[i], out var mission))
				{
					missions.Add(mission);
				}
			}
		}
		missionProviderNetId = info.msg.missionMapMarker.missionProviderNetId;
		if (base.isServer && (!BaseNetworkable.serverEntities.TryGetEntity(missionProviderNetId, out var entity) || !(entity is IMissionProvider)))
		{
			Debug.LogError("Failed to find a mission provider entity from net ID (" + missionProviderNetId.ToString() + ")");
		}
		nameToken = info.msg.missionMapMarker.nameToken;
	}
}
