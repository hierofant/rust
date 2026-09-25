using UnityEngine;

public class ChildAnimatorSubSystem : AnimationSubSystem
{
	[SerializeField]
	protected RuntimeAnimatorController ChildController;

	[SerializeField]
	[Range(0f, 1f)]
	protected float Weight;
}
