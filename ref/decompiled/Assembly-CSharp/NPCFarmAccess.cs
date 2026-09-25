public class NPCFarmAccess : NPCTalking
{
	public override void OnEntityMessage(BaseEntity from, string msg)
	{
		base.OnEntityMessage(from, msg);
		if (msg == "ForceOpenNPCDoor" && PointEntity<DeepSeaManager>.ServerInstance != null)
		{
			PointEntity<DeepSeaManager>.ServerInstance.RegisterPaidFoodToll(GetActionPlayer());
		}
	}
}
