namespace ConVar;

public static class party
{
	[ServerVar(Help = "(Generated) Maximum number of attempts the server makes to find a valid spawn point when spawning a party together; higher values increase the chance of grouping")]
	public static int maxpartyspawnattempts = 50;

	[ServerVar(Help = "(Generated) Maximum distance in metres between party member spawn points when spawning a group together on wake-up")]
	public static int maxpartyspawndistance = 100;

	[ServerVar(Help = "(Generated) When enabled, party members respawn near each other rather than at random map locations when joining a server together")]
	public static bool nearbypartyspawns = true;
}
