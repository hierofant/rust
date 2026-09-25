using System;
using System.Collections.Generic;
using ConVar;
using Rust;
using UnityEngine;

public static class RaidWindow
{
	private const int HorizonDays = 8;

	public static bool IsWindowOpen(DateTime now, float start, float end, bool weekendEnabled, float weekendStart, float weekendEnd, float offset)
	{
		int num;
		float num2;
		if (weekendEnabled)
		{
			if (now.DayOfWeek != DayOfWeek.Saturday)
			{
				num = ((now.DayOfWeek == DayOfWeek.Sunday) ? 1 : 0);
				if (num == 0)
				{
					goto IL_0020;
				}
			}
			else
			{
				num = 1;
			}
			num2 = weekendStart;
			goto IL_0025;
		}
		num = 0;
		goto IL_0020;
		IL_0020:
		num2 = start;
		goto IL_0025;
		IL_0025:
		float t = num2 - offset;
		float t2 = ((num != 0) ? weekendEnd : end) + offset;
		float num3 = (float)now.Hour + (float)now.Minute / 60f + (float)now.Second / 3600f;
		t = Mathf.Repeat(t, 24f);
		t2 = Mathf.Repeat(t2, 24f);
		if (Mathf.Approximately(t, t2))
		{
			return true;
		}
		if (t < t2)
		{
			if (num3 >= t)
			{
				return num3 < t2;
			}
			return false;
		}
		if (!(num3 >= t))
		{
			return num3 < t2;
		}
		return true;
	}

	public static bool IsWindowOpenNow()
	{
		return IsOpenAt(DateTime.Now);
	}

	private static bool IsOpenAt(DateTime t)
	{
		return IsWindowOpen(t, Softcore.raidwindow_start_hour, Softcore.raidwindow_end_hour, Softcore.raidwindow_weekend_enabled, Softcore.raidwindow_weekend_start_hour, Softcore.raidwindow_weekend_end_hour, Softcore.raidwindow_hours_offset);
	}

	public static void OnScheduleChanged()
	{
		if (BaseGameMode.GetActiveGameMode(serverside: true) is GameModeSoftcore gameModeSoftcore)
		{
			gameModeSoftcore.UpdateRaidWindow();
			BuildingPrivlidge.RefreshAllRaidStatus();
		}
	}

	private static bool TryGetNextChange(DateTime now, out DateTime change)
	{
		using (TimeWarning.New("RaidWindow.TryGetNextChange"))
		{
			change = default(DateTime);
			bool state = IsOpenAt(now);
			DateTime c = now.Date;
			int num = 0;
			while (num <= 8)
			{
				int num2;
				float num3;
				if (Softcore.raidwindow_weekend_enabled)
				{
					if (c.DayOfWeek != DayOfWeek.Saturday)
					{
						num2 = ((c.DayOfWeek == DayOfWeek.Sunday) ? 1 : 0);
						if (num2 == 0)
						{
							goto IL_004d;
						}
					}
					else
					{
						num2 = 1;
					}
					num3 = Softcore.raidwindow_weekend_start_hour;
					goto IL_0059;
				}
				num2 = 0;
				goto IL_004d;
				IL_004d:
				num3 = Softcore.raidwindow_start_hour;
				goto IL_0059;
				IL_0059:
				float a = Mathf.Repeat(num3 - Softcore.raidwindow_hours_offset, 24f);
				float b = Mathf.Repeat(((num2 != 0) ? Softcore.raidwindow_weekend_end_hour : Softcore.raidwindow_end_hour) + Softcore.raidwindow_hours_offset, 24f);
				if (TestCandidate(c, now, state, ref change))
				{
					return true;
				}
				if (TestCandidate(c.AddHours(Mathf.Min(a, b)), now, state, ref change))
				{
					return true;
				}
				if (TestCandidate(c.AddHours(Mathf.Max(a, b)), now, state, ref change))
				{
					return true;
				}
				num++;
				c = c.AddDays(1.0);
			}
			return false;
		}
	}

