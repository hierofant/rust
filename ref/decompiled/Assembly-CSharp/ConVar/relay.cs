using System;
using System.IO;
using Facepunch;
using Network.Relay;
using Newtonsoft.Json;

namespace ConVar;

public static class relay
{
	[ServerVar(Help = "Sets the URL for the Rust Relay server, changing this will STOP the Rust Relay feature (restart with relay.restart)")]
	public static string cfg_server_url
	{
		get
		{
			return RustRelay.Config.ServerUrl;
		}
		set
		{
			RustRelay.Failover("Config Changed");
			RustRelay.Config.ServerUrl = value;
			cfg_save();
		}
	}

	[ServerVar(Help = "Sets the authentication token for the Rust Relay server, changing this will STOP the Rust Relay feature (restart with relay.restart)")]
	public static string cfg_server_auth_token
	{
		get
		{
			return RustRelay.Config.AuthToken;
		}
		set
		{
			RustRelay.Failover("Config Changed");
			RustRelay.Config.AuthToken = value;
			cfg_save();
		}
	}

	[ServerVar(Help = "Sets whether packets should be encrypted when sent to the Rust Relay server, changing this will STOP the Rust Relay feature (restart with relay.restart)")]
	public static bool cfg_encryptpackets
	{
		get
		{
			return RustRelay.Config.EncryptPackets;
		}
		set
		{
			RustRelay.Failover("Config Changed");
			RustRelay.Config.EncryptPackets = value;
			cfg_save();
		}
	}

	[ServerVar(Help = "If enabled, Rust Relay spawns a fake invisible player and only relays packets sent to that player, changing this will STOP the Rust Relay feature (restart with relay.restart)")]
	public static bool cfg_fakeplayer
	{
		get
		{
			return RustRelay.Config.FakePlayer;
		}
		set
		{
			RustRelay.Failover("Config Changed");
			RustRelay.Config.FakePlayer = value;
			RustRelayFakePlayer.SyncWithConfig();
			cfg_save();
		}
	}

	[ServerVar(Help = "Sets the Filter Mode for RPC Messages, (0 = Ignore, 1 = AllowAll, 2 = AllowWhitelist)")]
	public static int cfg_filtermode
	{
		get
		{
			return (int)RustRelay.Config.RPCFilterMode;
		}
		set
		{
			if (value >= 0 && value <= 2)
			{
				RustRelay.Config.RPCFilterMode = (RPCFilterMode)value;
				cfg_save();
			}
		}
	}

	[ServerVar(Help = "If enabled, voice data will be sent to the relay server")]
	public static bool cfg_sendvoicedata
	{
		get
		{
			return RustRelay.Config.EnableVoiceData;
		}
		set
		{
			RustRelay.Config.EnableVoiceData = value;
			cfg_save();
		}
	}

	[ServerVar(Help = "If enabled, console data will be sent to the relay server")]
	public static bool cfg_sendconsoledata
	{
		get
		{
			return RustRelay.Config.EnableConsoleData;
		}
		set
		{
			RustRelay.Config.EnableConsoleData = value;
			cfg_save();
		}
	}

	[ServerVar(Help = "If enabled, Rust Relay will automatically attempt to reconnect when the WebSocket connection drops")]
	public static bool cfg_autoreconnect
	{
		get
		{
			return RustRelay.Config.AutoReconnect;
		}
		set
		{
			RustRelay.Config.AutoReconnect = value;
			cfg_save();
		}
	}

	[ServerVar(Help = "Maximum number of reconnect attempts allowed within the reconnect window before the relay permanently shuts down")]
	public static int cfg_maxreconnects
	{
		get
		{
			return RustRelay.Config.MaxReconnectsInWindow;
		}
		set
		{
			if (value >= 1)
			{
				RustRelay.Config.MaxReconnectsInWindow = value;
				cfg_save();
			}
		}
	}

