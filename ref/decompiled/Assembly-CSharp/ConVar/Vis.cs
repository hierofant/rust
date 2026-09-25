namespace ConVar;

[Factory("vis")]
public class Vis : ConsoleSystem
{
	[Help("Turns on debug display of lerp")]
	[ClientVar]
	public static bool lerp;

	[Help("Turns on debug display of damages")]
	[ServerVar]
	public static bool damage;

	[Help("Turns on debug display of attacks")]
	[ClientVar]
	[ServerVar]
	public static bool attack;

	[Help("Turns on debug display of protection")]
	[ClientVar]
	[ServerVar]
	public static bool protection;

	[ServerVar]
	[Help("Turns on debug display of weakspots")]
	public static bool weakspots;

	[ServerVar]
	[Help("Show trigger entries")]
	public static bool triggers;

	[ServerVar]
	[Help("Turns on debug display of hitboxes")]
	public static bool hitboxes;

	[Help("Turns on debug display of line of sight checks")]
	[ServerVar]
	public static bool lineofsight;

	[Help("Turns on debug display of senses, which are received by Ai")]
	[ServerVar]
	public static bool sense;
}
