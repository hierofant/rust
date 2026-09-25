using UnityEngine;

public abstract class SingletonComponent<T> : SingletonComponent where T : MonoBehaviour
{
	public static T Instance;

	public override void SingletonSetup()
	{
		if (Instance != this)
		{
			Instance = this as T;
		}
	}

	public override void SingletonClear()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}
}
public abstract class SingletonComponent : FacepunchBehaviour
{
	public abstract void SingletonSetup();

	public abstract void SingletonClear();

	protected virtual void Awake()
	{
		SingletonSetup();
	}

	protected virtual void OnDestroy()
	{
		SingletonClear();
	}

	public static void InitializeSingletons(GameObject go)
	{
		SingletonComponent[] componentsInChildren = go.GetComponentsInChildren<SingletonComponent>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].SingletonSetup();
		}
	}
}
