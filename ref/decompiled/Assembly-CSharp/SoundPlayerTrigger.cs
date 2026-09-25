using UnityEngine;

public class SoundPlayerTrigger : TriggerBase, IClientComponent
{
	[Header("Sound")]
	public SoundPlayer soundPlayer;

	public float fadeTime = 0.1f;
}
