using UnityEngine;

public class TriggerPlayerMovePos : TriggerBase, IServerComponent
{
	public BoxCollider triggerCollider;

	public Vector3 relativeMoveVector = Vector3.up;

	public bool shouldPauseMarkHostile;

	private const float HACK_DISABLE_TIME = 1.5f;

	internal override GameObject InterestedInObject(GameObject obj)
	{
		obj = base.InterestedInObject(obj);
		if (obj == null)
		{
			return null;
		}
		BaseEntity baseEntity = GameObjectEx.ToBaseEntity(obj);
		if (baseEntity != null)
		{
			return baseEntity.gameObject;
		}
		return null;
	}

	internal override void OnObjects()
	{
		InvokeRepeating(HackDisableTick, 0f, 1.25f);
	}

	internal override void OnEmpty()
	{
		base.OnEmpty();
		CancelInvoke(HackDisableTick);
	}

	protected override void OnDisable()
	{
		CancelInvoke(HackDisableTick);
		base.OnDisable();
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
					basePlayer.PauseVehicleNoClipDetection(1.5f);
					basePlayer.PauseSpeedHackDetection(1.5f);
				}
			}
		}
	}

	protected void FixedUpdate()
	{
		if (entityContents == null)
		{
			return;
		}
		foreach (BaseEntity entityContent in entityContents)
		{
			if (IsInterested(entityContent))
			{
				BasePlayer basePlayer = entityContent.ToPlayer();
				if (basePlayer != null && shouldPauseMarkHostile)
				{
					basePlayer.SetHostilePauseTime();
				}
				entityContent.transform.position = triggerCollider.bounds.center + relativeMoveVector;
			}
		}
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
			if (basePlayer.IsNpc)
			{
				return false;
			}
			if (basePlayer.IsAlive() && !basePlayer.IsSleeping())
			{
				return !basePlayer.isMounted;
			}
			return false;
		}
		return false;
	}
}
