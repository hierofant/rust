using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Facepunch;
using Facepunch.Extend;
using UnityEngine;

namespace ConVar;

[Factory("clan")]
public class Clan : ConsoleSystem
{
	[ReplicatedVar(Help = "If enabled then players will need to be near a Clan Table to make changes to clans", Default = "true")]
	public static bool editsRequireClanTable = true;

	[ServerVar(Help = "Enables the clan system if set to true (must be set at boot, requires restart)")]
	public static bool enabled = true;

	[ServerVar(Help = "Maximum number of members each clan can have (local backend only!)")]
	public static int maxMemberCount = 100;

	public static int scoreForKillingPlayerInOtherClan = 0;

	public static int scoreForKilledByPlayerInOtherClan = 0;

	public static int scoreForKillingUnarmedPlayer = 0;

	public static int scoreForDestroyingToolCupboards = 0;

	[ServerVar(Help = "How much score players earn for hacking crates")]
	public static int scoreForHackingCrates = 5;

	[ServerVar(Help = "How much score players earn for opening hacked crates")]
	public static int scoreForOpeningHackedCrates = 10;

	[ServerVar(Help = "How much score players earn for destroying bradley")]
	public static int scoreForDestroyingBradley = 50;

	[ServerVar(Help = "How much score players earn for running the excavator, per diesel fuel consumed")]
	public static int scoreForRunningExcavator = 3;

	[ServerVar(Help = "How much score players earn for reaching cargo ship")]
	public static int scoreForReachingCargoShip = 10;

	[ServerVar(Help = "How much score players earn for looting an elite crate")]
	public static int scoreForLootingEliteCrate = 5;

	[ServerVar(Help = "How much score players earn for destroying patrol heli")]
	public static int scoreForDestroyingPatrolHeli = 50;

	[ServerVar(Help = "How much score players earn for swiping a red keycard")]
	public static int scoreForSwipingRedKeycard = 5;

	[ServerVar(Help = "How much score players earn for inserting a heavy fuse into powerplant")]
	public static int scoreForInsertHeavyFuseInPowerPlant = 5;

	[ServerVar(Help = "How much score players earn for looting a crashed satellite's crates")]
	public static int scoreForLootingSatellite = 10;

	[ServerVar(Help = "How much score players earn for running the water treatment plant, per consumed item")]
	public static int scoreForEnablingWaterTreatmentPlant = 3;

	[ServerVar(Help = "How much score players earn for launching a satellite")]
	public static int scoreForLaunchingSatellite = 25;

	[ServerVar(Help = "How much score players earn for starting the oil rig fuel switch")]
	public static int scoreForStartingOilRigFuelSwitch = 5;

