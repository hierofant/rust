namespace ConVar;

[Factory("antihack")]
public class AntiHack : ConsoleSystem
{
	[ReplicatedVar(Default = "0.22")]
	[Help("collider margin when checking for noclipping on dismount")]
	public static float noclip_margin_dismount = 0.22f;

	[Help("collider backtracking when checking for noclipping")]
	[ReplicatedVar(Default = "0.01")]
	public static float noclip_backtracking = 0.01f;

	[Help("report violations to the anti cheat backend")]
	[ServerVar]
	public static bool reporting = false;

	[Help("are admins allowed to use their admin cheat")]
	[ServerVar]
	public static bool admincheat = true;

	[ServerVar]
	[Help("use antihack to verify object placement by players")]
	public static bool objectplacement = true;

	[ServerVar]
	[Help("use antihack to verify model state sent by players")]
	public static bool modelstate = true;

	[ServerVar]
	[Help("whether or not to force the position on the client")]
	public static bool forceposition = true;

	[Help("0 == allow RPCs from stalled players, 1 == ignore RPCs from currently stalled players, 2 == ignore RPCs from recently stalled players")]
	[ServerVar]
	public static int rpcstallmode = 1;

	[ServerVar]
	[Help("time in seconds before player is no longer treated as wasStalled")]
	public static float rpcstallfade = 2.5f;

	[Help("time in seconds we can receive no ticks for before player is considered stalling")]
	[ServerVar]
	public static float rpcstallthreshold = 0.8f;

	[Help("whether or not to restrict entity position packets to non-stalled players")]
	[ServerVar]
	public static bool stall_position_restrictions = true;

	[ServerVar]
	[Help("0 == users, 1 == admins, 2 == developers")]
	public static int userlevel = 2;

	[ServerVar]
	[Help("0 == no enforcement, 1 == kick, 2 == ban (DISABLED)")]
	public static int enforcementlevel = 1;

	[ServerVar]
	[Help("max allowed client desync, lower value = more false positives")]
	public static float maxdesync = 0.8f;

	[ServerVar]
	[Help("max allowed client tick interval delta time, lower value = more false positives")]
	public static float maxdeltatime = 1f;

	[ServerVar]
	[Help("for how many seconds to keep a tick history to use for distance checks")]
	public static float tickhistorytime = 0.5f;

	[Help("how much forgiveness to add when checking the distance from the player tick history")]
	[ServerVar]
	public static float tickhistoryforgiveness = 0.1f;

	[Help("use parent position history for mounted player projectile validation (more accurate, uses ~192 bytes per mounted player)")]
	[ServerVar]
	public static bool parenthistory = false;

	[Help("the rate at which violation values go back down")]
	[ServerVar]
	public static float relaxationrate = 0.1f;

	[ServerVar]
	[Help("the time before violation values go back down")]
	public static float relaxationpause = 10f;

	[Help("violation value above this results in enforcement")]
	[ServerVar]
	public static float maxviolation = 100f;

	[ServerVar]
	[Help("0 == disabled, 1 == enabled")]
	public static int terrain_protection = 1;

	[Help("how many slices to subdivide players into for the terrain check")]
	[ServerVar]
	public static int terrain_timeslice = 64;

	[Help("how far to penetrate the terrain before violating")]
	[ServerVar]
	public static float terrain_padding = 0.3f;

	[Help("violation penalty to hand out when terrain is detected")]
	[ServerVar]
	public static float terrain_penalty = 100f;

	[Help("whether or not to kill the player when terrain is detected")]
	[ServerVar]
	public static bool terrain_kill = true;

	[Help("whether or not to check for player inside geometry like rocks as well as base terrain")]
	[ServerVar]
	public static bool terrain_check_geometry = false;

	[ServerVar]
	[Help("0 == disabled, 1 == ray, 2 == sphere, 3 == curve")]
	public static int noclip_protection = 3;

	[ServerVar]
	[Help("whether or not to reject movement when noclip is detected")]
	public static bool noclip_reject = true;

	[ServerVar]
	[Help("violation penalty to hand out when noclip is detected")]
	public static float noclip_penalty = 0f;

	[ServerVar]
	[Help("collider margin when checking for noclipping")]
	public static float noclip_margin = 0.16f;

	[ServerVar]
	[Help("movement curve step size, lower value = less false positives")]
	public static float noclip_stepsize = 0.1f;

	[ServerVar]
	[Help("movement curve max steps, lower value = more false positives")]
	public static int noclip_maxsteps = 15;

	[ServerVar]
	[Help("0 == disabled, 1 == simple, 2 == advanced, 3 == vertical swim protection")]
	public static int speedhack_protection = 3;

	[Help("whether or not to reject movement when speedhack is detected")]
	[ServerVar]
	public static bool speedhack_reject = true;

