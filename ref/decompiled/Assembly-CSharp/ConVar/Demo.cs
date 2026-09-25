using System.IO;
using Network;
using ProtoBuf;
using UnityEngine;

namespace ConVar;

[Factory("demo")]
public class Demo : ConsoleSystem
{
	public class Header : DemoHeader, IDemoHeader
	{
		long IDemoHeader.Length
		{
			get
			{
				return length;
			}
			set
			{
				length = value;
			}
		}

		public void Write(BinaryWriter writer)
		{
			byte[] array = this.ToProtoBytes();
			writer.Write("RUST DEMO FORMAT");
			writer.Write(array.Length);
			writer.Write(array);
			writer.Write('\0');
		}
	}

	public static uint Version = 3u;

	[ClientVar(Help = "Enable demo compatibility layer to resolve renamed/moved prefabs from older recordings")]
	public static bool compatibilitylayer = false;

	[ServerVar(Help = "(Generated) Maximum duration in seconds before a demo recording is automatically split into a new file; default is 3600 (1 hour)")]
	public static float splitseconds = 3600f;

	[ServerVar(Help = "(Generated) Maximum file size in megabytes before a demo recording is automatically split; prevents individual demo files from becoming unmanageably large")]
	public static float splitmegabytes = 200f;

	[ServerVar(Saved = true, Help = "(Generated) Comma-separated list of player Steam IDs whose demos are automatically recorded on the server; empty means no targeted recording")]
	public static string recordlist = "";

	private static int _recordListModeValue = 0;

	[ServerVar(Saved = true, Help = "Controls the behavior of recordlist, 0=whitelist, 1=blacklist")]
	public static int recordlistmode
	{
		get
		{
			return _recordListModeValue;
		}
		set
		{
			_recordListModeValue = Mathf.Clamp(value, 0, 1);
		}
	}

	[ServerVar(Help = "(Generated) Starts recording a server-side demo for the specified player by name or Steam ID to a timestamped file; the player must be connected")]
	public static string record(Arg arg)
	{
		BasePlayer playerOrSleeper = ArgEx.GetPlayerOrSleeper(arg, 0);
		if (!playerOrSleeper || playerOrSleeper.net == null || playerOrSleeper.net.connection == null)
		{
			return "Player not found";
		}
		if (playerOrSleeper.net.connection.IsRecording)
		{
			return "Player already recording a demo";
		}
		playerOrSleeper.StartServerDemoRecording();
		return null;
	}

	[ServerVar(Help = "(Generated) Stops the active server-side demo recording for the specified player and finalises the demo file")]
	public static string stop(Arg arg)
	{
		BasePlayer playerOrSleeper = ArgEx.GetPlayerOrSleeper(arg, 0);
		if (!playerOrSleeper || playerOrSleeper.net == null || playerOrSleeper.net.connection == null)
		{
			return "Player not found";
		}
		if (!playerOrSleeper.net.connection.IsRecording)
		{
			return "Player not recording a demo";
		}
		playerOrSleeper.StopServerDemoRecording();
		return null;
	}
}
