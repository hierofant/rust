using UnityEngine;

public class MetalDetectorMissionSource : MetalDetectorSource
{
	[Header("Mission settings")]
	public BaseMission mission;

	public MissionObjective objective;

	public override bool IsValidSource(BasePlayer forPlayer)
	{
		if (forPlayer == null)
		{
			return false;
		}
		if (!forPlayer.TryGetActiveMissionInstance(out var instance))
		{
			return false;
		}
		BaseMission baseMission = instance.GetMission();
		if (baseMission == null)
		{
			return false;
		}
		if (baseMission != mission)
		{
			return false;
		}
		int count = instance.objectiveStatuses.Count;
		int num = baseMission.objectives.Length;
		if (count != num)
		{
			Debug.LogError($"Mission instance for mission {baseMission.name} contains data for {count} objectives but mission has {num} objectives", baseMission);
			return false;
		}
		for (int i = 0; i < instance.objectiveStatuses.Count; i++)
		{
			if (instance.objectiveStatuses[i].IsObjectiveActive() && baseMission.objectives[i].objective == objective)
			{
				return true;
			}
		}
		return false;
	}
}
