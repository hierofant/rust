using UnityEngine;

public interface IPrefabPreProcess
{
	bool CanRunDuringBundling { get; }

	void PreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling);
}
