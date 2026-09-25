using Rust.UI;
using UnityEngine;

public class ConfigurePartyBalloonDialog : UIDialog
{
	public RustInput textInput;

	public FlexibleColorPicker balloonColorPicker;

	public FlexibleColorPicker textColorPicker;

	[UnityEvent]
	public void OnClickedConfirm()
	{
	}

	[UnityEvent]
	public void OnTextChanged(string newText)
	{
	}

	[UnityEvent]
	public void OnColourChanged(Color newColour)
	{
	}

	[UnityEvent]
	public void OnTextColourChanged(Color newColour)
	{
	}
}
