using Oxide.Core;
using UnityEngine;

public class MetalDetectorFlag : BaseDiggableEntity
{
	public Collider Collision;

	public GameObject FlagModel;

	public float MoveUpBy = 0.2f;

	[ServerVar]
	public static float TimeoutDuration = 10800f;

	public override void ServerInit()
	{
		base.ServerInit();
		ResetTimeout();
	}

	private void ResetTimeout()
	{
		CancelInvoke(Timeout);
		Invoke(Timeout, TimeoutDuration * UnityEngine.Random.Range(0.8f, 1.2f));
	}

	private void Timeout()
	{
		Kill();
	}

	public override void OnFullyDug(BasePlayer player)
	{
		if (Interface.CallHook("OnPlayerDigComplete", player, this) == null)
		{
			if (Collision != null)
			{
				Collision.enabled = false;
			}
			BaseEntity baseEntity = SpawnLootListItem(player);
			BaseMission.MissionEventPayload missionEventPayload = default(BaseMission.MissionEventPayload);
			missionEventPayload.NetworkIdentifier = ((baseEntity == null) ? baseEntity.net.ID : default(NetworkableId));
			missionEventPayload.UintIdentifier = ((baseEntity == null) ? baseEntity.prefabID : 0u);
			missionEventPayload.WorldPosition = base.transform.position;
			BaseMission.MissionEventPayload payload = missionEventPayload;
			player.ProcessMissionEvent(BaseMission.MissionEventType.METAL_DETECTOR_FIND, payload, 1f);
		}
	}

	public override void OnSingleDig(BasePlayer player)
	{
		base.OnSingleDig(player);
	}

	public override void OnFirstDig(BasePlayer player)
	{
		base.OnFirstDig(player);
	}
}
