using System;
using Rust.UI;
using UnityEngine;

public class UIDialogueChoice : MonoBehaviour
{
	public RustText DialogueText;

	public GameObject MissionIcon;

	[NonSerialized]
	public BaseMission DisplayingMission;

	[NonSerialized]
	public int SpeechResponseIndex;

	public void SetMissionIconActive(bool isActive)
	{
		MissionIcon.SetActive(isActive);
	}

	public void SetDialoguePhrase(Translate.Phrase phrase)
	{
		DialogueText.SetPhrase(phrase);
	}

	private void OnDisable()
	{
		DisplayingMission = null;
	}
}
