using Oxide.Core;
using UnityEngine;

public class TriggeredEventPrefab : TriggeredEvent
{
	public GameObjectRef targetPrefab;

	public bool shouldBroadcastSpawn;

	public WorldNotificationConfig.NotificationType notificationType;

	public BaseEntity spawnedEntity;

	public override void RunEvent()
	{
		if (Interface.CallHook("OnEventTrigger", this) != null)
		{
			return;
		}
		Debug.Log("[event] " + targetPrefab.resourcePath);
		BaseEntity baseEntity = GameManager.server.CreateEntity(targetPrefab.resourcePath);
		if ((bool)baseEntity)
		{
			baseEntity.SendMessage("TriggeredEventSpawn", SendMessageOptions.DontRequireReceiver);
			baseEntity.Spawn();
			baseEntity.SendMessage("TriggeredEventPostSpawn", SendMessageOptions.DontRequireReceiver);
			spawnedEntity = baseEntity;
			if (shouldBroadcastSpawn)
			{
				BasePlayer.Server_SendWorldNotificationToAllActivePlayers(notificationType, spawnedEntity.transform.position);
			}
		}
	}

	public override void Kill()
	{
		if (!(spawnedEntity == null))
		{
			base.Kill();
			spawnedEntity.Kill();
			spawnedEntity = null;
			Debug.Log("Killed " + base.name);
		}
	}
}
