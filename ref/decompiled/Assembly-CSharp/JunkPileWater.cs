using System;
using UnityEngine;

public class JunkPileWater : JunkPile, IBudgetedFloatingEntity, IDestroyableOnPlayerBoatCollision
{
	public class JunkpileWaterWorkQueue : PersistentObjectWorkQueue<IBudgetedFloatingEntity>
	{
		protected override void RunJob(IBudgetedFloatingEntity entity)
		{
			if (ShouldAdd(entity))
			{
				entity.UpdateNearbyPlayers();
			}
		}

		protected override bool ShouldAdd(IBudgetedFloatingEntity entity)
		{
			if (base.ShouldAdd(entity))
			{
				return entity.Entity.IsValid();
			}
			return false;
		}
	}

	public static JunkpileWaterWorkQueue junkpileWaterWorkQueue = new JunkpileWaterWorkQueue();

	[ServerVar]
	[Help("How many milliseconds to budget for processing junk pile updates per frame")]
	public static float framebudgetms = 0.05f;

	public Transform[] buoyancyPoints;

	public bool debugDraw;

	public float updateCullRange = 16f;

	public float VehicleCheckRadius = 5f;

	public Rigidbody Body;

	[Range(0f, 1f)]
	public float buoyancyAmplitude = 1f;

	[ServerVar]
	public static bool DestroyableByPlayerBoats = true;

	[ServerVar]
	public static float MinimumPlayerBoatMassToBeDestroyed = 2000f;

	[ServerVar]
	public static float MinimumPlayerBoatVelocityToBeDestroyed = 5f;

	private Action updateMovementFixedTick;

	private Quaternion baseRotation = Quaternion.identity;

	private bool first = true;

	private bool hasPlayersNearby;

	private TimeUntil nextPlayerCheck;

	public BaseEntity Entity => this;

	public override void Spawn()
	{
		Vector3 position = base.transform.position;
		position.y = WaterLevel.GetWaterSurface(base.transform.position, waves: false, volumes: false);
		base.transform.position = position;
		base.Spawn();
		baseRotation = Quaternion.Euler(0f, base.transform.rotation.eulerAngles.y, 0f);
		if (Physics.CheckSphere(base.transform.position, VehicleCheckRadius, 134217728))
		{
			Kill();
		}
		else
		{
			KillIfInMonument();
		}
	}

	private void KillIfInMonument()
	{
		if (TerrainMeta.TopologyMap != null && ((uint)TerrainMeta.TopologyMap.GetTopology(base.transform.position) & 0x400u) != 0 && TerrainMeta.Path != null)
		{
			Kill();
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		InvokeRandomized(EnqueueNearPlayersCheck, 0f, 0.75f, 0.25f);
	}

	public void EnqueueNearPlayersCheck()
	{
		junkpileWaterWorkQueue.Add(this);
	}

	public void UpdateMovementFixedTick()
	{
		if (!isSinking)
		{
			SimpleBuoyancyUpdate(buoyancyPoints, base.transform, ref baseRotation, Body, ref first, debugDraw, 1f, buoyancyAmplitude);
		}
	}

	public static void SimpleBuoyancyUpdate(Transform[] buoyancyPoints, Transform forTransform, ref Quaternion baseRotation, Rigidbody body, ref bool first, bool debugDraw, float movementMultiplier = 1f, float buoyancyAmplitude = 1f)
	{
		if (buoyancyPoints != null && buoyancyPoints.Length >= 3)
		{
			Vector3 position = forTransform.position;
			Vector3 localPosition = buoyancyPoints[0].localPosition;
			Vector3 localPosition2 = buoyancyPoints[1].localPosition;
			Vector3 localPosition3 = buoyancyPoints[2].localPosition;
			Vector3 vector = localPosition + position;
			Vector3 vector2 = localPosition2 + position;
			Vector3 vector3 = localPosition3 + position;
			vector.y = WaterLevel.GetWaterSurface(vector, waves: true, volumes: false);
			vector2.y = WaterLevel.GetWaterSurface(vector2, waves: true, volumes: false);
			vector3.y = WaterLevel.GetWaterSurface(vector3, waves: true, volumes: false);
			Vector3 vector4 = new Vector3(position.x, vector.y - localPosition.y, position.z);
			Vector3 rhs = vector2 - vector;
			Vector3 vector5 = Vector3.Cross(vector3 - vector, rhs);
			Vector3 eulerAngles = Quaternion.LookRotation(new Vector3(vector5.x, vector5.z, vector5.y)).eulerAngles;
			Quaternion quaternion = Quaternion.Euler(0f - eulerAngles.x, 0f, 0f - eulerAngles.y);
			if (first)
			{
				baseRotation = Quaternion.Euler(0f, forTransform.rotation.eulerAngles.y, 0f);
				first = false;
			}
			Vector3 position2 = Vector3.Lerp(forTransform.position, vector4, movementMultiplier);
			Quaternion quaternion2 = Quaternion.Lerp(forTransform.rotation, quaternion * baseRotation, movementMultiplier);
			if (!Mathf.Approximately(buoyancyAmplitude, 1f))
			{
				float waterSurface = WaterLevel.GetWaterSurface(vector, waves: false, volumes: false);
				position2 = Vector3.Lerp(vector4.WithY(waterSurface), vector4, buoyancyAmplitude);
				quaternion2 = Quaternion.Slerp(baseRotation, quaternion2, buoyancyAmplitude);
			}
			if (!body)
			{
				forTransform.SetPositionAndRotation(position2, quaternion2);
				return;
			}
			body.MovePosition(position2);
			body.MoveRotation(quaternion2);
		}
		else
		{
			float waterSurface2 = WaterLevel.GetWaterSurface(forTransform.position, waves: true, volumes: false);
			if (!body)
			{
				forTransform.position = new Vector3(forTransform.position.x, waterSurface2, forTransform.position.z);
			}
			else
			{
				body.MovePosition(new Vector3(body.position.x, waterSurface2, body.position.z));
			}
		}
	}

	public void UpdateNearbyPlayers()
	{
		if ((float)nextPlayerCheck > 0f)
		{
			return;
		}
		nextPlayerCheck = UnityEngine.Random.Range(0.5f, 1f);
		hasPlayersNearby = BaseNetworkable.HasCloseConnections(base.transform.position, updateCullRange);
		ToggleNetworkPositionTick(hasPlayersNearby);
		if (updateMovementFixedTick == null)
		{
			updateMovementFixedTick = UpdateMovementFixedTick;
		}
		if (hasPlayersNearby)
		{
			if (!IsInvokingFixedTime(updateMovementFixedTick))
			{
				InvokeRepeatingFixedTime(updateMovementFixedTick);
			}
		}
		else
		{
			CancelInvokeFixedTime(updateMovementFixedTick);
		}
	}

	public bool ShouldBeDestroyedBy(PlayerBoat boat)
	{
		return ShouldJunkpileBeDestroyedBy(boat);
	}

	public virtual bool ShouldJunkpileBeDestroyedBy(PlayerBoat boat)
	{
		if (boat == null)
		{
			return false;
		}
		if (!DestroyableByPlayerBoats)
		{
			return false;
		}
		if (boat.rigidBody.mass >= MinimumPlayerBoatMassToBeDestroyed)
		{
			return boat.rigidBody.linearVelocity.magnitude >= MinimumPlayerBoatVelocityToBeDestroyed;
		}
		return false;
	}
}
