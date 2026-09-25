using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Facepunch;
using Facepunch.Extend;
using Facepunch.Nexus.Models;
using Network;
using Network.Visibility;
using ProtoBuf;
using ProtoBuf.Nexus;
using Rust;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

namespace ConVar;

[Factory("global")]
public class Global : ConsoleSystem
{
	private static int _developer;

	[ServerVar(Help = "(Generated) Maximum number of Unity job system worker threads; controls the background thread pool size for job dispatching")]
	[ClientVar(Help = "(Generated) Maximum number of Unity job system worker threads; controls the background thread pool size for job dispatching")]
	public static int maxthreads = 8;

	[ServerVar(Help = "(Generated) When enabled, asset bundles are unloaded from memory after their assets are extracted, saving memory; disable to keep bundles resident")]
	[ClientVar(Help = "(Generated) When enabled, asset bundles are unloaded from memory after their assets are extracted, saving memory; disable to keep bundles resident")]
	public static bool forceUnloadBundles = true;

	[ServerVar(Help = "(Generated) When true, the server network position is updated to match the debug camera world position while spectating; useful for testing position-dependent server logic from the spectator view")]
	public static bool updateNetworkPositionWithDebugCameraWhileSpectating = false;

	public static readonly string TopOfBaseFlag = "--topofbase";

	public static readonly string UndergroundFlag = "--underground";

	[ServerVar(Saved = true, Help = "(Generated) Controls the on-screen performance overlay detail level; 0 = off, higher values add more metrics such as FPS, ping, entity count, and memory usage")]
	[ClientVar(Saved = true, Help = "(Generated) Controls the on-screen performance overlay detail level; 0 = off, higher values add more metrics such as FPS, ping, entity count, and memory usage")]
	public static int perf = 0;

	[ClientVar(Saved = true, ClientAdmin = true, Help = "Media: This can be used to disable the performance text info when GC gets triggered")]
	public static bool perf_disable_gc_notif = false;

	private static bool _god = false;

	private static bool _forceOffAdminStatusOverlay = false;

	[ClientVar]
	[ServerVar(ClientAdmin = true, ServerAdmin = true, Help = "When enabled a player wearing a gingerbread suit will gib like the gingerbread NPC's")]
	public static bool cinematicGingerbreadCorpses = false;

	private static uint _gingerbreadMaterialID = 0u;

	[ServerVar(Saved = true, ShowInAdminUI = true, Help = "Multiplier applied to SprayDuration if a spray isn't in the sprayers auth (cannot go above 1f)")]
	public static float SprayOutOfAuthMultiplier = 0.5f;

	[ServerVar(Saved = true, ShowInAdminUI = true, Help = "Base time (in seconds) that sprays last")]
	public static float SprayDuration = 10800f;

	[ServerVar(Saved = true, ShowInAdminUI = true, Help = "If a player sprays more than this, the oldest spray will be destroyed. 0 will disable")]
	public static int MaxSpraysPerPlayer = 40;

	[ServerVar(Help = "Disables the backpacks that appear after a corpse times out")]
	public static bool disableBagDropping = false;

	[ClientVar(Saved = true, Help = "Disables any emoji animations")]
	public static bool blockEmojiAnimations = false;

	[ClientVar(Saved = true, Help = "Blocks any emoji from appearing")]
	public static bool blockEmoji = false;

	[ClientVar(Saved = true, Help = "Blocks emoji provided by servers from appearing")]
	public static bool blockServerEmoji = false;

	[ClientVar(Saved = true, Help = "Displays any emoji rendering errors in the console")]
	public static bool showEmojiErrors = false;

	[ServerVar(Help = "(Generated) Developer mode level: 0 = off, 1 = developer overlays and convar unlocks, higher values enable increasingly verbose debug logging")]
	[ClientVar(Help = "(Generated) Developer mode level: 0 = off, 1 = developer overlays and convar unlocks, higher values enable increasingly verbose debug logging")]
	public static int developer
	{
		get
		{
			return _developer;
		}
		set
		{
			_developer = value;
			Array.Fill(RustLog.Levels, _developer);
		}
	}

	[ServerVar(Help = "(Generated) Number of Unity job worker threads; 0 or -1 sets the default (auto); higher values improve parallel job throughput on many-core CPUs")]
	[ClientVar(Help = "(Generated) Number of Unity job worker threads; 0 or -1 sets the default (auto); higher values improve parallel job throughput on many-core CPUs")]
	public static int job_system_threads
	{
		get
		{
			return JobsUtility.JobWorkerCount;
		}
		set
		{
			if (value < 1)
			{
				JobsUtility.ResetJobWorkerCount();
				return;
			}
			value = Mathf.Clamp(value, 1, JobsUtility.JobWorkerMaximumCount);
			JobsUtility.JobWorkerCount = value;
		}
	}

	[ClientVar(ClientInfo = true, Saved = true, Help = "If you're an admin this will enable god mode")]
	public static bool god
	{
		get
		{
			return _god;
		}
		set
		{
			_god = value;
		}
	}

	[ClientVar(ClientInfo = true, Saved = true, Help = "Media: Forcefully disables all status overlays (god, creative, invis)")]
	public static bool forceOffAdminStatusOverlay
	{
		get
		{
			return _forceOffAdminStatusOverlay;
		}
		set
		{
			_forceOffAdminStatusOverlay = value;
		}
	}

	[ServerVar(Help = "(Generated) Schedules a server restart; optionally accepts a countdown in seconds and a broadcast message sent to all players before the restart occurs")]
	public static void restart(Arg args)
	{
		ServerMgr.RestartServer(args.GetString(1, string.Empty), args.GetInt(0, 300));
	}

