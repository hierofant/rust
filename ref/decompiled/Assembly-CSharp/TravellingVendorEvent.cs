using UnityEngine;

public class TravellingVendorEvent : TriggeredEvent
{
	public static TravellingVendor currentVendor = null;

	public static float dontSpawnHoursBeforeWipe = 24f;

	public override void RunEvent()
	{
		if (!(currentVendor != null) && !(TerrainMeta.Path == null) && TerrainMeta.Path.Roads.Count != 0 && TravellingVendor.should_spawn && RoadBradleys.StaticBradleyCount <= 0 && TerrainMeta.Path.MainRoads.Count != 0)
		{
			TravellingVendor travellingVendor = TravellingVendor.SpawnTravellingVendorForEvent();
			if ((bool)travellingVendor)
			{
				Debug.Log("[event] assets/prefabs/npc/travelling vendor/travellingvendor.prefab");
				currentVendor = travellingVendor;
				BasePlayer.Server_SendWorldNotificationToAllActivePlayers(WorldNotificationConfig.NotificationType.TravellingVendorSpawned, currentVendor.transform.position);
			}
			else
			{
				Debug.Log("Failed to spawn travelling vendor.");
			}
		}
	}

	private bool HoursCheck()
	{
		if (WipeTimer.serverinstance.GetTimeSpanUntilWipe().TotalHours > (double)dontSpawnHoursBeforeWipe)
		{
			return true;
		}
		return false;
	}
}
