using System.Collections.Generic;
using UnityEngine;

public class InstantCameraFlashController : MonoBehaviour
{
	[SerializeField]
	private List<Light> Flash = new List<Light>();

	private void Awake()
	{
		DisableFlash();
	}

	public void EnableFlash()
	{
		foreach (Light item in Flash)
		{
			if (item != null)
			{
				item.enabled = true;
			}
		}
	}

	public void DisableFlash()
	{
		foreach (Light item in Flash)
		{
			if (item != null)
			{
				item.enabled = false;
			}
		}
	}
}
