using Facepunch.Sqlite;

public class RelationshipManagerDB : Database
{
	public ulong lastTeamIndex { get; private set; }

	public void Initialize()
	{
		if (!TableExists("data"))
		{
			Execute("CREATE TABLE data (last_team_index INTEGER NOT NULL)");
			Execute("INSERT INTO data (last_team_index) VALUES (?)", 0);
		}
		else
		{
			lastTeamIndex = Query<ulong>("SELECT last_team_index FROM data");
		}
	}

	public ulong IncrementLastTeamIndex()
	{
		Execute("UPDATE data SET last_team_index = ?", ++lastTeamIndex);
		return lastTeamIndex;
	}
}
