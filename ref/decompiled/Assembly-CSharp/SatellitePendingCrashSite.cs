public class SatellitePendingCrashSite : BaseEntity
{
	public void ScheduleDespawn(float seconds)
	{
		Invoke(DespawnSelf, seconds);
	}

	private void DespawnSelf()
	{
		if (!base.IsDestroyed)
		{
			Kill();
		}
	}
}
