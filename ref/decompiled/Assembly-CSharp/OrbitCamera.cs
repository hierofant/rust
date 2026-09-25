using UnityEngine;
using UnityEngine.InputSystem;

public class OrbitCamera : MonoBehaviour
{
	[Header("References")]
	public Transform yaw;

	public Transform pitch;

	public Transform cam;

	[Header("Orbit Settings")]
	public float yawSpeed = 150f;

	public float pitchSpeed = 150f;

	[Header("Zoom Settings")]
	public float zoomSpeed = 0.03f;

	public float fastZoomMultiplier = 5f;

	public float minZoom = 0.25f;

	public float maxZoom = 10f;

	[Header("Lock Targets")]
	public Vector3[] lockPositions;

	private int _currentLockIndex;

	private float _currentZoom;

	protected void Start()
	{
		_currentZoom = 0f - cam.localPosition.z;
		base.transform.position = lockPositions[_currentLockIndex];
	}

	protected void Update()
	{
		HandleOrbit();
		HandleZoom();
	}

	private void HandleOrbit()
	{
		Mouse current = Mouse.current;
		if (current != null && current.rightButton.isPressed)
		{
			Vector2 vector = current.delta.ReadValue() * 0.1f;
			yaw.Rotate(0f, vector.x * yawSpeed * Time.deltaTime, 0f);
			float num = pitch.localEulerAngles.x;
			if (num > 180f)
			{
				num -= 360f;
			}
			num -= vector.y * pitchSpeed * Time.deltaTime;
			pitch.localEulerAngles = new Vector3(num, 0f, 0f);
		}
	}

	private void HandleZoom()
	{
		Mouse current = Mouse.current;
		if (current != null)
		{
			float y = current.scroll.ReadValue().y;
			float num = zoomSpeed;
			Keyboard current2 = Keyboard.current;
			if (current2 != null && (current2.leftShiftKey.isPressed || current2.rightShiftKey.isPressed))
			{
				num *= fastZoomMultiplier;
			}
			_currentZoom -= y * num;
			_currentZoom = Mathf.Clamp(_currentZoom, minZoom, maxZoom);
			cam.localPosition = new Vector3(0f, 0f, 0f - _currentZoom);
		}
	}

	private void ToggleLockPosition()
	{
		_currentLockIndex = (_currentLockIndex + 1) % lockPositions.Length;
		base.transform.position = lockPositions[_currentLockIndex];
	}

	public void SetLockPos(int i)
	{
		_currentLockIndex = Mathf.Clamp(i, 0, lockPositions.Length - 1);
		base.transform.position = lockPositions[_currentLockIndex];
	}
}
