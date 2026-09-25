using UnityEngine.Events;

namespace Rust.UI.MainMenu;

public class UI_SettingsTweakConvar : UI_SettingsTweakBase
{
	public string convarName;

	public bool ApplyImmediatelyOnChange = true;

	public UnityEvent onValueChanged = new UnityEvent();
}
