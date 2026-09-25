using UnityEngine;

public class EmissionBlink : FacepunchBehaviour, IClientComponent, INotifyLOD
{
	public EmissionToggle emissionToggle;

	public float blinkDuration = 1f;

	public float blinkInterval = 3f;

	public Light light;

	public float onIntensity = 3f;

	public float offIntensity = 1f;

	[Header("Optional Secondary Light")]
	public bool useSecondaryLight;

	public Light secondaryLight;

	public float secondaryOnIntensity = 2f;

	public float secondaryOffIntensity = 0.5f;
}
