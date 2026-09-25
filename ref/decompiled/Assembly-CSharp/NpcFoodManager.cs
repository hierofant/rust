using System.Collections.Generic;
using ConVar;
using Spatial;
using UnityEngine;

public class NpcFoodManager : SingletonComponent<NpcFoodManager>, IServerComponent
{
	private const float worldSize = 8096f;

	private const int cellSize = 32;

	private Grid<BaseEntity> foodGrid = new Grid<BaseEntity>();

	public bool Add(BaseEntity food)
	{
		if (!IsFood(food))
		{
			return false;
		}
		if (!foodGrid.AddUnique(food, food.transform.position.x, food.transform.position.z))
		{
			if (AI.logIssues)
			{
				Debug.LogWarning($"Failed to add food to grid: {food.ShortPrefabName}_{food.net.ID}");
			}
			return false;
		}
		return true;
	}

	public void Move(BaseEntity food)
	{
		using (TimeWarning.New("NpcFoodManager.Move"))
		{
			if (IsFood(food))
			{
				Vector3 position = food.transform.position;
				foodGrid.Move(food, position.x, position.z);
			}
		}
	}

	public bool Contains(BaseEntity food)
	{
		if (!IsFood(food))
		{
			return false;
		}
		return foodGrid.Contains(food);
	}

	public bool Remove(BaseEntity food)
	{
		if (!IsFood(food))
		{
			return false;
		}
		if (!foodGrid.Remove(food))
		{
			return false;
		}
		return true;
	}

	public void GetFoodAround(Vector3 position, float range, List<BaseEntity> results)
	{
		using (TimeWarning.New("NpcFoodManager.GetFoodAround"))
		{
			if (foodGrid != null)
			{
				foodGrid.Query(position.x, position.z, range, results);
			}
		}
	}

	public static bool IsFood(BaseEntity entity)
	{
		if (entity is DroppedItem droppedItem)
		{
			if (droppedItem.item == null)
			{
				return false;
			}
			return (droppedItem.item.info.Traits & BaseEntity.TraitFlag.Meat) == BaseEntity.TraitFlag.Meat;
		}
		return entity is BaseCorpse;
	}

	public static bool IsFoodImmobile(BaseEntity entity)
	{
		if (entity is DroppedItem { IsSleeping: not false })
		{
			return true;
		}
		if (entity is BaseCorpse { IsSleeping: not false })
		{
			return true;
		}
		return false;
	}
}
