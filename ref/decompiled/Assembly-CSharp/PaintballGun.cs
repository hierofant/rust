#define UNITY_ASSERTIONS
using System;
using ConVar;
using Network;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class PaintballGun : BaseProjectile
{
	[Header("Paintball Gun")]
	public Renderer[] worldModelAmmoRenderers;

	private static readonly int shaderProperty_Fill = Shader.PropertyToID("_Fill");

	private static readonly int shaderProperty_Color = Shader.PropertyToID("_Color");

	private static MaterialPropertyBlock ammoRendererBlock;

	private int currentPaintballColor;

	private int paintballColorPreReload;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("PaintballGun.OnRpcMessage"))
		{
			if (rpc == 3661258788u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - Server_PaintballColorChanged");
				}
				using (TimeWarning.New("Server_PaintballColorChanged"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(3661258788u, "Server_PaintballColorChanged", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg2 = rPCMessage;
							Server_PaintballColorChanged(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in Server_PaintballColorChanged");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override int GetImpactEffectNumberValue(HitInfo hitInfo)
	{
		AttackEntity weapon = hitInfo.Weapon;
		if (weapon != null)
		{
			Item item = weapon.GetCachedItem();
			if (item != null && item.instanceData != null)
			{
				return item.instanceData.dataInt;
			}
		}
		return base.GetImpactEffectNumberValue(hitInfo);
	}

	public override DamageType GetDamageTypeForEffect(HitInfo info)
	{
		return DamageType.Paintball;
	}

	public override Effect.Type GetEffectType(HitInfo info)
	{
		if (info.HitEntity.IsValid() && info.HitEntity.GetImpactEffect(info).isValid)
		{
			return base.GetEffectType(info);
		}
		return Effect.Type.PaintballSplat;
	}

	public override bool ForceSendMagazine(SaveInfo saveInfo)
	{
		return true;
	}

	public override void DidAttackServerside()
	{
		SendNetworkUpdate();
	}

	[RPC_Server.IsActiveItem]
	[RPC_Server]
	private void Server_PaintballColorChanged(RPCMessage msg)
	{
		if (PaintballColorLookup.instance == null)
		{
			Debug.LogError("Failed to retrieve PaintballColorLookup instance");
			return;
		}
		int num = currentPaintballColor;
		int num2 = msg.read.Int32();
		if (num != num2)
		{
			currentPaintballColor = Mathf.Clamp(num2, 0, PaintballColorLookup.instance.GetColorsCount() - 1);
			BasePlayer player = msg.player;
			player.Server_UpdatePaintballColor(currentPaintballColor);
			if (primaryMagazine.contents > 0)
			{
				player.GiveItem(ItemManager.CreateByItemID(primaryMagazine.ammoType.itemid, primaryMagazine.contents, 0uL, 0uL));
				SetAmmoCount(0);
			}
			SendNetworkUpdateImmediate();
			ItemManager.DoRemoves();
			player.inventory.ServerUpdate(0f);
		}
	}
}