	[ServerVar(Help = "The rolling window (in minutes) used to track reconnect attempts")]
	public static int cfg_reconnectwindow
	{
		get
		{
			return RustRelay.Config.ReconnectWindowMinutes;
		}
		set
		{
			if (value >= 1)
			{
				RustRelay.Config.ReconnectWindowMinutes = value;
				cfg_save();
			}
		}
	}

	[ServerVar(Help = "Delay (in seconds) between reconnect attempts")]
	public static int cfg_reconnectdelay
	{
		get
		{
			return RustRelay.Config.ReconnectDelaySeconds;
		}
		set
		{
			if (value >= 1)
			{
				RustRelay.Config.ReconnectDelaySeconds = value;
				cfg_save();
			}
		}
	}

	[ServerVar(Help = "Reloads the Rust Relay configuration from disk, this will STOP the Rust Relay feature (restart with relay.restart)")]
	public static string cfg_reload()
	{
		string path = Path.Join(Server.GetServerFolder("cfg"), "relay_cfg.json");
		try
		{
			RustRelay.LoadConfig(JsonConvert.DeserializeObject<RustRelayConfig>(File.ReadAllText(path)));
		}
		catch (Exception)
		{
			string contents = JsonConvert.SerializeObject(new RustRelayConfig(), Formatting.Indented);
			File.WriteAllText(path, contents);
			return "Failed to load file, a default relay_cfg.json has been created in the cfg folder";
		}
		RustRelay.InitUrls();
		RustRelayFakePlayer.SyncWithConfig();
		return null;
	}

	[ServerVar(Help = "Forces a save of the Rust Relay configuration to disk")]
	public static void cfg_save()
	{
		string path = Path.Join(Server.GetServerFolder("cfg"), "relay_cfg.json");
		string contents = JsonConvert.SerializeObject(RustRelay.Config, Formatting.Indented);
		File.WriteAllText(path, contents);
	}

	[ServerVar(Help = "Clears the reconnect attempt history, resetting the reconnect budget")]
	public static void reconnect_reset()
	{
		RustRelay.ClearReconnectHistory();
	}

	[ServerVar(Help = "Returns a detailed Rust Relay status report")]
	public static string status()
	{
		return RustRelay.GetStatusReport() + RustRelayFakePlayer.GetStatusReport();
	}

	[ServerVar(Help = "Clears the relay send queue (useful if things are going wrong with the relay server)")]
	public static void clear_queue()
	{
		RustRelay.WipeSendThreadQueue();
	}

	[ServerVar(Help = "Shutsdown the Rust Relay sever connection")]
	public static void shutdown()
	{
		RustRelay.Failover("Manually Shutdown");
	}

	[ServerVar(Help = "Attempts to restart the Rust Relay connection (may cause short lag)")]
	public static void restart()
	{
		RustRelay.AttemptRestart();
	}

	[ServerVar(Help = "Rebuilds the RPC Whitelist ID List")]
	public static void rpc_whitelist_rebuild()
	{
		RustRelay.BuildRPCWhitelist();
	}

	[ServerVar(Help = "Adds an RPC Message to the RPC Whitelist")]
	public static void rpc_whitelist_add(ConsoleSystem.Arg arg)
	{
		StringView[] args = arg.Args;
		for (int i = 0; i < args.Length; i++)
		{
			StringView stringView = args[i];
			RustRelay.Config.RPCWhitelist.Add(stringView.ToString());
		}
		rpc_whitelist_rebuild();
	}

	[ServerVar(Help = "Removes an RPC Message from the RPC Whitelist")]
	public static void rpc_whitelist_remove(ConsoleSystem.Arg arg)
	{
		StringView[] args = arg.Args;
		for (int i = 0; i < args.Length; i++)
		{
			StringView stringView = args[i];
			RustRelay.Config.RPCWhitelist.Remove(stringView.ToString());
		}
		rpc_whitelist_rebuild();
	}
}
