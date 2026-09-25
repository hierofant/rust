using System.Threading.Tasks;
using ConVar;
using Facepunch.Extend;
using ProtoBuf;

namespace CompanionServer.Handlers;

public class SendTeamChat : BasePlayerHandler<AppSendMessage>
{
	protected override double TokenCost => 2.0;

	public override async ValueTask Execute()
	{
		string text = base.Proto.message?.Trim();
		if (string.IsNullOrWhiteSpace(text))
		{
			SendSuccess();
			return;
		}
		text = text.Truncate(256, "…");
		string username = base.Player?.displayName ?? SingletonComponent<ServerMgr>.Instance.persistance.GetPlayerName(base.UserId) ?? "[unknown]";
		if (await Chat.sayAs(Chat.ChatChannel.Team, base.UserId, username, text, base.Player))
		{
			SendSuccess();
		}
		else
		{
			SendError("message_not_sent");
		}
	}
}
