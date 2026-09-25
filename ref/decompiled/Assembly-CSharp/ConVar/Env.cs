using System;
using UnityEngine;

namespace ConVar;

[Factory("env")]
public class Env : ConsoleSystem
{
	[ClientVar(Default = "1", Help = "(Generated) When enabled, ambient night lighting is applied at night; disabling makes nights pitch-black for horror or specific event scenarios")]
	public static bool nightlight_enabled = true;

	[ReplicatedVar(Default = "0")]
	public static bool redMoon = false;

	[ClientVar(Default = "0", Help = "Toggles nightlight screen effect when using debug camera")]
	public static bool nightlight_debugcamera_enabled = true;

	private static float nightlight_distance_internal = 5f;

	private static float nightlight_fadefraction_internal = 0.65f;

	private static float nightlight_brightness_internal = 0.008f;

	[ServerVar(Help = "(Generated) When enabled, the server advances the in-game time of day automatically; disabling freezes time at its current value")]
	public static bool progresstime
	{
		get
		{
			if (TOD_Sky.Instance == null)
			{
				return false;
			}
			return TOD_Sky.Instance.Components.Time.ProgressTime;
		}
		set
		{
			if (!(TOD_Sky.Instance == null))
			{
				TOD_Sky.Instance.Components.Time.ProgressTime = value;
			}
		}
	}

	[ServerVar(ShowInAdminUI = true, Help = "(Generated) Gets or sets the current in-game time of day as a decimal hour (0.0-24.0); writing a value immediately jumps the time to that point")]
	public static float time
	{
		get
		{
			if (TOD_Sky.Instance == null)
			{
				return 0f;
			}
			return TOD_Sky.Instance.Cycle.Hour;
		}
		set
		{
			if (!(TOD_Sky.Instance == null))
			{
				TOD_Sky.Instance.Cycle.Hour = value;
			}
		}
	}

	[ServerVar(Help = "(Generated) Gets or sets the current in-game day of the month; used for calendar-driven events and date display")]
	public static int day
	{
		get
		{
			if (TOD_Sky.Instance == null)
			{
				return 0;
			}
			return TOD_Sky.Instance.Cycle.Day;
		}
		set
		{
			if (!(TOD_Sky.Instance == null))
			{
				TOD_Sky.Instance.Cycle.Day = value;
			}
		}
	}

	[ServerVar(Help = "(Generated) Gets or sets the current in-game month (1-12); used for seasonal event triggers and calendar display")]
	public static int month
	{
		get
		{
			if (TOD_Sky.Instance == null)
			{
				return 0;
			}
			return TOD_Sky.Instance.Cycle.Month;
		}
		set
		{
			if (!(TOD_Sky.Instance == null))
			{
				TOD_Sky.Instance.Cycle.Month = value;
			}
		}
	}

	[ServerVar(Help = "(Generated) Gets or sets the current in-game year; used for date display and long-running server event tracking")]
	public static int year
	{
		get
		{
			if (TOD_Sky.Instance == null)
			{
				return 0;
			}
			return TOD_Sky.Instance.Cycle.Year;
		}
		set
		{
			if (!(TOD_Sky.Instance == null))
			{
				TOD_Sky.Instance.Cycle.Year = value;
			}
		}
	}

	[ReplicatedVar(Default = "0")]
	public static float oceanlevel
	{
		get
		{
			return WaterSystem.OceanLevel;
		}
		set
		{
			WaterSystem.OceanLevel = value;
		}
	}

	[ReplicatedVar(Default = "5")]
	public static float nightlight_distance
	{
		get
		{
			return nightlight_distance_internal;
		}
		set
		{
			value = Mathf.Clamp(value, 0f, 25f);
			nightlight_distance_internal = value;
		}
	}

	[ReplicatedVar(Default = "0.65")]
	public static float nightlight_fadefraction
	{
		get
		{
			return nightlight_fadefraction_internal;
		}
		set
		{
			nightlight_fadefraction_internal = value;
		}
	}

	[ReplicatedVar(Default = "0.008")]
	public static float nightlight_brightness
	{
		get
		{
			return nightlight_brightness_internal;
		}
		set
		{
			value = Mathf.Clamp(value, 0f, 0.2f);
			nightlight_brightness_internal = value;
		}
	}

	[ServerVar(Help = "(Generated) Advances the in-game time of day by the specified number of hours; useful for quickly cycling to day or night for testing")]
	public static void addtime(Arg arg)
	{
		if (!(TOD_Sky.Instance == null))
		{
			DateTime dateTime = TOD_Sky.Instance.Cycle.DateTime.AddTicks(arg.GetTicks(0, 0L));
			TOD_Sky.Instance.Cycle.DateTime = dateTime;
		}
	}
}
