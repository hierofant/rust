using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Rust.UI;

public class UI_RustButtonGroup : MonoBehaviour
{
	[SerializeField]
	[Header("Button Group")]
	protected List<RustButton> _buttons = new List<RustButton>();

	[SerializeField]
	protected bool _unpressSiblings;

	[Tooltip("This button will appear 'pressed' at the beginning.")]
	[SerializeField]
	private RustButton _defaultButton;

	[SerializeField]
	private bool _defaultButtonFiresEvent = true;

	[SerializeField]
	private bool _allowToggleOff;

	public List<RustButton> Buttons => _buttons;

	private void Start()
	{
		SetupButtons();
	}

	public void AddListenerToGroup(UnityAction action)
	{
		if (_buttons.Count <= 0)
		{
			Debug.LogError("No Buttons found in group.");
			return;
		}
		foreach (RustButton button in _buttons)
		{
			if (!(button == null))
			{
				button.OnPressed.AddListener(action);
			}
		}
	}

	public void AddListenerToIndex(int index, UnityAction action)
	{
		if (_buttons.Count <= 0)
		{
			Debug.LogError("No Buttons found in group.");
		}
		else if (_buttons.Count - 1 < index)
		{
			Debug.LogError($"No Buttons found at index {index}.");
		}
		else if (_buttons[index] == null)
		{
			Debug.LogError($"Button at index {index} is null.");
		}
		else
		{
			_buttons[index].OnPressed.AddListener(action);
		}
	}

	public void EnableButton(int index)
	{
		if (_buttons.Count <= index)
		{
			Debug.LogError($"Button with index {index} doesn't exist.");
		}
		else
		{
			_buttons[index].SetToggleTrue();
		}
	}

	public virtual void SetupButtons()
	{
		if (_buttons.Count <= 0)
		{
			Debug.LogError("No Buttons found in group.");
			return;
		}
		foreach (RustButton button in _buttons)
		{
			if (button == null)
			{
				continue;
			}
			if (_defaultButton != null && button == _defaultButton)
			{
				button.PreventToggleOff = false;
				button.SetToggleTrue(_defaultButtonFiresEvent);
			}
			if (!_unpressSiblings)
			{
				continue;
			}
			button.OnToggleEnabled.AddListener(delegate
			{
				if (!_allowToggleOff)
				{
					button.PreventToggleOff = true;
				}
				UnpressSiblings(button);
			});
		}
	}

	public void UnpressSiblings(RustButton thisButton)
	{
		foreach (RustButton button in _buttons)
		{
			if (!(thisButton == button) && !(button == null))
			{
				button.PreventToggleOff = false;
				button.SetToggleFalse();
			}
		}
	}

	public void AddButton(RustButton button)
	{
		_buttons.Add(button);
	}
}
