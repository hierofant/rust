namespace ConVar;

[Factory("workbench")]
public class Workbench : ConsoleSystem
{
	[ServerVar]
	[Help("Skip server-side upgrade clearance zone checks")]
	public static bool skipclearancechecks = false;

	[ServerVar]
	[Help("Whether the range upgrade scales the comfort trigger radius")]
	public static bool scalecomfortradius = true;

	[ServerVar]
	[Help("Multiplier applied to the comfort trigger radius after range upgrade scaling")]
	public static float comfortradiusscale = 0.95f;
}
