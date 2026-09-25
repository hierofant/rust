using UnityEngine;

public class PowergridIOAccessPoint : IOEntity, IPowergridEntity
{
	[Header("Powergrid IO Access Point")]
	public Translate.Phrase displayName;

	[Tooltip("Minimum wire slack enforced on wires connected to this entity's ports.")]
	public float minWireSlack = 0.8f;

	[Header("Electricity Loop Sound")]
	public GameObject soundEmitterObject;

	public SoundDefinition electricityLoopSound;

	private int currentPowergridStage;

	public override bool IsRootEntity()
	{
		return false;
	}

	public override float GetMinWireSlack(bool isInput, int slotIndex)
	{
		return minWireSlack;
	}

	public override int MaximalPowerOutput()
	{
		return 9999;
	}

	public override int ConsumptionAmount()
	{
		return 0;
	}

	public override bool AllowWireConnections()
	{
		if (Powergrid.enabled)
		{
			return base.AllowWireConnections();
		}
		return false;
	}

	public override bool CanSkipWireToolBuildAuthorisation()
	{
		return true;
	}

	public override bool CanBreakConnection(BasePlayer player, int slotIndex, bool isInput)
	{
		if (!TryGetConnectedTo(slotIndex, isInput, out var connectedTo))
		{
			return base.CanBreakConnection(player, slotIndex, isInput);
		}
		return player.CanBuild(connectedTo.transform.position, connectedTo.transform.rotation, connectedTo.bounds);
	}

	public override Translate.Phrase GetDisplayName()
	{
		return displayName;
	}

	bool IPowergridEntity.Server_ShouldConnectToPowergrid()
	{
		return true;
	}

	void IPowergridEntity.Server_OnPowergridStageChanged(int newStage)
	{
		currentPowergridStage = newStage;
		currentEnergy = GetCurrentEnergy();
		MarkDirtyForceUpdateOutputs();
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate_Flags);
		flagsUpdateScope.Set(Flags.Reserved8, currentEnergy > 0);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		PowergridManager.Server_AddPowergridAccessPoint(this);
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		PowergridManager.Server_RemovePowergridAccessPoint(this);
	}

	public override int CalculateCurrentEnergy(int inputAmount, int inputSlot)
	{
		return GetCurrentEnergy();
	}

	public override int GetCurrentEnergy()
	{
		if (PointEntity<PowergridManager>.ServerInstance == null)
		{
			return 0;
		}
		if (PowergridStageConfig.instance == null)
		{
			return 0;
		}
		int num = PointEntity<PowergridManager>.ServerInstance.Server_GetPowerPlantInsertedFuses();
		if (num <= 0)
		{
			return 0;
		}
		int num2 = PointEntity<PowergridManager>.ServerInstance.Server_GetFuseSocketsCount();
		float t = ((num2 > 1) ? Mathf.Clamp01((float)(num - 1) / (float)(num2 - 1)) : 0f);
		return (int)Mathf.Lerp(Powergrid.powerlineBasePowerOutput, Powergrid.powerlineMaxPowerOutput, t);
	}

	public override bool GetHasPower(int inputAmount, int inputSlot)
	{
		if (Powergrid.enabled && PointEntity<PowergridManager>.ServerInstance != null)
		{
			return currentEnergy > 0;
		}
		return false;
	}
}
