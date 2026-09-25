using Facepunch;
using ProtoBuf;
using UnityEngine;

public class VineSwingingTreeStump : BaseEntity
{
	public GameObjectRef[] TreePrefabs;

	public float MaxTreeRespawnTime = 5f;

	public float MinTreeRespawnTime = 10f;

	public GameObject PreventBuildingVolume;

	private TimeUntil treeRespawnTime;

	private int treeToRespawn;

	public void InitializeTree(VineSwingingTree fromTree)
	{
		treeRespawnTime = Random.Range(MinTreeRespawnTime, MaxTreeRespawnTime);
		Invoke(RespawnTreeInvoke, treeRespawnTime);
		treeToRespawn = 0;
		for (int i = 0; i < TreePrefabs.Length; i++)
		{
			if (TreePrefabs[i].resourceID == fromTree.prefabID)
			{
				treeToRespawn = i;
				break;
			}
		}
	}

	private void RespawnTreeInvoke()
	{
		RespawnTree();
	}

	public bool RespawnTree()
	{
		GameObjectRef gameObjectRef = TreePrefabs[Mathf.Clamp(treeToRespawn, 0, TreePrefabs.Length)];
		if (gameObjectRef.isValid)
		{
			if (!IsTreeRespawnClear())
			{
				Invoke(RespawnTreeInvoke, 10f);
				return false;
			}
			VineSwingingTree obj = base.gameManager.CreateEntity(gameObjectRef.resourcePath, base.transform.position, base.transform.rotation) as VineSwingingTree;
			obj.Spawn();
			obj.NotifyNearbyTreesSpawned();
			Kill();
			return true;
		}
		return false;
	}

	private bool IsTreeRespawnClear()
	{
		using PooledList<Collider> pooledList = Pool.Get<PooledList<Collider>>();
		PreventBuildingVolume.GetComponents(pooledList);
		foreach (Collider item in pooledList)
		{
			if (item is BoxCollider boxCollider)
			{
				if (GamePhysics.CheckOBB(new OBB(PreventBuildingVolume.transform, new Bounds(boxCollider.center, boxCollider.size)), 131072))
				{
					return false;
				}
			}
			else if (item is CapsuleCollider capsuleCollider)
			{
				Vector3 vector = new Vector3(0f, capsuleCollider.height * 0.5f, 0f);
				if (GamePhysics.CheckCapsule(item.transform.TransformPoint(capsuleCollider.center + vector), item.transform.TransformPoint(capsuleCollider.center - vector), capsuleCollider.radius, 131072))
				{
					return false;
				}
			}
		}
		return true;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.forDisk)
		{
			info.msg.treeRespawn = Pool.Get<TreeRespawn>();
			info.msg.treeRespawn.timeToRespawn = treeRespawnTime;
			info.msg.treeRespawn.treeIndex = treeToRespawn;
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.fromDisk && info.msg.treeRespawn != null)
		{
			treeRespawnTime = info.msg.treeRespawn.timeToRespawn;
			treeToRespawn = info.msg.treeRespawn.treeIndex;
			Invoke(RespawnTreeInvoke, treeRespawnTime);
		}
	}
}
