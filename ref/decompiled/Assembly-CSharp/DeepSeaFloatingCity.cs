public class DeepSeaFloatingCity : BaseEntity
{
	public GameObjectRef MapMarker;

	public override void ServerInit()
	{
		base.ServerInit();
		DeepSeaManager.ServerFloatingCities.Add(this);
		BakedShoreVectors bakedShoreVectors = PrefabAttribute.server.Find<BakedShoreVectors>(prefabID);
		if (bakedShoreVectors != null)
		{
			Invoke(delegate
			{
				TerrainMeta.Texturing.ApplyBakedDeepSeaVectors(bakedShoreVectors, base.transform);
			}, 1f);
		}
		if (MapMarker.isValid)
		{
			base.gameManager.CreateEntity(MapMarker.resourcePath, base.transform.position, base.transform.rotation).Spawn();
		}
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		DeepSeaManager.ServerFloatingCities.Remove(this);
	}

	public override void AdminKill()
	{
	}

	public void TriggerWipeAlarm(GameObjectRef effectPrefab)
	{
		Effect.server.Run(effectPrefab.resourcePath, this);
	}
}
