using Facepunch;
using UnityEngine;
using UnityEngine.InputSystem;

public class RagdollEditor : SingletonComponent<RagdollEditor>
{
	private Vector3 view;

	private Rigidbody grabbedRigid;

	private Vector3 grabPos;

	private Vector3 grabOffset;

	private void OnGUI()
	{
		GUI.Box(new Rect((float)Screen.width * 0.5f - 2f, (float)Screen.height * 0.5f - 2f, 4f, 4f), "");
	}

	protected override void Awake()
	{
		base.Awake();
	}

	private void Update()
	{
		Camera.main.fieldOfView = 75f;
		if (Mouse.current.rightButton.isPressed)
		{
			Vector2 vector = MouseDelta.Read();
			view.y += vector.x * 3f;
			view.x -= vector.y * 3f;
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
		else
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
		Camera.main.transform.rotation = Quaternion.Euler(view);
		Vector3 zero = Vector3.zero;
		if (Keyboard.current[Key.W].isPressed)
		{
			zero += Vector3.forward;
		}
		if (Keyboard.current[Key.S].isPressed)
		{
			zero += Vector3.back;
		}
		if (Keyboard.current[Key.A].isPressed)
		{
			zero += Vector3.left;
		}
		if (Keyboard.current[Key.D].isPressed)
		{
			zero += Vector3.right;
		}
		Camera.main.transform.position += base.transform.rotation * zero * 0.05f;
		if (Mouse.current.leftButton.wasPressedThisFrame)
		{
			StartGrab();
		}
		if (Mouse.current.leftButton.wasReleasedThisFrame)
		{
			StopGrab();
		}
	}

	private void FixedUpdate()
	{
		if (Mouse.current.leftButton.isPressed)
		{
			UpdateGrab();
		}
	}

	private void StartGrab()
	{
		if (Physics.Raycast(base.transform.position, base.transform.forward, out var hitInfo, 100f))
		{
			grabbedRigid = hitInfo.collider.GetComponent<Rigidbody>();
			if (!(grabbedRigid == null))
			{
				grabPos = grabbedRigid.transform.worldToLocalMatrix.MultiplyPoint(hitInfo.point);
				grabOffset = base.transform.worldToLocalMatrix.MultiplyPoint(hitInfo.point);
			}
		}
	}

	private void UpdateGrab()
	{
		if (!(grabbedRigid == null))
		{
			Vector3 vector = base.transform.TransformPoint(grabOffset);
			Vector3 vector2 = grabbedRigid.transform.TransformPoint(grabPos);
			Vector3 vector3 = vector - vector2;
			grabbedRigid.AddForceAtPosition(vector3 * 100f * grabbedRigid.mass, vector2, ForceMode.Force);
		}
	}

	private void StopGrab()
	{
		grabbedRigid = null;
	}
}
