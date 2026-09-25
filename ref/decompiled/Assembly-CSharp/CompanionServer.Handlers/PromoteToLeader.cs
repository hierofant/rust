using System.Threading.Tasks;
using ProtoBuf;

namespace CompanionServer.Handlers;

public class PromoteToLeader : BasePlayerHandler<AppPromoteToLeader>
{
	public override ValueTask Execute()
	{
		RelationshipManager.PlayerTeam playerTeam = RelationshipManager.ServerInstance.FindPlayersTeam(base.UserId);
		if (playerTeam == null)
		{
			SendError("no_team");
			return default(ValueTask);
		}
		if (playerTeam.teamLeader != base.UserId)
		{
			SendError("access_denied");
			return default(ValueTask);
		}
		if (playerTeam.teamLeader == base.Proto.steamId)
		{
			SendSuccess();
			return default(ValueTask);
		}
		if (!playerTeam.members.Contains(base.Proto.steamId))
		{
			SendError("not_found");
			return default(ValueTask);
		}
		playerTeam.SetTeamLeader(base.Proto.steamId);
		SendSuccess();
		return default(ValueTask);
	}
}
