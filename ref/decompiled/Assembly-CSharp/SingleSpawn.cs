public class SingleSpawn : SpawnGroup
{
	public override bool WantsInitialSpawn()
	{
		return false;
	}

	[UnityEvent]
	public void FillDelay(float delay)
	{
		Invoke(Fill, delay);
	}
}
