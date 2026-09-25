using UnityEngine;

public class FPConvexCollisionBaker : CoACD, IPrefabPreProcess
{
	public bool CanRunDuringBundling => true;

	public void PreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		preProcess.RemoveComponent(this);
	}
}