	[ServerVar(Help = "Prints info about a clan given its ID or a steamID of a player in that clan")]
	public static void Info(Arg arg)
	{
		if (ClanManager.ServerInstance == null)
		{
			arg.ReplyWith("ClanManager is null!");
			return;
		}
		long @long = arg.GetLong(0, 0L);
		if (@long == 0L || @long > 76500000000000000L)
		{
			BasePlayer basePlayer = ArgEx.Player(arg);
			ulong num = (ulong)@long;
			if (num == 0L)
			{
				if (basePlayer == null)
				{
					arg.ReplyWith("Usage: clan.info <clanID/steamID>");
					return;
				}
				num = basePlayer.userID;
			}
			SendClanInfoPlayer(num, basePlayer);
		}
		else
		{
			SendClanInfoConsole(@long);
		}
		static string FormatClan(IClan clan)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine($"Clan ID: {clan.ClanId}");
			stringBuilder.AppendLine("Name: " + clan.Name);
			stringBuilder.AppendLine("MoTD: " + clan.Motd);
			stringBuilder.AppendLine("Members:");
			using TextTable textTable = Facepunch.Pool.Get<TextTable>();
			textTable.AddColumns("steamID", "username", "online", "role");
			foreach (ClanMember member in clan.Members)
			{
				ClanRole? clanRole = clan.Roles.TryFindWith((ClanRole r) => r.RoleId, member.RoleId);
				string text = SingletonComponent<ServerMgr>.Instance.persistance.GetPlayerName(member.SteamId) ?? "[unknown]";
				bool flag = (NexusServer.Started ? NexusServer.IsOnline(member.SteamId) : ServerPlayers.IsOnline(member.SteamId));
				string[] array = new string[4];
				ulong steamId2 = member.SteamId;
				array[0] = steamId2.ToString();
				array[1] = text;
				array[2] = (flag ? "x" : "");
				array[3] = clanRole?.Name ?? "[null]";
				textTable.AddRow(array);
			}
			stringBuilder.Append(textTable);
			return stringBuilder.ToString();
		}
		static async void SendClanInfoConsole(long id)
		{
			try
			{
				IClan clan2 = await GetClanByID(id);
				if (clan2 != null)
				{
					Debug.Log(FormatClan(clan2));
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		static async void SendClanInfoPlayer(ulong steamId, BasePlayer player)
		{
			try
			{
				IClan clan3 = await GetPlayerClan(steamId, player);
				if (clan3 != null)
				{
					string msg = FormatClan(clan3);
					player.ConsoleMessage(msg);
				}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
				player.ConsoleMessage(ex.ToString());
			}
		}
	}

	private static async ValueTask<IClan> GetPlayerClan(ulong playerId, BasePlayer forPlayer = null)
	{
		ClanValueResult<IClan> clanValueResult = await ClanManager.ServerInstance.Backend.GetByMember(playerId);
		if (!clanValueResult.IsSuccess)
		{
			string text = ((clanValueResult.Result == ClanResult.NoClan) ? "Player is not in a clan!" : $"Failed to find player's clan ({clanValueResult.Result})!");
			if (forPlayer != null)
			{
				forPlayer.ConsoleMessage(text);
			}
			else
			{
				Debug.Log(text);
			}
			return null;
		}
		return clanValueResult.Value;
	}

	private static async ValueTask<IClan> GetClanByID(long clanId, BasePlayer forPlayer = null)
	{
		ClanValueResult<IClan> clanValueResult = await ClanManager.ServerInstance.Backend.Get(clanId);
		if (!clanValueResult.IsSuccess)
		{
			string text = ((clanValueResult.Result == ClanResult.NotFound) ? $"Clan with ID {clanId} was not found!" : $"Failed to get the clan with ID {clanId} ({clanValueResult.Result})!");
			if (forPlayer != null)
			{
				forPlayer.ConsoleMessage(text);
			}
			else
			{
				Debug.Log(text);
			}
			return null;
		}
		return clanValueResult.Value;
	}

	public static int GetScoreForEvent(ClanScoreEventType eventType)
	{
		return eventType switch
		{
			ClanScoreEventType.Generic => 1, 
			ClanScoreEventType.ClanPlayerKilled => scoreForKillingPlayerInOtherClan, 
			ClanScoreEventType.ClanPlayerDied => scoreForKilledByPlayerInOtherClan, 
			ClanScoreEventType.UnarmedPlayerKilled => scoreForKillingUnarmedPlayer, 
			ClanScoreEventType.DestroyedToolCupboard => scoreForDestroyingToolCupboards, 
			ClanScoreEventType.HackedCrate => scoreForHackingCrates, 
			ClanScoreEventType.OpenedHackedCrate => scoreForOpeningHackedCrates, 
			ClanScoreEventType.DestroyedBradley => scoreForDestroyingBradley, 
			ClanScoreEventType.RanExcavator => scoreForRunningExcavator, 
			ClanScoreEventType.ReachedCargoShip => scoreForReachingCargoShip, 
			ClanScoreEventType.LootedEliteCrate => scoreForLootingEliteCrate, 
			ClanScoreEventType.DestroyedPatrolHeli => scoreForDestroyingPatrolHeli, 
			ClanScoreEventType.SwipedRedKeycard => scoreForSwipingRedKeycard, 
			ClanScoreEventType.InsertHeavyFuseInPowerPlant => scoreForInsertHeavyFuseInPowerPlant, 
			ClanScoreEventType.LootSatellite => scoreForLootingSatellite, 
			ClanScoreEventType.EnableWaterTreatmentPlant => scoreForEnablingWaterTreatmentPlant, 
			ClanScoreEventType.LaunchSatellite => scoreForLaunchingSatellite, 
			ClanScoreEventType.StartedOilRigFuelSwitch => scoreForStartingOilRigFuelSwitch, 
			ClanScoreEventType.Invalid => 0, 
			_ => Unknown(eventType), 
		};
		static int Unknown(ClanScoreEventType type)
		{
			Debug.LogError($"Unhandled score event type: {type}");
			return 0;
		}
	}

	[ServerVar(Help = "Disbands your current clan")]
	public static void Disband(Arg arg)
	{
		BasePlayer player = ArgEx.Player(arg);
		if (player == null)
		{
			arg.ReplyWith("Can only be used by a player!");
		}
		else if (ClanManager.ServerInstance == null)
		{
			arg.ReplyWith("ClanManager is null!");
		}
		else
		{
			DisbandImpl();
		}
		async void DisbandImpl()
		{
			_ = 1;
			try
			{
				IClan clan = await GetPlayerClan(player.userID, player);
				if (clan != null)
				{
					ClanResult clanResult = await clan.Disband(player.userID);
					if (clanResult != ClanResult.Success)
					{
						player.ConsoleMessage($"Failed to disband clan: {clanResult}");
					}
					else
					{
						player.ConsoleMessage($"Disbanded clan ID {clan.ClanId}");
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
				player.ConsoleMessage(ex.ToString());
			}
		}
	}

	[ServerVar(Help = "Adds a player by SteamID64 to your current clan")]
	public static void AddToClan(Arg arg)
	{
		ulong steamId = arg.GetUInt64(0, 0uL);
		if (steamId == 0L)
		{
			arg.ReplyWith("Usage: clan.addtoclan <steamID>");
			return;
		}
		BasePlayer player = ArgEx.Player(arg);
		if (player == null)
		{
			arg.ReplyWith("Can only be used by a player!");
		}
		else if (ClanManager.ServerInstance == null)
		{
			arg.ReplyWith("ClanManager is null!");
		}
		else
		{
			AddToClanImpl();
		}
		async void AddToClanImpl()
		{
			_ = 2;
			try
			{
				IClan clan = await GetPlayerClan(player.userID, player);
				if (clan != null)
				{
					if (clan.Invites.All((ClanInvite i) => i.SteamId != steamId))
					{
						ClanResult clanResult = await clan.Invite(steamId, player.userID);
						if (clanResult != ClanResult.Success)
						{
							player.ConsoleMessage($"Failed to invite {steamId} to your clan: {clanResult}");
							return;
						}
					}
					ClanResult clanResult2 = await clan.AcceptInvite(steamId);
					if (clanResult2 != ClanResult.Success)
					{
						player.ConsoleMessage($"Failed to accept invite for {steamId} to join your clan: {clanResult2}");
					}
					else
					{
						player.ConsoleMessage($"Added {steamId} to your clan.");
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
				player.ConsoleMessage(ex.ToString());
			}
		}
	}

	[ServerVar(Help = "Adds a generic score event to your clan")]
	public static void ScoreTest(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null)
		{
			arg.ReplyWith("Can only be used by a player!");
		}
		else if (ClanManager.ServerInstance == null)
		{
			arg.ReplyWith("CLanManager is null!");
		}
		else if (basePlayer.serverClan == null)
		{
			arg.ReplyWith("Player's clan is null!");
		}
		else
		{
			basePlayer.AddClanScore(ClanScoreEventType.Generic);
		}
	}
}
