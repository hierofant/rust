using Rust.UI;
using UnityEngine;

public class MissionUIPanel : MonoBehaviour
{
	public GameObject activeMissionParent;

	public RustText missionTitleText;

	public RustText missionDescText;

	public GameObject mysteryRewardImageParent;

	public GameObject rewardsParent;

	public GameObject bonusRewardsParent;

	public VirtualItemIcon[] rewardIcons;

	public VirtualItemIcon[] bonusIcons;

	public Translate.Phrase noMissionText;

	public GameObject abandonButton;
}
