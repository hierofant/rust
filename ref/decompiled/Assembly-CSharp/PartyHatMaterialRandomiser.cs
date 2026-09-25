using UnityEngine;

public class PartyHatMaterialRandomiser : MonoBehaviour, IItemSetup
{
	public Material[] MaterialOptions;

	public Renderer[] TargetRenderers;

	public void OnItemSetup(Item item)
	{
	}

	public void OnSetupSkin(ulong skin, ItemDefinition definition)
	{
	}
}
