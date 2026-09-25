using Facepunch;
using ProtoBuf;
using UnityEngine;

public class ElectricGenerator : IOEntity, IPowergridEntity
{
	public enum PowergridDisabledOption
	{
		PoweredOff,
		UseElectricAmount
	}

	[Header("Electric Generator")]
	public float electricAmount = 8f;

	[Tooltip("Powergrid must be at least this stage to be powered on. Stage 0 is powergrid off, meaning this generator would be unaffected by changes in the powergrid.")]
	public int requiredPowergridStage;

	[Tooltip("What should this generator do if requiredPowergridStage > 0 but servervar 'powergrid.enabled' is false?")]
	public PowergridDisabledOption ifPowergridDisabled;

	private int currentPowergridStage;

	private PuzzleReset cachedPuzzleReset;

	public override bool IsRootEntity()
	{
		return true;
	}

	public override int MaximalPowerOutput()
	{
		return Mathf.FloorToInt(electricAmount);
	}

	public override int ConsumptionAmount()
	{
		return 0;
	}

	bool IPowergridEntity.Server_ShouldConnectToPowergrid()
	{
		return requiredPowergridStage > 0;
	}

	void IPowergridEntity.Server_OnPowergridStageChanged(int newStage)
	{
		currentPowergridStage = newStage;
		currentEnergy = GetCurrentEnergy();
		UpdateHasPower(currentEnergy, 0);
		MarkDirtyForceUpdateOutputs();
	}

	public override int CalculateCurrentEnergy(int inputAmount, int inputSlot)
	{
		return GetCurrentEnergy();
	}

	public override int GetCurrentEnergy()
	{
		int result = (int)electricAmount;
		if (requiredPowergridStage > 0)
		{
			if (Powergrid.enabled && PointEntity<PowergridManager>.ServerInstance != null)
			{
				if (currentPowergridStage < requiredPowergridStage)
				{
					return 0;
				}
				return result;
			}
			if (ifPowergridDisabled == PowergridDisabledOption.PoweredOff)
			{
				return 0;
			}
		}
		return result;
	}

	public override bool GetHasPower(int inputAmount, int inputSlot)
	{
		if (requiredPowergridStage > 0)
		{
			if (Powergrid.enabled && PointEntity<PowergridManager>.ServerInstance != null)
			{
				if (currentPowergridStage >= requiredPowergridStage)
				{
					return base.GetHasPower(inputAmount, inputSlot);
				}
				return false;
			}
			if (ifPowergridDisabled == PowergridDisabledOption.PoweredOff)
			{
				return false;
			}
		}
		return base.GetHasPower(inputAmount, inputSlot);
	}

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		return GetCurrentEnergy();
	}

	public override void UpdateOutputs()
	{
		currentEnergy = GetCurrentEnergy();
		IOSlot[] array = outputs;
		foreach (IOSlot iOSlot in array)
		{
			IOEntity iOEntity = iOSlot.connectedTo.Get();
			if (iOEntity != null)
			{
				iOEntity.UpdateFromInput(currentEnergy, iOSlot.connectedToSlot);
			}
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		Invoke(ForcePuzzleReset, 4f);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		cachedPuzzleReset = GetComponent<PuzzleReset>();
		if (!enableSaving)
		{
			Invoke(ForcePuzzleReset, 4f);
		}
	}

	private void ForcePuzzleReset()
	{
		PuzzleReset component = GetComponent<PuzzleReset>();
		if (component != null)
		{
			component.TryForceReset();
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.forDisk)
		{
			if (cachedPuzzleReset != null)
			{
				cachedPuzzleReset.Save(info);
			}
			info.msg.electricGenerator = Pool.Get<ProtoBuf.ElectricGenerator>();
			info.msg.electricGenerator.requiredPowergridStage = requiredPowergridStage;
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (!info.fromDisk)
		{
			return;
		}
		if (info.msg.puzzleReset != null && cachedPuzzleReset != null)
		{
			cachedPuzzleReset.Load(info);
		}
		if (info.msg.electricGenerator != null)
		{
			bool flag = requiredPowergridStage > 0;
			requiredPowergridStage = info.msg.electricGenerator.requiredPowergridStage;
			if (requiredPowergridStage > 0 && !flag)
			{
				PowergridManager.Server_AddPowergridEntity(this);
			}
		}
	}
}
