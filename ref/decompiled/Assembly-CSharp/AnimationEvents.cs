using UnityEngine;

public class AnimationEvents : BaseMonoBehaviour
{
	public const string EventFunctionName = "Event";

	public const string ClearEventName = "clear_viewmodel";

	public const string PreclearEventName = "preclear_viewmodel";

	public Transform rootObject;

	public HeldEntity targetEntity;

	[Tooltip("Path to the effect folder for these animations. Relative to this object.")]
	public string effectFolder;

	public bool enforceClipWeights;

	public string localFolder;

	[Tooltip("If true the localFolder field won't update with manifest updates, use for custom paths")]
	public bool customLocalFolder;

	public HeldEntity worldModelEntity;

	public bool IsBusy;

	public string[] ignoreInheritedEffectEvents;

	protected void OnEnable()
	{
		if (rootObject == null)
		{
			rootObject = base.transform;
		}
	}
}
