public interface IReceivePlayerTickListener
{
	void OnReceivePlayerTick(BasePlayer player, PlayerTick msg);

	bool ShouldRemoveOnPlayerDeath();
}
