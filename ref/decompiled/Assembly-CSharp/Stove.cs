public class Stove : BaseOven
{
	public override string TitleItemShortname => "stove";

	public override bool ShowFuelInLootPanel => false;

	protected override bool AutomaticallyStartCooking => true;

	public override bool CanRunWithNoFuel
	{
		protected get
		{
			return true;
		}
	}
}
