using UnityEngine;

public abstract class ProceduralComponent : MonoBehaviour
{
	public enum Realm
	{
		Client = 1,
		Server
	}

	[InspectorFlags]
	public Realm Mode = (Realm)(-1);

	public bool Disabled;

	[Space]
	public string Description = "Procedural Component";

	[Space]
	[Header("Map Creator Settings")]
	public bool HideInMapCreator;

	public string MapCreatorCategory = "";

	public virtual bool RunOnCache => false;

	public bool ShouldRun()
	{
		if (Disabled)
		{
			return false;
		}
		if (World.Cached && !RunOnCache)
		{
			return false;
		}
		if ((Mode & Realm.Server) != 0)
		{
			return true;
		}
		return false;
	}

	public abstract void Process(uint seed);
}