	private static bool TestCandidate(DateTime c, DateTime now, bool state, ref DateTime change)
	{
		if (c <= now || IsOpenAt(c) == state)
		{
			return false;
		}
		change = c;
		return true;
	}

	public static long NextChangeUnixUtc()
	{
		if (!TryGetNextChange(DateTime.Now, out var change))
		{
			return 0L;
		}
		return ((DateTimeOffset)new DateTime(change.Year, change.Month, change.Day, change.Hour, change.Minute, 0, DateTimeKind.Local)).ToUnixTimeSeconds();
	}

	public static int SecondsUntilNextChange()
	{
		DateTime now = DateTime.Now;
		if (!TryGetNextChange(now, out var change))
		{
			return 691200;
		}
		return Mathf.Max(1, (int)Math.Ceiling((change - now).TotalSeconds));
	}

	private static bool IsFrameInsert(BaseEntity victim)
	{
		List<EntityLink> entityLinks = victim.GetEntityLinks();
		for (int i = 0; i < entityLinks.Count; i++)
		{
			List<EntityLink> connections = entityLinks[i].connections;
			for (int j = 0; j < connections.Count; j++)
			{
				if (connections[j].owner is BuildingBlock)
				{
					return true;
				}
			}
		}
		return false;
	}

	private static bool HasParentVehicle(BaseEntity victim)
	{
		BaseEntity parentEntity = victim.GetParentEntity();
		while (parentEntity != null)
		{
			if (parentEntity is BaseVehicle)
			{
				return true;
			}
			parentEntity = parentEntity.GetParentEntity();
		}
		return false;
	}

	public static bool BlocksDamage(BaseCombatEntity victim, HitInfo info)
	{
		if (!Softcore.raidwindow_enabled)
		{
			return false;
		}
		if (!(BaseGameMode.GetActiveGameMode(serverside: true) is GameModeSoftcore))
		{
			return false;
		}
		using (TimeWarning.New("RaidWindow.BlocksDamage"))
		{
			bool flag = victim is DecayEntity && IsFrameInsert(victim);
			if (!(victim is BuildingBlock || victim is Door || flag))
			{
				return false;
			}
			if (victim is Gate || victim is SiegeTowerDoor)
			{
				return false;
			}
			if (HasParentVehicle(victim))
			{
				return false;
			}
			BuildingBlock buildingBlock = victim as BuildingBlock;
			bool flag2 = buildingBlock != null && buildingBlock.grade == BuildingGrade.Enum.Twigs;
			if (Softcore.raidwindow_allow_twig && flag2)
			{
				return false;
			}
			bool flag3 = info.damageTypes.Contains(DamageType.Explosion);
			if (!flag3 && Softcore.raidwindow_block_melee_doors && victim is Door && info.damageTypes.IsMeleeType())
			{
				flag3 = true;
			}
			if (!flag3 && Softcore.raidwindow_block_extra_enabled && !flag2)
			{
				flag3 = info.damageTypes.Contains((DamageType)Softcore.raidwindow_block_extra_type);
			}
			if (!flag3)
			{
				return false;
			}
			if (IsWindowOpenNow())
			{
				return false;
			}
			BuildingPrivlidge buildingPrivilege = ((DecayEntity)victim).GetBuildingPrivilege();
			if (Softcore.raidwindow_allow_tc_authed && info.InitiatorPlayer != null && buildingPrivilege != null && buildingPrivilege.IsAuthed(info.InitiatorPlayer))
			{
				return false;
			}
			if (Softcore.raidwindow_fresh_tc_seconds > 0f && buildingPrivilege != null && buildingPrivilege.timePlaced > 0f && UnityEngine.Time.time - buildingPrivilege.timePlaced <= Softcore.raidwindow_fresh_tc_seconds)
			{
				return false;
			}
			return true;
		}
	}
}
