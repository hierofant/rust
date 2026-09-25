using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Facepunch;

public static class KeyCombos
{
	public struct ComboPart
	{
		public Key Key;

		public int MouseButton;

		public bool MouseWheelUp;

		public bool MouseWheelDown;

		public bool IsKeyboardKey => Key != Key.None;

		public bool IsMouseButton => MouseButton >= 0;

		public bool IsFunctionKey
		{
			get
			{
				if (Key >= Key.F1)
				{
					return Key <= Key.F12;
				}
				return false;
			}
		}
	}

	public static bool TryParse(ref string name, out List<ComboPart> parts)
	{
		if (string.IsNullOrWhiteSpace(name) || name.Length < 5 || !name.StartsWith("[") || !name.EndsWith("]") || !name.Contains("+"))
		{
			parts = null;
			return false;
		}
		string[] array = name.Substring(1, name.Length - 2).ToLowerInvariant().Split('+');
		List<ComboPart> list = new List<ComboPart>(array.Length);
		List<string> list2 = new List<string>(array.Length);
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (text == "mousewheelup")
			{
				list.Add(new ComboPart
				{
					Key = Key.None,
					MouseButton = -1,
					MouseWheelUp = true
				});
				list2.Add(text);
				continue;
			}
			if (text == "mousewheeldown")
			{
				list.Add(new ComboPart
				{
					Key = Key.None,
					MouseButton = -1,
					MouseWheelDown = true
				});
				list2.Add(text);
				continue;
			}
			if (TryParseMouseButton(text, out var index))
			{
				list.Add(new ComboPart
				{
					Key = Key.None,
					MouseButton = index
				});
				list2.Add("mouse" + index);
				continue;
			}
			if (!UnityButtons.TryParseKeyboardKey(text, out var key))
			{
				parts = null;
				return false;
			}
			list.Add(new ComboPart
			{
				Key = key,
				MouseButton = -1
			});
			list2.Add(UnityButtons.FormatKeyName(key).ToLowerInvariant());
		}
		name = "[" + string.Join("+", list2) + "]";
		parts = list;
		return true;
	}

	public static void RegisterButton(string name, List<ComboPart> parts)
	{
		if (string.IsNullOrWhiteSpace(name) || parts == null)
		{
			return;
		}
		bool flag = parts.Any((ComboPart p) => p.MouseWheelUp);
		bool flag2 = parts.Any((ComboPart p) => p.MouseWheelDown);
		if ((parts.Count <= 1 && !flag && !flag2) || Input.HasButton(name))
		{
			return;
		}
		ComboPart[] localParts = parts.ToArray();
		Input.AddButton(name, delegate
		{
			Keyboard current = Keyboard.current;
			Mouse current2 = Mouse.current;
			ComboPart[] array = localParts;
			for (int i = 0; i < array.Length; i++)
			{
				ComboPart comboPart = array[i];
				if (comboPart.MouseWheelUp)
				{
					if (current2 == null || current2.scroll.ReadValue().y <= 0f)
					{
						return false;
					}
				}
				else if (comboPart.MouseWheelDown)
				{
					if (current2 == null || current2.scroll.ReadValue().y >= 0f)
					{
						return false;
					}
				}
				else if (comboPart.IsMouseButton)
				{
					ButtonControl mouseButton = GetMouseButton(current2, comboPart.MouseButton);
					if (mouseButton == null || !mouseButton.isPressed)
					{
						return false;
					}
					if (NeedsMouseButtons.AnyActive())
					{
						return false;
					}
				}
				else
				{
					if (!comboPart.IsKeyboardKey)
					{
						return false;
					}
					if (current == null || !current[comboPart.Key].isPressed)
					{
						return false;
					}
					if (!comboPart.IsFunctionKey && !KeyBinding.IsOpen && (NeedsKeyboard.AnyActive() || HudMenuInput.AnyActive()))
					{
						return false;
					}
				}
			}
			return true;
		});
	}

	public static string FormatHeldKeys(IReadOnlyList<string> heldKeys)
	{
		if (heldKeys == null || heldKeys.Count == 0)
		{
			return null;
		}
		if (heldKeys.Count > 1)
		{
			IEnumerable<string> values = heldKeys.Select((string k) => k.ToLowerInvariant());
			return "[" + string.Join("+", values) + "]";
		}
		return heldKeys[0];
	}

	private static bool TryParseMouseButton(string token, out int index)
	{
		switch (token)
		{
		case "mouse0":
			index = 0;
			return true;
		case "mouse1":
			index = 1;
			return true;
		case "mouse2":
			index = 2;
			return true;
		case "mouse3":
			index = 3;
			return true;
		case "mouse4":
			index = 4;
			return true;
		default:
			index = -1;
			return false;
		}
	}

	private static ButtonControl GetMouseButton(Mouse mouse, int index)
	{
		if (mouse == null)
		{
			return null;
		}
		return index switch
		{
			0 => mouse.leftButton, 
			1 => mouse.rightButton, 
			2 => mouse.middleButton, 
			3 => mouse.backButton, 
			4 => mouse.forwardButton, 
			_ => null, 
		};
	}
}
