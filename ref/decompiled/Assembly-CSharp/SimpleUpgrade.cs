using System.Collections.Generic;
using ConVar;
using Facepunch;
using UnityEngine;

internal static class SimpleUpgrade
{
	public static bool CanUpgrade(BaseEntity entity, ItemDefinition upgradeItem, BasePlayer player)
	{
		if (player == null)
		{
			return false;
		}
		if (entity == null)
		{
			return false;
		}
		if (upgradeItem == null)
		{
			return false;
		}
		if (!player.CanInteract())
		{
			return false;
		}
		if (player.IsBuildingBlocked(entity.transform.position, entity.transform.rotation, entity.bounds))
		{
			return false;
		}
		if (upgradeItem.GetComponent<ItemModDeployable>() == null)
		{
			return false;
		}
		if (IsUpgradeBlocked(entity, upgradeItem, player))
		{
			return false;
		}
		if (!CanAffordUpgrade(entity, upgradeItem, player))
		{
			return false;
		}
		return true;
	}

	public static bool CanAffordUpgrade(BaseEntity entity, ItemDefinition upgradeItem, BasePlayer player)
	{
		if (player == null)
		{
			return false;
		}
		ISimpleUpgradable simpleUpgradable = entity as ISimpleUpgradable;
		if (entity == null)
		{
			return false;
		}
		if (player.IsInCreativeMode && Creative.freeBuild)
		{
			return true;
		}
		if (simpleUpgradable.CostIsItem())
		{
			return player.inventory.GetAmount(upgradeItem) > 0;
		}
		if (upgradeItem.Blueprint == null)
		{
			return false;
		}
		if (!ItemModStudyBlueprint.IsBlueprintUnlocked(upgradeItem, player))
		{
			return false;
		}
		foreach (ItemAmount ingredient in upgradeItem.Blueprint.GetIngredients())
		{
			if ((float)player.inventory.GetAmount(ingredient.itemid) < ingredient.amount)
			{
				return false;
			}
		}
		return true;
	}

	public static void PayForUpgrade(BaseEntity entity, ItemDefinition upgradeItem, BasePlayer player)
	{
		if (player == null || (player.IsInCreativeMode && Creative.freeBuild) || !(entity is ISimpleUpgradable simpleUpgradable))
		{
			return;
		}
		List<Item> list = new List<Item>();
		if (simpleUpgradable.CostIsItem())
		{
			player.inventory.Take(list, upgradeItem.itemid, 1);
			player.Command("note.inv " + upgradeItem.itemid + " " + -1);
		}
		else
		{
			foreach (ItemAmount ingredient in upgradeItem.Blueprint.GetIngredients())
			{
				player.inventory.Take(list, ingredient.itemid, (int)ingredient.amount);
				player.Command("note.inv " + ingredient.itemid + " " + ingredient.amount * -1f);
			}
		}
		foreach (Item item in list)
		{
			item.Remove();
		}
	}

	public static void DoUpgrade(BaseEntity entity, BasePlayer player, ItemDefinition upgradeItem)
	{
		if (!(entity is ISimpleUpgradable simpleUpgradable) || !simpleUpgradable.CanUpgrade(player, upgradeItem))
		{
			return;
		}
		PayForUpgrade(entity, upgradeItem, player);
		EntityRef[] slots = entity.GetSlots();
		BaseEntity parentEntity = entity.GetParentEntity();
		bool flag = entity is DecayEntity decayEntity && decayEntity.HasFlag(BaseEntity.Flags.Reserved2);
		ItemModDeployable component = upgradeItem.GetComponent<ItemModDeployable>();
		BaseEntity baseEntity = GameManager.server.CreateEntity(component.entityPrefab.resourcePath, entity.transform.position, entity.transform.rotation);
		baseEntity.SetParent(parentEntity);
		baseEntity.OwnerID = player.userID;
		Deployable component2 = component.entityPrefab.Get().GetComponent<Deployable>();
		if (component2 != null && component2.placeEffect.isValid)
		{
			Effect.server.Run(component2.placeEffect.resourcePath, entity.transform.position, Vector3.up);
		}
		DecayEntity decayEntity2 = baseEntity as DecayEntity;
		if (decayEntity2 != null)
		{
			decayEntity2.timePlaced = entity.GetNetworkTime();
		}
		List<BaseEntity.ChildPreserveInfo> obj = Facepunch.Pool.Get<List<BaseEntity.ChildPreserveInfo>>();
		foreach (BaseEntity child in entity.children)
		{
			obj.Add(new BaseEntity.ChildPreserveInfo
			{
				targetEntity = child,
				targetBone = child.parentBone,
				localPosition = child.transform.localPosition,
				localRotation = child.transform.localRotation
			});
		}
		foreach (BaseEntity.ChildPreserveInfo item in obj)
		{
			item.targetEntity.SetParent(null, worldPositionStays: true);
		}
		entity.Kill();
		if (baseEntity is DecayEntity decayEntity3)
		{
			decayEntity3.AttachToBuilding(null);
		}
		baseEntity.Spawn();
		foreach (BaseEntity.ChildPreserveInfo item2 in obj)
		{
			item2.targetEntity.SetParent(baseEntity, item2.targetBone, worldPositionStays: true);
			item2.targetEntity.transform.localPosition = item2.localPosition;
			item2.targetEntity.transform.localRotation = item2.localRotation;
			item2.targetEntity.SendNetworkUpdate();
		}
		baseEntity.SetSlots(slots);
		if (!flag && baseEntity is DecayEntity decayEntity4)
		{
			decayEntity4.StopBeingDemolishable();
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	public static bool IsUpgradeBlocked(BaseEntity entity, ItemDefinition upgradeItem, BasePlayer player)
	{
		if (upgradeItem == null)
		{
			return true;
		}
		if (entity == null)
		{
			return true;
		}
		if (entity is DecorDeployable)
		{
			return false;
		}
		if (entity is BaseLock)
		{
			return false;
		}
		ItemModDeployable component = upgradeItem.GetComponent<ItemModDeployable>();
		if (component == null)
		{
			return false;
		}
		DeployVolume[] volumes = PrefabAttribute.server.FindAll<DeployVolume>(component.entityPrefab.resourceID);
		if (DeployVolume.Check(entity.transform.position, entity.transform.rotation, volumes, ~((1 << entity.gameObject.layer) | 0x20000000)))
		{
			if (DeployVolume.LastDeployHit != null)
			{
				string blockedByErrorFromCollider = ConstructionErrors.GetBlockedByErrorFromCollider(DeployVolume.LastDeployHit);
				if (!string.IsNullOrEmpty(blockedByErrorFromCollider))
				{
					Construction.lastPlacementError = blockedByErrorFromCollider;
					Construction.lastPlacementErrorIsDetailed = true;
				}
			}
			return true;
		}
		Socket_Base[] array = PrefabAttribute.server.FindAll<Socket_Base>(component.entityPrefab.resourceID);
		Construction.Target target = new Construction.Target
		{
			position = entity.transform.position,
			rotation = entity.transform.eulerAngles,
			normal = entity.transform.up,
			ray = player.eyes.HeadRay()
		};
		Construction.Placement placement = new Construction.Placement(target)
		{
			position = target.position,
			rotation = entity.transform.rotation,
			ignoredEntity = entity
		};
		Socket_Base[] array2 = array;
		foreach (Socket_Base socket_Base in array2)
		{
			if (socket_Base.male && !socket_Base.CheckSocketMods(ref placement))
			{
				return true;
			}
		}
		return false;
	}
}
