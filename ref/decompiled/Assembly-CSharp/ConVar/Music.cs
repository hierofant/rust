using System.Text;
using UnityEngine;

namespace ConVar;

[Factory("music")]
public class Music : ConsoleSystem
{
	[ClientVar(Help = "(Generated) Enables the in-game dynamic music system; disabling stops all background music tracks from playing")]
	public static bool enabled = true;

	[ClientVar(Help = "(Generated) Minimum seconds of silence between background music tracks; the game waits at least this long before starting the next song")]
	public static int songGapMin = 240;

	[ClientVar(Help = "(Generated) Maximum seconds of silence between background music tracks; a random gap between songGapMin and songGapMax is chosen between songs")]
	public static int songGapMax = 480;

	[ClientVar(Help = "(Generated) Prints the name of the currently playing music track and its playback position to the console")]
	public static void info(Arg arg)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (SingletonComponent<MusicManager>.Instance == null)
		{
			stringBuilder.Append("No music manager was found");
		}
		else
		{
			stringBuilder.Append("Current music info: ");
			stringBuilder.AppendLine();
			stringBuilder.Append("  theme: " + SingletonComponent<MusicManager>.Instance.currentTheme);
			stringBuilder.AppendLine();
			stringBuilder.Append("  intensity: " + SingletonComponent<MusicManager>.Instance.intensity);
			stringBuilder.AppendLine();
			stringBuilder.Append("  next music: " + SingletonComponent<MusicManager>.Instance.nextMusic);
			stringBuilder.AppendLine();
			stringBuilder.Append("  current time: " + UnityEngine.Time.time);
			stringBuilder.AppendLine();
		}
		arg.ReplyWith(stringBuilder.ToString());
	}
}
