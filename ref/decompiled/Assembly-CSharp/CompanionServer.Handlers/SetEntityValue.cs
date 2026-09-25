using System.Threading.Tasks;
using ProtoBuf;

namespace CompanionServer.Handlers;

public class SetEntityValue : BaseEntityHandler<AppSetEntityValue>
{
	public override ValueTask Execute()
	{
		if (base.Entity is SmartSwitch smartSwitch)
		{
			smartSwitch.Value = base.Proto.value;
			SendSuccess();
		}
		else
		{
			SendError("wrong_type");
		}
		return default(ValueTask);
	}
}
