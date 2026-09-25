using Development.Attributes;

[ResetStaticFields]
public class WeatherEffectWorkQueue : PersistentObjectWorkQueue<WeatherEffect>
{
	public static WeatherEffectWorkQueue WorkQueue = new WeatherEffectWorkQueue();

	protected override void RunJob(WeatherEffect entity)
	{
	}
}
