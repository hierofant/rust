public interface IConversationProvider
{
	BaseEntity GetEntity();

	bool ProviderBusy();
}
