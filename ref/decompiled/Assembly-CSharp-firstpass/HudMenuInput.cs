using Rust.UI;
using TMPro;
using UnityEngine.UI;

public class HudMenuInput : ListComponent<HudMenuInput>
{
	private InputField inputField;

	private RustInput rustInput;

	private TMP_InputField tmpInputField;

	public static bool AnyActive()
	{
		for (int i = 0; i < ListComponent<HudMenuInput>.InstanceList.Count; i++)
		{
			if (ListComponent<HudMenuInput>.InstanceList[i].IsCurrentlyActive())
			{
				return true;
			}
		}
		return false;
	}

	private void Start()
	{
		inputField = GetComponent<InputField>();
		rustInput = GetComponent<RustInput>();
		tmpInputField = GetComponent<TMP_InputField>();
	}

	private bool IsCurrentlyActive()
	{
		if (!base.enabled)
		{
			return false;
		}
		if (tmpInputField != null)
		{
			return tmpInputField.isFocused;
		}
		if (rustInput != null)
		{
			return rustInput.IsFocused;
		}
		if (inputField == null)
		{
			return false;
		}
		return inputField.isFocused;
	}
}
