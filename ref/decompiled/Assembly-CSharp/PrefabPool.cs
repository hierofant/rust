using System.Collections.Generic;
using UnityEngine;

public class PrefabPool
{
	public Stack<Poolable> stack = new Stack<Poolable>();

	public string PrefabName { get; private set; }

	public GameObject PrefabTemplate { get; private set; }

	public int Missed { get; private set; }

	public int Pushed { get; private set; }

	public int Popped { get; private set; }

	public int Count => stack.Count;

	public int TargetCapacity { get; private set; }

	public PrefabPool(uint prefabId, int targetCapacity, GameObject prefabTemplate)
	{
		PrefabName = StringPool.Get(prefabId);
		TargetCapacity = targetCapacity;
		PrefabTemplate = prefabTemplate;
	}

	public void Push(Poolable info)
	{
		Pushed++;
		stack.Push(info);
		info.EnterPool(PrefabTemplate);
	}

	public void Push(GameObject instance)
	{
		Poolable component = instance.GetComponent<Poolable>();
		Push(component);
	}

	public GameObject Pop(Vector3 pos = default(Vector3), Quaternion rot = default(Quaternion))
	{
		while (stack.Count > 0)
		{
			Poolable poolable = stack.Pop();
			Popped++;
			if ((bool)poolable)
			{
				poolable.transform.SetPositionAndRotation(pos, rot);
				poolable.LeavePool();
				return poolable.gameObject;
			}
		}
		Missed++;
		return null;
	}

	public void Clear()
	{
		foreach (Poolable item in stack)
		{
			if ((bool)item)
			{
				Object.Destroy(item.gameObject);
			}
		}
		stack.Clear();
	}
}