	[ServerVar(Help = "(Generated) Quits the application cleanly with no arguments; rejects calls with arguments to prevent accidental exit; in the editor exits play mode")]
	[ClientVar(Help = "(Generated) Quits the application cleanly with no arguments; rejects calls with arguments to prevent accidental exit; in the editor exits play mode")]
	public static void quit(Arg args)
	{
		if (args != null && args.HasArgs())
		{
			args.ReplyWith("Invalid quit command, quit only works if provided with no arguments.");
			return;
		}
		if (UnityEngine.Application.isEditor)
		{
			UnityEngine.Debug.LogWarning("Aborting quit because we're in the editor");
			return;
		}
		if (SingletonComponent<ServerMgr>.Instance != null)
		{
			SingletonComponent<ServerMgr>.Instance.Shutdown();
		}
		Rust.Application.isQuitting = true;
		Network.Net.sv?.Stop("quit");
		Process.GetCurrentProcess().Kill();
		UnityEngine.Debug.Log("Quitting");
		Rust.Application.Quit();
	}

	[ServerVar(Help = "(Generated) Runs a server performance diagnostic report covering entity counts, memory usage, and active invokes, outputting results to the server console")]
	public static void report(Arg args)
	{
		ServerPerformance.DoReport();
	}

	[ClientVar(Help = "(Generated) Prints all live Unity Object instances sorted by total memory usage, showing type, instance count, and estimated total size in bytes")]
	[ServerVar(Help = "(Generated) Prints all live Unity Object instances sorted by total memory usage, showing type, instance count, and estimated total size in bytes")]
	public static void objects(Arg args)
	{
		UnityEngine.Object[] array = UnityEngine.Object.FindObjectsByType<UnityEngine.Object>(FindObjectsSortMode.None);
		string text = "";
		Dictionary<Type, int> dictionary = new Dictionary<Type, int>();
		Dictionary<Type, long> dictionary2 = new Dictionary<Type, long>();
		UnityEngine.Object[] array2 = array;
		foreach (UnityEngine.Object @object in array2)
		{
			int runtimeMemorySize = Profiler.GetRuntimeMemorySize(@object);
			if (dictionary.ContainsKey(@object.GetType()))
			{
				dictionary[@object.GetType()]++;
			}
			else
			{
				dictionary.Add(@object.GetType(), 1);
			}
			if (dictionary2.ContainsKey(@object.GetType()))
			{
				dictionary2[@object.GetType()] += runtimeMemorySize;
			}
			else
			{
				dictionary2.Add(@object.GetType(), runtimeMemorySize);
			}
		}
		foreach (KeyValuePair<Type, long> item in dictionary2.OrderByDescending(delegate(KeyValuePair<Type, long> x)
		{
			KeyValuePair<Type, long> keyValuePair = x;
			return keyValuePair.Value;
		}))
		{
			text = text + dictionary[item.Key].ToString().PadLeft(10) + " " + item.Value.FormatBytes().PadLeft(15) + "\t" + item.Key?.ToString() + "\n";
		}
		args.ReplyWith(text);
	}

	[ServerVar(Help = "(Generated) Prints a list of all live Texture objects with their name and estimated runtime memory size")]
	[ClientVar(Help = "(Generated) Prints a list of all live Texture objects with their name and estimated runtime memory size")]
	public static void textures(Arg args)
	{
		UnityEngine.Texture[] array = UnityEngine.Object.FindObjectsByType<UnityEngine.Texture>(FindObjectsSortMode.None);
		string text = "";
		UnityEngine.Texture[] array2 = array;
		foreach (UnityEngine.Texture texture in array2)
		{
			string text2 = Profiler.GetRuntimeMemorySize(texture).FormatBytes();
			text = text + texture.ToString().PadRight(30) + texture.name.PadRight(30) + text2 + "\n";
		}
		args.ReplyWith(text);
	}

