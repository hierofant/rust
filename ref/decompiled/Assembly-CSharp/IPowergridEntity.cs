public interface IPowergridEntity
{
	BaseEntity GetEntity();

	bool Server_ShouldConnectToPowergrid();

	void Server_OnPowergridStageChanged(int newStage);
}
