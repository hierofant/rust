using System;

namespace ConVar;

[Factory("softcore")]
public class Softcore : ConsoleSystem
{
	private static bool _enabled = true;

	private static float _start_hour = 18f;

	private static float _end_hour = 21f;

	private static bool _weekend_enabled = false;

	private static float _weekend_start_hour = 17f;

	private static float _weekend_end_hour = 22f;

	private static float _hours_offset = 0f;

	[ServerVar(Saved = true, Help = "(Generated) Allow explosive damage to twig-grade building blocks even outside the raid window")]
	public static bool raidwindow_allow_twig = true;

	[ServerVar(Saved = true, Help = "(Generated) Allow players authed on the target base tool cupboard to use explosives outside the window (renovate, or continue a raid after capturing the TC)")]
	public static bool raidwindow_allow_tc_authed = true;

	private static float _fresh_tc_seconds = 3600f;

	[ServerVar(Saved = true, Help = "(Generated) Outside the window, also block a non-explosive damage type against non-twig building blocks (e.g. shotgunning down a stone wall). Melee is only affected by softcore.raidwindow_block_melee_doors")]
	public static bool raidwindow_block_extra_enabled = true;

	[ServerVar(Saved = true, Help = "(Generated) Outside the window, block melee damage to doors so they can't be beaten down while the window is closed")]
	public static bool raidwindow_block_melee_doors = true;

	[ServerVar(Saved = true, Help = "(Generated) Rust.DamageType value blocked against non-twig building blocks when softcore.raidwindow_block_extra_enabled is true (default 8 = Bullet, which covers guns and shotguns)")]
	public static int raidwindow_block_extra_type = 8;

	[ServerVar(Saved = true, Help = "(Generated) Block launching MLRS rockets outside the raid window so they are not wasted")]
	public static bool raidwindow_block_mlrs = true;

	[ReplicatedVar(Default = "true", Saved = true, Help = "(Generated) Enable Softcore raid windows: explosive damage to building blocks is only allowed inside the configured daily time window")]
	public static bool raidwindow_enabled
	{
		get
		{
			return _enabled;
		}
		set
		{
			_enabled = value;
			ReSchedule();
		}
	}

	[ReplicatedVar(Default = "18", Saved = true, Help = "(Generated) Raid window start hour in server system local time, 0-24 (default 18 = 6pm)")]
	public static float raidwindow_start_hour
	{
		get
		{
			return _start_hour;
		}
		set
		{
			_start_hour = value;
			ReSchedule();
		}
	}

	[ReplicatedVar(Default = "21", Saved = true, Help = "(Generated) Raid window end hour in server system local time, 0-24 (default 21 = 9pm). If end is less than start the window wraps past midnight")]
	public static float raidwindow_end_hour
	{
		get
		{
			return _end_hour;
		}
		set
		{
			_end_hour = value;
			ReSchedule();
		}
	}

	[ReplicatedVar(Default = "false", Saved = true, Help = "(Generated) Use separate raid hours on Saturday and Sunday")]
	public static bool raidwindow_weekend_enabled
	{
		get
		{
			return _weekend_enabled;
		}
		set
		{
			_weekend_enabled = value;
			ReSchedule();
		}
	}

	[ReplicatedVar(Default = "17", Saved = true, Help = "(Generated) Weekend raid window start hour, server local time (used when softcore.raidwindow_weekend_enabled is true)")]
	public static float raidwindow_weekend_start_hour
	{
		get
		{
			return _weekend_start_hour;
		}
		set
		{
			_weekend_start_hour = value;
			ReSchedule();
		}
	}

	[ReplicatedVar(Default = "22", Saved = true, Help = "(Generated) Weekend raid window end hour, server local time (used when softcore.raidwindow_weekend_enabled is true)")]
	public static float raidwindow_weekend_end_hour
	{
		get
		{
			return _weekend_end_hour;
		}
		set
		{
			_weekend_end_hour = value;
			ReSchedule();
		}
	}

	[ReplicatedVar(Default = "0", Saved = true, Help = "(Generated) Hours to widen the raid window on both sides for timezone spread (1 = opens one hour earlier and closes one hour later)")]
	public static float raidwindow_hours_offset
	{
		get
		{
			return _hours_offset;
		}
		set
		{
			_hours_offset = value;
			ReSchedule();
		}
	}

	[ServerVar(Default = "3600", Saved = true, Help = "(Generated) Seconds after a base's tool cupboard is placed during which the base can still be raided outside the window; 0 disables")]
	public static float raidwindow_fresh_tc_seconds
	{
		get
		{
			return _fresh_tc_seconds;
		}
		set
		{
			_fresh_tc_seconds = value;
			ReSchedule();
		}
	}

	private static void ReSchedule()
	{
		RaidWindow.OnScheduleChanged();
	}

	[ServerVar(ServerAdmin = true, Help = "(Generated) Print the current raid-window state: server local time, effective hours, and whether the window is open right now")]
	public static void raidwindow_status(Arg arg)
	{
		DateTime now = DateTime.Now;
		bool flag = raidwindow_weekend_enabled && (now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday);
		float num = (flag ? raidwindow_weekend_start_hour : raidwindow_start_hour) - raidwindow_hours_offset;
		float num2 = (flag ? raidwindow_weekend_end_hour : raidwindow_end_hour) + raidwindow_hours_offset;
		arg.ReplyWith($"enabled={raidwindow_enabled}  IsWindowOpenNow={RaidWindow.IsWindowOpenNow()}\n" + $"server local time={now:yyyy-MM-dd HH:mm:ss} ({now.DayOfWeek})\n" + $"effective window={num}->{num2}  (raw start={raidwindow_start_hour} end={raidwindow_end_hour} offset={raidwindow_hours_offset}, weekendActive={flag})");
	}
}
