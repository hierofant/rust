using Rust;
using UnityEngine;

public class SpawnPointInstance : MonoBehaviour
{
	internal BaseEntity Entity;

	public ISpawnPointUser parentSpawnPointUser;

	public BaseSpawnPoint parentSpawnPoint;

	private bool notified;

	public bool blockSpawnHandlerRespawns { get; set; }

	public void Notify()
	{
		if (!notified)
		{
			if (!ObjectEx.IsUnityNull(parentSpawnPointUser))
			{
				parentSpawnPointUser.ObjectSpawned(this);
			}
			if ((bool)parentSpawnPoint)
			{
				parentSpawnPoint.ObjectSpawned(this);
			}
			notified = true;
		}
	}

	public void Retire()
	{
		if (notified)
		{
			if (!ObjectEx.IsUnityNull(parentSpawnPointUser))
			{
				parentSpawnPointUser.ObjectRetired(this);
			}
			if ((bool)parentSpawnPoint)
			{
				parentSpawnPoint.ObjectRetired(this);
			}
		}
	}

	protected void OnDestroy()
	{
		if (!Rust.Application.isQuitting)
		{
			Retire();
		}
	}
}
