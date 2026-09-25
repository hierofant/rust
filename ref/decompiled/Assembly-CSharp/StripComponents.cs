using UnityEngine;

public abstract class StripComponents<T> : BaseMonoBehaviour, IPrefabPreProcess where T : Component
{
	public bool CanRunDuringBundling { get; } = true;


	public void PreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
	}
}
