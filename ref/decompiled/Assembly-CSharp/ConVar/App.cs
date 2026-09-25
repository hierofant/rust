using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using CompanionServer;
using Facepunch.Extend;
using Steamworks;
using UnityEngine;

namespace ConVar;

[Factory("app")]
public class App : ConsoleSystem
{
	[ServerVar(Clientside = true, Help = "Base address for the Rust+ companion server API")]
	public static string endpoint = "https://companion-rust.facepunch.com/api";

	[ServerVar(Help = "(Generated) IP address the server listens on for incoming connections; leave empty to bind to all available network interfaces")]
	public static string listenip = "";

	[ServerVar(Help = "(Generated) UDP port number the server listens on; default is 28015; must be open in firewall for players to connect")]
	public static int port;

	[ServerVar(Help = "(Generated) Public IP address advertised to the Steam server browser; leave empty to auto-detect; set explicitly if behind NAT")]
	public static string publicip = "";

	[ServerVar(Help = "Disables updating entirely - emergency use only")]
	public static bool update = true;

	[ServerVar(Help = "Enables sending push notifications")]
	public static bool notifications = true;

	[ServerVar(Help = "Max number of queued messages - set to 0 to disable message processing")]
	public static int queuelimit = 100;

	[ReplicatedVar(Default = "")]
	public static string serverid = "";

	[ServerVar(Help = "Cooldown time before alarms can send another notification (in seconds)")]
	public static float alarmcooldown = 30f;

	[ServerVar(Help = "(Generated) Maximum number of simultaneous player connections allowed; connections above this limit are rejected with a server full message")]
	public static int maxconnections = 500;

	[ServerVar(Help = "(Generated) Maximum simultaneous connections from the same IP address; prevents a single host from consuming all connection slots")]
	public static int maxconnectionsperip = 10;

	[ServerVar(Help = "(Generated) Maximum allowed size in bytes of a single network message; oversized messages are dropped to prevent memory exhaustion attacks")]
	public static int maxmessagesize = 1048576;

	[ServerVar(Help = "(Generated) When enabled, server-side C# exceptions are written to the server log file; disabling reduces log noise on servers with known non-critical exceptions")]
	public static bool logexceptions = true;

	[ServerUserVar]
	public static async void pair(Arg arg)
	{
		try
		{
			BasePlayer basePlayer = ArgEx.Player(arg);
			if (!(basePlayer == null))
			{
				Dictionary<string, string> dictionary = Util.TryGetPlayerPairingData(basePlayer);
				if (dictionary == null)
				{
					arg.ReplyWith("This server is not configured to allow Rust+ connections.");
					return;
				}
				NotificationSendResult notificationSendResult = await Util.SendPairNotification("server", basePlayer, Server.hostname.Truncate(128), "Tap to pair with this server.", dictionary);
				arg.ReplyWith((notificationSendResult == NotificationSendResult.Sent) ? "Sent pairing notification." : notificationSendResult.ToErrorMessage().english);
			}
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
		}
	}

	[ServerUserVar]
	public static void regeneratetoken(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!(basePlayer == null))
		{
			SingletonComponent<ServerMgr>.Instance.persistance.RegenerateAppToken(basePlayer.userID);
			arg.ReplyWith("Regenerated Rust+ token");
		}
	}

	[ServerVar(Help = "(Generated) Prints current server info including name, level, connected players, max players, and network address to the console")]
	public static void info(Arg arg)
	{
		if (!CompanionServer.Server.IsEnabled)
		{
			arg.ReplyWith("Companion server is not enabled");
			return;
		}
		Listener listener = CompanionServer.Server.Listener;
		arg.ReplyWith(string.Format("Server ID: {0}\nListening on: {1}:{2}\nApp connects to: {3}:{4}", serverid, listener.Address, listener.Port, GetPublicIP() ?? "null", port));
	}

	[ServerVar(Help = "Retry initializing the Rust+ companion server if it previously failed")]
	public static void retry_initialize(Arg arg)
	{
		if (CompanionServer.Server.IsEnabled)
		{
			arg.ReplyWith("Companion server is already initialized!");
			return;
		}
		if (port < 0)
		{
			arg.ReplyWith("Companion server port is invalid, cannot initialize companion server");
			return;
		}
		BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(serverside: true);
		if (activeGameMode != null && !activeGameMode.rustPlus)
		{
			arg.ReplyWith("Companion server is disabled by gamemode, cannot initialize companion server");
			return;
		}
		arg.ReplyWith("Trying to initialize companion server...");
		CompanionServer.Server.Initialize();
	}

	[ServerVar(Help = "(Generated) Bans a player by Steam ID from the server via the app layer, adding them to the banlist and kicking them if connected")]
	public static void appban(Arg arg)
	{
		ulong uLong = arg.GetULong(0, 0uL);
		if (uLong == 0L)
		{
			arg.ReplyWith("Usage: app.appban <steamID64>");
			return;
		}
		string strValue = (SingletonComponent<ServerMgr>.Instance.persistance.SetAppTokenLocked(uLong, locked: true) ? $"Banned {uLong} from using the companion app" : $"{uLong} is already banned from using the companion app");
		arg.ReplyWith(strValue);
	}

	[ServerVar(Help = "(Generated) Removes a ban for the specified Steam ID from the server banlist, allowing the player to reconnect")]
	public static void appunban(Arg arg)
	{
		ulong uLong = arg.GetULong(0, 0uL);
		if (uLong == 0L)
		{
			arg.ReplyWith("Usage: app.appunban <steamID64>");
			return;
		}
		string strValue = (SingletonComponent<ServerMgr>.Instance.persistance.SetAppTokenLocked(uLong, locked: false) ? $"Unbanned {uLong}, they can use the companion app again" : $"{uLong} is not banned from using the companion app");
		arg.ReplyWith(strValue);
	}

	public static IPAddress GetListenIP()
	{
		if (!string.IsNullOrWhiteSpace(listenip))
		{
			if (!IPAddress.TryParse(listenip, out var address) || address.AddressFamily != AddressFamily.InterNetwork)
			{
				UnityEngine.Debug.LogError("Invalid app.listenip: " + listenip);
				return IPAddress.Any;
			}
			return address;
		}
		return IPAddress.Any;
	}

	public static async ValueTask<string> GetPublicIPAsync()
	{
		Stopwatch timer = null;
		string publicIP;
		while (true)
		{
			bool num = timer != null && timer.Elapsed.TotalMinutes > 5.0;
			publicIP = GetPublicIP();
			if (num || (!string.IsNullOrWhiteSpace(publicIP) && publicIP != "0.0.0.0"))
			{
				break;
			}
			if (timer == null)
			{
				timer = Stopwatch.StartNew();
			}
			await Task.Delay(10000);
		}
		return publicIP;
	}

	public static string GetPublicIP()
	{
		if (!string.IsNullOrWhiteSpace(publicip) && IPAddress.TryParse(publicip, out var address) && address.AddressFamily == AddressFamily.InterNetwork)
		{
			return publicip;
		}
		if (!SteamServer.IsValid)
		{
			return null;
		}
		return SteamServer.PublicIp.ToString();
	}
}
