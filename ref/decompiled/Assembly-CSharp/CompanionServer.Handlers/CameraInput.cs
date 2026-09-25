using System.Threading.Tasks;
using CompanionServer.Cameras;
using Facepunch;
using ProtoBuf;
using UnityEngine;

namespace CompanionServer.Handlers;

public class CameraInput : BaseHandler<AppCameraInput>
{
	protected override double TokenCost => 0.01;

	public override ValueTask Execute()
	{
		if (!CameraRenderer.enabled)
		{
			SendError("not_enabled");
			return default(ValueTask);
		}
		if (base.Client.CurrentCamera == null || !base.Client.IsControllingCamera)
		{
			SendError("no_camera");
			return default(ValueTask);
		}
		InputState inputState = base.Client.InputState;
		if (inputState == null)
		{
			inputState = new InputState();
			base.Client.InputState = inputState;
		}
		InputMessage inputMessage = Pool.Get<InputMessage>();
		inputMessage.buttons = base.Proto.buttons;
		inputMessage.mouseDelta = Sanitize(base.Proto.mouseDelta);
		inputMessage.aimAngles = Vector3.zero;
		inputState.Flip(inputMessage);
		inputMessage.Dispose();
		inputMessage = null;
		base.Client.CurrentCamera.UserInput(inputState, new CameraViewerId(base.Client.ControllingSteamId, base.Client.ConnectionId));
		SendSuccess();
		return default(ValueTask);
	}

	private static Vector3 Sanitize(Vector3 value)
	{
		return new Vector3(Sanitize(value.x), Sanitize(value.y), Sanitize(value.z));
	}

	private static float Sanitize(float value)
	{
		if (float.IsNaN(value) || float.IsInfinity(value))
		{
			return 0f;
		}
		return Mathf.Clamp(value, -100f, 100f);
	}
}
