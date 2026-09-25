using Facepunch;
using Rust.UI;
using UnityEngine;

public class UIMissionRewardsChoice : MonoBehaviour
{
	public GameObject HeaderRoot;

	public GameObject ItemRewardsRoot;

	public GameObject MysteryRewardImageRoot;

	public GameObject CompleteMissionRoot;

	public RustText CompleteMissionText;

	public VirtualItemIcon[] RewardIcons;

	public UIMissionNonItemReward[] NonItemRewards;

	public void SetAllChildElementsActive(bool isActive)
	{
		SetHeaderActive(isActive);
		SetItemRewardsRootActive(isActive);
		SetRewardIconsActive(isActive);
		SetMysteryRewardRootActive(isActive);
		SetCompleteMissionActive(isActive);
	}

	public void SetHeaderActive(bool isActive)
	{
		HeaderRoot.SetActive(isActive);
	}

	public void SetItemRewardsRootActive(bool isActive)
	{
		ItemRewardsRoot.SetActive(isActive);
	}

	public void SetRewardIconsActive(bool isActive)
	{
		VirtualItemIcon[] rewardIcons = RewardIcons;
		for (int i = 0; i < rewardIcons.Length; i++)
		{
			rewardIcons[i].SetActive(isActive);
		}
		UIMissionNonItemReward[] nonItemRewards = NonItemRewards;
		for (int i = 0; i < nonItemRewards.Length; i++)
		{
			nonItemRewards[i].SetActive(isActive);
		}
	}

	public void SetMysteryRewardRootActive(bool isActive)
	{
		MysteryRewardImageRoot.SetActive(isActive);
	}

	public void SetCompleteMissionActive(bool isActive)
	{
		CompleteMissionRoot.SetActive(isActive);
	}

	public void SetCompleteMissionText(bool isMultipleChoice)
	{
		CompleteMissionText.SetPhrase(isMultipleChoice ? NPCConversationPhrases.SelectReward : NPCConversationPhrases.MissionCompleted);
	}
}
