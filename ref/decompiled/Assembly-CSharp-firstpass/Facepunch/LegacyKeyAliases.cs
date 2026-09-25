using System.Collections.Generic;

namespace Facepunch;

public static class LegacyKeyAliases
{
	private static readonly Dictionary<string, string> Aliases = new Dictionary<string, string>
	{
		{ "return", "enter" },
		{ "leftcontrol", "leftctrl" },
		{ "rightcontrol", "rightctrl" },
		{ "print", "printscreen" },
		{ "menu", "contextmenu" },
		{ "alpha0", "digit0" },
		{ "alpha1", "digit1" },
		{ "alpha2", "digit2" },
		{ "alpha3", "digit3" },
		{ "alpha4", "digit4" },
		{ "alpha5", "digit5" },
		{ "alpha6", "digit6" },
		{ "alpha7", "digit7" },
		{ "alpha8", "digit8" },
		{ "alpha9", "digit9" },
		{ "keypad0", "numpad0" },
		{ "keypad1", "numpad1" },
		{ "keypad2", "numpad2" },
		{ "keypad3", "numpad3" },
		{ "keypad4", "numpad4" },
		{ "keypad5", "numpad5" },
		{ "keypad6", "numpad6" },
		{ "keypad7", "numpad7" },
		{ "keypad8", "numpad8" },
		{ "keypad9", "numpad9" },
		{ "keypaddivide", "numpaddivide" },
		{ "keypadmultiply", "numpadmultiply" },
		{ "keypadminus", "numpadminus" },
		{ "keypadplus", "numpadplus" },
		{ "keypadperiod", "numpadperiod" },
		{ "keypadenter", "numpadenter" },
		{ "keypadequals", "numpadequals" }
	};

	private static bool updatedSinceLastCheck;

	public static string Normalize(string token)
	{
		if (string.IsNullOrEmpty(token))
		{
			return token;
		}
		if (Aliases.TryGetValue(token.ToLowerInvariant(), out var value))
		{
			updatedSinceLastCheck = true;
			return value;
		}
		return token;
	}

	public static bool HasUpdatedOldBinds()
	{
		bool result = updatedSinceLastCheck;
		updatedSinceLastCheck = false;
		return result;
	}
}
