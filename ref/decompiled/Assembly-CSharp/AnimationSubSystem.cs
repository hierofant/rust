using System;
using UnityEngine;

public abstract class AnimationSubSystem : MonoBehaviour, IClientComponent
{
	public enum PriorityLevels
	{
		Low,
		Medium,
		High
	}

	public class SubSystemVariable : Attribute
	{
	}

	public const int TotalPriorityLevels = 3;

	[Tooltip("Used for debugging, should describe what this system does")]
	[SerializeField]
	[SubSystemVariable]
	private string SubSystemName = string.Empty;

	[SubSystemVariable]
	[SerializeField]
	[Tooltip("Controls what part of the body this system modifies, if none full body mask will be used")]
	protected AvatarMask Mask;

	[SerializeField]
	protected float FadeInTime = 0.25f;

	[SerializeField]
	protected float FadeOutTime = 0.25f;

	[SerializeField]
	protected bool DisableDuringGestures;

	[SerializeField]
	protected bool DisableSpineIK;

	[SerializeField]
	[SubSystemVariable]
	private PriorityLevels Priority = PriorityLevels.Medium;
}
