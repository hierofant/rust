using UnityEngine;

public class BaseLadder : BaseCombatEntity
{
	[SerializeField]
	private TriggerParent triggerParent;

	public override bool ShouldBlockProjectiles()
	{
		return false;
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		base.OnDeployed(parent, deployedBy, fromItem);
		if (base.isServer && (bool)triggerParent)
		{
			BaseVehicle baseVehicle = parent as BaseVehicle;
			bool flag = baseVehicle != null;
			triggerParent.enabled = flag;
			triggerParent.associatedMountable = (flag ? baseVehicle : null);
		}
	}
}
