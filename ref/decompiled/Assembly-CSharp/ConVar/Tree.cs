namespace ConVar;

[Factory("tree")]
public class Tree : ConsoleSystem
{
	[ServerVar(Help = "(Generated) When enabled, tree harvest events are broadcast to all connected clients, not just nearby players; useful for testing tree sync across the network")]
	public static bool global_broadcast;

	[ServerVar(Help = "(Generated) When enabled, trees use a simplified capsule collider instead of the full mesh collider, reducing physics CPU cost at the expense of collision accuracy")]
	public static bool simplified_collider;
}
