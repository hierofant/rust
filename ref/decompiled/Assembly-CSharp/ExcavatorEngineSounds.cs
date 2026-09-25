using UnityEngine;

public class ExcavatorEngineSounds : MonoBehaviour, IClientComponent
{
	public SoundPlayer[] engineStartClunks;

	[UnityEvent]
	public void PlayStartClunks()
	{
	}
}
