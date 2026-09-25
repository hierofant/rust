using UnityEngine;

public class RANDSwitch : ElectricalBlocker
{
	private bool rand;

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		return GetCurrentEnergy() * (IsOn() ? 1 : 0);
	}

	public override bool WantsPower(int inputIndex)
	{
		if (inputIndex == 0)
		{
			return IsOn();
		}
		return false;
	}

	public override void UpdateBlocked()
	{
		bool flag = IsOn();
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.Local))
		{
			flagsUpdateScope.Set(Flags.On, rand);
			flagsUpdateScope.Set(Flags.Reserved8, rand);
		}
		UpdateHasPower(input1Amount + input2Amount, 1);
		if (flag != IsOn())
		{
			MarkDirty();
		}
	}

	public bool RandomRoll()
	{
		return Random.Range(0, 2) == 1;
	}

	public override void UpdateFromInput(int inputAmount, int inputSlot)
	{
		if (inputSlot == 1 && inputAmount > 0)
		{
			input1Amount = inputAmount;
			rand = RandomRoll();
			UpdateBlocked();
		}
		if (inputSlot == 2)
		{
			if (inputAmount > 0)
			{
				rand = false;
				UpdateBlocked();
			}
		}
		else
		{
			base.UpdateFromInput(inputAmount, inputSlot);
		}
	}
}
