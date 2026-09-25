using UnityEngine;

public class MortarFlightSound : MonoBehaviour, IClientComponent
{
	public SoundDefinition soundDef;

	public float startDelay = 1f;

	public float fadeInTime = 2f;

	public AnimationCurve heightPitchCurve;
}
