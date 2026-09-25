using UnityEngine.EventSystems;

public class PaintableImageGrid : UIBehaviour, IServerFileReceiver
{
	public GameObjectRef templateImage;

	public int cols = 4;

	public int rows = 4;

	public bool readOnly;

	public BasePlayer.FogMode FogMode = BasePlayer.FogMode.Mainland;

	public const int MaxImageNum = 16;

	public int ImageNumberOffset
	{
		get
		{
			if (FogMode != BasePlayer.FogMode.Mainland)
			{
				return 16;
			}
			return 0;
		}
	}
}
