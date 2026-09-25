using System.Diagnostics;
using UnityEngine;

namespace ConVar;

[Factory("satellite")]
public class Satellite : ConsoleSystem
{
	[ServerVar(Help = "Minimum cooldown between satellite events, in game hours (not real minutes) — converted to real time via the map's day length, same as the airdrop event scheduler.")]
	public static float cooldown_hours_min = 18f;

	[ServerVar(Help = "Maximum cooldown between satellite events, in game hours")]
	public static float cooldown_hours_max = 18f;

	[ServerVar(Help = "Seconds of thruster control window")]
	public static float control_window = 180f;

	[ServerVar(Help = "Minimum crash radius in meters")]
	public static float min_crash_radius = 100f;

	[ServerVar(Help = "Starting impact radius before thruster adjustments, in meters")]
	public static float default_crash_radius = 300f;

	[ServerVar(Help = "Lateral offset multiplier per thruster fire (meters)")]
	public static float lateral_distance = 200f;

	[ServerVar(Help = "Impact radius change per thruster fire (meters)")]
	public static float shrink_expand_radius = 25f;

	[ServerVar(Help = "Heading rotation per thruster fire (degrees)")]
	public static float rotation_strength = 15f;

	[ServerVar(Help = "Minimum random aim drift (meters) applied when tightening the impact radius, at a fully random angle so it can't be cancelled out exactly by the cardinal-direction lateral thrusters")]
	public static float nudge_distance_min = 100f;

	[ServerVar(Help = "Maximum random aim drift (meters) applied when tightening the impact radius")]
	public static float nudge_distance_max = 150f;

	[ServerVar(Help = "How many times to re-roll the initial random map position, re-checking the full crash-site acceptance each time, until a valid starting target is found. Guarantees the control phase never starts on an unusable spot.")]
	public static int initial_offset_attempts = 8;

	[ServerVar(Help = "When a thruster move finds no valid crash site, how many extra steps to take in the same direction before giving up. Fuel cost is linear in the number of steps taken (a 3-step move costs 3 fuel), and a further step isn't attempted if the player can't afford it.")]
	public static int thruster_extra_steps = 2;

	[ServerVar(Help = "Thruster modules salvaged from fully harvesting the crash remains")]
	public static int thruster_module_count = 3;

	[ServerVar(Help = "Minimum condition fraction of salvaged thruster modules, regardless of how much fuel the satellite burned before lock-in")]
	public static float thruster_module_min_condition = 0.2f;

	private static float descent_seconds_backing = 600f;

	[ReplicatedVar(Help = "Seconds before the scheduled crash that the satellite begins its phase-2 physics descent. Phase 1 slides in to the phase-2 start point up until then, then the satellite drops at a speed timed to hit the ground exactly on schedule. The start point is placed at the satellite's finalDescentSpeed times this window, so lengthening it starts the descent higher rather than slowing the fall.", Default = "300")]
	public static float final_descent_seconds = 300f;

	[ServerVar(Help = "Speed (m/s) of the phase-2 physics descent (0 = use the satellite prefab's finalDescentSpeed). Together with final_descent_seconds this sets where phase 2 starts — speed times seconds back up the descent line — so e.g. 180s at 80 m/s starts the drop 14.4km out.")]
	public static float final_descent_speed = 0f;

	[ReplicatedVar(Help = "Multiplier on the authored lifetime of the satellite's trail renderers — a trail spans this many times the authored seconds of flight path, so it directly scales the visible streak length. Applied as the trails switch on (they sit out the projected far-distance stretch and only run once the satellite is drawn at its real position), so changes take effect at the next handover, not mid-trail. Replicated because trails render client-side.", Default = "1")]
	public static float trail_time_scale = 1f;

	[ReplicatedVar(Help = "Multiplier on the alpha of the satellite trails' authored colour gradients, saturating at full opacity — the authored fades drop to near-transparent within the first quarter of the trail, so values above 1 keep progressively more of its length visible. Applied alongside trail_time_scale as the trails switch on.", Default = "1")]
	public static float trail_alpha_scale = 1f;

