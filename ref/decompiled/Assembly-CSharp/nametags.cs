[Factory("nametags")]
public class nametags : ConsoleSystem
{
	[ClientVar(Saved = true, Help = "(Generated) When enabled, player name tags are shown above other players; disable to hide all name tags on the client; saved between sessions")]
	public static bool enabled = true;

	[ClientVar(Saved = true, Help = "When enabled, name tags are also shown while watching a demo; demos hide name tags by default")]
	public static bool showindemos = false;
}
