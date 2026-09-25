using UnityEngine;

public class MoveForward : MonoBehaviour
{
	public float Speed = 2f;

	protected void Update()
	{
		GetComponent<Rigidbody>().linearVelocity = Speed * base.transform.forward;
	}
}
