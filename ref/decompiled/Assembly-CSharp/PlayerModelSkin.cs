using UnityEngine;

public class PlayerModelSkin : MonoBehaviour, IPrefabPreProcess
{
	public enum SkinMaterialType
	{
		HEAD,
		EYE,
		BODY
	}

	public SkinMaterialType MaterialType;

	public Renderer SkinRenderer;

	bool IPrefabPreProcess.CanRunDuringBundling => false;

	public void Setup(SkinSet skin, float hairNum)
	{
		if ((bool)SkinRenderer && (bool)skin)
		{
			SkinRenderer.enabled = true;
			switch (MaterialType)
			{
			case SkinMaterialType.HEAD:
				SkinRenderer.sharedMaterial = skin.HeadMaterial;
				break;
			case SkinMaterialType.BODY:
				SkinRenderer.sharedMaterial = skin.BodyMaterial;
				break;
			case SkinMaterialType.EYE:
				SkinRenderer.sharedMaterial = skin.EyeMaterial;
				break;
			default:
				SkinRenderer.sharedMaterial = skin.BodyMaterial;
				break;
			}
		}
	}

	public void ToggleSkinRenderer(bool toggle)
	{
		if ((bool)SkinRenderer)
		{
			SkinRenderer.enabled = toggle;
		}
	}

	public void PreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		if (clientside)
		{
			SkinRenderer = GetComponent<Renderer>();
		}
	}
}
