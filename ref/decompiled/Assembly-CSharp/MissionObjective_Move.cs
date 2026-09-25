using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Rust/Missions/OBJECTIVES/Move")]
public class MissionObjective_Move : MissionObjective
{
	[BaseMission.PositionGenerator.PositionPoint]
	public string positionName = "default";

	[Tooltip("Distance threshold to player for objective to complete.")]
	[InspectorName("Distance For Completion (m)")]
	public float distForCompletion = 3f;

	[Tooltip("If true, this objective will no longer be marked as completed if the objective criteria are no longer met.")]
	public bool canBeReset;

	[InspectorName("Distance For Reset (m)")]
	[Tooltip("If \"Can Be Reset\" is true, then distance ")]
	public float distanceForReset = 3f;

	[Tooltip("If true, disregards distance on the y-plane.")]
	[FormerlySerializedAs("use2D")]
	public bool use2DDistance;

	[Tooltip("If set, player must be mounted on this mountable for objective to complete.")]
	public BaseMountable requiredMountable;

	[Tooltip("If true, displays a UI objective marker for this objective. Only works if at Tutorial Island.")]
	[InspectorName("Should Ping (Tutorial Only)")]
	public bool shouldPing;

	[SerializeField]
	[Tooltip("Ping type for when shouldPing is enabled.")]
	private BasePlayer.PingType pingType = BasePlayer.PingType.GoTo;

	private float sqrDistanceForCompletion;

	private float sqrDistanceForReset;

	public override BasePlayer.PingType PingType => pingType;

	private void OnEnable()
	{
		CacheSqrDistances();
	}

	private void CacheSqrDistances()
	{
		sqrDistanceForCompletion = distForCompletion * distForCompletion;
		sqrDistanceForReset = distanceForReset * distanceForReset;
	}

	public override void ServerObjectiveStarted(BasePlayer playerFor, int index, BaseMission.MissionInstance instance)
	{
		base.ServerObjectiveStarted(playerFor, index, instance);
		instance.TryGetMissionPoint(positionName, out var point);
		SetObjectiveWorldLocation(index, instance, point);
		playerFor.MissionsDirty();
		if (shouldPing)
		{
			TutorialIsland currentTutorialIsland = playerFor.GetCurrentTutorialIsland();
			if (currentTutorialIsland != null)
			{
				playerFor.AddPingAtLocation(pingType, GetObjectiveWorldLocation(index, instance), 86400f, currentTutorialIsland.net.ID);
			}
		}
	}

	protected override void DoServerThink(int index, BaseMission.MissionInstance instance, BasePlayer assignee, float delta)
	{
		if (!CanProgress(index, instance) || (IsCompleted(index, instance) && canBeReset && instance.objectiveStatuses[index].blockReset) || (IsCompleted(index, instance) && !canBeReset) || (requiredMountable != null && (!assignee.isMounted || assignee.GetMounted().prefabID != requiredMountable.prefabID)))
		{
			return;
		}
		instance.TryGetMissionPoint(positionName, out var point);
		float num = (use2DDistance ? (point - assignee.transform.position).SqrMagnitude2D() : Vector3.SqrMagnitude(point - assignee.transform.position));
		bool completed = instance.objectiveStatuses[index].completed;
		bool flag = num <= sqrDistanceForCompletion;
		if (completed == flag)
		{
			return;
		}
		if (flag)
		{
			CompleteObjective(index, instance, assignee);
			if (shouldPing)
			{
				TutorialIsland currentTutorialIsland = assignee.GetCurrentTutorialIsland();
				if (currentTutorialIsland != null)
				{
					assignee.RemovePingAtLocation(pingType, GetObjectiveWorldLocation(index, instance), float.MaxValue, currentTutorialIsland.net.ID);
				}
			}
		}
		else if (canBeReset && num >= sqrDistanceForReset)
		{
			ResetObjective(index, instance, assignee);
		}
	}
}
