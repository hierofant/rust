using System.Linq;
using UnityEngine;

namespace ConVar;

public class DDraw
{
	[ServerVar(Help = "(Generated) Sends a ddraw.text debug draw command to a specific player by Steam ID or name; allows admins to display debug text overlays on another player screen")]
	public static void ddrawother(ConsoleSystem.Arg arg)
	{
		arg.GetULong(0, 0uL);
		BasePlayer player = ArgEx.GetPlayer(arg, 0);
		if (player == null || !player.IsConnected)
		{
			arg.ReplyWith("Player not found");
			return;
		}
		if (arg.Args.Length < 2)
		{
			arg.ReplyWith("Usage: ddrawother <player> <ddraw.text arguments>");
			return;
		}
		string command = ConsoleSystem.BuildCommand("ddraw.text", arg.Args.Skip(1));
		player.SendConsoleCommand(command);
	}
}
