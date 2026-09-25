public class BeeTriggerHurt : TriggerHurtEx
{
	public GameObjectRef BeeGrenadePrefab;

	protected override void ModifyHit(HitInfo info)
	{
		base.ModifyHit(info);
		if (info.Initiator != null && info.Initiator.OwnerID != 0L)
		{
			info.Initiator = BasePlayer.FindByID(info.Initiator.OwnerID);
			info.WeaponPrefab = GameManager.server.FindPrefab(BeeGrenadePrefab.resourcePath).GetComponent<BaseEntity>();
		}
	}
}
