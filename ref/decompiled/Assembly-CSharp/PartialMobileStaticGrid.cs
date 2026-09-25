using Spatial;
using UnityEngine;

public class PartialMobileStaticGrid<T> where T : MonoBehaviour
{
	private Grid<T> grid = new Grid<T>();

	private ListDictionary<T, Vector3> mobilePositions = new ListDictionary<T, Vector3>();

	private TimeSince lastMobileUpdate;

	public Grid<T> Grid => grid;

	public void UpdateMobileEntities()
	{
		if ((float)lastMobileUpdate < 1f)
		{
			return;
		}
		using (TimeWarning.New("UpdateMobileEntities"))
		{
			lastMobileUpdate = 0f;
			float num = 1f;
			for (int i = 0; i < mobilePositions.Keys.Count; i++)
			{
				T val = mobilePositions.Keys[i];
				if (!(val == null) && !(val.transform == null))
				{
					Vector3 position = val.transform.position;
					Vector3 v = mobilePositions.Values[i];
					if ((position - v.WithY(position.y)).sqrMagnitude > num)
					{
						grid.Remove(val);
						grid.Add(val, position.x, position.z);
						mobilePositions[val] = position;
					}
				}
			}
		}
	}

	public void OnParentChanged(BaseEntity target, BaseEntity oldParent, BaseEntity newParent)
	{
		OnParentChanged(target as T, target, oldParent, newParent);
	}

	public void OnParentChanged(T target, BaseEntity targetEntity, BaseEntity oldParent, BaseEntity newParent)
	{
		BaseEntity baseEntity = ((oldParent != null) ? oldParent.GetRootParentEntity() : null);
		BaseEntity baseEntity2 = ((newParent != null) ? newParent.GetRootParentEntity() : null);
		bool num = baseEntity != null && baseEntity.syncPosition;
		bool flag = baseEntity2 != null && baseEntity2.syncPosition;
		if (num != flag)
		{
			DeregisterEntity(target);
			RegisterEntity(target, targetEntity, baseEntity2);
		}
	}

	public void RegisterEntity(BaseEntity ent)
	{
		RegisterEntity(ent as T, ent);
	}

	public void RegisterEntity(T target, BaseEntity associatedEntity, BaseEntity rootParent = null)
	{
		if (target == null)
		{
			return;
		}
		Vector3 position = target.transform.position;
		if (grid.Contains(target))
		{
			grid.Remove(target);
		}
		grid.Add(target, position.x, position.z);
		bool flag = false;
		if (rootParent == null)
		{
			rootParent = associatedEntity.GetRootParentEntity();
		}
		if (rootParent != null && rootParent.syncPosition)
		{
			flag = true;
		}
		if (flag)
		{
			if (mobilePositions.Contains(target))
			{
				mobilePositions[target] = position;
			}
			else
			{
				mobilePositions.Add(target, position);
			}
		}
		else if (mobilePositions.Contains(target))
		{
			mobilePositions.Remove(target);
		}
	}

	public void DeregisterEntity(T target)
	{
		grid.Remove(target);
		if (mobilePositions.Contains(target))
		{
			mobilePositions.Remove(target);
		}
	}
}
