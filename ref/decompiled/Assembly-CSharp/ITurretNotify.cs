public interface ITurretNotify
{
	void WarmupTick(bool wantsShoot);

	bool CanShoot();

	void OnAddedRemovedToTurret(bool added);
}
