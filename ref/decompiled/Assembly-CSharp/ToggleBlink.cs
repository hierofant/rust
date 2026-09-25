using UnityEngine;
using UnityEngine.Events;

public class ToggleBlink : FacepunchBehaviour, IClientComponent, INotifyLOD
{
	public enum BlinkState
	{
		Off,
		On,
		Blinking
	}

	[SerializeField]
	private UnityEvent onEnabled = new UnityEvent();

	[SerializeField]
	private UnityEvent onDisabled = new UnityEvent();

	public BlinkState initialState;

	public float blinkDuration = 1f;
}
