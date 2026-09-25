using UnityEngine;
using UnityEngine.Events;

public class UIEscapeCapture : ListComponent<UIEscapeCapture>
{
	public UnityEvent onEscape = new UnityEvent();

	[Tooltip("If true, pressing escape will call only this callback and not any others.")]
	public bool blockOtherCallbacks = true;

	[Tooltip("Set this to true if you want this EscapeCapture to take priority over any older EscapeCapture when enabled. Surely this should be default?")]
	public bool insertAtTop = true;

	[ClientVar(ClientAdmin = true, Help = "(Generated) When enabled, draws debug visualisations for this system (seismic sensor range sphere, escape capture state, etc.); editor/admin-only")]
	public static bool debug;

	public override void Setup()
	{
		if (!ListComponent<UIEscapeCapture>.InstanceList.Contains(this))
		{
			if (insertAtTop && ListComponent<UIEscapeCapture>.InstanceList.Count > 0)
			{
				ListComponent<UIEscapeCapture>.InstanceList.Insert(0, this);
			}
			else
			{
				ListComponent<UIEscapeCapture>.InstanceList.Add(this);
			}
		}
	}

	[UnityEvent]
	public static bool EscapePressed()
	{
		foreach (UIEscapeCapture instance in ListComponent<UIEscapeCapture>.InstanceList)
		{
			if (debug)
			{
				Debug.Log("Escape key pressed by: " + instance.GetType().Name + " - " + instance.name);
			}
			instance.onEscape.Invoke();
			if (instance.blockOtherCallbacks)
			{
				return true;
			}
		}
		return false;
	}
}
