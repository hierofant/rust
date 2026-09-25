using UnityEngine;

public class AmmoCounterViewmodelComponent : MonoBehaviour, IViewmodelComponent, IAnimationEventReceiver
{
	public DigitController AmmoCountLeft;

	public DigitController AmmoCountRight;

	public MeshRenderer BurstOn;

	public MeshRenderer LowAmmoOn;

	public Light DisplayLight;

	public LeanTweenType deployFadeInEaseType;

	[Min(0f)]
	public float deployFadeInDuration = 0.5f;

	[Tooltip("If ammo is less than or equal to this, then display the low ammo indicator.")]
	[Min(0f)]
	public int lowAmmoAmount;

	[Tooltip("Recommend to use an \"In\" function here rather than \"In Out\" as the fade-in animation gets reversed upon starting a fade-out (effectively turning the In function into an Out)")]
	public LeanTweenType lowAmmoIndicatorFadeEaseType;

	[Min(0f)]
	[Tooltip("How long a fade takes. Total fade in+out duration will be this *2")]
	public float lowAmmoIndicatorFadeDuration = 0.5f;

	[Min(0f)]
	public float lightOnIntensity = 0.125f;
}
