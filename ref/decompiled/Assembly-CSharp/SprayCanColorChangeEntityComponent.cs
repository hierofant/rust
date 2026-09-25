using Facepunch;
using ProtoBuf;
using UnityEngine;

public class SprayCanColorChangeEntityComponent : EntityComponent<BaseEntity>
{
	public Renderer[] renderersToApplyColor;

	public uint currentColorIndex { get; private set; }

	public override void OnEntityDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		base.OnEntityDeployed(parent, deployedBy, fromItem);
		Server_UpdateFromPlayerPreferredColor(deployedBy);
	}

	public void Server_UpdateFromPlayerPreferredColor(BasePlayer player)
	{
		Server_UpdateColor(Server_GetPreferredColorIndexForPlayer(player));
	}

	public uint Server_GetPreferredColorIndexForPlayer(BasePlayer player)
	{
		if (player == null)
		{
			return 0u;
		}
		if (!base.baseEntity.TryGetColorDataset(out var colorDataset))
		{
			return 0u;
		}
		string clientConVar = colorDataset.clientConVar;
		if (string.IsNullOrWhiteSpace(clientConVar))
		{
			return 0u;
		}
		int infoInt = player.GetInfoInt(clientConVar, 0);
		if (infoInt < 0 || infoInt >= colorDataset.colorDataEntries.Length)
		{
			return 0u;
		}
		return (uint)infoInt;
	}

	public void Server_UpdateColor(uint newColorIndex)
	{
		if (newColorIndex != currentColorIndex && base.baseEntity.TryGetColorDataset(out var colorDataset))
		{
			int num = colorDataset.colorDataEntries.Length;
			if (newColorIndex >= num)
			{
				newColorIndex = (uint)(num - 1);
			}
			currentColorIndex = newColorIndex;
			base.baseEntity.SendNetworkUpdateImmediate();
		}
	}

	public override void SaveComponent(BaseNetworkable.SaveInfo info)
	{
		base.SaveComponent(info);
		if (currentColorIndex != 0)
		{
			info.msg.simpleUint = Pool.Get<SimpleUInt>();
			info.msg.simpleUint.value = currentColorIndex;
		}
	}

	public override void LoadComponent(BaseNetworkable.LoadInfo info)
	{
		base.LoadComponent(info);
		currentColorIndex = info.msg.simpleUint?.value ?? 0;
	}
}
