using System.Threading.Tasks;
using ProtoBuf;

namespace CompanionServer.Handlers;

public class SetClanMotd : BaseClanHandler<AppSendMessage>
{
	public override async ValueTask Execute()
	{
		ClanValidatorResult validatedMotd = ClanValidator.ValidateMotd(base.Proto.message);
		if (!validatedMotd.Success)
		{
			SendError("invalid_motd");
			return;
		}
		IClan clan = await GetClan();
		if (clan == null)
		{
			SendError("no_clan");
			return;
		}
		long previousTimestamp = clan.MotdTimestamp;
		ClanResult clanResult = await clan.SetMotd(validatedMotd.Value, base.UserId);
		if (clanResult == ClanResult.Success)
		{
			SendSuccess();
			ClanPushNotifications.SendClanAnnouncement(clan, previousTimestamp, base.UserId);
		}
		else
		{
			SendError(clanResult);
		}
	}
}
