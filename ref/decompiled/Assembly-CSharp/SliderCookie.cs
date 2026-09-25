using System.Globalization;
using Rust;
using Rust.UI;
using UnityEngine;

public class SliderCookie : MonoBehaviour
{
	public string MaxValueConvarName;

	private ConsoleSystem.Command Command;

	public void OnEnable()
	{
		if (!TryGetComponent<RustSlider>(out var component))
		{
			return;
		}
		if (!string.IsNullOrEmpty(MaxValueConvarName))
		{
			Command = ConsoleSystem.Index.Client.Find(MaxValueConvarName);
			if (Command != null)
			{
				component.SetMaxValue(Command.AsFloat);
			}
		}
		float result;
		float num = (float.TryParse(PlayerPrefs.GetString("SliderCookie_" + base.name), NumberStyles.Float, CultureInfo.InvariantCulture, out result) ? result : component.ValueInternal);
		component.ValueInternal = num + 1f;
		component.Value = num;
		component.OnChanged.AddListener(OnSliderChanged);
	}

	public void OnDisable()
	{
		if (!Rust.Application.isQuitting && TryGetComponent<RustSlider>(out var component))
		{
			component.OnChanged.RemoveListener(OnSliderChanged);
		}
	}

	[UnityEvent]
	private void OnSliderChanged(float v)
	{
		PlayerPrefs.SetString("SliderCookie_" + base.name, v.ToString(CultureInfo.InvariantCulture));
	}
}
