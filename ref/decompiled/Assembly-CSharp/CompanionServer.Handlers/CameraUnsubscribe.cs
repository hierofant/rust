using System.Threading.Tasks;
using CompanionServer.Cameras;
using ProtoBuf;

namespace CompanionServer.Handlers;

public class CameraUnsubscribe : BaseHandler<AppEmpty>
{
	public override ValueTask Execute()
	{
		if (!CameraRenderer.enabled)
		{
			SendError("not_enabled");
			return default(ValueTask);
		}
		base.Client.EndViewing();
		SendSuccess();
		return default(ValueTask);
	}
}
