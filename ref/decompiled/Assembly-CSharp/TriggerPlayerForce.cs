using UnityEngine;

public class TriggerPlayerForce : TriggerBase, IServerComponent
{
	public Collider triggerCollider;

	public float pushVelocity = 5f;

	public bool requireUpAxis;

	public bool pushDown;

	private const float HACK_DISABLE_TIME = 4f;

	internal override GameObject InterestedInObject(GameObject obj)
	{
		obj = base.InterestedInObject(obj);
		if (obj == null)
		{
			return null;
		}
		BaseEntity baseEntity = GameObjectEx.ToBaseEntity(obj);
		if (baseEntity != null && baseEntity.isServer)
		{
			return baseEntity.gameObject;
		}
		return null;
	}

	internal override void OnObjects()
	{
		InvokeRepeatingFixedTime(ProcessContentsFixed);
		InvokeRepeating(HackDisableTick, 0f, 3.75f);
	}

	internal override void OnEmpty()
	{
		base.OnEmpty();
		CancelInvokeFixedTime(ProcessContentsFixed);
		CancelInvoke(HackDisableTick);
	}

	protected override void OnDisable()
	{
		CancelInvoke(HackDisableTick);
		base.OnDisable();
	}

	internal override void OnEntityLeave(BaseEntity ent)
	{
		base.OnEntityLeave(ent);
		ent.ApplyInheritedVelocity(Vector3.zero);
	}

	private void HackDisableTick()
	{
		if (entityContents == null || !base.enabled)
		{
			return;
		}
		foreach (BaseEntity entityContent in entityContents)
		{
			if (IsInterested(entityContent))
			{
				BasePlayer basePlayer = entityContent.ToPlayer();
				if (basePlayer != null && !basePlayer.IsNpc)
				{
					basePlayer.PauseVehicleNoClipDetection(4f);
					basePlayer.PauseSpeedHackDetection(4f);
				}
			}
		}
	}

	private void ProcessContentsFixed()
	{
		if (entityContents == null)
		{
			return;
		}
		foreach (BaseEntity entityContent in entityContents)
		{
			if ((!requireUpAxis || !(Vector3.Dot(entityContent.transform.up, base.transform.up) < 0f)) && IsInterested(entityContent))
			{
				Vector3 velocity = GetPushVelocity(entityContent.gameObject);
				entityContent.ApplyInheritedVelocity(velocity);
			}
		}
	}

	private Vector3 GetPushVelocity(GameObject obj)
	{
		Vector3 position = obj.transform.position;
		Vector3 zero = Vector3.zero;
		if (pushDown)
		{
			zero = -triggerCollider.transform.up;
		}
		else
		{
			zero = position - triggerCollider.bounds.center;
			zero.Normalize();
			zero.y = 0.2f;
			zero.Normalize();
		}
		return zero * pushVelocity;
	}

	private bool IsInterested(BaseEntity entity)
	{
		if (entity == null || entity.isClient)
		{
			return false;
		}
		BasePlayer basePlayer = entity.ToPlayer();
		if (basePlayer != null)
		{
			if ((basePlayer.IsAdmin || basePlayer.IsDeveloper) && basePlayer.IsFlying)
			{
				return false;
			}
			if (basePlayer != null && basePlayer.IsAlive())
			{
				return !basePlayer.isMounted;
			}
			return false;
		}
		return true;
	}
}
