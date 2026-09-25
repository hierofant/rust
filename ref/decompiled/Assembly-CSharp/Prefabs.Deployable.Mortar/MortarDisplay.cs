using UnityEngine;

namespace Prefabs.Deployable.Mortar;

public class MortarDisplay : FacepunchBehaviour
{
	[SerializeField]
	private RectTransform distanceBar;

	[SerializeField]
	private Vector2 minMaxY;

	[SerializeField]
	private GameObject fragImage;

	[SerializeField]
	private GameObject heImage;

	public void SetPitch(float minMax01)
	{
		Vector2 anchoredPosition = distanceBar.anchoredPosition;
		anchoredPosition.y = Mathf.Lerp(minMaxY.x, minMaxY.y, minMax01);
		distanceBar.anchoredPosition = anchoredPosition;
	}

	public void SetAmmoIcon(ItemDefinition ammo)
	{
		if (!(ammo == null))
		{
			bool flag = ammo.shortname.EndsWith("fragment");
			fragImage.SetActive(flag);
			heImage.SetActive(!flag);
		}
	}
}
