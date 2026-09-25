using System;

public class EventScheduleDynamicTickrate : EventSchedule
{
	[NonSerialized]
	public float tickRate = 1f;

	protected override void CountHours()
	{
		if ((bool)TOD_Sky.Instance)
		{
			if (lastRun != 0L)
			{
				hoursRemaining -= (float)TOD_Sky.Instance.Cycle.DateTime.Subtract(DateTime.FromBinary(lastRun)).TotalSeconds / 60f / 60f * tickRate;
			}
			lastRun = TOD_Sky.Instance.Cycle.DateTime.ToBinary();
		}
	}
}
