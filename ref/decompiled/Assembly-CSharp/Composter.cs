using UnityEngine;

public class Composter : BaseOven
{
	[Header("Composter")]
	public ItemDefinition FertilizerDef;

	[Tooltip("If enabled, entire item stacks will be composted each tick, instead of a single item of a stack.")]
	public bool CompostEntireStack;

	protected override bool AutomaticallyStartCooking => true;

	public override bool CanRunWithNoFuel
	{
		protected get
		{
			return true;
		}
	}

	public override bool ShowFuelInLootPanel => false;

	public override string TitleItemShortname => "composter";
}
