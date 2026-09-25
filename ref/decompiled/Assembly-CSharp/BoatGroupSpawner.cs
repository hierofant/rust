using System.Collections.Generic;
using Facepunch;
using Oxide.Core;
using UnityEngine;

public class BoatGroupSpawner : BaseMonoBehaviour
{
	public float radius = 30f;

	public float protectionAreaRadius = 80f;

	public void SpawnBoatGroup(BoatAI.AILoadMode loadMode = BoatAI.AILoadMode.LoadAi)
	{
		using PooledHashSet<RHIB> list = Pool.Get<PooledHashSet<RHIB>>();
		SpawnBoatGroup(list, loadMode);
	}

	public void SpawnBoatGroup(HashSet<RHIB> list, BoatAI.AILoadMode loadMode = BoatAI.AILoadMode.LoadAi, bool spawnsPT = true, ScientistBoatOilrigManager manager = null)
	{
		Vector2 vector = new Vector2(base.transform.position.x, base.transform.position.z);
		if (!BoatAI.FindBoatSpawnPositionInRadius(vector, radius, out var position))
		{
			return;
		}
		Vector3 forward = (vector - position).normalized;
		bool flag = false;
		if (PointEntity<DeepSeaManager>.ServerInstance != null)
		{
			flag = DeepSeaManager.IsInsideDeepSea(vector);
		}
		forward.y = 0f;
		Quaternion quaternion = Quaternion.LookRotation(forward);
		if (Interface.CallHook("OnBoatGroupSpawn", this, position, quaternion, list, flag, spawnsPT) != null)
		{
			return;
		}
		BoatAI.SpawnBoatGroup(position, quaternion, list, flag, spawnsPT);
		foreach (RHIB item in list)
		{
			BoatAI componentInChildren = item.GetComponentInChildren<BoatAI>();
			componentInChildren.MoveTo(base.transform.position);
			componentInChildren.SetProtectionArea(base.transform.position, protectionAreaRadius);
			componentInChildren.LoadMode = loadMode;
			componentInChildren.SetOilRigManager(manager);
		}
		Interface.CallHook("OnBoatGroupSpawned", this, position, quaternion, list, flag, spawnsPT);
	}
}
