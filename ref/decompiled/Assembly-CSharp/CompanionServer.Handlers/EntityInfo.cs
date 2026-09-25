using System.Threading.Tasks;
using Facepunch;
using ProtoBuf;

namespace CompanionServer.Handlers;

public class EntityInfo : BaseEntityHandler<AppEmpty>
{
	public override ValueTask Execute()
	{
		AppEntityInfo appEntityInfo = Pool.Get<AppEntityInfo>();
		appEntityInfo.type = base.Entity.Type;
		appEntityInfo.payload = Pool.Get<AppEntityPayload>();
		base.Entity.FillEntityPayload(appEntityInfo.payload);
		AppResponse appResponse = Pool.Get<AppResponse>();
		appResponse.entityInfo = appEntityInfo;
		Send(appResponse);
		return default(ValueTask);
	}
}
