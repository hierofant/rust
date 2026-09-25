using System.Collections.Generic;
using System.Linq;
using Facepunch.Math;
using UnityEngine;

namespace ConVar;

[Factory("decay")]
public class Decay : ConsoleSystem
{
	[ReplicatedVar(Help = "Can players upgrade building blocks to wood")]
	public static bool upgrade_wood_enabled = true;

	[ReplicatedVar(Help = "Can players upgrade building blocks to stone")]
	public static bool upgrade_stone_enabled = true;

	[ReplicatedVar(Help = "Can players upgrade building blocks to metal")]
	public static bool upgrade_metal_enabled = true;

	[ReplicatedVar(Help = "Can players upgrade building blocks to hqm")]
	public static bool upgrade_hqm_enabled = true;

	[ReplicatedVar(Help = "Can players upgrade building blocks to wood")]
	public static float build_twig_cost_multiplier = 1f;

	[ReplicatedVar(Help = "Can players upgrade building blocks to wood")]
	public static float upgrade_wood_cost_multiplier = 1f;

	[ReplicatedVar(Help = "Can players upgrade building blocks to stone")]
	public static float upgrade_stone_cost_multiplier = 1f;

	[ReplicatedVar(Help = "Can players upgrade building blocks to metal")]
	public static float upgrade_metal_cost_multiplier = 1f;

	[ReplicatedVar(Help = "Can players upgrade building blocks to hqm")]
	public static float upgrade_hqm_cost_multiplier = 1f;

	[ServerVar(Help = "Maximum distance to test to see if a structure is outside, higher values are slower but accurate for huge buildings")]
	public static float outside_test_range = 50f;

	[ServerVar(Help = "(Generated) Interval in seconds between decay processing ticks; default is 10 minutes; lower values cause buildings to lose health more frequently")]
	public static float tick = 600f;

	[ServerVar(Help = "(Generated) Multiplier applied to all decay damage per tick; 1.0 = normal, 2.0 = double decay rate, 0.0 = no decay")]
	public static float scale = 1f;

	[ServerVar(Help = "(Generated) When enabled, logs decay tick details to the console including which entities took damage and how much")]
	public static bool debug = false;

	[ServerVar(Help = "Is upkeep enabled")]
	public static bool upkeep = true;

	[ServerVar(Help = "How many minutes does the upkeep cost last? default : 1440 (24 hours)")]
	public static float upkeep_period_minutes = 1440f;

	[ServerVar(Help = "How many minutes can the upkeep cost last after the cupboard was destroyed? default : 1440 (24 hours)")]
	public static float upkeep_grief_protection = 1440f;

	[ServerVar(Help = "Scale at which objects heal when upkeep conditions are met, default of 1 is same rate at which they decay")]
	public static float upkeep_heal_scale = 1f;

	[ServerVar(Help = "Scale at which objects decay when they are inside, default of 0.1")]
	public static float upkeep_inside_decay_scale = 0.1f;

	[ServerVar(Help = "When set to a value above 0 everything will decay with this delay")]
	public static float delay_override = 0f;

	[ServerVar(Help = "How long should this building grade decay be delayed when not protected by upkeep, in hours")]
	public static float delay_twig = 0f;

	[ServerVar(Help = "How long should this building grade decay be delayed when not protected by upkeep, in hours")]
	public static float delay_wood = 0f;

	[ServerVar(Help = "How long should this building grade decay be delayed when not protected by upkeep, in hours")]
	public static float delay_stone = 0f;

	[ServerVar(Help = "How long should this building grade decay be delayed when not protected by upkeep, in hours")]
	public static float delay_metal = 0f;

	[ServerVar(Help = "How long should this building grade decay be delayed when not protected by upkeep, in hours")]
	public static float delay_toptier = 0f;

	[ServerVar(Help = "When set to a value above 0 everything will decay with this duration")]
	public static float duration_override = 0f;

	[ServerVar(Help = "How long should this building grade take to decay when not protected by upkeep, in hours")]
	public static float duration_twig = 1f;

	[ServerVar(Help = "How long should this building grade take to decay when not protected by upkeep, in hours")]
	public static float duration_wood = 3f;

	[ServerVar(Help = "How long should this building grade take to decay when not protected by upkeep, in hours")]
	public static float duration_stone = 5f;

	[ServerVar(Help = "How long should this building grade take to decay when not protected by upkeep, in hours")]
	public static float duration_metal = 8f;

	[ServerVar(Help = "How long should this building grade take to decay when not protected by upkeep, in hours")]
	public static float duration_toptier = 12f;

	public const int UpkeepBracketCount = 4;

