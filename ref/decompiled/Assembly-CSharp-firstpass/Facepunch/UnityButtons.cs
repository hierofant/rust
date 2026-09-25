using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Facepunch;

public static class UnityButtons
{
	private static bool isRegistered = false;

	private static readonly Dictionary<Key, Input.Button> keyButtons = new Dictionary<Key, Input.Button>();

	private static readonly Dictionary<char, string> SpecialKeycaps = new Dictionary<char, string>
	{
		{ '`', "backquote" },
		{ '-', "minus" },
		{ '=', "equals" },
		{ '[', "leftbracket" },
		{ ']', "rightbracket" },
		{ '(', "leftparenthesis" },
		{ ')', "rightparenthesis" },
		{ '\\', "backslash" },
		{ ';', "semicolon" },
		{ '\'', "quote" },
		{ '"', "doublequote" },
		{ ',', "comma" },
		{ '.', "period" },
		{ '/', "slash" },
		{ '+', "plus" },
		{ '*', "asterisk" },
		{ '^', "caret" },
		{ '$', "dollar" },
		{ '_', "underscore" },
		{ '&', "ampersand" },
		{ '<', "less" },
		{ '>', "greater" }
	};

	public static void Register()
	{
		if (isRegistered)
		{
			Debug.LogError("UnityButtons.Register called twice!");
			return;
		}
		InputSystem.onDeviceChange += OnDeviceChange;
		isRegistered = true;
		foreach (Key value in Enum.GetValues(typeof(Key)))
		{
			if (value == Key.None || value == Key.IMESelected)
			{
				continue;
			}
			Key localKey = value;
			bool isFKey = value >= Key.F1 && value <= Key.F24;
			KeyControl control = Keyboard.current?[value];
			string name = ResolveButtonName(value, control);
			Input.AddButton(name, delegate
			{
				Keyboard current3 = Keyboard.current;
				if (current3 == null)
				{
					return false;
				}
				if (!current3[localKey].isPressed)
				{
					return false;
				}
				return (isFKey || KeyBinding.IsOpen || (!NeedsKeyboard.AnyActive(localKey) && !HudMenuInput.AnyActive())) ? true : false;
			}, null, transient: false, control);
			Input.Button button = Input.GetButton(name);
			if (button != null)
			{
				keyButtons[value] = button;
			}
		}
		RegisterMouseButton("mouse0", (Mouse m) => m.leftButton);
		RegisterMouseButton("mouse1", (Mouse m) => m.rightButton);
		RegisterMouseButton("mouse2", (Mouse m) => m.middleButton);
		RegisterMouseButton("mouse3", (Mouse m) => m.backButton);
		RegisterMouseButton("mouse4", (Mouse m) => m.forwardButton);
		float wheelValue2 = 0f;
		float lastWheelValue2 = 0f;
		Input.AddButton("MouseWheelUp", delegate
		{
			if (lastWheelValue2 > 0f)
			{
				wheelValue2 = 0f;
			}
			lastWheelValue2 = wheelValue2;
			wheelValue2 = 0f;
			return lastWheelValue2 > 0f;
		}, delegate
		{
			if (Cursor.visible)
			{
				wheelValue2 = 0f;
			}
			else
			{
				Mouse current2 = Mouse.current;
				if (current2 != null)
				{
					wheelValue2 = Mathf.Max(wheelValue2, current2.scroll.ReadValue().y);
				}
			}
		});
		float wheelValue = 0f;
		float lastWheelValue = 0f;
		Input.AddButton("MouseWheelDown", delegate
		{
			if (lastWheelValue > 0f)
			{
				wheelValue = 0f;
			}
			lastWheelValue = wheelValue;
			wheelValue = 0f;
			return lastWheelValue > 0f;
		}, delegate
		{
			if (Cursor.visible)
			{
				wheelValue = 0f;
			}
			else
			{
				Mouse current = Mouse.current;
				if (current != null)
				{
					wheelValue = Mathf.Max(wheelValue, current.scroll.ReadValue().y * -1f);
				}
			}
		});
	}

	private static string ResolveButtonName(Key key, KeyControl control)
	{
		if (key >= Key.NumpadEnter && key <= Key.Numpad9)
		{
			return FormatKeyName(key);
		}
		if (control != null)
		{
			string displayName = control.displayName;
			if (!string.IsNullOrEmpty(displayName) && displayName.Length == 1 && displayName[0] <= 'ſ')
			{
				char c = displayName[0];
				if (SpecialKeycaps.TryGetValue(c, out var value))
				{
					return value;
				}
				return char.ToLowerInvariant(c).ToString();
			}
		}
		return FormatKeyName(key);
	}

	public static string LocalizeBindToken(string usToken)
	{
		if (TryParseKeyboardKey(usToken, out var key))
		{
			return LocalizeBindToken(key);
		}
		return usToken;
	}

	public static string LocalizeBindToken(Key key)
	{
		if (key == Key.None)
		{
			return FormatKeyName(key);
		}
		return ResolveButtonName(key, Keyboard.current?[key]);
	}

	public static bool TryParseKeyboardKey(string token, out Key key)
	{
		key = Key.None;
		if (string.IsNullOrEmpty(token))
		{
			return false;
		}
		token = FormatKeyName(token);
		if (token.Length == 1 && char.IsDigit(token[0]))
		{
			token = "Digit" + token;
		}
		if (Enum.TryParse<Key>(token, ignoreCase: true, out key))
		{
			return key != Key.None;
		}
		return false;
	}

	public static string FormatKeyName(string token)
	{
		if (string.IsNullOrEmpty(token))
		{
			return token;
		}
		token = LegacyKeyAliases.Normalize(token);
		if (token.StartsWith("Digit", StringComparison.OrdinalIgnoreCase))
		{
			return token.Substring("Digit".Length).ToLowerInvariant();
		}
		if (token.Length == 1 && SpecialKeycaps.TryGetValue(token[0], out var value))
		{
			return value;
		}
		return token.ToLowerInvariant();
	}

	public static string FormatKeyName(Key key)
	{
		return FormatKeyName(key.ToString());
	}

	private static void RegisterMouseButton(string name, Func<Mouse, ButtonControl> selector)
	{
		ButtonControl control = ((Mouse.current != null) ? selector(Mouse.current) : null);
		Input.AddButton(name, delegate
		{
			Mouse current = Mouse.current;
			if (current == null)
			{
				return false;
			}
			ButtonControl buttonControl = selector(current);
			if (buttonControl == null || !buttonControl.isPressed)
			{
				return false;
			}
			return !NeedsMouseButtons.AnyActive();
		}, null, transient: false, control);
	}

	private static void OnDeviceChange(InputDevice device, InputDeviceChange change)
	{
		if (device is Keyboard && (change == InputDeviceChange.ConfigurationChanged || change == InputDeviceChange.Added || change == InputDeviceChange.Reconnected))
		{
			RebuildButtonNames();
		}
	}

	private static void RebuildButtonNames()
	{
		Keyboard current = Keyboard.current;
		if (current == null)
		{
			return;
		}
		foreach (KeyValuePair<Key, Input.Button> keyButton in keyButtons)
		{
			KeyControl control = current[keyButton.Key];
			Input.Button value = keyButton.Value;
			string name = ResolveButtonName(keyButton.Key, control);
			value.Name = name;
		}
	}
}
