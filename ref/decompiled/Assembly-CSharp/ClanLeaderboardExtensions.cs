using System.Collections.Generic;
using Development.Attributes;
using Facepunch;
using ProtoBuf;

public static class ClanLeaderboardExtensions
{
	[PoolAnalyzerGetWrapper]
	public static ClanLeaderboard ToProto(this List<ClanLeaderboardEntry> leaderboard)
	{
		List<ClanLeaderboard.Entry> list = Pool.Get<List<ClanLeaderboard.Entry>>();
		foreach (ClanLeaderboardEntry item in leaderboard)
		{
			list.Add(ToProto(item));
		}
		ClanLeaderboard clanLeaderboard = Pool.Get<ClanLeaderboard>();
		clanLeaderboard.entries = list;
		return clanLeaderboard;
	}

	[PoolAnalyzerGetWrapper]
	public static ClanLeaderboard.Entry ToProto(this ClanLeaderboardEntry entry)
	{
		ClanLeaderboard.Entry entry2 = Pool.Get<ClanLeaderboard.Entry>();
		entry2.clanId = entry.ClanId;
		entry2.name = entry.Name;
		entry2.score = entry.Score;
		return entry2;
	}
}
