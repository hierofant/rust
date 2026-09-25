using UnityEngine;

public static class VehicleAI
{
	public const string BOAT_AI_PATH = "assets/prefabs/npc/travelling vendor/travellingvendor.prefab";

	public static void AttachBoatAI(BaseBoat boat)
	{
		if (PrefabAttribute.server.Find<AIDriverData>(boat.prefabID) == null)
		{
			Debug.LogError($"No AIDriverData found for {boat.name} with prefabID {boat.prefabID}. Can't attach AI.");
		}
		GameManager.server.CreatePrefab("assets/prefabs/npc/travelling vendor/travellingvendor.prefab", boat.transform);
	}
}
