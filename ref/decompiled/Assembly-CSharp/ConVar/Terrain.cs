namespace ConVar;

[Factory("terrain")]
public class Terrain : ConsoleSystem
{
	[ClientVar(Saved = true, Help = "(Generated) Controls terrain rendering quality (0-100) including heightmap resolution and detail mesh density; lower values improve performance significantly on large maps")]
	public static float quality = 100f;
}
