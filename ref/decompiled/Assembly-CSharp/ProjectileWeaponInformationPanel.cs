using UnityEngine;

public class ProjectileWeaponInformationPanel : ItemInformationPanel
{
	public ItemModifiableStatValue damageDisplay;

	public ItemModifiableStatValue recoilDisplay;

	public ItemModifiableStatValue rofDisplay;

	public ItemModifiableStatValue accuracyDisplay;

	public ItemModifiableStatValue rangeDisplay;

	public ItemStatValue minigunMoveDisplay;

	public GameObject shieldCompatible;

	public GameObject bypassModTogglesButtonParent;

	public ItemIcon[] attachmentIcons;

	[UnityEvent]
	public void SetBypassModToggles(bool toggle)
	{
	}
}
