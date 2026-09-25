using System.Threading.Tasks;
using ProtoBuf;

namespace CompanionServer.Handlers;

public class CheckSubscription : BaseEntityHandler<AppEmpty>
{
	public override ValueTask Execute()
	{
		if (base.Entity is ISubscribable subscribable)
		{
			bool value = subscribable.HasSubscription(base.UserId);
			SendFlag(value);
		}
		else
		{
			SendError("wrong_type");
		}
		return default(ValueTask);
	}
}
