public class ComputerStationIO : IOEntity
{
	public bool visibilityPassesThroughParent;

	public int powerConsumption = 1;

	private ComputerStation parentComputer;

	public override bool VisibilityPassesThroughParent => visibilityPassesThroughParent;

	public override int ConsumptionAmount()
	{
		return powerConsumption;
	}

	public override bool ShouldDrainBattery(IOEntity battery)
	{
		if (parentComputer == null && parentEntity.IsValid(base.isServer))
		{
			parentComputer = parentEntity.Get(base.isServer).GetComponent<ComputerStation>();
		}
		if (parentComputer != null && parentComputer.IsOn())
		{
			return base.ShouldDrainBattery(battery);
		}
		return false;
	}

	public override void ResetIOState()
	{
		base.ResetIOState();
		if (IsOn())
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate_Flags))
			{
				flagsUpdateScope.Set(Flags.On, b: false);
			}
		}
	}

	public override void IOStateChanged(int inputAmount, int inputSlot)
	{
		base.IOStateChanged(inputAmount, inputSlot);
		bool num = IsOn();
		bool flag = IsPowered();
		if (num != flag)
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate_Flags))
			{
				flagsUpdateScope.Set(Flags.On, flag);
			}
		}
	}
}
