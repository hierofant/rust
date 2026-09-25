using UnityEngine;
using UnityEngine.Events;

public class SkinViewerEvents : MonoBehaviour
{
	public UnityEvent OnEnteredFullScreenEvent = new UnityEvent();

	public UnityEvent OnExitFullScreenEvent = new UnityEvent();

	[UnityEvent]
	public void OnEnteredFullScreen()
	{
		OnEnteredFullScreenEvent?.Invoke();
	}

	[UnityEvent]
	public void OnExitFullScreen()
	{
		OnExitFullScreenEvent?.Invoke();
	}
}
