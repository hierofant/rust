namespace Rust.Ai.Gen2;

public class Crocodile : BaseNPC2
{
	[ServerVar(Help = "Population active on the server, per square km", ShowInAdminUI = true)]
	public static float Population = 1f;

	public override string Categorize()
	{
		return "Crocodile";
	}
}
