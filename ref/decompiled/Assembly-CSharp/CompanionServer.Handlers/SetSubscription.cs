using System.Threading.Tasks;
using ProtoBuf;

namespace CompanionServer.Handlers;

public class SetSubscription : BaseEntityHandler<AppFlag>
{
	public override ValueTask Execute()
	{
		if (base.Entity is ISubscribable subscribable)
		{
			if (base.Proto.value)
			{
				if (!subscribable.AddSubscription(base.UserId))
				{
					SendError("too_many_subscribers");
					return default(ValueTask);
				}
			}
			else
			{
				subscribable.RemoveSubscription(base.UserId);
			}
			SendSuccess();
		}
		else
		{
			SendError("wrong_type");
		}
		return default(ValueTask);
	}
}
