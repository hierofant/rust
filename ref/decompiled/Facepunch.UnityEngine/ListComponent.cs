using System;
using UnityEngine;

public abstract class ListComponent<T> : ListComponent where T : MonoBehaviour
{
	public static ListHashSet<T> InstanceList = new ListHashSet<T>();

	public override void Setup()
	{
		if (!InstanceList.Contains(this as T))
		{
			InstanceList.Add(this as T);
		}
	}

	public override void Clear()
	{
		InstanceList.Remove(this as T);
	}

	public static void RunOnAll(Action<T> toRun)
	{
		foreach (T instance in InstanceList)
		{
			toRun?.Invoke(instance);
		}
	}
}
public abstract class ListComponent : FacepunchBehaviour
{
	public abstract void Setup();

	public abstract void Clear();

	protected virtual void OnEnable()
	{
		Setup();
	}

	protected virtual void OnDisable()
	{
		Clear();
	}
}
