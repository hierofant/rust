using UnityEngine;

public class RepositionOnGroundMissing : EntityComponent<BaseEntity>, IServerComponent
{
	public GameObjectRef originalPrefab;

	public bool killIfInvalid;

	public LayerMask castLayers = 10551552;

	private void OnGroundMissing()
	{
		Invoke(Process, 0.1f);
	}

	private void Process()
	{
		BaseEntity baseEntity = GameObjectEx.ToBaseEntity(base.gameObject);
		if (!(baseEntity == null))
		{
			BaseCombatEntity baseCombatEntity = baseEntity as BaseCombatEntity;
			Vector3 position = baseCombatEntity.transform.position;
			Quaternion rotation = baseCombatEntity.transform.rotation;
			if (GamePhysics.Trace(new Ray(base.transform.position, Vector3.down), 0f, out var hitInfo, 100f, castLayers))
			{
				position = hitInfo.point;
				rotation = Quaternion.FromToRotation(baseEntity.transform.up, hitInfo.normal) * baseCombatEntity.transform.rotation;
			}
			else
			{
				float height = TerrainMeta.HeightMap.GetHeight(base.transform.position);
				Vector3 normal = TerrainMeta.HeightMap.GetNormal(base.transform.position);
				position = baseEntity.ServerPosition.WithY(height);
				rotation = Quaternion.LookRotation(baseEntity.transform.forward, normal);
			}
			uint prefabID = (originalPrefab.isValid ? originalPrefab.resourceID : baseEntity.prefabID);
			if (baseEntity is ContainerCorpse containerCorpse)
			{
				prefabID = containerCorpse.entityToSpawn.resourceID;
			}
			if (!ContainerCorpse.IsValidPointForEntity(prefabID, position, rotation, baseEntity) && killIfInvalid)
			{
				Debug.LogWarning($"Killing {baseCombatEntity.ShortPrefabName} instead of repositioning as we couldn't find a valid position for {position}");
				baseCombatEntity.Kill();
			}
			else
			{
				baseEntity.ServerPosition = position;
				baseEntity.ServerRotation = rotation;
				baseEntity.SendNetworkUpdate();
			}
		}
	}
}
