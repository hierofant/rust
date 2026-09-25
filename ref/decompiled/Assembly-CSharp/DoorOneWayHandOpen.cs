public class DoorOneWayHandOpen : Door
{
	public float AutoCloseTime = 5f;

	protected override void OnPlayerOpenedDoor(BasePlayer p)
	{
		base.OnPlayerOpenedDoor(p);
		if (AutoCloseTime > 0f)
		{
			Invoke(base.CloseRequest, AutoCloseTime);
		}
	}
}
