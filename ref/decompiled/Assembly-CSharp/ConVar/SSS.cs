namespace ConVar;

[Factory("SSS")]
public class SSS : ConsoleSystem
{
	[ClientVar(Saved = true, Help = "(Generated) Enables the sub-surface scattering post-process effect on character skin, giving skin a translucent appearance under direct lighting")]
	public static bool enabled = true;

	[ClientVar(Saved = true, Help = "(Generated) Sub-surface scattering quality level; deprecated and no longer used in current builds")]
	public static int quality = 0;

	[ClientVar(Saved = true, Help = "(Generated) Renders the sub-surface scattering effect at half resolution, halving the GPU cost at the expense of slight softness in skin shading")]
	public static bool halfres = true;

	[ClientVar(Saved = true, Help = "(Generated) Controls the intensity of the sub-surface scattering effect; higher values make skin appear more translucent under direct light")]
	public static float scale = 1f;
}
