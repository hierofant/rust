using UnityEngine;

public class ViewmodelLower : MonoBehaviour
{
	public bool lowerOnSprint = true;

	public bool lowerWhenCantAttack = true;

	public bool forceLower;

	public float lowerScale = 1f;

	[UnityEvent]
	public void SetShouldLower(bool shouldLower)
	{
	}
}
