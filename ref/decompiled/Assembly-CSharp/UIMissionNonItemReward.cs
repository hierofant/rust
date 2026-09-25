using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class UIMissionNonItemReward : MonoBehaviour
{
	public RustText TextField;

	public Image Icon;

	public void Populate(BaseMission.NonItemReward reward)
	{
		TextField.SetPhrase(reward.DisplayPhrase);
		Icon.sprite = reward.DisplaySprite;
	}
}
