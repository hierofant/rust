using UnityEngine;

public class DisableIfDLSSNotSupported : MonoBehaviour
{
	private void OnEnable()
	{
		base.gameObject.SetActive(value: false);
	}
}
