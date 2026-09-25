using UnityEngine;

public class UI_LoadingRotate : MonoBehaviour
{
	[SerializeField]
	private Transform RotateImage;

	private bool _keepRotating;

	public void Toggle()
	{
		_keepRotating = !_keepRotating;
	}

	public void ContinuouslyRotate(bool state)
	{
		_keepRotating = state;
	}

	public void Update()
	{
		if (_keepRotating && !(RotateImage == null))
		{
			RotateImage.transform.localEulerAngles = new Vector3(0f, 0f, RotateImage.localEulerAngles.z - Time.deltaTime * 500f);
		}
	}

	public void RotateOnce()
	{
		float z = RotateImage.localEulerAngles.z;
		float to = z + 360f;
		LeanTween.value(RotateImage.gameObject, z, to, 0.5f).setEase(LeanTweenType.linear).setOnUpdate(delegate(float angle, object obj)
		{
			if (obj is Transform transform)
			{
				transform.localEulerAngles = new Vector3(0f, 0f, angle);
			}
		}, RotateImage);
	}

	public void Reset()
	{
		RotateImage.localEulerAngles = Vector3.zero;
	}
}
