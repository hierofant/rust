using System;

namespace Rust.UI.MainMenu;

public class UI_SettingsTweakPresetSelect : UI_SettingsTweakDropdown
{
	[Serializable]
	public struct Presets
	{
		public string[] PresetValues;
	}

	public string StreamingAssetsFilename = "";

	public UI_SettingsTweakConvar[] TargetOptions;

	public string[] AdditionalTargetOptions;

	public Presets[] PresetsArray;

	public int CustomIndex;
}
