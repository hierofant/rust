public static class CameraModeEx
{
	public static bool IsFirstPerson(this BasePlayer.CameraMode cameraMode)
	{
		if (cameraMode != 0)
		{
			return cameraMode == BasePlayer.CameraMode.FirstPersonWithArms;
		}
		return true;
	}
}
