using System.Collections.Generic;
using Development.Attributes;
using Facepunch;
using ProtoBuf;
using UnityEngine;

[ResetStaticFields]
public static class TelephoneManager
{
	public const int MaxPhoneNumber = 99990000;

	public const int MinPhoneNumber = 10000000;

	[ServerVar(Help = "(Generated) Maximum number of simultaneous active telephone calls allowed on the server at any time")]
	public static int MaxConcurrentCalls = 10;

	[ServerVar(Help = "(Generated) Maximum duration in seconds a telephone call can remain active before it is automatically terminated")]
	public static int MaxCallLength = 120;

	public static Dictionary<int, PhoneController> allTelephones = new Dictionary<int, PhoneController>();

	public static int maxAssignedPhoneNumber = 99990000;

	public static IReadOnlyCollection<PhoneController> AllTelephones => allTelephones.Values;

	public static int GetUnusedTelephoneNumber()
	{
		int num = Random.Range(10000000, 99990000);
		int num2 = 0;
		int num3 = 1000;
		while (allTelephones.ContainsKey(num) && num2 < num3)
		{
			num2++;
			num = Random.Range(10000000, 99990000);
		}
		if (num2 == num3)
		{
			num = maxAssignedPhoneNumber + 1;
		}
		maxAssignedPhoneNumber = Mathf.Max(maxAssignedPhoneNumber, num);
		return num;
	}

	public static void RegisterTelephone(PhoneController t, bool checkPhoneNumber = false)
	{
		if (checkPhoneNumber && allTelephones.ContainsKey(t.PhoneNumber) && allTelephones[t.PhoneNumber] != t)
		{
			t.PhoneNumber = GetUnusedTelephoneNumber();
		}
		if (!allTelephones.ContainsKey(t.PhoneNumber) && t.PhoneNumber != 0)
		{
			allTelephones.Add(t.PhoneNumber, t);
			maxAssignedPhoneNumber = Mathf.Max(maxAssignedPhoneNumber, t.PhoneNumber);
		}
	}

	public static void DeregisterTelephone(PhoneController t)
	{
		allTelephones.Remove(t.PhoneNumber);
	}

	public static PhoneController GetTelephone(int number)
	{
		return allTelephones.GetValueOrDefault(number);
	}

	public static PhoneController GetRandomTelephone(int ignoreNumber)
	{
		foreach (KeyValuePair<int, PhoneController> allTelephone in allTelephones)
		{
			if (allTelephone.Value.PhoneNumber != ignoreNumber)
			{
				return allTelephone.Value;
			}
		}
		return null;
	}

	public static int GetCurrentActiveCalls()
	{
		int num = 0;
		foreach (KeyValuePair<int, PhoneController> allTelephone in allTelephones)
		{
			if (allTelephone.Value.serverState != 0)
			{
				num++;
			}
		}
		if (num == 0)
		{
			return 0;
		}
		return num / 2;
	}

	public static void GetPhoneDirectory(int ignoreNumber, int page, int perPage, PhoneDirectory directory)
	{
		directory.entries = Pool.Get<List<PhoneDirectory.DirectoryEntry>>();
		int startIndex = page * perPage;
		int count = 0;
		if (!AddPhonesToDirectory(playerOnly: true) && !AddPhonesToDirectory(playerOnly: false))
		{
			directory.atEnd = true;
		}
		bool AddPhonesToDirectory(bool playerOnly)
		{
			foreach (KeyValuePair<int, PhoneController> allTelephone in allTelephones)
			{
				if (allTelephone.Key != ignoreNumber && !string.IsNullOrEmpty(allTelephone.Value.PhoneName) && (!playerOnly || allTelephone.Value.CanModifyPhoneName) && (playerOnly || !allTelephone.Value.CanModifyPhoneName))
				{
					count++;
					if (count >= startIndex)
					{
						PhoneDirectory.DirectoryEntry directoryEntry = Pool.Get<PhoneDirectory.DirectoryEntry>();
						directoryEntry.phoneName = allTelephone.Value.GetDirectoryName();
						directoryEntry.phoneNumber = allTelephone.Value.PhoneNumber;
						directory.entries.Add(directoryEntry);
						if (directory.entries.Count >= perPage)
						{
							directory.atEnd = false;
							return true;
						}
					}
				}
			}
			return false;
		}
	}

	[ServerVar(Help = "(Generated) Prints a table of all registered telephone entities showing their number, directory name, and world position")]
	public static void PrintAllPhones(ConsoleSystem.Arg arg)
	{
		using TextTable textTable = Pool.Get<TextTable>();
		textTable.AddColumns("Number", "Name", "Position");
		foreach (KeyValuePair<int, PhoneController> allTelephone in allTelephones)
		{
			Vector3 position = allTelephone.Value.transform.position;
			textTable.AddRow(allTelephone.Key.ToString(), allTelephone.Value.GetDirectoryName(), $"{position.x} {position.y} {position.z}");
		}
		arg.ReplyWith(textTable.ToString());
	}
}
