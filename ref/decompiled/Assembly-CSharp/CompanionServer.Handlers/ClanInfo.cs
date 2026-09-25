using System.Threading.Tasks;
using Facepunch;
using ProtoBuf;

namespace CompanionServer.Handlers;

public class ClanInfo : BaseClanHandler<AppEmpty>
{
	public override async ValueTask Execute()
	{
		IClan clan = await GetClan();
		if (clan == null)
		{
			SendError("no_clan");
			return;
		}
		await clan.RefreshIfStale();
		AppClanInfo appClanInfo = Pool.Get<AppClanInfo>();
		appClanInfo.clanInfo = ClanInfoExtensions.ToProto(clan);
		AppResponse appResponse = Pool.Get<AppResponse>();
		appResponse.clanInfo = appClanInfo;
		Send(appResponse);
	}
}
