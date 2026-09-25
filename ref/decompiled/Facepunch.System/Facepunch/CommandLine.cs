using System;
using System.Collections.Generic;
using Facepunch.Extend;

namespace Facepunch;

public static class CommandLine
{
	private static bool initialized = false;

	private static string commandline = "";

	private static string safecommandline = "";

	private static Dictionary<string, string> switches = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	public static string Full
	{
		get
		{
			Initalize();
			return commandline;
		}
	}

	public static string FullSafe
	{
		get
		{
			Initalize();
			return safecommandline;
		}
	}

	public static void Force(string val)
	{
		commandline = val;
		initialized = false;
	}

	private static void Initalize()
	{
		if (initialized)
		{
			return;
		}
		initialized = true;
		string[] array;
		if (commandline == "")
		{
			string[] commandLineArgs = Environment.GetCommandLineArgs();
			bool flag = false;
			array = commandLineArgs;
			foreach (string text in array)
			{
				commandline = commandline + "\"" + text + "\" ";
				if (flag)
				{
					flag = false;
					safecommandline += "\"******\" ";
				}
				else
				{
					safecommandline = safecommandline + "\"" + text + "\" ";
				}
				if (text.Contains("password", StringComparison.InvariantCultureIgnoreCase))
				{
					flag = true;
				}
				if (text.Contains("anticheatid", StringComparison.InvariantCultureIgnoreCase))
				{
					flag = true;
				}
				if (text.Contains("anticheatkey", StringComparison.InvariantCultureIgnoreCase))
				{
					flag = true;
				}
				if (text.Contains("azure_client_secret", StringComparison.InvariantCultureIgnoreCase))
				{
					flag = true;
				}
				if (text.Contains("azure_client_id", StringComparison.InvariantCultureIgnoreCase))
				{
					flag = true;
				}
				if (text.Contains("azure_tenant_id", StringComparison.InvariantCultureIgnoreCase))
				{
					flag = true;
				}
			}
		}
		if (commandline == "")
		{
			return;
		}
		string text2 = "";
		array = commandline.SplitQuotesStrings();
		foreach (string text3 in array)
		{
			if (text3.Length == 0)
			{
				continue;
			}
			if (text3[0] == '-' || text3[0] == '+')
			{
				if (text2 != "" && !switches.ContainsKey(text2))
				{
					switches.Add(text2, "");
				}
				text2 = text3;
			}
			else if (text2 != "")
			{
				if (!switches.ContainsKey(text2))
				{
					switches.Add(text2, text3);
				}
				text2 = "";
			}
		}
		if (text2 != "" && !switches.ContainsKey(text2))
		{
			switches.Add(text2, "");
		}
	}

	public static bool HasSwitch(string strName)
	{
		Initalize();
		return switches.ContainsKey(strName);
	}

	public static string GetSwitch(string strName, string strDefault)
	{
		Initalize();
		string value = "";
		if (!switches.TryGetValue(strName, out value))
		{
			return strDefault;
		}
		return value;
	}

	public static int GetSwitchInt(string strName, int iDefault)
	{
		Initalize();
		string value = "";
		if (!switches.TryGetValue(strName, out value))
		{
			return iDefault;
		}
		int result = iDefault;
		if (!int.TryParse(value, out result))
		{
			return iDefault;
		}
		return result;
	}

	public static Dictionary<string, string> GetSwitches()
	{
		Initalize();
		return switches;
	}
}
