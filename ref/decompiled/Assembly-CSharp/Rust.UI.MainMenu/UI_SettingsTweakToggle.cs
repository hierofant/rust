namespace Rust.UI.MainMenu;

public class UI_SettingsTweakToggle : UI_SettingsTweakConvar
{
	public RustButton buttonControl;

	public bool inverse;

	public static string lastConVarChanged;

	public static TimeSince timeSinceLastConVarChange;
}