	[ServerVar(Help = "Number of blocks in the 1st upkeep bracket")]
	public static int bracket_0_blockcount = 15;

	[ServerVar(Help = "Blocks in the 1st upkeep bracket will cost this value per day to maintain")]
	public static float bracket_0_costfraction = 0.1f;

	[ServerVar(Help = "Number of blocks in the 2nd upkeep bracket")]
	public static int bracket_1_blockcount = 50;

	[ServerVar(Help = "Blocks in the 2nd upkeep bracket will cost this value per day to maintain")]
	public static float bracket_1_costfraction = 0.15f;

	[ServerVar(Help = "The number of blocks in the 3rd upkeep bracket")]
	public static int bracket_2_blockcount = 125;

	[ServerVar(Help = "Blocks in the 3rd upkeep bracket will cost this value per day to maintain")]
	public static float bracket_2_costfraction = 0.2f;

	[ServerVar(Help = "Blocks in the 4th upkeep bracket will cost this value per day to maintain")]
	public static float bracket_3_costfraction = 0.333f;

	[ServerVar(Help = "Should doors have their own upkeep brackets separate from building blocks")]
	public static bool use_door_upkeep_brackets = false;

	[ServerVar(Help = "Number of doors in the 1st upkeep bracket")]
	public static int bracket_0_doorcount = 15;

	[ServerVar(Help = "Doors in the 1st upkeep bracket will cost this value per day to maintain")]
	public static float bracket_0_doorfraction = 0.1f;

	[ServerVar(Help = "Number of doors in the 2nd upkeep bracket")]
	public static int bracket_1_doorcount = 50;

	[ServerVar(Help = "Doors in the 2nd upkeep bracket will cost this value per day to maintain")]
	public static float bracket_1_doorfraction = 0.15f;

	[ServerVar(Help = "The number of doors in the 3rd upkeep bracket")]
	public static int bracket_2_doorcount = 125;

	[ServerVar(Help = "Doors in the 3rd upkeep bracket will cost this value per day to maintain")]
	public static float bracket_2_doorfraction = 0.2f;

	[ServerVar(Help = "Doors in the 4th upkeep bracket will cost this value per day to maintain")]
	public static float bracket_3_doorfraction = 0.333f;

	public const int UpkeepGroupTierCount = 3;

	[ServerVar(Help = "Should upkeep cost scale with the number of players authed on the tool cupboard")]
	public static bool upkeep_group_scaling = true;

	[ServerVar(Help = "Players who authed within this many hours still count towards the group size, even if they have since deauthed")]
	public static float upkeep_group_window_hours = 24f;

	[ServerVar(Help = "Number of players in the 1st (free) upkeep group tier")]
	public static int upkeep_group_tier_0_playercount = 4;

	[ServerVar(Help = "Each player in the 1st upkeep group tier increases upkeep cost by this fraction")]
	public static float upkeep_group_tier_0_increase = 0f;

	[ServerVar(Help = "Number of players in the 2nd (small group) upkeep group tier")]
	public static int upkeep_group_tier_1_playercount = 6;

	[ServerVar(Help = "Each player in the 2nd upkeep group tier increases upkeep cost by this fraction")]
	public static float upkeep_group_tier_1_increase = 0.02f;

	[ServerVar(Help = "Each player in the 3rd (large group) upkeep group tier increases upkeep cost by this fraction, this tier is unlimited")]
	public static float upkeep_group_tier_2_increase = 0.04f;

	[ServerVar(Help = "Upper limit on the group size upkeep multiplier")]
	public static float upkeep_group_max_multiplier = 3f;

	[ServerVar(Help = "Should players holding a code on one of the building's doors count towards the group size, whether it is the master code or the guest code")]
	public static bool upkeep_group_count_locks = true;

	[ServerVar(Help = "Maximum number of players a tool cupboard remembers in the group window, oldest are dropped first")]
	public static int upkeep_group_history_max = 256;

	[ServerVar(Help = "If a code lock has <= this number of users it won't count its users towards group upkeep tax")]
	public static int upkeep_lock_min_users = 2;

	[ReplicatedVar(Help = "Upkeep scale for external walls and gates")]
	public static float high_wall_upkeep = 0.2f;