	[ReplicatedVar(Help = "Render the satellite's descent as a map-wide sky ribbon tracing the real flight path, visible from any distance at any draw distance setting. Rendered client-side from the entity's networked positions.", Default = "True")]
	public static bool sky_trail = true;

	[ReplicatedVar(Help = "Seconds of flight path the sky ribbon spans — its TrailRenderer-style lifetime: with sky_trail_age_fade on, points dissolve to nothing as they reach this age. Capped by the ribbon's internal point budget at ~128s.", Default = "60")]
	public static float sky_trail_seconds = 60f;

	[ReplicatedVar(Help = "On-screen thickness of the sky ribbon's head, in degrees of view angle — it holds the same apparent width whatever distance it's projected at, tapering to a point at the tail.", Default = "0.075")]
	public static float sky_trail_width_degrees = 0.075f;

	[ReplicatedVar(Help = "Seconds the sky ribbon takes to drain away tail-first after the satellite crashes or is destroyed. The recorded point ages are compressed into this window, so the ribbon keeps shrinking exactly the way it does in flight, and the impact end of the streak is the last to go.", Default = "10")]
	public static float sky_trail_drain_seconds = 10f;

	[ReplicatedVar(Help = "Maximum length of the sky ribbon in meters of flight path — the tail is trimmed to keep the ribbon at or under this. 0 = no length cap, letting the sky_trail_seconds lifetime fade own the tail instead of a hard cut.", Default = "0")]
	public static float sky_trail_max_length = 0f;

	[ReplicatedVar(Help = "Fade each point of the sky ribbon out over its lifetime (sky_trail_seconds), like a TrailRenderer — the streak continuously dissolves behind the satellite instead of holding full brightness until its points expire.", Default = "True")]
	public static bool sky_trail_age_fade = true;

	[ReplicatedVar(Help = "Meters of smooth random wobble baked into the sky ribbon as it's laid down, so the streak reads as turbulent rather than a perfect straight line. Keep it under the dish's size (~3m) or the trail visibly detaches from the model. 0 = perfectly straight.", Default = "1")]
	public static float sky_trail_noise = 1f;

	[ReplicatedVar(Help = "Run the crashing FX pass (FX Pass 3) through the whole phase-2 descent, including the distance-projected stretch, instead of waiting until the satellite enters real draw distance. The projected-mode fixup still applies — its particles simulate in local space and scale with the model; its trails stay gated until real rendering.", Default = "True")]
	public static bool crashing_fx_at_distance = true;

	[ReplicatedVar(Help = "Multiplier the orbiting satellite's glow billboard grows to by the end of phase 1 (starts at 1x). Purely a visual telegraph; replicated because the grow runs client-side.", Default = "4")]
	public static float phase1_grow_scale = 4f;

	[ServerVar(Help = "Extra distance (m) further out, along the descent line, that phase 1 starts. The satellite slides steadily from there in to the orbit point over phase 1, then phase 2 continues along the same line.")]
	public static float phase1_extra_distance = 30000f;

	[ServerVar(Help = "Angle (degrees from vertical) the satellite descends at. Higher = more horizontal streak across the sky. Clamped to 45 so it always reaches the ground within 45 degrees of straight down. Only used when flyover_altitude is 0 — the flyover path has its own geometry.")]
	public static float descent_angle = 45f;

	[ServerVar(Help = "Altitude (m above the crash target) of the phase-2 flyover cruise. The satellite enters near-horizontally at this height, streaks across the sky, then bends into a dive onto the target from flyover_dive_distance out. 0 = disable the flyover and use the legacy straight-line descent at descent_angle.")]
	public static float flyover_altitude = 2500f;

