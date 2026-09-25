using ProtoBuf;

public class ItemModPaintballOveralls : ItemModSpriteConfig
{
	public override void OnParentChanged(Item item)
	{
		if (!item.isServer)
		{
			return;
		}
		ItemContainer rootContainer = item.GetRootContainer();
		if (rootContainer != null)
		{
			BasePlayer ownerPlayer = rootContainer.GetOwnerPlayer();
			if (!(ownerPlayer == null) && ownerPlayer.inventory != null && ownerPlayer.inventory.containerWear != null && ownerPlayer.inventory.containerWear == item.parent)
			{
				OnWorn(item, ownerPlayer);
			}
		}
	}

	private void OnWorn(Item item, BasePlayer player)
	{
		if (player.TryGetHeldEntity(out PaintballGun _))
		{
			if (item.instanceData == null)
			{
				item.instanceData = new ProtoBuf.Item.InstanceData();
				item.instanceData.ShouldPool = false;
			}
			item.instanceData.dataInt = player.server_paintballColor;
		}
		else
		{
			player.Server_UpdatePaintballColor(item.instanceData?.dataInt ?? 0);
		}
		item.MarkDirty();
	}
}
