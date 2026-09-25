using UnityEngine.Events;

namespace Rust.UI.MainMenu;

public class UI_SettingsTweakSlider : UI_SettingsTweakConvar
{
	public bool manuallyCallInit;

	public RustSlider rustSliderControl;

	public RustInput rustInput;

	public bool applyOnMouseUp;

	private TimeSince mouseDown;

	private bool isRefreshingFromConvar;

	public UnityEvent OnMouseUp;

	private float GetSliderValue()
	{
		return rustSliderControl.Value;
	}
}
