using Rust;
using Rust.UI;
using UnityEngine;

public class ToggleCookie : MonoBehaviour
{
	public void OnEnable()
	{
		if (TryGetComponent<RustButton>(out var component))
		{
			bool result;
			bool value = (bool.TryParse(PlayerPrefs.GetString("ToggleCookie_" + base.name), out result) ? result : component.Value);
			component.Toggle(value, forced: true);
			component.OnPressed.AddListener(OnPressed);
			component.OnReleased.AddListener(OnReleased);
		}
	}

	public void OnDisable()
	{
		if (!Rust.Application.isQuitting && TryGetComponent<RustButton>(out var component))
		{
			component.OnPressed.RemoveListener(OnPressed);
			component.OnReleased.RemoveListener(OnReleased);
		}
	}

	[UnityEvent]
	private void OnPressed()
	{
		OnChanged(v: true);
	}

	[UnityEvent]
	private void OnReleased()
	{
		OnChanged(v: false);
	}

	private void OnChanged(bool v)
	{
		PlayerPrefs.SetString("ToggleCookie_" + base.name, v.ToString());
	}
}
