using System;
using UnityEngine;

public class IgnoreRotation : MonoBehaviour
{
	[Serializable]
	public enum RotationType
	{
		None,
		X,
		Y,
		Z
	}

	public RotationType ignoreType;

	public Transform parent;

	private void LateUpdate()
	{
		if (ignoreType != 0)
		{
			if (ignoreType == RotationType.X)
			{
				base.transform.localRotation = Quaternion.Euler(0f, parent.localRotation.eulerAngles.y, parent.localRotation.eulerAngles.z);
			}
			else if (ignoreType == RotationType.Y)
			{
				base.transform.localRotation = Quaternion.Euler(parent.localRotation.eulerAngles.x, 0f, parent.localRotation.eulerAngles.z);
			}
			else if (ignoreType == RotationType.Z)
			{
				base.transform.localRotation = Quaternion.Euler(parent.localRotation.eulerAngles.x, parent.localRotation.eulerAngles.y, 0f);
			}
		}
	}
}
