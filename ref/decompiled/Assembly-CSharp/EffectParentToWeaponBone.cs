using UnityEngine;

[RequireComponent(typeof(ParticleSystemPostIK))]
public class EffectParentToWeaponBone : BaseMonoBehaviour, IEffect, IPrefabPreProcess
{
	public string boneName;

	public bool singleFrame;

	public bool CanRunDuringBundling => false;

	public void PreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
	}
}
