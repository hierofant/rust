using System;
using System.Collections.Generic;
using Facepunch;
using Facepunch.Rust;
using UnityEngine;

public class VehicleSpawner : BaseEntity
{
	public interface IVehicleSpawnUser
	{
		string ShortPrefabName { get; }

		bool IsClient { get; }

		bool IsDestroyed { get; }

		void SetupOwner(BasePlayer owner, Vector3 newSafeAreaOrigin, float newSafeAreaRadius);

		bool IsDespawnEligable();

		IFuelSystem GetFuelSystem();

		int StartingFuelUnits();

		void Kill(DestroyMode mode, bool runCallbacks);
	}

	public enum VehicleSpawnerType
	{
		Unknown,
		Boats,
		Helicopter,
		Horse
	}

	[Serializable]
	public class SpawnPair
	{
		public string message;

		public GameObjectRef prefabToSpawn;
	}

	public float spawnNudgeRadius = 6f;

	public float cleanupRadius = 10f;

	public float occupyRadius = 5f;

	public VehicleSpawnerType spawnerType;

	public TriggerBase additionalNudgeTrigger;

	public SpawnPair[] objectsToSpawn;

	public Transform spawnOffset;

	public float safeRadius = 10f;

	protected virtual bool LogAnalytics => true;

	public virtual int GetOccupyLayer()
	{
		return 32768;
	}

	public IVehicleSpawnUser GetVehicleOccupying()
	{
		IVehicleSpawnUser result = null;
		List<IVehicleSpawnUser> obj = Pool.Get<List<IVehicleSpawnUser>>();
		Vis.Entities(spawnOffset.transform.position, occupyRadius, obj, GetOccupyLayer(), QueryTriggerInteraction.Ignore);
		if (obj.Count > 0)
		{
			result = obj[0];
		}
		Pool.FreeUnmanaged(ref obj);
		return result;
	}

	public bool IsPadOccupied()
	{
		IVehicleSpawnUser vehicleOccupying = GetVehicleOccupying();
		if (vehicleOccupying != null)
		{
			return !vehicleOccupying.IsDespawnEligable();
		}
		return false;
	}

	public override void OnEntityMessage(BaseEntity from, string msg)
	{
		BasePlayer newOwner = null;
		NPCTalking component = from.GetComponent<NPCTalking>();
		if ((bool)component)
		{
			newOwner = component.GetActionPlayer();
		}
		SpawnPair[] array = objectsToSpawn;
		foreach (SpawnPair spawnPair in array)
		{
			if (msg == spawnPair.message)
			{
				SpawnVehicle(spawnPair.prefabToSpawn.resourcePath, newOwner);
				break;
			}
		}
	}

	public IVehicleSpawnUser SpawnVehicle(string prefabToSpawn, BasePlayer newOwner)
	{
		CleanupArea(cleanupRadius);
		NudgePlayersInRadius(spawnNudgeRadius);
		BaseEntity baseEntity = GameManager.server.CreateEntity(prefabToSpawn, spawnOffset.transform.position, spawnOffset.transform.rotation);
		baseEntity.Spawn();
		IVehicleSpawnUser component = baseEntity.GetComponent<IVehicleSpawnUser>();
		if (newOwner != null)
		{
			component.SetupOwner(newOwner, spawnOffset.transform.position, safeRadius);
		}
		VehicleSpawnPoint.AddStartingFuel(component);
		VehicleSpawnPoint.AddStartingFlares(baseEntity.GetComponent<ICanFireHelicopterFlares>());
		if (newOwner != null)
		{
			Analytics.Azure.OnVehiclePurchased(newOwner, baseEntity);
		}
		return component;
	}

	public void CleanupArea(float radius)
	{
		List<IVehicleSpawnUser> obj = Pool.Get<List<IVehicleSpawnUser>>();
		Vis.Entities(spawnOffset.transform.position, radius, obj, 32768);
		foreach (IVehicleSpawnUser item in obj)
		{
			if (!item.IsClient && !item.IsDestroyed && (spawnerType != VehicleSpawnerType.Boats || !(item is BaseHelicopter)))
			{
				item.Kill(DestroyMode.None, runCallbacks: true);
			}
		}
		List<ServerGib> obj2 = Pool.Get<List<ServerGib>>();
		Vis.Entities(spawnOffset.transform.position, radius, obj2, -2147483647);
		foreach (ServerGib item2 in obj2)
		{
			if (!item2.isClient)
			{
				item2.Kill();
			}
		}
		Pool.FreeUnmanaged(ref obj);
		Pool.FreeUnmanaged(ref obj2);
	}

	public void NudgePlayersInRadius(float radius)
	{
		List<BasePlayer> obj = Pool.Get<List<BasePlayer>>();
		Vis.Entities(spawnOffset.transform.position, radius, obj, 131072);
		foreach (BasePlayer item in obj)
		{
			if ((!additionalNudgeTrigger || (additionalNudgeTrigger.HasAnyEntityContents && additionalNudgeTrigger.entityContents.Contains(item))) && !item.IsNpc && !item.isMounted && item.IsConnected)
			{
				Vector3 position = spawnOffset.transform.position;
				position += Vector3Ex.Direction2D(item.transform.position, spawnOffset.transform.position) * radius;
				position += Vector3.up * 0.1f;
				item.MovePosition(position);
				item.ClientRPC(RpcTarget.Player("ForcePositionTo", item), position);
			}
		}
		Pool.FreeUnmanaged(ref obj);
	}
}
