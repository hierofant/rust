using System;
using UnityEngine;

public class XORSwitch : IOEntity
{
	private int input1Amount;

	private int input2Amount;

	private bool input1Set;

	private bool input2Set;

	private bool firstRun = true;

	private Action _actionUpdateFlagsDelayed;

	private Action actionUpdateFlagsDelayed => UpdateFlags_Delayed;

	public override int ConsumptionAmount()
	{
		return 0;
	}

	public override bool WantsPower(int inputIndex)
	{
		bool num = input1Amount > 0;
		bool flag = input2Amount > 0;
		return num != flag;
	}

	public override void ResetState()
	{
		base.ResetState();
		input1Set = false;
		input2Set = false;
		firstRun = true;
	}

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		if ((HasFlag(Flags.Reserved1) && !input1Set) || (HasFlag(Flags.Reserved2) && !input2Set))
		{
			return 0;
		}
		if (input1Amount > 0 && input2Amount > 0)
		{
			return 0;
		}
		int b = Mathf.Max(input1Amount, input2Amount);
		return Mathf.Max(0, b);
	}

	public override void UpdateHasPower(int inputAmount, int inputSlot)
	{
		SetFlagLocal(Flags.Reserved8, input1Amount > 0 || input2Amount > 0);
	}

	public override void IOStateChanged(int inputAmount, int inputSlot)
	{
		base.IOStateChanged(inputAmount, inputSlot);
	}

	public override void UpdateFromInput(int inputAmount, int slot)
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		if (inputAmount > 0 && IsConnectedTo(this, slot, IOEntity.backtracking))
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
			input1Set = true;
			input1Amount = inputAmount;
			break;
		case 1:
			input2Set = true;
			input2Amount = inputAmount;
			break;
		}
		if (firstRun)
		{
			if (!IsInvoking(actionUpdateFlagsDelayed))
			{
				Invoke(actionUpdateFlagsDelayed, 0.1f);
			}
		}
		else
		{
			UpdateFlags(networkUpdate: false);
		}
		firstRun = false;
		base.UpdateFromInput(inputAmount, slot);
	}

	private void UpdateFlags(bool networkUpdate)
	{
		int num = ((input1Amount <= 0 || input2Amount <= 0) ? Mathf.Max(input1Amount, input2Amount) : 0);
		bool b = num > 0;
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(networkUpdate ? FlagsUpdateMode.SendNetworkUpdate_Flags : FlagsUpdateMode.Local);
		flagsUpdateScope.Set(Flags.Reserved1, input1Amount > 0);
		flagsUpdateScope.Set(Flags.Reserved2, input2Amount > 0);
		flagsUpdateScope.Set(Flags.Reserved3, b);
		flagsUpdateScope.Set(Flags.Reserved4, input1Amount > 0 || input2Amount > 0);
		flagsUpdateScope.Set(Flags.On, num > 0);
	}

	private void UpdateFlags_Delayed()
	{
		UpdateFlags(networkUpdate: true);
	}
}
