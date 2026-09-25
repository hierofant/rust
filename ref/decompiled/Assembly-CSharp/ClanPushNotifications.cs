using System;
using System.Collections.Generic;
using CompanionServer;
using ConVar;
using Facepunch;
using UnityEngine;

public static class ClanPushNotifications
{
	public static async void SendClanAnnouncement(IClan clan, long previousTimestamp, ulong ignorePlayer)
	{
		if (ClanUtility.Timestamp() - previousTimestamp < 300000)
		{
			return;
		}
		try
		{
			List<ulong> steamIds = Facepunch.Pool.Get<List<ulong>>();
			foreach (ClanMember member in clan.Members)
			{
				if (member.SteamId != ignorePlayer)
				{
					steamIds.Add(member.SteamId);
				}
			}
			Dictionary<string, string> dictionary = Util.TryGetServerPairingData();
			if (dictionary != null)
			{
				dictionary.Add("type", "clan");
				dictionary.Add("clanId", clan.ClanId.ToString("G"));
				dictionary.Add("fromId", ignorePlayer.ToString("G"));
				await NotificationList.SendNotificationTo(steamIds, NotificationChannel.ClanAnnouncement, "[" + clan.Name + "] Announcement was updated", ConVar.Server.hostname, dictionary);
			}
			Facepunch.Pool.FreeUnmanaged(ref steamIds);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}
}
