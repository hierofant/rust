using Rust.UI;
using UnityEngine;

public class ConfigureOrnateFrameDialog : UIDialog
{
	public RustInput textInput;

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
	public void OnTextColourChanged(Color newColour)
	{
	}
}
