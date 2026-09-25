using System.Threading.Tasks;
using CompanionServer.Cameras;
using Facepunch;
using ProtoBuf;
using UnityEngine;

namespace CompanionServer.Handlers;

public class CameraSubscribe : BasePlayerHandler<AppCameraSubscribe>
{
	public override ValueTask Execute()
	{
		if (!CameraRenderer.enabled)
		{
			SendError("not_enabled");
			return default(ValueTask);
		}
		CameraRendererManager instance = SingletonComponent<CameraRendererManager>.Instance;
		if (instance == null)
		{
			SendError("server_error");
			return default(ValueTask);
		}
		if (string.IsNullOrEmpty(base.Proto.cameraId))
		{
			base.Client.EndViewing();
			SendError("invalid_id");
			return default(ValueTask);
		}
		bool flag = CameraRenderer.developerPermissions && DeveloperList.Contains(base.UserId);
		if (!base.Player.IsValid())
		{
			base.Client.EndViewing();
			SendError("no_player");
			return default(ValueTask);
		}
		if (!flag && base.Player.IsConnected)
		{
			base.Client.EndViewing();
			SendError("player_online");
			return default(ValueTask);
		}
		IRemoteControllable remoteControllable = RemoteControlEntity.FindByID(base.Proto.cameraId);
		if (remoteControllable == null || !remoteControllable.CanControl(base.UserId))
		{
			base.Client.EndViewing();
			SendError("not_found");
			return default(ValueTask);
		}
		if (!flag && remoteControllable is CCTV_RC cCTV_RC && cCTV_RC.IsStatic())
		{
			base.Client.EndViewing();
			SendError("access_denied");
			return default(ValueTask);
		}
		BaseEntity ent = remoteControllable.GetEnt();
		if (!ent.IsValid())
		{
			base.Client.EndViewing();
			SendError("not_found");
			return default(ValueTask);
		}
		float num = Vector3.Distance(base.Player.transform.position, ent.transform.position);
		if (!flag && num >= remoteControllable.MaxRange)
		{
			base.Client.EndViewing();
			SendError("not_found");
			return default(ValueTask);
		}
		if (!base.Client.BeginViewing(remoteControllable))
		{
			base.Client.EndViewing();
			SendError("not_found");
			return default(ValueTask);
		}
		instance.StartRendering(remoteControllable);
		AppResponse appResponse = Pool.Get<AppResponse>();
		AppCameraInfo appCameraInfo = Pool.Get<AppCameraInfo>();
		appCameraInfo.width = CameraRenderer.width;
		appCameraInfo.height = CameraRenderer.height;
		appCameraInfo.nearPlane = CameraRenderer.nearPlane;
		appCameraInfo.farPlane = CameraRenderer.farPlane;
		appCameraInfo.controlFlags = (int)(base.Client.IsControllingCamera ? remoteControllable.RequiredControls : RemoteControllableControls.None);
		appResponse.cameraSubscribeInfo = appCameraInfo;
		Send(appResponse);
		return default(ValueTask);
	}
}
