using UnityEngine;

public class CompassViewmodel : MonoBehaviour, IViewmodelComponent, IAnimationEventReceiver
{
	[SerializeField]
	private Transform needleBone;
}
