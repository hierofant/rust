namespace ConVar;

[Factory("harmony")]
public class Harmony : ConsoleSystem
{
	[ServerVar(Name = "load", Help = "(Generated) Loads and applies a Harmony patch assembly by file name, enabling server-side code patching without restarting")]
	public static void Load(Arg args)
	{
		HarmonyLoader.TryLoadMod(args.GetString(0));
	}

	[ServerVar(Name = "unload", Help = "(Generated) Unloads a previously loaded Harmony patch assembly by file name, reverting any code modifications it applied")]
	public static void Unload(Arg args)
	{
		HarmonyLoader.TryUnloadMod(args.GetString(0));
	}
}
