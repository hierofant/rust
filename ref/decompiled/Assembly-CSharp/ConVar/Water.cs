namespace ConVar;

[Factory("water")]
public class Water : ConsoleSystem
{
	[ClientVar(Saved = true, Help = "(Generated) Controls the draw distance and density quality of decorative foliage and props; lower values improve performance by reducing the amount of decoration rendered")]
	public static int quality = 1;

	public static int MaxQuality = 2;

	public static int MinQuality = 0;

	[ClientVar(Saved = true, Help = "(Generated) Controls the quality of water surface reflections (0=off, 1=simple probe, 2=full planar reflections); higher values are significantly more GPU-intensive")]
	public static int reflections = 1;

	public static int MaxReflections = 2;

	public static int MinReflections = 0;

	[ClientVar(ClientAdmin = true, Default = "0", Help = "(Generated) When enabled, water simulation advances in scaled (game) time rather than real time; useful for testing water behaviour during time-scale debugging")]
	public static bool scaled_time = false;
}
