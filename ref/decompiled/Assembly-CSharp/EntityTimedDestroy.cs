using UnityEngine;

public class EntityTimedDestroy : EntityComponent<BaseEntity>
{
	public float secondsTillDestroy = 1f;

	private void OnEnable()
	{
		Invoke(TimedDestroy, secondsTillDestroy);
	}

	public void SetTime(float newTime)
	{
		CancelInvoke(TimedDestroy);
		secondsTillDestroy = newTime;
		Invoke(TimedDestroy, secondsTillDestroy);
	}

	private void TimedDestroy()
	{
		if (base.baseEntity != null)
		{
			base.baseEntity.Kill();
		}
		else
		{
			Debug.LogWarning("EntityTimedDestroy failed, baseEntity was already null!");
		}
	}
}
