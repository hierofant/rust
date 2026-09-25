using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Missions/Move Mission")]
public class MoveMission : BaseMission
{
	public float minDistForMovePoint = 20f;

	public float maxDistForMovePoint = 25f;

	private float minDistFromLocation = 3f;

	public override void MissionStart(MissionInstance instance, BasePlayer assignee)
	{
		Vector3 onUnitSphere = Random.onUnitSphere;
		onUnitSphere.y = 0f;
		onUnitSphere.Normalize();
		Vector3 vector = assignee.transform.position + onUnitSphere * Random.Range(minDistForMovePoint, maxDistForMovePoint);
		vector.y = WaterLevel.GetWaterOrTerrainSurface(vector, waves: false, volumes: false);
		instance.objectiveStatuses[0].worldLocation = vector;
		base.MissionStart(instance, assignee);
	}

	public override void ServerThink(MissionInstance instance, BasePlayer assignee, float delta)
	{
		float num = Vector3.Distance(instance.objectiveStatuses[0].worldLocation, assignee.transform.position);
		if (instance.status == MissionStatus.Active && num <= minDistFromLocation)
		{
			MissionSuccess(instance, assignee);
			BaseNetworkable baseNetworkable = BaseNetworkable.serverEntities.Find(instance.providerID);
			if ((bool)baseNetworkable)
			{
				instance.objectiveStatuses[0].worldLocation = baseNetworkable.transform.position;
			}
		}
		else
		{
			if (instance.status == MissionStatus.Accomplished)
			{
				_ = minDistFromLocation;
			}
			base.ServerThink(instance, assignee, delta);
		}
	}
}