	[ServerVar]
	[Help("violation penalty to hand out when speedhack is detected")]
	public static float speedhack_penalty = 0f;

	[ServerVar]
	[Help("speed threshold to assume speedhacking, lower value = more false positives")]
	public static float speedhack_forgiveness = 2f;

	[ServerVar]
	[Help("speed threshold to assume speedhacking, lower value = more false positives")]
	public static float speedhack_forgiveness_inertia = 10f;

	[Help("speed forgiveness when moving down slopes, lower value = more false positives")]
	[ServerVar]
	public static float speedhack_slopespeed = 10f;

	[Help("server threshold before we consider a client might be swimming in water (to account for client/server waterfactor mismatch")]
	[ServerVar]
	public static float speedhack_water_threshold = 0.4f;

	[ServerVar]
	[Help("0 == disabled, 1 == client, 2 == capsule, 3 == curve")]
	public static int flyhack_protection = 3;

	[ServerVar]
	[Help("whether or not to reject movement when flyhack is detected")]
	public static bool flyhack_reject = true;

	[Help("violation penalty to hand out when flyhack is detected")]
	[ServerVar]
	public static float flyhack_penalty = 100f;

	[Help("distance threshold to assume flyhacking, lower value = more false positives")]
	[ServerVar]
	public static float flyhack_forgiveness_vertical = 1f;

	[ServerVar]
	[Help("distance threshold to assume flyhacking, lower value = more false positives")]
	public static float flyhack_forgiveness_vertical_inertia = 7f;

	[Help("distance threshold to assume flyhacking, lower value = more false positives")]
	[ServerVar]
	public static float flyhack_forgiveness_horizontal = 1.5f;

	[Help("distance threshold to assume flyhacking, lower value = more false positives")]
	[ServerVar]
	public static float flyhack_forgiveness_horizontal_inertia = 10f;

	[Help("collider downwards extrusion when checking for flyhacking")]
	[ServerVar]
	public static float flyhack_extrusion = 2f;

	[Help("collider margin when checking for flyhacking")]
	[ServerVar]
	public static float flyhack_margin = 0.1f;

	[Help("movement curve step size, lower value = less false positives")]
	[ServerVar]
	public static float flyhack_stepsize = 0.1f;

	[ServerVar]
	[Help("movement curve max steps, lower value = more false positives")]
	public static int flyhack_maxsteps = 15;

	[ServerVar]
	[Help("0 == disabled, 1 == speed, 2 == speed + entity, 3 == speed + entity + LOS, 4 == speed + entity + LOS + trajectory, 5 == speed + entity + LOS + trajectory + update, 6 == speed + entity + LOS + trajectory + tickhistory")]
	public static int projectile_protection = 6;

	[ServerVar]
	[Help("violation penalty to hand out when projectile hack is detected")]
	public static float projectile_penalty = 0f;

	[Help("projectile speed forgiveness in percent, lower value = more false positives")]
	[ServerVar]
	public static float projectile_forgiveness = 0.5f;

	[ServerVar]
	[Help("projectile server frames to include in delay, lower value = more false positives")]
	public static float projectile_serverframes = 2f;

	[ServerVar]
	[Help("projectile client frames to include in delay, lower value = more false positives")]
	public static float projectile_clientframes = 2f;

	[ServerVar]
	[Help("projectile trajectory forgiveness, lower value = more false positives")]
	public static float projectile_trajectory = 2f;

	[Help("projectile trajectory forgiveness for projectile updates, lower value = more false positives")]
	[ServerVar]
	public static float projectile_trajectory_update = 0.02f;

	[Help("projectile penetration angle change, lower value = more false positives")]
	[ServerVar]
	public static float projectile_anglechange = 60f;

	[ServerVar]
	[Help("projectile penetration velocity change, lower value = more false positives")]
	public static float projectile_velocitychange = 1.1f;

	[ServerVar]
	[Help("projectile desync forgiveness, lower value = more false positives")]
	public static float projectile_desync = 1f;

	[ServerVar]
	[Help("projectile backtracking when checking for LOS")]
	public static float projectile_backtracking = 0.01f;

	[ServerVar]
	[Help("line of sight directional forgiveness when checking eye or center position")]
	public static float projectile_losforgiveness = 0.2f;

	[Help("how often a projectile is allowed to penetrate something before its damage is ignored")]
	[ServerVar]
	public static int projectile_damagedepth = 2;

	[Help("how often a projectile is allowed to penetrate something before its impact spawn is ignored")]
	[ServerVar]
	public static int projectile_impactspawndepth = 1;

	[Help("whether or not to include terrain in the projectile LOS checks")]
	[ServerVar]
	public static bool projectile_terraincheck = true;

