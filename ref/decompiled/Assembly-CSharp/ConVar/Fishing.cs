namespace ConVar;

[Factory("fishing")]
public class Fishing : ConsoleSystem
{
	[ServerVar(Saved = true)]
	public static bool disableOverfishing = false;

	[ServerVar(Saved = true)]
	public static float overfishedAreaRadius = 16f;

	[ServerVar(Saved = true)]
	public static float overfishedAreaDurationMinutes = 30f;

	[ServerVar(Saved = true)]
	public static int successesUntilOverfished = 25;

	[ServerVar]
	public static bool debugOverfishing = false;
}
