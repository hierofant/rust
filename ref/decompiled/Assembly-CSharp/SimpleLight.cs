public class SimpleLight : IOEntity
{
	public bool visibilityPassesThroughParent;

	public int powerConsumption = 1;

	public override bool VisibilityPassesThroughParent => visibilityPassesThroughParent;

	public override int ConsumptionAmount()
	{
		return powerConsumption;
	}

	public override void ResetIOState()
	{
		base.ResetIOState();
		if (IsOn())
		{
			SetFlagLocal(Flags.On, b: false);
			SendNetworkUpdate_Flags();
		}
	}

	public override void IOStateChanged(int inputAmount, int inputSlot)
	{
		base.IOStateChanged(inputAmount, inputSlot);
		bool num = IsOn();
		bool flag = IsPowered();
		if (num != flag)
		{
			SetFlagLocal(Flags.On, flag);
			SendNetworkUpdate_Flags();
		}
	}
}
