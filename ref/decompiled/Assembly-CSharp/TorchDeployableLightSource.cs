#define UNITY_ASSERTIONS
using System;
using ConVar;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class TorchDeployableLightSource : StorageContainer, ISplashable, IIgniteable, IAlwaysOn
{
	[Serializable]
	public struct AllowedTorch
	{
		[ItemSelector]
		public ItemDefinition torch;

		public Vector3 offset;

		public Vector3 rotationOffset;
	}

	public AllowedTorch[] AllowedTorches;

	public Transform TorchRoot;

	public const Flags HasTorch = Flags.Reserved1;

	public const Flags UseBuiltInFx = Flags.Reserved4;

	public const Flags AlwaysOn = Flags.Reserved3;

	public ItemDefinition[] BuiltInFxItems = new ItemDefinition[0];

	private EntityRef<TorchWeapon> spawnedTorch;

	private ItemDefinition spawnedTorchDef;

	private Item CurrentTorch => base.inventory.GetSlot(0);

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("TorchDeployableLightSource.OnRpcMessage"))
		{
			if (rpc == 3305620958u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RequestTurnOnOff");
				}
				using (TimeWarning.New("RequestTurnOnOff"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3305620958u, "RequestTurnOnOff", this, player, 3f))
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
							RequestTurnOnOff(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RequestTurnOnOff");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		foreach (BaseEntity child in children)
		{
			if (child is TorchWeapon torchWeapon)
			{
				spawnedTorch.Set(torchWeapon);
				using (FlagsUpdateScope flagsUpdateScope = torchWeapon.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
				{
					flagsUpdateScope.Set(Flags.On, IsOn());
				}
				break;
			}
		}
		if (HasFlag(Flags.Reserved1) && IsOn())
		{
			InvokeRepeating(TickTorchDurability, 1f, 1f);
		}
		Sprinkler.SplashableGrid.RegisterEntity(this);
	}

	public override void OnParentChanging(BaseEntity oldParent, BaseEntity newParent)
	{
		base.OnParentChanging(oldParent, newParent);
		Sprinkler.SplashableGrid.OnParentChanged(this, oldParent, newParent);
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		base.OnDeployed(parent, deployedBy, fromItem);
		Sprinkler.SplashableGrid.RegisterEntity(this);
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		Sprinkler.SplashableGrid.DeregisterEntity(this);
	}

	public override bool ItemFilter(BasePlayer player, Item item, int targetSlot)
	{
		AllowedTorch[] allowedTorches = AllowedTorches;
		for (int i = 0; i < allowedTorches.Length; i++)
		{
			if (allowedTorches[i].torch == item.info)
			{
				return true;
			}
		}
		return false;
	}

	private bool ShouldUseBuiltInFx(ItemDefinition def)
	{
		if (def == null)
		{
			return false;
		}
		ItemDefinition[] builtInFxItems = BuiltInFxItems;
		for (int i = 0; i < builtInFxItems.Length; i++)
		{
			if (builtInFxItems[i] == def)
			{
				return true;
			}
		}
		return false;
	}

	private void GetTorchSpawn(ItemDefinition def, out Vector3 position, out Quaternion rotation)
	{
		AllowedTorch[] allowedTorches = AllowedTorches;
		for (int i = 0; i < allowedTorches.Length; i++)
		{
			AllowedTorch allowedTorch = allowedTorches[i];
			if (allowedTorch.torch == def)
			{
				position = TorchRoot.TransformPoint(allowedTorch.offset);
				rotation = TorchRoot.rotation * Quaternion.Euler(allowedTorch.rotationOffset);
				return;
			}
		}
		position = TorchRoot.position;
		rotation = TorchRoot.rotation;
	}

	private void UpdateTorch()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		Item item = CurrentTorch;
		if (item != null && item.isBroken)
		{
			item = null;
		}
		ItemDefinition itemDefinition = item?.info;
		if (itemDefinition != spawnedTorchDef)
		{
			spawnedTorchDef = itemDefinition;
			flagsUpdateScope.Set(Flags.Reserved4, ShouldUseBuiltInFx(itemDefinition));
			TorchWeapon torchWeapon = spawnedTorch.Get(serverside: true);
			if (torchWeapon != null)
			{
				torchWeapon.Kill();
			}
			spawnedTorch.Set(null);
			if (itemDefinition != null)
			{
				GetTorchSpawn(itemDefinition, out var position, out var rotation);
				TorchWeapon component = GameManager.server.CreateEntity(itemDefinition.GetComponent<ItemModEntity>().entityPrefab.resourcePath, position, rotation).GetComponent<TorchWeapon>();
				component.SetParent(this, worldPositionStays: true);
				using (FlagsUpdateScope flagsUpdateScope2 = component.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
				{
					flagsUpdateScope2.Set(Flags.Reserved1, b: true);
				}
				component.Spawn();
				spawnedTorch.Set(component);
			}
			else
			{
				flagsUpdateScope.Set(Flags.On, b: false);
			}
		}
		flagsUpdateScope.Set(Flags.Reserved1, spawnedTorch.Get(serverside: true) != null);
		if (!HasFlag(Flags.Reserved1) && IsInvoking(TickTorchDurability))
		{
			CancelInvoke(TickTorchDurability);
		}
	}

	private void TickTorchDurability()
	{
		Item currentTorch = CurrentTorch;
		if (currentTorch != null && !IsAlwaysOn())
		{
			currentTorch.LoseCondition(1f / 12f);
		}
	}

	public override void OnItemAddedOrRemoved(Item item, bool added)
	{
		base.OnItemAddedOrRemoved(item, added);
		UpdateTorch();
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	private void RequestTurnOnOff(RPCMessage msg)
	{
		bool wantsOn = msg.read.Bit();
		TryToggle(wantsOn);
	}

	private void TryToggle(bool wantsOn)
	{
		if (CurrentTorch == null)
		{
			return;
		}
		TorchWeapon torchWeapon = spawnedTorch.Get(serverside: true);
		if (!(torchWeapon == null))
		{
			using (FlagsUpdateScope flagsUpdateScope = torchWeapon.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.On, wantsOn);
			}
			using (FlagsUpdateScope flagsUpdateScope2 = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope2.Set(Flags.On, wantsOn);
			}
			if (HasFlag(Flags.Reserved1) && wantsOn)
			{
				InvokeRepeating(TickTorchDurability, 1f, 1f);
			}
			else
			{
				CancelInvoke(TickTorchDurability);
			}
		}
	}

	public bool WantsSplash(ItemDefinition splashType, int amount)
	{
		if (HasFlag(Flags.Reserved1))
		{
			return IsOn();
		}
		return false;
	}

	public int DoSplash(ItemDefinition splashType, int amount)
	{
		TryToggle(wantsOn: false);
		return 10;
	}

	public void Ignite(Vector3 fromPos)
	{
		TryToggle(wantsOn: true);
	}

	public bool CanIgnite()
	{
		if (HasFlag(Flags.Reserved1))
		{
			return !IsOn();
		}
		return false;
	}

	public virtual bool IsAlwaysOn()
	{
		if (HasFlag(Flags.Reserved3))
		{
			return Creative.alwaysOnEnabled;
		}
		return false;
	}

	public void SetAlwaysOn(bool flag)
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved3, flag);
		}
		AlwaysOnToggled(flag);
	}

	public void AlwaysOnToggled(bool flag)
	{
		if (flag)
		{
			if (AllowedTorches == null || AllowedTorches.Length == 0 || base.inventory == null)
			{
				return;
			}
			if (!HasFlag(Flags.Reserved1))
			{
				ItemDefinition torch = AllowedTorches[0].torch;
				if (torch == null)
				{
					return;
				}
				ItemManager.Create(torch, 1, 0uL, isServerSide: true, 0uL).MoveToContainer(base.inventory, 0, allowStack: false);
			}
		}
		TryToggle(flag);
	}
}
