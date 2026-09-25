public class BoatBuildingBlockDecay : BuildingBlockDecay
{
	[ServerVar(Help = "Multiplied by the base BuildingBlockDecay duration")]
	public static float DecayDurationMultiplier = 1f;

	[ServerVar(Help = "Multiplied by the base BuildingBlockDecay duration")]
	public static float DecayDelayMinutes = 30f;

	public override float GetDecayDuration(BaseEntity entity)
	{
		return base.GetDecayDuration(entity) * DecayDurationMultiplier;
	}

	public override float GetDecayDelay(BaseEntity entity)
	{
		return DecayDelayMinutes * 60f;
	}
}
