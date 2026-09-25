using UnityEngine;

public class TriggerBoatMagnet : TriggerBase, IServerComponent
{
	public BoatBuildingStation ParentStation;

	public float AngularVelocityLerpAmount = 10f;

	public float PositionLerpAmount = 10f;

	public float RotationLerpAmount = 10f;

	public SphereCollider SphereTrigger;

	[ServerVar(Help = "(Generated) When enabled, boat building station magnets are active and will magnetically attract compatible boat building blocks into position")]
	public static bool BoatMagnetsEnabled = true;

	private bool run;

	internal override GameObject InterestedInObject(GameObject obj)
	{
		BaseEntity baseEntity = GameObjectEx.ToBaseEntity(obj);
		if (baseEntity == null || baseEntity.isClient)
		{
			return null;
		}
		if (baseEntity is PlayerBoat playerBoat)
		{
			return playerBoat.gameObject;
		}
		PlayerBoat componentInParent = baseEntity.GetComponentInParent<PlayerBoat>();
		if (!(componentInParent != null))
		{
			return null;
		}
		return componentInParent.gameObject;
	}

	internal override void OnObjectAdded(GameObject obj, Collider col)
	{
		base.OnObjectAdded(obj, col);
		if (entityContents.Count == 1)
		{
			run = true;
		}
	}

	internal override void OnObjectRemoved(GameObject obj)
	{
		base.OnObjectRemoved(obj);
		if (entityContents.Count == 0)
		{
			run = false;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		run = false;
	}

	private void FixedUpdate()
	{
		if (!run || !BoatMagnetsEnabled || ParentStation.IsOn() || ParentStation.IsBusy() || entityContents == null)
		{
			return;
		}
		PlayerBoat targetBoat = GetTargetBoat();
		base.transform.GetPositionAndRotation(out var position, out var rotation);
		if (targetBoat != null && !targetBoat.Anchored && !targetBoat.rigidBody.isKinematic)
		{
			position.y = targetBoat.transform.position.y;
			targetBoat.transform.GetPositionAndRotation(out var position2, out var rotation2);
			float num = Mathf.InverseLerp(SphereTrigger.radius, 0f, Vector3.Distance(position2, position));
			if (targetBoat.EngineOn())
			{
				num = 0.05f;
			}
			targetBoat.rigidBody.angularVelocity = Vector3.Lerp(targetBoat.rigidBody.angularVelocity, Vector3.zero, Time.deltaTime * AngularVelocityLerpAmount * num);
			targetBoat.rigidBody.MovePosition(Vector3.Lerp(position2, position, Time.fixedDeltaTime * PositionLerpAmount * num));
			targetBoat.rigidBody.MoveRotation(Quaternion.Lerp(rotation2, rotation, Time.fixedDeltaTime * RotationLerpAmount * num));
		}
	}

	private PlayerBoat GetTargetBoat()
	{
		PlayerBoat result = null;
		float num = float.MaxValue;
		base.transform.GetPositionAndRotation(out var position, out var _);
		foreach (BaseEntity entityContent in entityContents)
		{
			if (!(entityContent == null) && entityContent is PlayerBoat { IsDying: false, IsDestructibleWreck: false } playerBoat)
			{
				float num2 = Vector3.Distance(playerBoat.transform.position, position);
				if (num2 < num)
				{
					num = num2;
					result = playerBoat;
				}
			}
		}
		return result;
	}
}
