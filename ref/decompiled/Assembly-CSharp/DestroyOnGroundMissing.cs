using ConVar;
using Oxide.Core;
using UnityEngine;

public class DestroyOnGroundMissing : MonoBehaviour, IServerComponent
{
	private void OnGroundMissing()
	{
		BaseEntity baseEntity = GameObjectEx.ToBaseEntity(base.gameObject);
		if (baseEntity != null && Interface.CallHook("OnEntityGroundMissing", baseEntity) == null)
		{
			BaseCombatEntity baseCombatEntity = baseEntity as BaseCombatEntity;
			if (Stability.log_ground_missing_death)
			{
				Debug.Log($"Killing '{baseEntity.ToString()}' at position {base.transform.position} due to ground missing");
			}
			if (baseCombatEntity != null)
			{
				baseCombatEntity.Die();
			}
			else
			{
				baseEntity.Kill(BaseNetworkable.DestroyMode.Gib);
			}
		}
	}
}
