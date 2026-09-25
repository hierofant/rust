public class ItemModRFListener : ItemModAssociatedEntity<BaseEntity>
{
	public static readonly Translate.Phrase SetFreqTitle = new Translate.Phrase("setfreq", "Set Frequency");

	public static readonly Translate.Phrase SetFreqDesc = new Translate.Phrase("setfreq_desc", "Configure which frequency to listen to");

	public GameObjectRef frequencyPanelPrefab;

	public override void ServerCommand(Item item, string command, BasePlayer player)
	{
		base.ServerCommand(item, command, player);
	}
}