	[ServerVar(Help = "Horizontal distance (m) from the crash target at which the flyover bends into its terminal dive. Smaller = a longer level cruise with a steeper final plunge.")]
	public static float flyover_dive_distance = 1500f;

	[ServerVar(Help = "If true, powering up the satellite computer does not require any items")]
	public static bool free_power = false;

	[ServerVar(Help = "If true, firing satellite thrusters does not consume fuel")]
	public static bool free_fuel = false;

	[ServerVar(Help = "Minimum fuel rolled for small satellites (500-1500kg)")]
	public static int fuel_small_min = 24;

	[ServerVar(Help = "Maximum fuel rolled for small satellites (500-1500kg)")]
	public static int fuel_small_max = 36;

	[ServerVar(Help = "Minimum fuel rolled for medium satellites (1500-3000kg)")]
	public static int fuel_medium_min = 16;

	[ServerVar(Help = "Maximum fuel rolled for medium satellites (1500-3000kg)")]
	public static int fuel_medium_max = 26;

	[ServerVar(Help = "Minimum fuel rolled for large satellites (3000-5000kg)")]
	public static int fuel_large_min = 10;

	[ServerVar(Help = "Maximum fuel rolled for large satellites (3000-5000kg)")]
	public static int fuel_large_max = 16;

	[ServerVar(Help = "Hard minimum on generated fuel, applied after the per-size roll regardless of size")]
	public static int fuel_min = 8;

	[ServerVar(Help = "Hard maximum on generated fuel, applied after the per-size roll regardless of size")]
	public static int fuel_max = 36;

	[ServerVar(Help = "If true, the satellite control computer can only be powered up while the power plant is active (powergrid stage 1+). Ignored when the powergrid system itself is disabled.")]
	public static bool require_powerplant = true;

	[ServerVar(Help = "Log crash-target search diagnostics (samples, blockers, reasons)")]
	public static bool debug = false;

	[ServerVar(Help = "Crash targeting: use the tool-cupboard grid to test likely-occupied samples last, so a clear spot is usually found with fewer physics checks. The physics check stays authoritative.")]
	public static bool obstruction_tc_reorder = true;

	[ServerVar(Help = "Crash targeting: before scanning, re-test the previously found crash spot. Small thruster nudges usually leave it valid, returning in a single check.")]
	public static bool reuse_last_crash_spot = true;

	[ServerVar(Help = "Crash targeting: max milliseconds per frame spent scanning for a safe crash site. The scan resumes next frame when exceeded.")]
	public static float targeting_budget_ms = 5f;

	[ServerVar(Help = "Crash targeting: candidate samples tested per clearance-footprint, i.e. per (targetingRadius/clearanceRadius)^2. The sample count scales with the targeting area, so a small circle runs far fewer checks than a large one. ~1.2 is bare geometric coverage (samples spaced ~one clearance apart); the default adds margin to catch tight gaps. Pushing it much higher mostly adds correlated, redundant checks.")]
	public static float targeting_coverage_factor = 3f;

	[ServerVar(Help = "Crash targeting: minimum candidate samples per search, regardless of how small the targeting circle is. Floors the area-scaled sample count so a small but cluttered area still gets a fair number of attempts.")]
	public static int targeting_min_samples = 8;

	[ServerVar(Help = "Seconds the crash loot crates burn and stay unlootable after impact. All crates unlock together when this expires.")]
	public static float crate_fire_duration = 300f;

	[ServerVar(Help = "Seconds the main wreck stays too hot to harvest after impact")]
	public static float wreck_fire_duration = 480f;

	[ServerVar(Help = "Radius (m) around the impact point cleared on crash — players killed, construction/deployables/vehicles destroyed, vegetation removed")]
	public static float kill_radius = 10f;

	[ServerVar(Help = "Clearance (m) the crash-site search demands around a candidate — rejected if any player building, prevent-building volume or safezone is within this range. Kept separate from kill_radius so targeting standoff can exceed what the impact destroys; 16 matches the building-privilege radius, so a valid site is never inside a base's TC influence.")]
	public static float site_clearance_radius = 16f;