	[ServerVar]
	[Help("whether or not to include vehicles in the projectile LOS checks")]
	public static bool projectile_vehiclecheck = true;

	[ServerVar]
	[Help("whether or not to include default layer in the projectile LOS checks")]
	public static bool projectile_defaultcheck = true;

	[ServerVar]
	[Help("whether or not to include deployed layer in the projectile LOS checks")]
	public static bool projectile_deployedcheck = true;

	[ServerVar]
	[Help("whether or not to compensate for the client / server vehicle position offset")]
	public static bool projectile_positionoffset = true;

	[ServerVar]
	[Help("minimum distance before we verify client projectile distance mismatch, lower value = more false positives")]
	public static float projectile_distance_forgiveness_minimum = 25f;

	[ServerVar]
	[Help("maximum number of projectile updates to allow before rejecting damage")]
	public static int projectile_update_limit = 4;

	[Help("0 == disabled, 1 == initiator, 2 == initiator + target, 3 == initiator + target + LOS, 4 == initiator + target + LOS + tickhistory, 5 == entity collider Overlap Sphere (bounds validation)")]
	[ServerVar]
	public static int melee_protection = 5;

	[Help("violation penalty to hand out when melee hack is detected")]
	[ServerVar]
	public static float melee_penalty = 0f;

	[Help("melee distance forgiveness in percent, lower value = more false positives")]
	[ServerVar]
	public static float melee_forgiveness = 0.5f;

	[ServerVar]
	[Help("melee server frames to include in delay, lower value = more false positives")]
	public static float melee_serverframes = 2f;

	[ServerVar]
	[Help("melee client frames to include in delay, lower value = more false positives")]
	public static float melee_clientframes = 2f;

	[ServerVar]
	[Help("melee backtracking when checking for LOS")]
	public static float melee_backtracking = 0.01f;

	[Help("line of sight directional forgiveness when checking eye or center position")]
	[ServerVar]
	public static float melee_losforgiveness = 0.2f;

	[ServerVar]
	[Help("whether or not to include terrain in the melee LOS checks")]
	public static bool melee_terraincheck = true;

	[ServerVar]
	[Help("whether or not to include vehicles in the melee LOS checks")]
	public static bool melee_vehiclecheck = true;

	[ServerVar]
	[Help("melee collider bounds validation radius for hit entities, higher value = more forgiveness")]
	public static float melee_entity_bounds_radius = 0.05f;

	[ServerVar]
	[Help("whether or not to verify line of sight to the entity's real server-side position")]
	public static bool melee_los_entity_realpos = true;

	[ServerVar]
	[Help("max tolerance for distance between hit pos and closest bound pos")]
	public static float melee_los_entity_realpos_distance = 1.3f;

	[ServerVar]
	[Help("0 == disabled, 1 == distance, 2 == distance + LOS, 3 = distance + LOS + altitude, 4 = distance + LOS + altitude + noclip, 5 = distance + LOS + altitude + noclip + history")]
	public static int eye_protection = 4;

	[ServerVar]
	[Help("violation penalty to hand out when eye hack is detected")]
	public static float eye_penalty = 0f;

	[ServerVar]
	[Help("eye distance forgiveness, lower value = more false positives")]
	public static float eye_forgiveness = 0.4f;

	[Help("eye distance forgiveness for parented or mounted players, lower value = more false positives")]
	[ServerVar]
	public static float eye_distance_parented_mounted_forgiveness = 2f;

	[Help("eye server frames to include in delay, lower value = more false positives")]
	[ServerVar]
	public static float eye_serverframes = 2f;

	[Help("eye client frames to include in delay, lower value = more false positives")]
	[ServerVar]
	public static float eye_clientframes = 2f;

	[Help("whether or not to include terrain in the eye LOS checks")]
	[ServerVar]
	public static bool eye_terraincheck = true;

	[Help("whether or not to include vehicles in the eye LOS checks")]
	[ServerVar]
	public static bool eye_vehiclecheck = true;

	[Help("distance at which to start testing eye noclipping")]
	[ServerVar]
	public static float eye_noclip_cutoff = 0.06f;

	[Help("collider margin when checking for noclipping")]
	[ServerVar]
	public static float eye_noclip_margin = 0.25f;

	[ServerVar]
	[Help("collider backtracking when checking for noclipping")]
	public static float eye_noclip_backtracking = 0.01f;

	[ServerVar]
	[Help("line of sight sphere cast radius, 0 == raycast")]
	public static float eye_losradius = 0.158f;

	[Help("violation penalty to hand out when eye history mismatch is detected")]
	[ServerVar]
	public static float eye_history_penalty = 100f;

	[Help("how much forgiveness to add when checking the distance between player tick history and player eye history")]
	[ServerVar]
	public static float eye_history_forgiveness = 0.1f;

