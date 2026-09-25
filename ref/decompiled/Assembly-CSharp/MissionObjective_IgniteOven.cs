using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Rust/Missions/OBJECTIVES/Ignite Oven")]
public class MissionObjective_IgniteOven : MissionObjective
{
	public BaseEntityRef TargetOven;

	public LayerMask targetLayerMask = -1;

	public bool PingTarget;

	[FormerlySerializedAs("PingType")]
	[SerializeField]
	private BasePlayer.PingType pingType = BasePlayer.PingType.GoTo;

	public override BasePlayer.PingType PingType => pingType;

	public override void MissionStarted(int index, BaseMission.MissionInstance instance, BasePlayer forPlayer)
	{
		base.MissionStarted(index, instance, forPlayer);
		if (PingTarget && TryFindNearby<BaseCombatEntity>(forPlayer.transform.position, targetLayerMask, out var entity, 200f))
		{
			SetObjectiveWorldLocation(index, instance, entity.transform.position);
			forPlayer.RegisterPingedEntity(entity, PingType);
		}
	}

	public override bool IsEntityValidForObjective<T>(T entity)
	{
		if (!(entity is BaseCombatEntity baseCombatEntity))
		{
			return false;
		}
		if (!TargetOven.isValid)
		{
			return false;
		}
		BaseEntity baseEntity = TargetOven.Get();
		if (baseEntity == null)
		{
			return false;
		}
		if (!baseCombatEntity.IsAlive())
		{
			return false;
		}
		return entity.prefabID == baseEntity.prefabID;
	}

	public override void ProcessMissionEvent(BasePlayer playerFor, BaseMission.MissionInstance instance, int index, BaseMission.MissionEventType type, BaseMission.MissionEventPayload payload, float amount)
	{
		base.ProcessMissionEvent(playerFor, instance, index, type, payload, amount);
		if (type != BaseMission.MissionEventType.STARTOVEN || IsCompleted(index, instance) || !CanProgress(index, instance))
		{
			return;
		}
		if (TargetOven.resourceID == payload.UintIdentifier)
		{
			CompleteObjective(index, instance, playerFor);
			if (PingTarget)
			{
				playerFor.DeregisterPingedEntity(payload.NetworkIdentifier, PingType);
			}
		}
		playerFor.MissionsDirty(saveImmediately: true);
	}
}
