using System.Collections.Generic;
using UnityEngine;

public class TriggerAnalytic : TriggerBase, IServerComponent
{
	private struct RecentPlayerEntrance
	{
		public BasePlayer Player;

		public TimeSince Time;
	}

	public string AnalyticMessage;

	public float Timeout = 120f;

	private List<RecentPlayerEntrance> recentEntrances = new List<RecentPlayerEntrance>();

	internal override GameObject InterestedInObject(GameObject obj)
	{
		if (GameObjectEx.ToBaseEntity(obj) is BasePlayer { IsNpc: false, isServer: not false } basePlayer)
		{
			return basePlayer.gameObject;
		}
		return null;
	}

	internal override void OnEntityEnter(BaseEntity ent)
	{
		base.OnEntityEnter(ent);
		BasePlayer basePlayer = ent.ToPlayer();
		if (basePlayer != null && !basePlayer.IsNpc)
		{
			CheckTimeouts();
			if (IsPlayerValid(basePlayer))
			{
				recentEntrances.Add(new RecentPlayerEntrance
				{
					Player = basePlayer,
					Time = 0f
				});
			}
		}
	}

	private void CheckTimeouts()
	{
		for (int num = recentEntrances.Count - 1; num >= 0; num--)
		{
			if ((float)recentEntrances[num].Time > Timeout)
			{
				recentEntrances.RemoveAt(num);
			}
		}
	}

	private bool IsPlayerValid(BasePlayer p)
	{
		for (int i = 0; i < recentEntrances.Count; i++)
		{
			if (recentEntrances[i].Player == p)
			{
				return false;
			}
		}
		return true;
	}
}