	[Help("maximum distance an impact effect can be from the entities bounds")]
	[ServerVar]
	public static float impact_effect_distance_forgiveness = 0.45f;

	[ServerVar]
	[Help("line of sight sphere cast radius, 0 == raycast")]
	public static float build_losradius = 0.01f;

	[Help("line of sight sphere cast radius, 0 == raycast")]
	[ServerVar]
	public static float build_losradius_sleepingbag = 0.3f;

	[ServerVar]
	[Help("whether or not to include terrain in the build LOS checks")]
	public static bool build_terraincheck = true;

	[Help("whether or not to include vehicles in the build LOS checks")]
	[ServerVar]
	public static bool build_vehiclecheck = true;

	[ServerVar]
	[Help("whether or not to check for building being done on the wrong side of something (e.g. inside rocks). 0 = Disabled, 1 = Info only, 2 = Enabled")]
	public static int build_inside_check = 2;

	[ServerVar]
	[Help("the maximum distance we check for for inside mesh")]
	public static float mesh_inside_check_distance = 50f;

	[ServerVar]
	[Help("use the older, simpler is inside check. has several loopholes that aren't properly catered to")]
	public static bool use_legacy_mesh_inside_check = true;

	[Help("whether or not to ensure players are always networked to server administrators")]
	[ServerVar]
	public static bool server_occlusion_admin_bypass = false;

	[ServerVar]
	[Help("number of blocked grids permitted before line-of-sight fails")]
	public static int server_occlusion_blocked_grid_threshold = 0;

	[ServerVar]
	[Help("number of neighbour checks permitted before line-of-sight fails")]
	public static int server_occlusion_neighbour_threshold = 2;

	[ServerVar]
	[Help("whether to use server occlusion caching")]
	public static bool server_occlusion_caching = true;

	[Help("whether to save sub grid for map to a file on first time init, faster subsequent server launching")]
	[ServerVar]
	public static bool server_occlusion_save_grid = true;

	[ServerVar]
	[Help("whether to use neighbour threshold limitations")]
	public static bool server_occlusion_use_neighbour_thresholds = false;

	[ServerVar]
	[Help("whether or not to skip the line-of-sight process, this will effectively disable server occlusion")]
	public static bool server_occlusion_disable_los = false;

	[ServerVar]
	[Help("whether or not to skip the line-of-sight process for sleepers")]
	public static bool server_occlusion_disable_sleeper_los = false;

	[Help("How far a player is allowed to move in a single tick")]
	[ServerVar]
	public static float tick_max_distance = 1.1f;

	[Help("How far a player is allowed to move in a single tick when falling")]
	[ServerVar]
	public static float tick_max_distance_falling = 4f;

	[ServerVar]
	[Help("How far a player is allowed to move in a single tick when parented")]
	public static float tick_max_distance_parented = 3f;

	[ServerVar]
	[Help("Whether or not to enable additional tick validation measures")]
	public static bool tick_buffer_preventions = true;

	[Help("How many seconds worth of ticks can be sent before server tick finalizing before we revert to noclip_protection 2")]
	[ServerVar]
	public static float tick_buffer_noclip_threshold = 2f;

	[Help("How many seconds worth of ticks can be sent before server tick finalizing before we reject movement")]
	[ServerVar]
	public static float tick_buffer_reject_threshold = 3f;

	[Help("How long it should take for a server to process a frame before we decide to skip additional tick validation measures")]
	[ServerVar]
	public static float tick_buffer_server_lag_threshold = 0.3f;

	[ServerVar]
	[Help("How far a player is allowed to move in forgiveness scenarios")]
	public static float tick_distance_forgiveness = 5f;

	[ServerVar(Help = "Should explosives parented to vehicles damage deployables")]
	public static bool explosive_vehicle_parent_damage_deployables = false;

	[ServerVar]
	[Help("How long, in seconds, can the clients progress timer deviate from the servers")]
	public static float rpc_timer_forgiveness = 1f;

	[ServerVar]
	[Help("Should we do extra physics checks when mounting vehicles to prevent players clipping through small gaps. 0 = off, 1 = bypass switch seats, 2 = always")]
	public static int strict_mount_checks = 1;

	[ServerVar]
	[Help("Should the server check the distance to mountable.maxMountDistance? 0 = off, 1 = for seats with 'checkPlayerLosOnMount' enabled, 2 = always")]
	public static int check_mount_distance = 1;

	[ServerVar]
	[Help("Should the server send the hotbar to other players, 0 = always, 1 = only held entities")]
	public static int hotbar_network_mode = 0;

	[Help("0 == silent, 1 == print max violation, 2 == print nonzero violation, 3 == print any violation except noclip, 4 == print any violation")]
	[ServerVar]
	public static int debuglevel = 1;
}
