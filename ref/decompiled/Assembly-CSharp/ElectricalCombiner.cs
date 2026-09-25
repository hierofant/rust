using UnityEngine;

public class ElectricalCombiner : IOEntity
{
	public int input1Amount;

	public int input2Amount;

	public int input3Amount;

	public override bool BlockFluidDraining => true;

	public override bool IsRootEntity()
	{
		return true;
	}

	public override int ConsumptionAmount()
	{
		return 0;
	}

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		int num = input1Amount + input2Amount + input3Amount;
		Mathf.Clamp(num, 0, num);
		return num;
	}

	public override void UpdateHasPower(int inputAmount, int inputSlot)
	{
		SetFlagLocal(Flags.Reserved8, input1Amount > 0 || input2Amount > 0);
	}

	public override void UpdateFromInput(int inputAmount, int slot)
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate_Flags);
		if (inputAmount > 0 && IsConnectedTo(this, slot, IOEntity.backtracking * 2, defaultReturn: true))
		{
			inputAmount = 0;
			flagsUpdateScope.Set(Flags.Reserved7, b: true);
		}
		else
		{
			flagsUpdateScope.Set(Flags.Reserved7, b: false);
		}
		switch (slot)
		{
		case 0:
			input1Amount = inputAmount;
			break;
		case 1:
			input2Amount = inputAmount;
			break;
		case 2:
			input3Amount = inputAmount;
			break;
		}
		int num = input1Amount + input2Amount + input3Amount;
		bool b = num > 0;
		flagsUpdateScope.Set(Flags.Reserved1, input1Amount > 0);
		flagsUpdateScope.Set(Flags.Reserved2, input2Amount > 0);
		flagsUpdateScope.Set(Flags.Reserved3, b);
		flagsUpdateScope.Set(Flags.Reserved4, input1Amount > 0 || input2Amount > 0 || input3Amount > 0);
		flagsUpdateScope.Set(Flags.On, num > 0);
		base.UpdateFromInput(num, slot);
	}
}
