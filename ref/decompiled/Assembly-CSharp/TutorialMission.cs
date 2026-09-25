using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Missions/Tutorial Mission")]
public class TutorialMission : BaseMission
{
	public BasePlayer.TutorialItemAllowance AllowedTutorialItems;

	public override void MissionStart(MissionInstance instance, BasePlayer assignee)
	{
		base.MissionStart(instance, assignee);
		if (AllowedTutorialItems != 0)
		{
			assignee.SetTutorialAllowance(AllowedTutorialItems);
		}
	}
}
