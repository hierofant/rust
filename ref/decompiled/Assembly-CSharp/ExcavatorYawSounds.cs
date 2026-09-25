using UnityEngine;

public class ExcavatorYawSounds : MonoBehaviour, IClientComponent
{
	public SoundPlayer[] miningStartClunks;

	[UnityEvent]
	public void PlayStartClunks()
	{
	}
}