	[ServerVar(Help = "(Generated) Prints the count of enabled versus disabled Collider components currently in the scene")]
	[ClientVar(Help = "(Generated) Prints the count of enabled versus disabled Collider components currently in the scene")]
	public static void colliders(Arg args)
	{
		int num = (from x in UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsSortMode.None)
			where x.enabled
			select x).Count();
		int num2 = (from x in UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsSortMode.None)
			where !x.enabled
			select x).Count();
		string strValue = num + " colliders enabled, " + num2 + " disabled";
		args.ReplyWith(strValue);
	}

	[ClientVar(Help = "(Generated) Prints the current state of server-side stability check and surroundings update queues; reports nothing useful on client")]
	[ServerVar(Help = "(Generated) Prints the current state of server-side stability check and surroundings update queues; reports nothing useful on client")]
	public static void queue(Arg args)
	{
		string text = "";
		text = text + "stabilityCheckQueue:        " + StabilityEntity.stabilityCheckQueue.Info() + "\n";
		text = text + "updateSurroundingsQueue:    " + StabilityEntity.updateSurroundingsQueue.Info() + "\n";
		args.ReplyWith(text);
	}

	[ServerUserVar]
	public static void setinfo(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((bool)basePlayer)
		{
			string @string = args.GetString(0, null);
			string string2 = args.GetString(1, null);
			if (@string != null && string2 != null)
			{
				basePlayer.SetInfo(@string, string2);
			}
		}
	}

	[ServerVar(Help = "(Generated) Puts the calling player into the sleeping state, disconnecting their control and making them a sleeping entity on the server")]
	public static void sleep(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((bool)basePlayer && !basePlayer.IsSleeping() && !basePlayer.IsSpectating() && !basePlayer.IsDead())
		{
			basePlayer.StartSleeping();
		}
	}

	[ServerVar(Help = "(Generated) Puts the player that the calling admin is looking at into the sleeping state; useful for testing sleeping player interactions")]
	public static void sleeptarget(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((bool)basePlayer)
		{
			BasePlayer lookingAtPlayer = RelationshipManager.GetLookingAtPlayer(basePlayer);
			if (!(lookingAtPlayer == null))
			{
				lookingAtPlayer.StartSleeping();
			}
		}
	}

	[ServerUserVar]
	public static void kill(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if (!basePlayer || basePlayer.IsSpectating() || basePlayer.IsDead())
		{
			return;
		}
		if (basePlayer.IsRestrained)
		{
			Handcuffs handcuffs = basePlayer.Belt?.GetRestraintItem();
			if (handcuffs != null && handcuffs.BlockSuicide)
			{
				return;
			}
		}
		if (basePlayer.CanSuicide())
		{
			basePlayer.Hurt(1000f, DamageType.Suicide, basePlayer, useProtection: false);
			if (basePlayer.IsDead())
			{
				basePlayer.MarkSuicide();
			}
		}
		else
		{
			basePlayer.ConsoleMessage("You can't suicide again so quickly, wait a while");
		}
	}

	[ServerUserVar]
	public static void respawn(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if (!basePlayer)
		{
			return;
		}
		if (!basePlayer.IsDead() && !basePlayer.IsSpectating())
		{
			if (developer > 0)
			{
				UnityEngine.Debug.LogWarning(basePlayer?.ToString() + " wanted to respawn but isn't dead or spectating");
			}
			basePlayer.SendNetworkUpdate();
		}
		else if (basePlayer.CanRespawn())
		{
			basePlayer.MarkRespawn();
			basePlayer.Respawn();
		}
		else
		{
			basePlayer.ConsoleMessage("You can't respawn again so quickly, wait a while");
		}
	}

	[ServerVar(Help = "(Generated) Puts the calling player or a named target into the wounded/downed state, simulating the critical injury bleed-out state")]
	public static void injure(Arg args)
	{
		InjurePlayer(ArgEx.Player(args));
	}

	public static void InjurePlayer(BasePlayer ply)
	{
		if (ply == null || ply.IsDead())
		{
			return;
		}
		HitInfo hitInfo = Facepunch.Pool.Get<HitInfo>();
		hitInfo.Init(ply, ply, DamageType.Suicide, 1000f, ply.transform.position);
		hitInfo.UseProtection = false;
		if (Server.woundingenabled && !ply.IsIncapacitated() && !ply.IsSleeping() && !ply.isMounted)
		{
			if (ply.IsCrawling())
			{
				ply.GoToIncapacitated(hitInfo);
			}
			else
			{
				ply.BecomeWounded(hitInfo);
			}
		}
		else
		{
			ply.ConsoleMessage("Can't go to wounded state right now.");
		}
	}

	[ServerVar(Help = "(Generated) Revives the calling player or a named target from the wounded state, restoring them to standing with a small amount of health")]
	public static void recover(Arg args)
	{
		RecoverPlayer(ArgEx.Player(args));
	}

	public static void RecoverPlayer(BasePlayer ply)
	{
		if (!(ply == null) && !ply.IsDead())
		{
			ply.StopWounded();
		}
	}

	[ServerVar(Help = "(Generated) Enters spectator mode; optionally accepts a player name or Steam ID to spectate that specific player from a third-person camera")]
	public static void spectate(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((bool)basePlayer)
		{
			basePlayer.wantsSpectate = true;
			if (!basePlayer.IsDead())
			{
				basePlayer.DieInstantly();
			}
			string @string = args.GetString(0);
			if (basePlayer.IsDead())
			{
				basePlayer.StartSpectating();
				basePlayer.UpdateSpectateTarget(@string);
			}
			basePlayer.wantsSpectate = false;
		}
	}

	[ServerVar(Help = "(Generated) Toggles display of the team info overlay (health, location, vitals) for the spectated player while in spectator mode")]
	public static void toggleSpectateTeamInfo(Arg args)
	{
		bool @bool = args.GetBool(0);
		BasePlayer basePlayer = ArgEx.Player(args);
		if (basePlayer != null)
		{
			basePlayer.SetSpectateTeamInfo(@bool);
			args.ReplyWith($"ToggleSpectateTeamInfo is now {@bool}");
		}
		else
		{
			args.ReplyWith("Invalid player or player is not spectating");
		}
	}

	[ServerVar(Help = "(Generated) Enters spectator mode targeting the entity or player with the given network entity ID")]
	public static void spectateid(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((bool)basePlayer)
		{
			basePlayer.wantsSpectate = true;
			if (!basePlayer.IsDead())
			{
				basePlayer.DieInstantly();
			}
			ulong uLong = args.GetULong(0, 0uL);
			if (basePlayer.IsDead())
			{
				basePlayer.StartSpectating();
				basePlayer.UpdateSpectateTarget(uLong);
			}
			basePlayer.wantsSpectate = false;
		}
	}

	[ServerUserVar]
	public static void respawn_sleepingbag(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if (!basePlayer || !basePlayer.IsDead())
		{
			return;
		}
		NetworkableId entityID = ArgEx.GetEntityID(args, 0);
		if (!entityID.IsValid)
		{
			args.ReplyWith("Missing sleeping bag ID");
			return;
		}
		string @string = args.GetString(1);
		string errorMessage;
		if (NexusServer.Started && !string.IsNullOrWhiteSpace(@string))
		{
			if (!ZoneController.Instance.CanRespawnAcrossZones(basePlayer))
			{
				args.ReplyWith("You cannot respawn to a different zone");
				return;
			}
			NexusZoneDetails nexusZoneDetails = NexusServer.FindZone(@string);
			if (nexusZoneDetails == null)
			{
				args.ReplyWith("Zone was not found");
			}
			else if (!basePlayer.CanRespawn())
			{
				args.ReplyWith("You can't respawn again so quickly, wait a while");
			}
			else
			{
				NexusRespawn(basePlayer, nexusZoneDetails, entityID);
			}
		}
		else if (!SleepingBag.TrySpawnPlayer(basePlayer, entityID, out errorMessage))
		{
			args.ReplyWith(errorMessage);
		}
		static async void NexusRespawn(BasePlayer player, NexusZoneDetails toZone, NetworkableId sleepingBag)
		{
			_ = 1;
			try
			{
				player.nextRespawnTime = float.PositiveInfinity;
				Request request = Facepunch.Pool.Get<Request>();
				request.respawnAtBag = Facepunch.Pool.Get<SleepingBagRespawnRequest>();
				request.respawnAtBag.userId = player.userID;
				request.respawnAtBag.sleepingBagId = sleepingBag;
				request.respawnAtBag.secondaryData = player.SaveSecondaryData();
				using (Response response = await NexusServer.ZoneRpc(toZone.Key, request))
				{
					if (!response.status.success)
					{
						if (player.IsConnected)
						{
							player.ConsoleMessage("RespawnAtBag failed: " + response.status.errorMessage);
						}
						return;
					}
				}
				await NexusServer.ZoneClient.Assign(player.userID, toZone.Key);
				if (player.IsConnected)
				{
					ConsoleNetwork.SendClientCommandImmediate(player.net.connection, "nexus.redirect", toZone.IpAddress, toZone.GamePort, NexusUtil.ConnectionProtocol(toZone));
					player.Kick("Redirecting to another zone...");
				}
			}
			catch (Exception ex)
			{
				if (player.IsConnected)
				{
					player.ConsoleMessage(ex.ToString());
				}
			}
			finally
			{
				player.MarkRespawn();
			}
		}
	}

	[ServerUserVar]
	public static void respawn_sleepingbag_remove(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if (!basePlayer)
		{
			return;
		}
		NetworkableId entityID = ArgEx.GetEntityID(args, 0);
		if (!entityID.IsValid)
		{
			args.ReplyWith("Missing sleeping bag ID");
			return;
		}
		string @string = args.GetString(1);
		if (NexusServer.Started && !string.IsNullOrWhiteSpace(@string))
		{
			NexusZoneDetails nexusZoneDetails = NexusServer.FindZone(@string);
			if (nexusZoneDetails == null)
			{
				args.ReplyWith("Zone was not found");
			}
			else if (ZoneController.Instance.CanRespawnAcrossZones(basePlayer))
			{
				NexusRemoveBag(basePlayer, nexusZoneDetails.Key, entityID);
			}
		}
		else
		{
			SleepingBag.DestroyBag(basePlayer.userID, entityID);
		}
		static async void NexusRemoveBag(BasePlayer player, string zoneKey, NetworkableId sleepingBag)
		{
			try
			{
				Request request = Facepunch.Pool.Get<Request>();
				request.destroyBag = Facepunch.Pool.Get<SleepingBagDestroyRequest>();
				request.destroyBag.userId = player.userID;
				request.destroyBag.sleepingBagId = sleepingBag;
				(await NexusServer.ZoneRpc(zoneKey, request)).Dispose();
			}
			catch (Exception ex)
			{
				if (player.IsConnected)
				{
					player.ConsoleMessage(ex.ToString());
				}
			}
		}
	}

	[ServerUserVar]
	public static void respawn_sleepingbag_favourite(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((bool)basePlayer)
		{
			NetworkableId entityID = ArgEx.GetEntityID(args, 0);
			if (!entityID.IsValid)
			{
				args.ReplyWith("Missing sleeping bag ID");
				return;
			}
			if (!basePlayer.IsDead())
			{
				args.ReplyWith("Can only modify while dead");
				return;
			}
			bool favourite = args.GetInt(1) != 0;
			SleepingBag.SetBagFavourite(basePlayer.userID, entityID, favourite);
		}
	}

	[ServerUserVar]
	public static void status_sv(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((bool)basePlayer)
		{
			args.ReplyWith(basePlayer.GetDebugStatus());
		}
	}

	[ClientVar(Help = "(Generated) Prints client-side connection status information including connected state, ping, player entity ID, and current map to the client console")]
	public static void status_cl(Arg args)
	{
	}

	[ServerVar(Help = "(Generated) Teleports the calling admin to a player by name or partial name match; if two arguments are given, moves the first-named player to the second")]
	public static void teleport(Arg args)
	{
		if (args.HasArgs(2))
		{
			BasePlayer playerOrSleeperOrBot = ArgEx.GetPlayerOrSleeperOrBot(args, 0);
			if ((bool)playerOrSleeperOrBot && playerOrSleeperOrBot.IsAlive())
			{
				BasePlayer playerOrSleeperOrBot2 = ArgEx.GetPlayerOrSleeperOrBot(args, 1);
				if ((bool)playerOrSleeperOrBot2 && playerOrSleeperOrBot2.IsAlive())
				{
					playerOrSleeperOrBot.Teleport(playerOrSleeperOrBot2);
				}
			}
			return;
		}
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((bool)basePlayer && basePlayer.IsAlive())
		{
			BasePlayer playerOrSleeperOrBot3 = ArgEx.GetPlayerOrSleeperOrBot(args, 0);
			if ((bool)playerOrSleeperOrBot3 && playerOrSleeperOrBot3.IsAlive())
			{
				basePlayer.Teleport(playerOrSleeperOrBot3);
			}
		}
	}

	[ServerVar(Help = "(Generated) Teleports the named player to the calling admin current position")]
	public static void teleport2me(Arg args)
	{
		BasePlayer playerOrSleeperOrBot = ArgEx.GetPlayerOrSleeperOrBot(args, 0);
		if (playerOrSleeperOrBot == null)
		{
			args.ReplyWith("Player or bot not found");
			return;
		}
		if (!playerOrSleeperOrBot.IsAlive())
		{
			args.ReplyWith("Target is not alive");
			return;
		}
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((bool)basePlayer && basePlayer.IsAlive())
		{
			playerOrSleeperOrBot.Teleport(basePlayer);
		}
	}

	[ServerVar(Help = "(Generated) Teleports all connected players to the calling admin current position")]
	public static void teleporteveryone2me(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((bool)basePlayer)
		{
			TeleportPlayersToMe(basePlayer, includeSleepers: true, includeNonSleepers: true, 0uL);
		}
	}

	[ServerVar(Help = "(Generated) Teleports all sleeping player entities to the calling admin current position")]
	public static void teleportsleepers2me(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((bool)basePlayer)
		{
			TeleportPlayersToMe(basePlayer, includeSleepers: true, includeNonSleepers: false, 0uL);
		}
	}

	[ServerVar(Help = "(Generated) Teleports all connected active (non-sleeping) players to the calling admin current position")]
	public static void teleportnonsleepers2me(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((bool)basePlayer)
		{
			TeleportPlayersToMe(basePlayer, includeSleepers: false, includeNonSleepers: true, 0uL);
		}
	}

	[ServerVar(Help = "(Generated) Teleports all members of the calling player team to the calling admin current position")]
	public static void teleportteam2me(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((bool)basePlayer)
		{
			if (basePlayer.Team == null)
			{
				args.ReplyWith("Player is not in a team");
			}
			else
			{
				TeleportPlayersToMe(basePlayer, includeSleepers: true, includeNonSleepers: true, basePlayer.Team.teamID);
			}
		}
	}

	[ServerVar(Help = "(Generated) Teleports all members of the named player team to the calling admin current position")]
	public static void teleporttargetteam2me(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((bool)basePlayer)
		{
			if (basePlayer.Team == null)
			{
				args.ReplyWith("Player is not in a team");
				return;
			}
			ulong uLong = args.GetULong(0, 0uL);
			TeleportPlayersToMe(basePlayer, includeSleepers: true, includeNonSleepers: true, uLong);
		}
	}

	private static void TeleportPlayersToMe(BasePlayer player, bool includeSleepers, bool includeNonSleepers, ulong filterByTeam = 0uL)
	{
		if (player == null || !player || !player.IsAlive())
		{
			return;
		}
		foreach (BasePlayer allPlayer in BasePlayer.allPlayerList)
		{
			if (allPlayer.IsAlive() && !(allPlayer == player) && (!allPlayer.IsSleeping() || includeSleepers) && (allPlayer.IsSleeping() || includeNonSleepers) && (filterByTeam == 0L || (allPlayer.Team != null && allPlayer.Team.teamID == filterByTeam)))
			{
				allPlayer.Teleport(player);
			}
		}
	}

	[ServerVar(Help = "(Generated) Teleports the calling admin to the nearest entity matching the given prefab short name, with an optional radius filter")]
	public static void teleportany(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((bool)basePlayer && basePlayer.IsAlive())
		{
			basePlayer.Teleport(args.GetString(0), playersOnly: false);
		}
	}

	[Help("Teleport to the current closest entity matching the first argument name. Add second int argument to teleport to the nth closest entity (teleport2nearest horse 2 will teleport to the 3rd closest horse)")]
	[ServerVar]
	public static void teleport2nearest(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((bool)basePlayer && basePlayer.IsAlive())
		{
			string @string = args.GetString(0);
			int @int = args.GetInt(1);
			basePlayer.TeleportToNearestTargetEntity(@string, @int);
		}
	}

	[ServerVar(Help = "Teleport to the entity with the specified network ID")]
	public static void teleport2entityid(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((bool)basePlayer && basePlayer.IsAlive())
		{
			NetworkableId entityID = ArgEx.GetEntityID(args, 0);
			BaseNetworkable baseNetworkable = BaseNetworkable.serverEntities.Find(entityID);
			if (baseNetworkable == null)
			{
				args.ReplyWith($"No entity found with id {entityID}");
				return;
			}
			args.ReplyWith($"Teleporting to {baseNetworkable.ShortPrefabName} at {baseNetworkable.transform.position}");
			basePlayer.Teleport(baseNetworkable.transform.position);
		}
	}

	[ServerVar(Help = "(Generated) Teleports the calling admin or a named player to exact world coordinates specified as X Y Z arguments")]
	public static void teleportpos(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((bool)basePlayer && basePlayer.IsAlive())
		{
			bool num = args.HasArg(TopOfBaseFlag);
			bool flag = args.HasArg(UndergroundFlag);
			StringView str = args.FullString.Replace(", ", ",").Replace(TopOfBaseFlag, "").Replace(UndergroundFlag, "")
				.Trim('"');
			if (num)
			{
				TeleportToTopOfBase(basePlayer, str.ToVector3());
			}
			else if (flag)
			{
				TeleportToUnderground(basePlayer, str.ToVector3());
			}
			else
			{
				basePlayer.Teleport(str.ToVector3());
			}
		}
	}

	[ServerVar(Help = "(Generated) Teleports the calling admin to the point in the world that their line of sight is currently hitting")]
	public static void teleportlos(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((bool)basePlayer && basePlayer.IsAlive())
		{
			Ray ray = basePlayer.eyes.HeadRay();
			int @int = args.GetInt(0, 1000);
			if (UnityEngine.Physics.Raycast(ray, out var hitInfo, @int, 1218652417))
			{
				basePlayer.Teleport(hitInfo.point);
			}
			else
			{
				basePlayer.Teleport(ray.origin + ray.direction * @int);
			}
		}
	}

	[ServerVar(Help = "(Generated) Teleports the calling admin to an entity owned by a specified player, identified by Steam ID or name")]
	public static void teleport2owneditem(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		BasePlayer playerOrSleeper = ArgEx.GetPlayerOrSleeper(arg, 0);
		ulong result;
		if (playerOrSleeper != null)
		{
			result = playerOrSleeper.userID;
		}
		else if (!ulong.TryParse(arg.GetString(0), out result))
		{
			arg.ReplyWith("No player with that id found");
			return;
		}
		string @string = arg.GetString(1);
		BaseEntity[] array = BaseEntity.Util.FindTargetsOwnedBy(result, @string);
		if (array.Length == 0)
		{
			arg.ReplyWith("No targets found");
			return;
		}
		int num = UnityEngine.Random.Range(0, array.Length);
		arg.ReplyWith($"Teleporting to {array[num].ShortPrefabName} at {array[num].transform.position}");
		basePlayer.Teleport(array[num].transform.position);
	}

	[ServerVar(Help = "<steamID/name> <optional: filter> - Teleport to a random entity the player is authed on")]
	public static void teleport2autheditem(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		BasePlayer playerOrSleeper = ArgEx.GetPlayerOrSleeper(arg, 0);
		ulong result;
		if (playerOrSleeper != null)
		{
			result = playerOrSleeper.userID;
		}
		else if (!ulong.TryParse(arg.GetString(0), out result))
		{
			arg.ReplyWith("No player with that id found");
			return;
		}
		string @string = arg.GetString(1);
		BaseEntity[] array = BaseEntity.Util.FindTargetsAuthedTo(result, @string);
		if (array.Length == 0)
		{
			arg.ReplyWith("No targets found");
			return;
		}
		int num = UnityEngine.Random.Range(0, array.Length);
		arg.ReplyWith($"Teleporting to {array[num].ShortPrefabName} at {array[num].transform.position}");
		basePlayer.Teleport(array[num].transform.position);
	}

	[ServerVar(Help = "(Generated) Teleports the calling admin to the map marker they have placed on their in-game map")]
	public static void teleport2marker(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null)
		{
			arg.ReplyWith("Must be called from a player");
			return;
		}
		if (basePlayer.State.pointsOfInterest == null || basePlayer.State.pointsOfInterest.Count == 0)
		{
			arg.ReplyWith("You don't have a marker set");
			return;
		}
		string @string = arg.GetString(0);
		if (arg.HasArgs() && @string != "True")
		{
			int num = arg.GetInt(0);
			if (num == -1)
			{
				num = basePlayer.State.pointsOfInterest.Count - 1;
			}
			if (num >= 0 && num < basePlayer.State.pointsOfInterest.Count)
			{
				TeleportToMarker(basePlayer.State.pointsOfInterest[num], basePlayer);
				return;
			}
		}
		if (!string.IsNullOrEmpty(@string))
		{
			foreach (MapNote item in basePlayer.State.pointsOfInterest)
			{
				if (!string.IsNullOrEmpty(item.label) && string.Equals(item.label, @string, StringComparison.InvariantCultureIgnoreCase))
				{
					TeleportToMarker(item, basePlayer);
					return;
				}
			}
		}
		int debugMapMarkerIndex = basePlayer.DebugMapMarkerIndex;
		debugMapMarkerIndex++;
		if (debugMapMarkerIndex >= basePlayer.State.pointsOfInterest.Count)
		{
			debugMapMarkerIndex = 0;
		}
		TeleportToMarker(basePlayer.State.pointsOfInterest[debugMapMarkerIndex], basePlayer);
		basePlayer.DebugMapMarkerIndex = debugMapMarkerIndex;
	}

	private static void TeleportToMarker(MapNote marker, BasePlayer player)
	{
		TeleportToTopOfBase(player, marker.worldPosition);
	}

	private static void TeleportToTopOfBase(BasePlayer player, Vector3 position)
	{
		position.y = WaterLevel.GetWaterOrTerrainSurface(position, waves: true, volumes: true);
		if (UnityEngine.Physics.Raycast(new Ray(position + Vector3.up * 100f, Vector3.down), out var hitInfo, 110f, 1218652417))
		{
			position.y = hitInfo.point.y + 0.5f;
		}
		player.Teleport(position);
	}

	private static void TeleportToUnderground(BasePlayer player, Vector3 position)
	{
		bool flag = false;
		position.y = WaterLevel.GetWaterOrTerrainSurface(position, waves: true, volumes: true) - 10f;
		BufferList<RaycastHit> obj = Facepunch.Pool.Get<BufferList<RaycastHit>>();
		obj.Resize(10);
		int num = UnityEngine.Physics.RaycastNonAlloc(new Ray(position, Vector3.down), obj.Buffer, 200f, 1210263809);
		float num2 = 0f;
		for (int i = 0; i < num; i++)
		{
			float y = obj[i].transform.position.y;
			if (y < num2)
			{
				position.y = y + 2f;
				if (!global::AntiHack.TestInsideTerrain(position))
				{
					flag = true;
					num2 = y;
				}
			}
		}
		if (flag)
		{
			position.y = num2 + 2f;
			player.Teleport(position);
		}
		else
		{
			TeleportToTopOfBase(player, position);
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	[ServerVar(Help = "(Generated) Teleports the calling admin to the centre of the named map grid square (e.g. A1, B3)")]
	public static void teleport2grid(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!(basePlayer == null))
		{
			Vector3? vector = MapHelper.StringToPosition(arg.GetString(0));
			if (!vector.HasValue)
			{
				arg.ReplyWith("Invalid grid reference, should look like 'A1'");
			}
			else
			{
				TeleportToTopOfBase(basePlayer, vector.Value);
			}
		}
	}

	[ServerVar(Help = "(Generated) Teleports the calling admin to their own most recent death location")]
	public static void teleport2death(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null)
		{
			arg.ReplyWith("Must be called from a player");
			return;
		}
		if (basePlayer.State.deathMarker == null)
		{
			arg.ReplyWith("No death marker found");
			return;
		}
		Vector3 worldPosition = basePlayer.ServerCurrentDeathNote.worldPosition;
		basePlayer.Teleport(worldPosition);
	}

	[ServerVar(Help = "(Generated) Teleports the calling admin to the currently locked-in satellite crash site. Does nothing if no satellite is descending.")]
	public static void teleport2satellitecrashsite(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!(basePlayer == null))
		{
			SatelliteControlComputer activeDescending = SatelliteControlComputer.ActiveDescending;
			if (activeDescending == null || activeDescending.IsDestroyed)
			{
				arg.ReplyWith("No locked-in satellite crash site");
			}
			else
			{
				TeleportToTopOfBase(basePlayer, activeDescending.LockedCrashPosition);
			}
		}
	}

	[ServerVar(Help = "(Generated) Teleports the calling admin to the target location of their currently active mission objective")]
	public static void teleport2mission(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null || !basePlayer.TryGetActiveMissionInstance(out var instance))
		{
			return;
		}
		for (int i = 0; i < instance.objectiveStatuses.Count; i++)
		{
			BaseMission.MissionInstance.ObjectiveStatus objectiveStatus = instance.objectiveStatuses[i];
			if (objectiveStatus.started && !objectiveStatus.completed && !objectiveStatus.failed && !(objectiveStatus.worldLocation == default(Vector3)))
			{
				TeleportToTopOfBase(basePlayer, objectiveStatus.worldLocation);
				break;
			}
		}
	}

	private static PlayerBoat GetPlayerBoat(BasePlayer player)
	{
		return PlayerBoat.GetParentPlayerBoat(player);
	}

	private static bool TeleportBoatToWater(PlayerBoat boat, Vector3 position)
	{
		position.y = WaterLevel.GetWaterOrTerrainSurface(position, waves: true, volumes: true);
		if (!WaterLevel.Test(position, waves: true, volumes: true))
		{
			return false;
		}
		boat.Teleport(position);
		return true;
	}

	[ServerVar(Help = "(Generated) Teleports the calling admin and their player boat to the map marker they have placed on their in-game map")]
	public static void teleportboat2marker(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null)
		{
			arg.ReplyWith("Must be called from a player");
			return;
		}
		PlayerBoat playerBoat = GetPlayerBoat(basePlayer);
		if (playerBoat == null)
		{
			arg.ReplyWith("You are not on a player boat");
			return;
		}
		if (basePlayer.State.pointsOfInterest == null || basePlayer.State.pointsOfInterest.Count == 0)
		{
			arg.ReplyWith("You don't have a marker set");
			return;
		}
		string @string = arg.GetString(0);
		if (arg.HasArgs() && @string != "True")
		{
			int num = arg.GetInt(0);
			if (num == -1)
			{
				num = basePlayer.State.pointsOfInterest.Count - 1;
			}
			if (num >= 0 && num < basePlayer.State.pointsOfInterest.Count)
			{
				if (!TeleportBoatToWater(playerBoat, basePlayer.State.pointsOfInterest[num].worldPosition))
				{
					arg.ReplyWith("Target position is not in water");
				}
				return;
			}
		}
		if (!string.IsNullOrEmpty(@string))
		{
			foreach (MapNote item in basePlayer.State.pointsOfInterest)
			{
				if (!string.IsNullOrEmpty(item.label) && string.Equals(item.label, @string, StringComparison.InvariantCultureIgnoreCase))
				{
					if (!TeleportBoatToWater(playerBoat, item.worldPosition))
					{
						arg.ReplyWith("Target position is not in water");
					}
					return;
				}
			}
		}
		int debugMapMarkerIndex = basePlayer.DebugMapMarkerIndex;
		debugMapMarkerIndex++;
		if (debugMapMarkerIndex >= basePlayer.State.pointsOfInterest.Count)
		{
			debugMapMarkerIndex = 0;
		}
		if (!TeleportBoatToWater(playerBoat, basePlayer.State.pointsOfInterest[debugMapMarkerIndex].worldPosition))
		{
			arg.ReplyWith("Target position is not in water");
		}
		basePlayer.DebugMapMarkerIndex = debugMapMarkerIndex;
	}

	[ServerVar(Help = "(Generated) Teleports the calling admin and their player boat to exact world coordinates specified as X Y Z arguments")]
	public static void teleportboatpos(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if (!basePlayer || !basePlayer.IsAlive())
		{
			return;
		}
		PlayerBoat playerBoat = GetPlayerBoat(basePlayer);
		if (playerBoat == null)
		{
			args.ReplyWith("You are not on a player boat");
			return;
		}
		string str = args.FullString.Replace(", ", ",").Trim('"').ToString();
		if (!TeleportBoatToWater(playerBoat, str.ToVector3()))
		{
			args.ReplyWith("Target position is not in water");
		}
	}

	[ServerVar(Help = "(Generated) Teleports the calling admin and their player boat to the centre of the named map grid square (e.g. A1, B3)")]
	public static void teleportboat2grid(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null)
		{
			return;
		}
		PlayerBoat playerBoat = GetPlayerBoat(basePlayer);
		if (playerBoat == null)
		{
			arg.ReplyWith("You are not on a player boat");
			return;
		}
		Vector3? vector = MapHelper.StringToPosition(arg.GetString(0));
		if (!vector.HasValue)
		{
			arg.ReplyWith("Invalid grid reference, should look like 'A1'");
		}
		else if (!TeleportBoatToWater(playerBoat, vector.Value))
		{
			arg.ReplyWith("Target position is not in water");
		}
	}

	[ClientVar(Help = "(Generated) Clears prefab pools and releases pooled objects; delegates to pool.clear_prefabs; admin/developer only")]
	[ServerVar(Help = "(Generated) Clears prefab pools and releases pooled objects; delegates to pool.clear_prefabs; admin/developer only")]
	public static void free(Arg args)
	{
		Pool.clear_prefabs(args);
		Pool.clear_assets(args);
		Pool.clear_memory(args);
		GC.collect();
		GC.unload();
	}

	[ServerVar(ServerUser = true, Help = "(Generated) Prints the current game version string to the console, including build number and branch")]
	[ClientVar(Help = "(Generated) Prints the current game version string to the console, including build number and branch")]
	public static void version(Arg arg)
	{
		arg.ReplyWith($"Protocol: {Protocol.printable}\nBuild Date: {BuildInfo.Current.BuildDate}\nUnity Version: {UnityEngine.Application.unityVersion}\nChangeset: {BuildInfo.Current.Scm.ChangeId}\nBranch: {BuildInfo.Current.Scm.Branch}");
	}

	[ServerVar(Help = "(Generated) Prints a summary of the current machine hardware and OS info including CPU, GPU, RAM, and platform")]
	[ClientVar(Help = "(Generated) Prints a summary of the current machine hardware and OS info including CPU, GPU, RAM, and platform")]
	public static void sysinfo(Arg arg)
	{
		arg.ReplyWith(SystemInfoGeneralText.currentInfo);
	}

	[ClientVar(Help = "(Generated) Prints the unique device identifier for the current machine as reported by Unity SystemInfo.deviceUniqueIdentifier")]
	[ServerVar(Help = "(Generated) Prints the unique device identifier for the current machine as reported by Unity SystemInfo.deviceUniqueIdentifier")]
	public static void sysuid(Arg arg)
	{
		arg.ReplyWith(SystemInfo.deviceUniqueIdentifier);
	}

	[ServerVar(Help = "(Generated) Reduces the condition of all items in the calling player inventory whose short name matches the given string to zero, breaking them")]
	public static void breakitem(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((bool)basePlayer)
		{
			Item activeItem = basePlayer.GetActiveItem();
			activeItem?.LoseCondition(activeItem.condition);
		}
	}

	[ServerVar(Help = "(Generated) Breaks all equipped clothing items currently worn by the calling player, reducing their condition to zero")]
	public static void breakclothing(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if (!basePlayer)
		{
			return;
		}
		foreach (Item item in basePlayer.inventory.containerWear.itemList)
		{
			item?.LoseCondition(item.condition);
		}
	}

	[ServerVar(Help = "(Generated) Prints a table of active network group subscriptions for the calling player, showing realm and group ID; supports --json flag")]
	[ClientVar(Help = "(Generated) Prints a table of active network group subscriptions for the calling player, showing realm and group ID; supports --json flag")]
	public static void subscriptions(Arg arg)
	{
		bool flag = arg.HasArg("--json");
		using TextTable textTable = Facepunch.Pool.Get<TextTable>();
		textTable.ShouldPadColumns = !flag;
		textTable.AddColumn("realm");
		textTable.AddColumn("group");
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((bool)basePlayer)
		{
			foreach (Group item in basePlayer.net.subscriber.subscribed)
			{
				textTable.AddRow("sv", item.ID.ToString());
			}
		}
		arg.ReplyWith(flag ? textTable.ToJson() : textTable.ToString());
	}

	public static uint GingerbreadMaterialID()
	{
		if (_gingerbreadMaterialID == 0)
		{
			_gingerbreadMaterialID = StringPool.Get("Gingerbread");
		}
		return _gingerbreadMaterialID;
	}

	[ServerVar(Help = "(Generated) Removes all spray paint entities from the server world; useful for cleaning up excessive player spray art")]
	public static void ClearAllSprays()
	{
		List<SprayCanSpray> obj = Facepunch.Pool.Get<List<SprayCanSpray>>();
		foreach (SprayCanSpray allSpray in SprayCanSpray.AllSprays)
		{
			obj.Add(allSpray);
		}
		foreach (SprayCanSpray item in obj)
		{
			item.Kill();
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	[ServerVar(Help = "(Generated) Removes all spray paint entities created by a specific player, identified by Steam ID or name")]
	public static void ClearAllSpraysByPlayer(Arg arg)
	{
		if (!arg.HasArgs())
		{
			return;
		}
		ulong uLong = arg.GetULong(0, 0uL);
		List<SprayCanSpray> obj = Facepunch.Pool.Get<List<SprayCanSpray>>();
		foreach (SprayCanSpray allSpray in SprayCanSpray.AllSprays)
		{
			if (allSpray.sprayedByPlayer == uLong)
			{
				obj.Add(allSpray);
			}
		}
		foreach (SprayCanSpray item in obj)
		{
			item.Kill();
		}
		int count = obj.Count;
		Facepunch.Pool.FreeUnmanaged(ref obj);
		arg.ReplyWith($"Deleted {count} sprays by {uLong}");
	}

	[ServerVar(Help = "(Generated) Removes all spray paint entities within the given radius of the calling admin current position")]
	public static void ClearSpraysInRadius(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!(basePlayer == null))
		{
			float @float = arg.GetFloat(0, 16f);
			int num = ClearSpraysInRadius(basePlayer.transform.position, @float);
			arg.ReplyWith($"Deleted {num} sprays within {@float} of {basePlayer.displayName}");
		}
	}

	private static int ClearSpraysInRadius(Vector3 position, float radius)
	{
		List<SprayCanSpray> obj = Facepunch.Pool.Get<List<SprayCanSpray>>();
		foreach (SprayCanSpray allSpray in SprayCanSpray.AllSprays)
		{
			if (allSpray.Distance(position) <= radius)
			{
				obj.Add(allSpray);
			}
		}
		foreach (SprayCanSpray item in obj)
		{
			item.Kill();
		}
		int count = obj.Count;
		Facepunch.Pool.FreeUnmanaged(ref obj);
		return count;
	}

	[ServerVar(Help = "(Generated) Removes all spray paint entities within a given radius of the specified world position (X Y Z)")]
	public static void ClearSpraysAtPositionInRadius(Arg arg)
	{
		Vector3 vector = arg.GetVector3(0);
		float @float = arg.GetFloat(1);
		if (@float != 0f)
		{
			int num = ClearSpraysInRadius(vector, @float);
			arg.ReplyWith($"Deleted {num} sprays within {@float} of {vector}");
		}
	}

	[ServerVar(Help = "(Generated) Removes all dropped item entities from the server, cleaning up every piece of loot on the ground")]
	public static void ClearDroppedItems()
	{
		List<DroppedItem> obj = Facepunch.Pool.Get<List<DroppedItem>>();
		foreach (BaseNetworkable serverEntity in BaseNetworkable.serverEntities)
		{
			if (serverEntity is DroppedItem item)
			{
				obj.Add(item);
			}
		}
		foreach (DroppedItem item2 in obj)
		{
			item2.Kill();
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	[ClientVar(Help = "(Generated) Prints all scenes registered in the build settings with their build index and asset path")]
	[ServerVar(Help = "(Generated) Prints all scenes registered in the build settings with their build index and asset path")]
	public static string printAllScenesInBuild(Arg args)
	{
		StringBuilder stringBuilder = new StringBuilder();
		int sceneCountInBuildSettings = SceneManager.sceneCountInBuildSettings;
		stringBuilder.AppendLine($"Scenes: {sceneCountInBuildSettings}");
		for (int i = 0; i < sceneCountInBuildSettings; i++)
		{
			stringBuilder.AppendLine(SceneUtility.GetScenePathByBuildIndex(i));
		}
		return stringBuilder.ToString();
	}

	[ServerVar(Clientside = true, Help = "Immediately update the manifest")]
	public static void UpdateManifest(Arg args)
	{
		Facepunch.Manifest.UpdateManifest();
	}
}
