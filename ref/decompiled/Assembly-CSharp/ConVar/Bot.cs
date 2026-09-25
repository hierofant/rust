using Facepunch;
using UnityEngine;

namespace ConVar;

[Factory("bot")]
public class Bot : ConsoleSystem
{
	[ServerVar(ShowInAdminUI = false, Help = "(Generated) Executes a console command on a specific bot player by name or Steam ID; hidden from admin UI as it is intended for bot scripting only")]
	public static string sv_exec_command(Arg args)
	{
		if (TryGetBotServer(args, out var bot, out var error))
		{
			return error;
		}
		string @string = args.GetString(1);
		if (string.IsNullOrEmpty(@string))
		{
			return "No command provided";
		}
		bot.Command(@string);
		return string.Empty;
	}

	[ServerVar(ShowInAdminUI = false, Help = "(Generated) Executes a console command on all bot players within a given radius of the calling admin; hidden from admin UI")]
	public static string sv_exec_command_sphere(Arg args)
	{
		string @string = args.GetString(1);
		if (string.IsNullOrEmpty(@string))
		{
			return "invalid command";
		}
		BasePlayer basePlayer = ArgEx.Player(args);
		if (basePlayer == null)
		{
			return "no player context";
		}
		using PooledList<BasePlayer> pooledList = Facepunch.Pool.Get<PooledList<BasePlayer>>();
		global::Vis.Entities(basePlayer.transform.position, args.GetFloat(0, 50f), pooledList);
		int num = 0;
		foreach (BasePlayer item in pooledList)
		{
			if (item.IsBot && item.isServer && !item.IsNpc)
			{
				item.Command(@string);
				num++;
			}
		}
		return $"Executed command on {num} bots.";
	}

	[ServerVar(ShowInAdminUI = false, Help = "(Generated) Executes a console command on every bot player currently on the server; hidden from admin UI")]
	public static string sv_exec_command_all(Arg args)
	{
		string @string = args.GetString(0);
		if (string.IsNullOrEmpty(@string))
		{
			return "invalid command";
		}
		int num = 0;
		foreach (BasePlayer bot in BasePlayer.bots)
		{
			if (bot.IsBot && bot.isServer && !bot.IsNpc)
			{
				num++;
				bot.Command(@string);
			}
		}
		return $"Executed command on {num} bots.";
	}

	[ServerVar(ShowInAdminUI = false, Help = "(Generated) Sets the ducked/crouching model state on a specific bot by name or Steam ID; used to control bot posture in testing scenarios")]
	public static string crouch_server(Arg args)
	{
		if (TryGetBotServer(args, out var bot, out var error))
		{
			return error;
		}
		bot.modelState.ducked = args.GetBool(0, def: true);
		bot.SendNetworkUpdate();
		return "Crouched " + bot.displayName + ".";
	}

	private static bool TryGetBotServer(Arg args, out BasePlayer bot, out string error)
	{
		ulong uLong = args.GetULong(0, 0uL);
		if (uLong == 0L)
		{
			bot = null;
			error = "No user id";
			return true;
		}
		bot = BasePlayer.FindBot(uLong);
		if (bot == null || bot.IsNpc)
		{
			error = $"No bot found with id{uLong}";
			return true;
		}
		error = string.Empty;
		return false;
	}
}