	[ServerVar(Help = "Radius (m) around a candidate crash spot sampled for ground shape — roughly the crash remains footprint. Used both to reject unsuitable spots and to fit the spawned remains to the ground.")]
	public static float site_footprint_radius = 15f;

	[ServerVar(Help = "Max overall ground slope (degrees) across the crash footprint before a spot is rejected")]
	public static float site_max_slope = 25f;

	[ServerVar(Help = "Max terrain height deviation (m) from the crash footprint's best-fit plane before a spot is rejected as too uneven (cliff edges, crests, dips the remains meshes can't hide)")]
	public static float site_max_unevenness = 2f;

	[ServerVar(Help = "Also raycast the crash-site footprint against real collision geometry, rejecting spots with rocks or scenery the heightmap can't see")]
	public static bool site_check_physical_geometry = true;

	private const string crashPrefabPath = "assets/prefabs/satellitecrash/satellite.entity.prefab";

	private const string pendingCrashSitePrefabPath = "assets/prefabs/satellitecrash/satellite_pending_crash_site.entity.prefab";

	private const float PendingCrashSiteDespawnMargin = 30f;

	private const float QuickLaunchSecondsToImpact = 30f;

	[ServerVar(Help = "Descent duration in seconds (0 = use day length fraction; nonzero values are clamped to a minimum of final_descent_seconds so the phase-2 window always fits inside the descent)")]
	public static float descent_seconds
	{
		get
		{
			return descent_seconds_backing;
		}
		set
		{
			descent_seconds_backing = ((value <= 0f) ? 0f : Mathf.Max(value, final_descent_seconds));
		}
	}

	[ServerVar(Help = "Trigger a satellite crash at the calling player's position or optional coordinates, with an optional satellite mass (kg) driving loot like a computer-controlled crash: satellite.trigger [mass] or satellite.trigger [x z [mass]]")]
	public static void trigger(Arg arg)
	{
		float num = 0f;
		Vector3 targetPos;
		if (arg.HasArgs(2))
		{
			float @float = arg.GetFloat(0);
			float float2 = arg.GetFloat(1);
			float height = TerrainMeta.HeightMap.GetHeight(new Vector3(@float, 0f, float2));
			targetPos = new Vector3(@float, height, float2);
			num = arg.GetFloat(2);
		}
		else
		{
			BasePlayer basePlayer = ArgEx.Player(arg);
			if (!basePlayer)
			{
				arg.ReplyWith("No player found. Provide coordinates: satellite.trigger <x> <z> [mass]");
				return;
			}
			targetPos = basePlayer.transform.position;
			num = arg.GetFloat(0);
		}
		SpawnCrashAt(arg, targetPos, "Satellite crash triggered", num);
	}

	[ServerVar(Help = "Dev: search for a safe crash site around the calling player using the exact same acceptance checks a control computer's lock-in uses (topology, water, unevenness, safezones, obstructions), then launch the full orbital descent there. Not a full session replica: searches around the player rather than a computer's semi-random targeting center, always does a fresh scan (no thruster-history reuse), and has no owning computer (no countdown screen, radius floor still enforced, no-build volume self-despawns after impact instead of being computer-managed): satellite.launch [radius]")]
	public static void launch(Arg arg)
	{
		LaunchValidated(arg, 0f);
	}

	[ServerVar(Help = "Dev: exactly satellite.launch — same crash-site search and validation — but skips the wait: the satellite spawns already in phase 2, 30 seconds from impact, at the point of the descent path it would normally reach then: satellite.launchquick [radius]")]
	public static void launchquick(Arg arg)
	{
		LaunchValidated(arg, 30f);
	}

