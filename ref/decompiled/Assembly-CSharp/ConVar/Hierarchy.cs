using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ConVar;

[Factory("hierarchy")]
public class Hierarchy : ConsoleSystem
{
	private static GameObject currentDir;

	private static Transform[] GetCurrent()
	{
		if (currentDir == null)
		{
			return TransformUtil.GetRootObjects().ToArray();
		}
		List<Transform> list = new List<Transform>();
		for (int i = 0; i < currentDir.transform.childCount; i++)
		{
			list.Add(currentDir.transform.GetChild(i));
		}
		return list.ToArray();
	}

	[ServerVar(Help = "(Generated) Lists all GameObjects in the current hierarchy context, similar to the Unix ls command; used for navigating the scene hierarchy from the console")]
	public static void ls(Arg args)
	{
		string text = "";
		string filter = args.GetString(0);
		text = ((!currentDir) ? (text + "Listing .\n\n") : (text + "Listing " + TransformEx.GetRecursiveName(currentDir.transform) + "\n\n"));
		foreach (Transform item in (from x in GetCurrent()
			where string.IsNullOrEmpty(filter) || x.name.Contains(filter)
			select x).Take(40))
		{
			text += $"   {item.name} [{item.childCount}]\n";
		}
		text += "\n";
		args.ReplyWith(text);
	}

	[ServerVar(Help = "(Generated) Changes the current hierarchy context to the named child GameObject, similar to the Unix cd command; allows drilling into nested scene objects")]
	public static void cd(Arg args)
	{
		if (args.FullString == ".")
		{
			currentDir = null;
			args.ReplyWith("Changed to .");
			return;
		}
		if (args.FullString == "..")
		{
			if ((bool)currentDir)
			{
				currentDir = (currentDir.transform.parent ? currentDir.transform.parent.gameObject : null);
			}
			currentDir = null;
			if ((bool)currentDir)
			{
				args.ReplyWith("Changed to " + TransformEx.GetRecursiveName(currentDir.transform));
			}
			else
			{
				args.ReplyWith("Changed to .");
			}
			return;
		}
		string argsStringLower = args.FullString.ToString().ToLower();
		Transform transform = GetCurrent().FirstOrDefault((Transform x) => x.name.ToLower() == argsStringLower);
		if (transform == null)
		{
			transform = GetCurrent().FirstOrDefault((Transform x) => x.name.StartsWith(argsStringLower, StringComparison.CurrentCultureIgnoreCase));
		}
		if ((bool)transform)
		{
			currentDir = transform.gameObject;
			args.ReplyWith("Changed to " + TransformEx.GetRecursiveName(currentDir.transform));
		}
		else
		{
			args.ReplyWith("Couldn't find \"" + args.FullString.ToString() + "\"");
		}
	}

	[ServerVar(Help = "(Generated) Deletes the named GameObject from the scene hierarchy; use with caution as this permanently removes the object")]
	public static void del(Arg args)
	{
		if (!args.HasArgs())
		{
			return;
		}
		string argsStringLower = args.FullString.ToString().ToLower();
		IEnumerable<Transform> enumerable = from x in GetCurrent()
			where x.name.ToLower() == argsStringLower
			select x;
		if (enumerable.Count() == 0)
		{
			enumerable = from x in GetCurrent()
				where x.name.StartsWith(argsStringLower, StringComparison.CurrentCultureIgnoreCase)
				select x;
		}
		if (enumerable.Count() == 0)
		{
			args.ReplyWith("Couldn't find  " + args.FullString);
			return;
		}
		foreach (Transform item in enumerable)
		{
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(item.gameObject);
			if (baseEntity.IsValid())
			{
				if (baseEntity.isServer)
				{
					baseEntity.Kill();
				}
			}
			else
			{
				GameManager.Destroy(item.gameObject);
			}
		}
		args.ReplyWith("Deleted " + enumerable.Count() + " objects");
	}
}
