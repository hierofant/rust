using System.Collections.Generic;
using ConVar;
using Facepunch;
using UnityEngine;

public class GroundWatch : EntityComponent<BaseEntity>, IServerComponent
{
	public Vector3 groundPosition = Vector3.zero;

	public LayerMask layers = 161546240;

	public float radius = 0.1f;

	public bool needBuildingBlock;

	[Tooltip("By default, we consider a deployable as not grounded when at least one AreaCheck fails. This allows you to consider it grounded as long as one AreaCheck passes.")]
	public bool needOnlyOneAreaCheckValid;

	[Header("Whitelist")]
	public BaseEntity[] whitelist;

	public int fails;

	public BaseCombatEntity cachedGround { get; private set; }

	public override void InitShared()
	{
		base.InitShared();
		CacheGround();
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(groundPosition, radius);
	}

	public static void PhysicsChanged(GameObject obj)
	{
		if (obj == null)
		{
			return;
		}
		Collider component = obj.GetComponent<Collider>();
		if (!component)
		{
			return;
		}
		Bounds bounds = component.bounds;
		List<BaseEntity> obj2 = Facepunch.Pool.Get<List<BaseEntity>>();
		Vis.Entities(bounds.center, bounds.extents.magnitude + 1f, obj2, 136481024);
		foreach (BaseEntity item in obj2)
		{
			if (!item.IsDestroyed && !item.isClient && !(item is BuildingBlock))
			{
				item.BroadcastMessage("OnPhysicsNeighbourChanged", SendMessageOptions.DontRequireReceiver);
			}
		}
		Facepunch.Pool.FreeUnmanaged(ref obj2);
	}

	public static void PhysicsChanged(Vector3 origin, float radius, int layerMask)
	{
		List<BaseEntity> obj = Facepunch.Pool.Get<List<BaseEntity>>();
		Vis.Entities(origin, radius, obj, layerMask);
		foreach (BaseEntity item in obj)
		{
			if (!item.IsDestroyed && !item.isClient && !(item is BuildingBlock))
			{
				item.BroadcastMessage("OnPhysicsNeighbourChanged", SendMessageOptions.DontRequireReceiver);
			}
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	public void DirectCallOnPhysicsNeighbourChanged()
	{
		OnPhysicsNeighbourChanged();
	}

	public void OnPhysicsNeighbourChanged()
	{
		bool flag = OnGround();
		if (flag && needBuildingBlock)
		{
			flag = HasBuildingBlock();
		}
		if (!flag)
		{
			fails++;
			if (fails >= ConVar.Physics.groundwatchfails)
			{
				BaseEntity baseEntity = GameObjectEx.ToBaseEntity(base.gameObject);
				if ((bool)baseEntity)
				{
					baseEntity.transform.BroadcastMessage("OnGroundMissing", cachedGround, SendMessageOptions.DontRequireReceiver);
				}
			}
			else
			{
				if (ConVar.Physics.groundwatchdebug)
				{
					Debug.Log("GroundWatch retry: " + fails);
				}
				Invoke(OnPhysicsNeighbourChanged, ConVar.Physics.groundwatchdelay);
			}
		}
		else
		{
			fails = 0;
		}
	}

	private bool HasBuildingBlock()
	{
		BaseEntity component = GetComponent<BaseEntity>();
		List<Collider> obj = Facepunch.Pool.Get<List<Collider>>();
		if (component != null && !PlayerBoat.IsChildOfFinishedPlayerBoat(component))
		{
			Vis.Colliders(base.transform.TransformPoint(groundPosition), radius, obj, 2097152);
		}
		else
		{
			Vis.Colliders(base.transform.TransformPoint(groundPosition), radius, obj, 136314880);
		}
		bool result = false;
		foreach (Collider item in obj)
		{
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(item.gameObject);
			if (!(baseEntity == null) && !baseEntity.IsDestroyed && !baseEntity.isClient && baseEntity is BuildingBlock)
			{
				result = true;
				break;
			}
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
		return result;
	}

	public bool OnGround()
	{
		BaseEntity component = GetComponent<BaseEntity>();
		if ((bool)component && component.isServer)
		{
			if (component.HasParent() && !PlayerBoat.IsChildOfFinishedPlayerBoat(component))
			{
				return true;
			}
			Construction construction = PrefabAttribute.server.Find<Construction>(component.prefabID);
			if ((bool)construction)
			{
				Socket_Base[] allSockets = construction.allSockets;
				for (int i = 0; i < allSockets.Length; i++)
				{
					SocketMod[] socketMods = allSockets[i].socketMods;
					for (int j = 0; j < socketMods.Length; j++)
					{
						SocketMod_AreaCheck socketMod_AreaCheck = socketMods[j] as SocketMod_AreaCheck;
						if (!socketMod_AreaCheck || !socketMod_AreaCheck.wantsInside)
						{
							continue;
						}
						if (needOnlyOneAreaCheckValid)
						{
							if (socketMod_AreaCheck.DoCheck(component.transform.position, component.transform.rotation, component))
							{
								return true;
							}
						}
						else if (!socketMod_AreaCheck.DoCheck(component.transform.position, component.transform.rotation, component))
						{
							if (ConVar.Physics.groundwatchdebug)
							{
								Debug.Log("GroundWatch failed: " + socketMod_AreaCheck.hierachyName);
							}
							return false;
						}
					}
				}
			}
		}
		if (ConVar.Physics.groundwatchdebug)
		{
			Debug.Log("GroundWatch failed: Legacy radius check");
		}
		if (LegacyRadiusCheck(component))
		{
			return true;
		}
		return false;
	}

	private void CacheGround()
	{
		BaseEntity baseEntity = GetBaseEntity();
		if (baseEntity != null && baseEntity.isServer)
		{
			LegacyRadiusCheck(baseEntity);
		}
	}

	private bool LegacyRadiusCheck(BaseEntity entity)
	{
		if (entity == null)
		{
			return false;
		}
		List<Collider> obj = Facepunch.Pool.Get<List<Collider>>();
		Vis.Colliders(base.transform.TransformPoint(groundPosition), radius, obj, layers);
		foreach (Collider item in obj)
		{
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(item.gameObject);
			if (baseEntity == null)
			{
				Facepunch.Pool.FreeUnmanaged(ref obj);
				return true;
			}
			if (baseEntity != null && (baseEntity == entity || baseEntity.IsDestroyed || baseEntity.isClient))
			{
				continue;
			}
			if (whitelist != null && whitelist.Length != 0)
			{
				bool flag = false;
				BaseEntity[] array = whitelist;
				foreach (BaseEntity baseEntity2 in array)
				{
					if (baseEntity.prefabID == baseEntity2.prefabID)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					continue;
				}
			}
			DecayEntity decayEntity = entity as DecayEntity;
			DecayEntity decayEntity2 = baseEntity as DecayEntity;
			if (!(decayEntity != null) || decayEntity.buildingID == 0 || !(decayEntity2 != null) || decayEntity2.buildingID == 0 || decayEntity.buildingID == decayEntity2.buildingID)
			{
				cachedGround = baseEntity as BaseCombatEntity;
				Facepunch.Pool.FreeUnmanaged(ref obj);
				return true;
			}
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
		return false;
	}
}
