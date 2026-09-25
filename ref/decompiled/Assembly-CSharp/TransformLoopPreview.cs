using UnityEngine;

[ExecuteAlways]
public class TransformLoopPreview : MonoBehaviour
{
	public enum MovementAxis
	{
		PositiveX,
		NegativeX,
		PositiveY,
		NegativeY,
		PositiveZ,
		NegativeZ
	}

	[Header("Preview")]
	[SerializeField]
	private bool previewMotion = true;

	[Min(0.001f)]
	[SerializeField]
	private float movementDistance = 10f;

	[SerializeField]
	[Min(0f)]
	private float movementSpeed = 5f;

	[Tooltip("Uses the object's rotated local axes instead of world axes.")]
	[SerializeField]
	private bool useLocalAxis = true;

	[SerializeField]
	private MovementAxis movementAxis = MovementAxis.PositiveZ;

	[SerializeField]
	[HideInInspector]
	private Vector3 originPosition;

	[SerializeField]
	[HideInInspector]
	private bool originCaptured;

	[SerializeField]
	[HideInInspector]
	private float travelledDistance;

	private void OnEnable()
	{
		if (!originCaptured)
		{
			CaptureOrigin();
		}
	}

	private void OnDisable()
	{
	}

	private void Update()
	{
		if (Application.isPlaying)
		{
			Move(Time.deltaTime);
		}
	}

	private void Move(float deltaTime)
	{
		if (previewMotion && originCaptured && !(movementSpeed <= 0f) && !(movementDistance <= 0f))
		{
			travelledDistance += movementSpeed * deltaTime;
			if (travelledDistance >= movementDistance)
			{
				travelledDistance = 0f;
			}
			Vector3 movementDirection = GetMovementDirection();
			base.transform.position = originPosition + movementDirection * travelledDistance;
		}
	}

	private Vector3 GetMovementDirection()
	{
		Vector3 direction = movementAxis switch
		{
			MovementAxis.PositiveX => Vector3.right, 
			MovementAxis.NegativeX => Vector3.left, 
			MovementAxis.PositiveY => Vector3.up, 
			MovementAxis.NegativeY => Vector3.down, 
			MovementAxis.NegativeZ => Vector3.back, 
			_ => Vector3.forward, 
		};
		if (useLocalAxis)
		{
			direction = base.transform.TransformDirection(direction);
		}
		return direction.normalized;
	}

	public void CaptureOrigin()
	{
		originPosition = base.transform.position;
		originCaptured = true;
		travelledDistance = 0f;
	}

	public void RestartFromOrigin()
	{
		if (!originCaptured)
		{
			CaptureOrigin();
		}
		travelledDistance = 0f;
		base.transform.position = originPosition;
	}

	public void StopAndReset()
	{
		previewMotion = false;
		RestartFromOrigin();
	}
}
