using UnityEngine;

public class DisableIfNvidiaReflexNotSupported : MonoBehaviour
{
	public GameObject reflexModeOption;

	public GameObject reflexLatencyMarkerOption;

	private void OnEnable()
	{
		reflexModeOption.SetActive(value: false);
		reflexLatencyMarkerOption.SetActive(value: false);
	}
}