	[ServerVar(Help = "drawnearbybuildings <duration> <radius>, shows building ID of entities")]
	public static void drawnearbybuildings(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null)
		{
			arg.ReplyWith("This command can only be run by a player");
			return;
		}
		arg.GetFloat(0, 15f);
		float @float = arg.GetFloat(1, 30f);
		List<DecayEntity> list = new List<DecayEntity>();
		global::Vis.Entities(basePlayer.transform.position, @float, list);
		foreach (DecayEntity item in list)
		{
			if (item.isServer)
			{
				basePlayer.SendConsoleCommand(DDrawCommand.Text(item.transform.position, 10f, Color.red, item.buildingID.ToString()));
			}
		}
	}

	[ServerVar(Help = "Call the decay tick on every single entity on the server (for testing decay works)")]
	public static void forcedecaytick(Arg arg)
	{
		foreach (DecayEntity item in BaseNetworkable.serverEntities.OfType<DecayEntity>())
		{
			item.DecayTick(force: true);
		}
	}

	private static BuildingPrivlidge GetPrivilegeForAuthHistoryCommand(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null)
		{
			arg.ReplyWith("This command can only be run by a player");
			return null;
		}
		BuildingPrivlidge buildingPrivilege = basePlayer.GetBuildingPrivilege();
		if (buildingPrivilege == null)
		{
			arg.ReplyWith("You are not in range of a tool cupboard");
			return null;
		}
		return buildingPrivilege;
	}

	private static void ReplyWithGroupUpkeep(Arg arg, BuildingPrivlidge privilege)
	{
		arg.ReplyWith($"Group size {privilege.GetGroupAuthCount()} ({privilege.authorizedPlayers.Count} authed, {privilege.recentGroupMembers.Count} in the group window), upkeep multiplier {privilege.GetGroupUpkeepMultiplier():0.###}");
	}

	[ServerVar(Help = "addfakeauthhistory <count>, adds fake deauthed players to the auth history of the tool cupboard you are standing in, for testing group upkeep tiers")]
	public static void addfakeauthhistory(Arg arg)
	{
		BuildingPrivlidge privilegeForAuthHistoryCommand = GetPrivilegeForAuthHistoryCommand(arg);
		if (privilegeForAuthHistoryCommand == null)
		{
			return;
		}
		int @int = arg.GetInt(0, 1);
		if (@int <= 0)
		{
			arg.ReplyWith("Count must be greater than zero");
			return;
		}
		uint current = (uint)Epoch.Current;
		ulong num = 1uL;
		for (int i = 0; i < @int; i++)
		{
			for (; privilegeForAuthHistoryCommand.recentGroupMembers.ContainsKey(num) || privilegeForAuthHistoryCommand.authorizedPlayers.Contains(num); num++)
			{
			}
			privilegeForAuthHistoryCommand.recentGroupMembers[num] = current;
			num++;
		}
		privilegeForAuthHistoryCommand.RecalculateGroupUpkeep();
		privilegeForAuthHistoryCommand.MarkProtectedMinutesDirty();
		privilegeForAuthHistoryCommand.SendNetworkUpdate();
		ReplyWithGroupUpkeep(arg, privilegeForAuthHistoryCommand);
	}

	[ServerVar(Help = "Clears the deauthed player history on the tool cupboard you are standing in, dropping its group upkeep back to the number of authed players")]
	public static void clearauthhistory(Arg arg)
	{
		BuildingPrivlidge privilegeForAuthHistoryCommand = GetPrivilegeForAuthHistoryCommand(arg);
		if (!(privilegeForAuthHistoryCommand == null))
		{
			privilegeForAuthHistoryCommand.recentGroupMembers.Clear();
			privilegeForAuthHistoryCommand.RecalculateGroupUpkeep();
			privilegeForAuthHistoryCommand.MarkProtectedMinutesDirty();
			privilegeForAuthHistoryCommand.SendNetworkUpdate();
			ReplyWithGroupUpkeep(arg, privilegeForAuthHistoryCommand);
		}
	}

	public static float GetCostMultiplier(BuildingGrade.Enum grade)
	{
		return grade switch
		{
			BuildingGrade.Enum.Twigs => build_twig_cost_multiplier, 
			BuildingGrade.Enum.Wood => upgrade_wood_cost_multiplier, 
			BuildingGrade.Enum.Stone => upgrade_stone_cost_multiplier, 
			BuildingGrade.Enum.Metal => upgrade_metal_cost_multiplier, 
			BuildingGrade.Enum.TopTier => upgrade_hqm_cost_multiplier, 
			_ => 1f, 
		};
	}

	public static bool CanUpgradeToGrade(BuildingGrade.Enum grade)
	{
		return grade switch
		{
			BuildingGrade.Enum.Twigs => true, 
			BuildingGrade.Enum.Wood => upgrade_wood_enabled, 
			BuildingGrade.Enum.Stone => upgrade_stone_enabled, 
			BuildingGrade.Enum.Metal => upgrade_metal_enabled, 
			BuildingGrade.Enum.TopTier => upgrade_hqm_enabled, 
			_ => true, 
		};
	}
}