	private static void LaunchValidated(Arg arg, float secondsToImpact)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!basePlayer)
		{
			arg.ReplyWith("No player found — this command must be run by a connected player.");
			return;
		}
		float num = Mathf.Max(arg.GetFloat(0, default_crash_radius), min_crash_radius);
		Vector3 position = basePlayer.transform.position;
		float clearanceRadius = site_clearance_radius;
		CrashSpotSearch crashSpotSearch = new CrashSpotSearch(position, num, clearanceRadius);
		Stopwatch stopwatch = Stopwatch.StartNew();
		CrashSpotSearch.Status status;
		while ((status = crashSpotSearch.Step()) == CrashSpotSearch.Status.InProgress)
		{
		}
		stopwatch.Stop();
		if (status != CrashSpotSearch.Status.Found)
		{
			arg.ReplyWith($"No safe crash spot within {num:F0}m ({crashSpotSearch.SamplesTested} samples in {stopwatch.Elapsed.TotalMilliseconds:F1}ms) — launch aborted (enable satellite.debug for blocker tallies).");
			return;
		}
		Vector3 result = crashSpotSearch.Result;
		SatelliteData satelliteData = SatelliteData.Generate(Random.Range(int.MinValue, int.MaxValue));
		if (GameManager.server.CreateEntity("assets/prefabs/satellitecrash/satellite.entity.prefab", result) is SatelliteCrash satelliteCrash)
		{
			float num2;
			if (secondsToImpact > 0f)
			{
				satelliteCrash.InitLateDescent(satelliteData, result, secondsToImpact);
				num2 = secondsToImpact;
			}
			else
			{
				satelliteCrash.InitOrbit(satelliteData, result);
				num2 = SatelliteCrash.GetScheduledDescentSeconds(satelliteCrash.descentDayFraction);
			}
			satelliteCrash.Spawn();
			if (GameManager.server.CreateEntity("assets/prefabs/satellitecrash/satellite_pending_crash_site.entity.prefab", result) is SatellitePendingCrashSite satellitePendingCrashSite)
			{
				satellitePendingCrashSite.Spawn();
				satellitePendingCrashSite.ScheduleDespawn(num2 + 30f);
			}
			arg.ReplyWith($"{satelliteData.name} ({satelliteData.mass:F0}kg) locked on ({result.x:F0}, {result.y:F0}, {result.z:F0}) " + $"after {crashSpotSearch.SamplesTested} samples in {stopwatch.Elapsed.TotalMilliseconds:F1}ms, impact in {num2:F0}s");
		}
		else
		{
			arg.ReplyWith("Failed to create satellite crash entity. Check prefab path.");
		}
	}

	[ServerVar(Help = "Trigger a satellite crash at a random valid position on the map")]
	public static void random(Arg arg)
	{
		float num = TerrainMeta.Size.x * 0.4f;
		Vector3 vector = new Vector3(Random.Range(0f - num, num), 0f, Random.Range(0f - num, num));
		vector.y = TerrainMeta.HeightMap.GetHeight(vector);
		SpawnCrashAt(arg, vector, "Random satellite crash triggered");
	}

	private static void SpawnCrashAt(Arg arg, Vector3 targetPos, string successLabel, float mass = 0f)
	{
		targetPos = SatelliteCrash.ClampAwayFromMonuments(targetPos, 200f);
		if (GameManager.server.CreateEntity("assets/prefabs/satellitecrash/satellite.entity.prefab", targetPos) is SatelliteCrash satelliteCrash)
		{
			if (mass > 0f)
			{
				satelliteCrash.satelliteMass = mass;
			}
			satelliteCrash.InitDirectDescent(targetPos);
			satelliteCrash.Spawn();
			arg.ReplyWith($"{successLabel} at ({targetPos.x:F0}, {targetPos.y:F0}, {targetPos.z:F0}), mass {satelliteCrash.satelliteMass:F0}kg");
		}
		else
		{
			arg.ReplyWith("Failed to create satellite crash entity. Check prefab path.");
		}
	}
}
