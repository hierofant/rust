namespace ConVar;

[Factory("net")]
public class Net : ConsoleSystem
{
	[ServerVar(Help = "(Generated) When enabled, logs network visibility group changes to the server console, showing when entities enter or leave a player visibility radius")]
	public static bool visdebug = false;

	[ClientVar(Help = "(Generated) Enables verbose client-side network debug logging, printing all incoming and outgoing message types and their byte sizes")]
	public static bool debug = false;

	[ServerVar(Help = "(Generated) Overrides the far network visibility radius in grid cells; -1 = use default; increase to send more distant entity updates to clients")]
	public static int visibilityRadiusFarOverride = -1;

	[ServerVar(Help = "(Generated) Network visibility radius in grid cells used in the deep-sea zone; kept smaller than overworld to limit underwater entity update overhead")]
	public static int visibilityRadiusDeepSea = 10;

	[ServerVar(Help = "(Generated) Overrides the near (high-priority) network visibility radius in grid cells; -1 = use default")]
	public static int visibilityRadiusNearOverride = -1;

	[ServerVar(Name = "global_networked_bases", Help = "(Generated) When enabled, base entities are networked to all clients regardless of distance; disabling restricts base updates to players within the normal visibility radius")]
	public static bool globalNetworkedBases = true;

	[ServerVar(Help = "Toggle printing time taken to send all global entities to client when they connect")]
	public static bool global_network_debug = false;

	[ServerVar(Help = "Toggle checking network group bounds whenever an entity changes its network group")]
	public static bool network_group_debug = false;

	[ServerVar(Help = "(default) true = only broadcast to clients with global networking enabled, false = broadcast to every client regardless")]
	public static bool limit_global_update_broadcast = true;
}
