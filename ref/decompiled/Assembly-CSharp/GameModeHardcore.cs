using ConVar;

public class GameModeHardcore : GameModeVanilla
{
	protected override void OnCreated()
	{
		base.OnCreated();
	}

	public override void InitShared()
	{
		base.InitShared();
	}

	public override void DestroyShared()
	{
		base.DestroyShared();
	}

	protected override float GetCraftingCostConVar(CraftingCostConVar conVar)
	{
		if (conVar == CraftingCostConVar.HardcoreFirearmAmmunition)
		{
			return Server.hardcoreFirearmAmmunitionCraftingMultiplier;
		}
		return base.GetCraftingCostConVar(conVar);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		foreach (BaseNetworkable serverEntity in BaseNetworkable.serverEntities)
		{
			if (serverEntity is Recycler recycler)
			{
				recycler.UpdateInSafeZone();
			}
		}
	}
}
